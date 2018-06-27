using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace TFBGames.Editor
{
	/// <summary>
	/// Class to post-process generated csproj and sln files to give them useful platforms
	/// </summary>
	public class SlnPostProcessor : AssetPostprocessor
	{
		private class Group
		{
			public string Name;
			public string[] Symbols;
		}

		private class Platform : Group
		{
			public BuildTargetGroup AssociatedGroup;
			public string UnityEngineRootPath;
		}

		private class Configuration : Group
		{
			public bool Debug;
			public bool Editor;
		}

		private static readonly string[] DebugSymbols =
		{
			"ENABLE_PROFILER",
			"DEBUG",
			"DEVELOPMENT_BUILD",
			"TRACE",
			"UNITY_ASSERTIONS"
		};

		private static readonly string[] EditorSymbols =
		{
			"UNITY_EDITOR",
			"UNITY_EDITOR_64",
			"UNITY_EDITOR_OSX",
			"UNITY_EDITOR_WIN",
			"UNITY_TEAM_LICENSE",
			"ENABLE_PROFILER",
			"DEBUG",
			"DEVELOPMENT_BUILD",
			"TRACE",
			"UNITY_ASSERTIONS"
		};

		private static readonly string[] WindowsSymbols =
		{
			"UNITY_STANDALONE_WIN",
			"UNITY_STANDALONE"
		};

		private static readonly string[] OsxSymbols =
		{
			"UNITY_STANDALONE_OSX",
			"UNITY_STANDALONE"
		};

		private static readonly string[] LinuxSymbols =
		{
			"UNITY_STANDALONE_LINUX",
			"UNITY_STANDALONE"
		};

		private static readonly string[] AndroidSymbols =
		{
			"UNITY_ANDROID",
			"UNITY_ANDROID_API"
		};

		private static readonly string[] IosSymbols =
		{
			"UNITY_IOS",
			"UNITY_IPHONE",
			"UNITY_IPHONE_API",
			"ENABLE_IOS_ON_DEMAND_RESOURCES",
			"ENABLE_IOS_APP_SLICING"
		};

		private static readonly string[] SwitchSymbols =
		{
			"UNITY_SWITCH",
			"UNITY_SWITCH_API"
		};

		private static readonly string[] Ps4Symbols =
		{
			"UNITY_PS4",
			"UNITY_PS4_API"
		};

		private static readonly string[] PreviousDefines =
		{
			"$(DefineConstants)"
		};

		private static IEnumerable<string> _allDynamicSymbols =
			DebugSymbols
				.Concat(EditorSymbols)
				.Concat(WindowsSymbols)
				.Concat(OsxSymbols)
				.Concat(LinuxSymbols)
				.Concat(AndroidSymbols)
				.Concat(IosSymbols)
				.Concat(SwitchSymbols)
				.Concat(Ps4Symbols);

		private static readonly Configuration[] Configurations =
		{
			new Configuration() {Name = "Editor", Symbols = EditorSymbols, Debug = true, Editor = true},
			new Configuration() {Name = "Debug", Symbols = DebugSymbols, Debug = true, Editor = false},
			new Configuration() {Name = "Release", Symbols = new string[0], Debug = false, Editor = false}
		};

		private static readonly List<Platform> Platforms = new List<Platform>()
		{
			new Platform()
			{
				Name = "Android",
				Symbols = AndroidSymbols,
				AssociatedGroup = BuildTargetGroup.Android,
				UnityEngineRootPath = "PlaybackEngines/AndroidPlayer/variations/mono/Managed/"
			},
			new Platform()
			{
				Name = "IOs",
				Symbols = IosSymbols,
				AssociatedGroup = BuildTargetGroup.iOS,
				UnityEngineRootPath = "PlaybackEngines/iOSSupport/Managed/"
			},
			new Platform()
			{
				Name = "Windows",
				Symbols = WindowsSymbols,
				AssociatedGroup = BuildTargetGroup.Standalone,
				UnityEngineRootPath = "PlaybackEngines/WindowsStandaloneSupport/Managed/"
			},
			new Platform()
			{
				Name = "OSX",
				Symbols = OsxSymbols,
				AssociatedGroup = BuildTargetGroup.Standalone,
				UnityEngineRootPath = "PlaybackEngines/macstandalonesupport/Managed/"
			},
			new Platform()
			{
				Name = "Switch",
				Symbols = SwitchSymbols,
				AssociatedGroup = BuildTargetGroup.Switch,
				UnityEngineRootPath = "PlaybackEngines/Switch/Managed/"
			},
			new Platform()
			{
				Name = "PS4",
				Symbols = Ps4Symbols,
				AssociatedGroup = BuildTargetGroup.PS4,
				UnityEngineRootPath = "PlaybackEngines/PS4/Managed/"
			}
		};

		private static bool UsingMonodev
		{
			get { return EditorPrefs.GetString("kScriptsDefaultApp").Contains("internet"); }
		}

		[JetBrains.Annotations.UsedImplicitly]
		// ReSharper disable once InconsistentNaming -- Is Unity message
		public static void OnGeneratedCSProjectFiles()
		{
			string currentDirectory = Directory.GetCurrentDirectory();
			string[] projectFiles = Directory.GetFiles(currentDirectory, "*.csproj");

			// Create platform list
			List<Platform> installedPlatforms =
				Platforms.Where(platform =>
				         {
					         var platformLibPath =
						         GetLibPath(platform.UnityEngineRootPath);

					         if (!Directory.Exists(platformLibPath))
					         {
						         return false;
					         }

					         return true;
				         })
				         .ToList();

			// Place current platform first
			Platform defaultPlatform =
				installedPlatforms.FirstOrDefault(p => p.AssociatedGroup ==
				                                       EditorUserBuildSettings.selectedBuildTargetGroup);

			if (installedPlatforms[0] != defaultPlatform && defaultPlatform != null)
			{
				Platform prev = installedPlatforms[0];
				installedPlatforms[installedPlatforms.IndexOf(defaultPlatform)] = prev;
				installedPlatforms[0] = defaultPlatform;
			}

			foreach (string file in projectFiles)
			{
				UpgradeProjectFile(file, installedPlatforms);
			}

			string[] solutionFiles = Directory.GetFiles(currentDirectory, "*.sln");
			foreach (string file in solutionFiles)
			{
				UpdateSolutionFile(file, installedPlatforms);
			}
		}

		private static void UpdateSolutionFile(string solutionFile, List<Platform> platforms)
		{
			var combinations = new List<string>();

			foreach (Configuration configuration in Configurations)
			{
				foreach (Platform platform in platforms)
				{
					string configurationName = string.Format("{0}|{1}", configuration.Name, platform.Name);
					combinations.Add(configurationName);
				}
			}

			string[] solutionLines = File.ReadAllLines(solutionFile);
			var newFile = new List<string>();
			var projects = new List<string>();
			var editorProjects = new List<string>();

			var projectRegex = new Regex(@"Project\(""{(.*)}""\) = "".*"", ""(.*\.csproj)"", ""({.*})""");

			bool inGroup = false;

			// Parse file
			foreach (string line in solutionLines)
			{
				// In a group?
				if (inGroup)
				{
					if (line.Trim().StartsWith("EndGlobalSection"))
					{
						inGroup = false;
						newFile.Add(line);
					}

					continue;
				}

				var match = projectRegex.Match(line);
				if (match.Success)
				{
					string projFileName = match.Groups[2].Value;
					string projId = match.Groups[3].Value;

					if (projFileName.Contains("Editor"))
					{
						editorProjects.Add(projId);
					}
					else
					{
						projects.Add(projId);
					}

					// To make sure each project has a unique name, rewrite these lines (fixes problem on Windows where all
					// projects are called Project)
					newFile.Add(string.Format(@"Project(""{{{0}}}"") = ""{1}"", ""{2}"", ""{3}""",
					                          match.Groups[1].Value,
					                          projFileName.Substring(0, projFileName.IndexOf(".csproj")),
					                          projFileName,
					                          projId));

					continue;
				}

				if (!string.IsNullOrEmpty(line.Trim()))
				{
					newFile.Add(line);
				}

				if (line.Trim().StartsWith("GlobalSection(SolutionConfigurationPlatforms)"))
				{
					inGroup = true;
					// Write out configurations
					foreach (string combination in combinations)
					{
						newFile.Add(string.Format("\t\t{0} = {0}", combination));
					}

					continue;
				}

				if (line.Trim().StartsWith("GlobalSection(ProjectConfigurationPlatforms)"))
				{
					inGroup = true;

					// Write out all projects
					foreach (string project in projects.Concat(editorProjects))
					{
						foreach (Configuration configuration in Configurations)
						{
							foreach (Platform platform in platforms)
							{
								newFile.Add(string.Format("\t\t{0}.{1}|{2}.ActiveCfg = {1}|{2}",
								                          project, configuration.Name, platform.Name));

								if ((configuration.Name == "Editor") || !editorProjects.Contains(project))
								{
									newFile.Add(string.Format("\t\t{0}.{1}|{2}.Build.0 = {1}|{2}",
									                          project, configuration.Name, platform.Name));
								}
							}
						}
					}
				}
			}

			File.WriteAllLines(solutionFile, newFile.ToArray());
		}

		private static void UpgradeProjectFile(string projectFile, List<Platform> platforms)
		{
			XDocument doc = XDocument.Load(projectFile);
			XElement projectContentElement = doc.Root;
			XNamespace xmlns = projectContentElement.Name.NamespaceName;

			string[] defines = EditorUserBuildSettings.activeScriptCompilationDefines;
			string[] currentPlatformDefines = PlayerSettings
			                                  .GetScriptingDefineSymbolsForGroup(
				                                  EditorUserBuildSettings.selectedBuildTargetGroup)
			                                  .Split(';');

			string[] baseDefines = defines.Except(_allDynamicSymbols)
			                              .Except(currentPlatformDefines)
			                              .ToArray();

			// Remove unity conditional propertyGroups with bad platforms
			RemoveAllPropertyGroupsWithConditions(xmlns, projectContentElement);

			// Remove UnityEngine and UnityEditor references. We'll add them back with proper references per platform
			if (!UsingMonodev)
			{
				RemoveUnityDlls(xmlns, projectContentElement);
			}

			XElement defaultPropertyGroup =
				projectContentElement.Elements()
				                     .First(e => e.Elements().Any(ce => ce.Name == xmlns + "Configuration") &&
					                             e.Elements().Any(ce => ce.Name == xmlns + "Platform"));

			Platform defaultPlatform =
				platforms.FirstOrDefault(p => p.AssociatedGroup == EditorUserBuildSettings.selectedBuildTargetGroup);
			
			// Remove property group with 'default' condition - we cover it with all our specialised ones
#if UNITY_2018_OR_NEWER
			XElement defaultConditionalPropertyGroup =
				projectContentElement.Elements()
				                     .First(e => e.Attributes().Any(a => a.Name == "Condition"));
			defaultConditionalPropertyGroup.Remove();
#endif

			if (defaultPlatform == null)
			{
				defaultPlatform = platforms[0];
			}

			// Set default config
			defaultPropertyGroup.Elements().First(e => e.Name == xmlns + "Configuration").Value = "Editor";
			defaultPropertyGroup.Elements().First(e => e.Name == xmlns + "Platform").Value = defaultPlatform.Name;

			// Set global options that are not platform specific
			SetCommon(xmlns, defaultPropertyGroup, baseDefines);
			
			// Add defines and settings per configuration
			foreach (Configuration configuration in Configurations)
			{
				string[] configurationSymbols = PreviousDefines.Concat(configuration.Symbols).ToArray();
				XElement element = CreateConfigurationWithDefineConstants(xmlns, configurationSymbols,
				                                                          "Configuration", configuration.Name);

				if (configuration.Debug)
				{
					SetDebug(xmlns, element);
				}
				else
				{
					SetRelease(xmlns, element);
				}

				// Add refs for editor configs
				if (!UsingMonodev && configuration.Editor)
				{
					AddReference(xmlns, projectContentElement,
					             "PlaybackEngines/iOSSupport/UnityEditor.iOS.Extensions.Xcode.dll",
					             "Configuration", configuration.Name);
					AddReference(xmlns, projectContentElement,
					             "PlaybackEngines/iOSSupport/UnityEditor.iOS.Extensions.Common.dll",
					             "Configuration", configuration.Name);

					// UnityEditor
					AddReference(xmlns, projectContentElement,
					             "Managed/UnityEditor.dll",
					             "Configuration", configuration.Name);
				}

				defaultPropertyGroup.AddAfterSelf(element);
			}

			// Add defines and settings per platform
			foreach (Platform platform in platforms)
			{
				var platformLibPath = GetLibPath(platform.UnityEngineRootPath);
				string[] unityDefines = PlayerSettings
				                        .GetScriptingDefineSymbolsForGroup(platform.AssociatedGroup)
				                        .Split(';');
				string[] platformSymbols = PreviousDefines
				                           .Concat(platform.Symbols)
				                           .Concat(unityDefines)
				                           .ToArray();

				XElement element = CreateConfigurationWithDefineConstants(xmlns, platformSymbols,
				                                                          "Platform", platform.Name);
				
				defaultPropertyGroup.AddAfterSelf(element);
				
				// Find all Engine DLLs in root folder and add them all
				if (!UsingMonodev)
				{
					if (platformLibPath != null)
					{
						string[] engineLibs = Directory.GetFiles(platformLibPath, "UnityEngine*.dll");

						foreach (string engineDlL in engineLibs)
						{
							// Non editor platforms use game specific UnityEngine dll
							AddReference(xmlns, projectContentElement, engineDlL,
							             "Platform", platform.Name);
						}
					}
					else
					{
						Debug.LogWarningFormat(
							"Couldn't find platform libraries for platform {0}. Probably it is not installed.",
							platform.Name);
					}
				}
			}

			// Fix target framework version
			var targetFrameworkVersion = projectContentElement
			                             .Elements(xmlns + "PropertyGroup")
			                             .Elements(xmlns + "TargetFrameworkVersion")
			                             .FirstOrDefault();
			if (targetFrameworkVersion != null)
			{
				var version = new Version(targetFrameworkVersion.Value.Substring(1));
				if (version < new Version(4, 5))
				{
					targetFrameworkVersion.SetValue("v4.5");
				}
			}

			doc.Save(projectFile);
		}

		/// <summary>
		/// Remove Unity-configured configurations
		/// </summary>
		private static void RemoveAllPropertyGroupsWithConditions(XNamespace xmlns, XElement rootElement)
		{
			foreach (XNode node in rootElement.Nodes())
			{
				var element = node as XElement;

				if ((element != null) &&
				    (element.Name == xmlns + "PropertyGroup") &&
				    element.Attributes().Any(a => a.Name == "Condition"))
				{
					node.Remove();
				}
			}
		}


		/// <summary>
		/// Remove references
		/// </summary>
		private static void RemoveUnityDlls(XNamespace xmlns, XElement rootElement)
		{
			foreach (XNode itemGroupNode in rootElement.Nodes())
			{
				var itemGroup = itemGroupNode as XElement;
				if ((itemGroup != null) && (itemGroup.Name == xmlns + "ItemGroup"))
				{
					var refsToRemove = new List<XNode>();
					// Find references
					foreach (XNode referenceNode in itemGroup.Nodes())
					{
						var reference = referenceNode as XElement;
						if ((reference != null) &&
						    (reference.Name == xmlns + "Reference") &&
						    reference.Attributes()
						             .Any(
							             a => a.Name == "Include" &&
							                  (a.Value.Contains("UnityEngine") ||
							                   a.Value.Contains("UnityEditor"))) &&
							!reference.Elements().Any(e =>
								e.Name == xmlns + "HintPath" &&
								e.Value.Contains("UnityExtensions"))) // Leave unity extensions that are not platform specific
						{
							refsToRemove.Add(reference);
						}
					}

					foreach (XNode xNode in refsToRemove)
					{
						xNode.Remove();
					}

					if (itemGroup.IsEmpty)
					{
						itemGroup.Remove();
					}
				}
			}
		}

		private static void SetCommon(XNamespace xmlns, XElement propertyGroup, string[] defines)
		{
			propertyGroup.Add(
				new XElement(xmlns + "DebugType", "full"),
				new XElement(xmlns + "Optimize", false),
				new XElement(xmlns + "DebugSymbols", true),
				new XElement(xmlns + "OutputPath", @"Temp\bin\Debug"),
				new XElement(xmlns + "ErrorReport", "prompt"),
				new XElement(xmlns + "WarningLevel", 4),
				new XElement(xmlns + "NoWarn", 0169),
				new XElement(xmlns + "DefineConstants", defines.Aggregate((a, r) => a + ";" + r))
			);
		}

		private static XElement CreateConfigurationWithDefineConstants(XNamespace xmlns, string[] defines,
		                                                               string testParameter, string testValue)
		{
			var element = new XElement(
				xmlns + "PropertyGroup",
				new XAttribute("Condition", string.Format("'$({0})' == '{1}'", testParameter, testValue))
			);
			if (defines.Length > 0)
			{
				element.Add(
					new XElement(xmlns + "DefineConstants", defines.Aggregate((a, r) => a + ";" + r)));
			}

			return element;
		}

		private static void SetDebug(XNamespace xmlns, XElement propertyGroup)
		{
			propertyGroup.AddFirst(
				new XElement(xmlns + "DebugType", "full"),
				new XElement(xmlns + "Optimize", false),
				new XElement(xmlns + "DebugSymbols", true),
				new XElement(xmlns + "OutputPath", @"Temp\bin\Debug")
			);
		}

		private static void SetRelease(XNamespace xmlns, XElement propertyGroup)
		{
			propertyGroup.AddFirst(
				new XElement(xmlns + "DebugType", "pdbonly"),
				new XElement(xmlns + "Optimize", false),
				new XElement(xmlns + "OutputPath", @"Temp\bin\Release")
			);
		}

		/// <summary>
		/// Gets a full path to a library given a unity-installation relative path
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static string GetLibPath(string path)
		{
			string unityAppBaseFolder = Path.GetDirectoryName(EditorApplication.applicationPath);

			// Windows
			string dllPath = Path.Combine(unityAppBaseFolder, Path.Combine("Data", path));
			if (Directory.Exists(dllPath))
			{
				return dllPath;
			}

			// OSX
			dllPath = Path.Combine(unityAppBaseFolder, path);
			if (Directory.Exists(dllPath))
			{
				return dllPath;
			}

			// Bundled OSX
			dllPath = Path.Combine(EditorApplication.applicationPath, Path.Combine("Contents", path));

			return !Directory.Exists(dllPath) ? null : dllPath;
		}

		private static void AddReference(XNamespace xmlns, XElement parentElement, string dll, 
		                                 string testParameter,
		                                 string testValue)
		{
			string unityAppBaseFolder = Path.GetDirectoryName(EditorApplication.applicationPath);

			// Try find dll path based on three different formats
			// for absolute
			string dllPath = dll;

			// For windows
			if (!File.Exists(dllPath))
			{
				dllPath = Path.Combine(unityAppBaseFolder, Path.Combine("Data", dll));
			}

			// For OSX plugin
			if (!File.Exists(dllPath))
			{
				dllPath = Path.Combine(unityAppBaseFolder, dll);
			}

			// For OSX bundled
			if (!File.Exists(dllPath))
			{
				dllPath = Path.Combine(EditorApplication.applicationPath, Path.Combine("Contents", dll));
			}

			if (File.Exists(dllPath))
			{
				string condition = string.Format("'$({0})' == '{1}'", testParameter, testValue);

				// Try find item group with condition, else create
				XElement itemGroup = parentElement.Nodes()
				                                  .FirstOrDefault(n =>
					                                  {
						                                  XElement element = n as XElement;
						                                  return element != null &&
						                                         element.Name == xmlns + "ItemGroup" &&
						                                         element.Attributes()
						                                                .Any(a => a.Name == "Condition" &&
						                                                          a.Value == condition);
					                                  }) as XElement;

				if (itemGroup == null)
				{
					itemGroup = new XElement(xmlns + "ItemGroup");
					itemGroup.Add(new XAttribute("Condition", condition));
					parentElement.Add(itemGroup);
				}

				var reference = new XElement(xmlns + "Reference");
				reference.Add(new XAttribute("Include", Path.GetFileNameWithoutExtension(dllPath)));
				reference.Add(new XElement(xmlns + "HintPath", dllPath));
				itemGroup.Add(reference);
			}
		}
	}
}