using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEditor.Build.Reporting;

#if USE_ASSETBUNDLES
using TFBGames.Management.Asset;
#endif

#if USE_WWISE
using System.IO;
#endif

namespace TFBGames.Editor
{
	/// <summary>
	/// Build system scripts. Intended to be invoked via the command line.
	/// </summary>
	public static class BuildSystem
	{
		[MenuItem("24 Bit Games/Build Project", priority = 1)]
		public static void BuildInEditor()
		{
			DoBuild(EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None, EditorUserBuildSettings.activeBuildTarget, null, null, false);
		}

		[MenuItem("24 Bit Games/Build Project with Bundles", priority = 1)]
		public static void BuildInEditorWithBundles()
		{
			DoBuild(EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None, EditorUserBuildSettings.activeBuildTarget, null, null, true);
		}


		/// <summary>
		/// Generate our solution file
		/// </summary>
		public static void GenerateSolution()
		{
			EditorApplication.ExecuteMenuItem("Assets/Open C# Project");
		}


		/// <summary>
		/// Invoke a build from the command line. Can take a number of command line arguments
		/// --platform=X          Select the platform to build for (one of iOS or Android)
		/// --developmentMode=X   Build in development mode (default to 'false')
		/// --buildNumber=x	      Set the build number (optional)
		/// --transferBundles=x   Whether to copy the appropriate asset bundles, if they exist
		/// --noSymlink=x         Whether to do the symlinking
		/// -o=x                  Output build path (optional)
		/// </summary>
		public static void Build()
		{
			var options = BuildOptions.None;
			BuildTarget target;
			bool devMode;
			int? buildNumber;
			string outputDir;
			bool transferBundles;
			bool symlink;

			ParseCommandLine(out target, out devMode, out buildNumber, out outputDir, out transferBundles, out symlink);
			RunCustomSettings();

			if (devMode)
			{
				options |= BuildOptions.Development;
			}

			DoBuild(options, target, buildNumber, outputDir, transferBundles, symlink);
		}

		/// <summary>
		/// Perform actual build tasks
		/// </summary>
		public static void DoBuild(BuildOptions options, BuildTarget target, int? buildNumber, string outputDir,
		                           bool transferBundles = true, bool symlink = true)
		{
			if (buildNumber.HasValue)
			{
				PlayerSettings.iOS.buildNumber = buildNumber.ToString();
				PlayerSettings.Android.bundleVersionCode = buildNumber.Value;
				
#if UNITY_SWITCH
				try
				{
					Version ver = new Version(PlayerSettings.Switch.displayVersion);
					PlayerSettings.Switch.displayVersion =
						new Version(ver.Major, ver.Minor, buildNumber.Value).ToString();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
#endif
			}

#if USE_WWISE
			string wWisePlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
#endif

			// Default path
			switch (target)
			{
				case BuildTarget.StandaloneOSX:
#if USE_WWISE
					wWisePlatform = "Mac";
#endif
					if (outputDir == null)
					{
						outputDir = "../Builds/OSX";
					}

					break;
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
#if USE_WWISE
					wWisePlatform = "Windows";
#endif
					if (outputDir == null)
					{
						outputDir = "../Builds/PC";
					}

					break;
				case BuildTarget.iOS:
					PlayerSettings.iOS.appleEnableAutomaticSigning = false;
					if (symlink)
					{
						options |= BuildOptions.SymlinkLibraries;
					}

					if (outputDir == null)
					{
						outputDir = "../Builds/XcodeProject";
					}

					break;
				case BuildTarget.Android:
					if (outputDir == null)
					{
						outputDir = "../Builds/build.apk";
					}

					break;
				case BuildTarget.Switch:
#if UNITY_SWITCH
					if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
					{
						EditorUserBuildSettings.switchCreateRomFile = true;
					}
#endif
					if (outputDir == null)
					{
						outputDir = EditorUserBuildSettings.switchCreateRomFile
							? "../Builds/build.nsp"
							: "../Builds/build.nspd";
					}
					break;
				default:
					throw new InvalidOperationException("Unsupported platform " + target);
			}

#if USE_WWISE
			string wwiseProjFile = Path.Combine(Application.dataPath, WwiseSetupWizard.Settings.WwiseProjectPath);
			string wwiseProjectFolder = wwiseProjFile.Remove(wwiseProjFile.LastIndexOf(Path.DirectorySeparatorChar));
			string sourceSoundBankFolder = Path.Combine(wwiseProjectFolder, AkBasePathGetter.GetPlatformBasePath());
			string destinationSoundBankFolder = Path.Combine(Path.Combine(Application.dataPath, "StreamingAssets"),
															 Path.Combine(WwiseSetupWizard.Settings.SoundbankPath, wWisePlatform));
			//Copy the soundbank from the Wwise project to the unity project (Inside the StreamingAssets folder as defined in Window->Wwise Settings)
			if (!AkUtilities.DirectoryCopy(sourceSoundBankFolder,      //source folder
										   destinationSoundBankFolder, //destination
										   true))                      //copy subfolders
			{
				Debug.LogError("WwiseUnity: The soundbank folder for the " + wWisePlatform + " platform doesn't exist. Make sure it was generated in your Wwise project");
				throw new Exception("Build error occurred:\nCould not find wWise soundbank");
			}
#endif

#if USE_ASSETBUNDLES
			string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
			List<string> copiedBundles = null;
			bool createdStreamingAssets = false;

			if (transferBundles)
			{
				// Copy bundles to asset bundle folder
				string bundlePath = AssetBundleManager.GetPathForBundles(target);

				if (!Directory.Exists(bundlePath))
				{
					throw new Exception(string.Format("No asset bundles to copy for platform {0}", target));
				}

				copiedBundles = new List<string>();

				// Make streaming assets path
				if (!Directory.Exists(streamingAssetsPath))
				{
					createdStreamingAssets = true;
					Directory.CreateDirectory(streamingAssetsPath);
				}

				foreach (string file in Directory.GetFiles(bundlePath))
				{
					// Skip metas and manifests
					if (!file.EndsWith(".meta") && 
					    !file.EndsWith(".manifest"))
					{
						string outputFile = Path.Combine(streamingAssetsPath, Path.GetFileName(file));
						File.Copy(file, outputFile);
						copiedBundles.Add(outputFile);
					}
				}
			}
#endif

			bool buildSucceeded;
#if UNITY_2018_1_OR_NEWER
			BuildReport report = BuildPipeline.BuildPlayer(GetScenes(), outputDir, target, options);
			buildSucceeded = report.summary.result == BuildResult.Succeeded;
#else
			string error = BuildPipeline.BuildPlayer(GetScenes(), outputDir, target, options);
			buildSucceeded = string.IsNullOrEmpty(error);
#endif
	
#if USE_WWISE
			Directory.Delete(destinationSoundBankFolder, true);
#endif

#if USE_ASSETBUNDLES
			// Delete copied bundles
			if (transferBundles)
			{
				foreach (string copiedFile in copiedBundles)
				{
					File.Delete(copiedFile);
				}

				if (createdStreamingAssets)
				{
					Directory.Delete(streamingAssetsPath);
				}
			}
#endif

			if (buildSucceeded)
			{
				throw new Exception("Build error occurred");
			}
			
#if UNITY_SWITCH
			// Copy debug symbols out to build folder too
			string debugSymbolsPath =
				Path.Combine(Application.dataPath, @"..\Temp\StagingArea\Native\SwitchPlayer.nss");
			string destination = Path.Combine(Path.GetDirectoryName(outputDir), "SwitchPlayer.nss");
			if (File.Exists(debugSymbolsPath))
			{
				File.Copy(debugSymbolsPath, destination, true);
				Debug.LogFormat("Copied Switch debug symbols to {0}", destination);
			}
#endif
		}


		private static string[] GetScenes()
		{
			return EditorBuildSettings.scenes.Where(t => t.enabled).Select(t => t.path).ToArray();
		}


		private static string TryGetCommandLineOption(string option, string defaultOption = null)
		{
			var args = Environment.GetCommandLineArgs();
			string searchString = option + "=";

			string result = args.FirstOrDefault(t => t.StartsWith(searchString));

			// Split out option setting, or use default
			result = result == null || result.Length == searchString.Length
				? defaultOption
				: result.Substring(searchString.Length);

			return result;
		}


		private static bool TryGetCommandLineFlag(string option)
		{
			var args = Environment.GetCommandLineArgs();

			return args.Any(t => t.Trim() == option);
		}

		private static void ParseCommandLine(out BuildTarget buildTarget, out bool devMode, out int? buildNumber, out string outPath, out bool transferBundles, out bool symlink)
		{
			string platform = TryGetCommandLineOption("--platform");
			if (platform == null)
			{
				throw new InvalidOperationException("Must provide platform");
			}

			buildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), platform);

			devMode = TryGetCommandLineOption("--developmentMode", "false").ToLowerInvariant() == "true";
			transferBundles = TryGetCommandLineOption("--transferBundles", "false").ToLowerInvariant() == "true";
			symlink = TryGetCommandLineOption("--noSymlink", "false").ToLowerInvariant() == "false";

			buildNumber = null;
			string buildNumberString = TryGetCommandLineOption("--buildNumber");
			if (buildNumberString != null)
			{
				int outBuildNumber;
				if (!int.TryParse(buildNumberString, out outBuildNumber))
				{
					Debug.LogError("Failed to parse build number from command line.");
				}
				else
				{
					buildNumber = outBuildNumber;
				}
			}

			outPath = TryGetCommandLineOption("-o");
		}

		private static void RunCustomSettings()
		{
			// Find all methods marked with BuildConfiguration attribute
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				var methods = assembly.GetTypes()
									  .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
									  .Where(m => m.GetCustomAttributes(typeof(BuildConfigurationAttribute), false).Any())
									  .ToArray();

				foreach (var method in methods)
				{
					bool execute = false;

					foreach (
						var attr in
						method.GetCustomAttributes(typeof(BuildConfigurationAttribute), false)
							  .Cast<BuildConfigurationAttribute>())
					{
						if (TryGetCommandLineFlag("--" + attr.BuildStepFlag))
						{
							Debug.Log("Found command line flag " + attr.BuildStepFlag);
							execute = true;
							break;
						}
					}

					if (execute)
					{
						Debug.Log("Executing method " + method);
						method.Invoke(null, null);
					}
				}
			}
		}

		[BuildConfiguration("enableSpritePacking")]
		[JetBrains.Annotations.UsedImplicitly]
		private static void EnableSpritePacking()
		{
			EditorSettings.spritePackerMode = SpritePackerMode.BuildTimeOnly;
		}

		[BuildConfiguration("testFlag")]
		[JetBrains.Annotations.UsedImplicitly]
		private static void TestBuildSetting()
		{
			Debug.Log("This method doesn't do anything, but it writes to the log!");
		}

		[BuildConfiguration("androidSign")]
		[JetBrains.Annotations.UsedImplicitly]
		private static void DoAndroidSigning()
		{
			string keyPass = TryGetCommandLineOption("--keystorePass");
			string alias = TryGetCommandLineOption("--keyAlias");
			string aliasPass = TryGetCommandLineOption("--keyAliasPass");

			if (!string.IsNullOrEmpty(keyPass) &&
			    !string.IsNullOrEmpty(alias) &&
			    !string.IsNullOrEmpty(aliasPass))
			{
				PlayerSettings.Android.keystorePass = keyPass;
				PlayerSettings.Android.keyaliasName = alias;
				PlayerSettings.Android.keyaliasPass = aliasPass;
			}
		}

		[BuildConfiguration("osxSign")]
		[JetBrains.Annotations.UsedImplicitly]
		private static void DoOsxSigning()
		{
			PlayerSettings.useMacAppStoreValidation = true;
		}
	}
}