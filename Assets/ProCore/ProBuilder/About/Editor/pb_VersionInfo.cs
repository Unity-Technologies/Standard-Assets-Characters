using System.Text.RegularExpressions;

namespace ProBuilder2.EditorCommon
{
	public enum VersionType
	{
		Final = 3,
		Beta = 2,
		Patch = 1
	}

	[System.Serializable]
	public struct pb_VersionInfo : System.IEquatable<pb_VersionInfo>, System.IComparable<pb_VersionInfo>
	{
		public int major;
		public int minor;
		public int patch;
		public int build;
		public VersionType type;
		public string text;
		public bool valid;

		public override bool Equals(object o)
		{
			return o is pb_VersionInfo && this.Equals((pb_VersionInfo) o);
		}

		public override int GetHashCode()
		{
			int hash = 13;

			unchecked
			{
				if(valid)
				{
					hash = (hash * 7) + major.GetHashCode();
					hash = (hash * 7) + minor.GetHashCode();
					hash = (hash * 7) + patch.GetHashCode();
					hash = (hash * 7) + build.GetHashCode();
					hash = (hash * 7) + type.GetHashCode();
				}
				else
				{
					return text.GetHashCode();
				}
			}

			return hash;
		}

		public bool Equals(pb_VersionInfo version)
		{
			if(valid != version.valid)
				return false;

			if(valid)
			{
				return 	major == version.major &&
						minor == version.minor &&
						patch == version.patch &&
						type == version.type &&
						build == version.build;
			}
			else
			{
				if( string.IsNullOrEmpty(text) || string.IsNullOrEmpty(version.text) )
					return false;

				return text.Equals(version.text);
			}
		}

		public int CompareTo(pb_VersionInfo version)
		{
			const int GREATER = 1;
			const int LESS = -1;

			if(this.Equals(version))
				return 0;
			else if(major > version.major)
				return GREATER;
			else if(major < version.major)
				return LESS;
			else if(minor > version.minor)
				return GREATER;
			else if(minor < version.minor)
				return LESS;
			else if(patch > version.patch)
				return GREATER;
			else if(patch < version.patch)
				return LESS;
			else if((int)type > (int)version.type)
				return GREATER;
			else if((int)type < (int)version.type)
				return LESS;
			else if(build > version.build)
				return GREATER;
			else
				return LESS;
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}{3}{4}", major, minor, patch, type.ToString().ToLower()[0], build);
		}

		/**
		 *	Create a pb_VersionInfo type from a string.
		 *	Ex: "2.5.3b1"
		 */
		public static pb_VersionInfo FromString(string str)
		{
			pb_VersionInfo version = new pb_VersionInfo();
			version.text = str;

			try
			{
				string[] split = Regex.Split(str, @"[\.A-Za-z]");
				Match type = Regex.Match(str, @"A-Za-z");
				int.TryParse(split[0], out version.major);
				int.TryParse(split[1], out version.minor);
				int.TryParse(split[2], out version.patch);
				int.TryParse(split[3], out version.build);
				version.type = GetVersionType(type != null && type.Success ? type.Value : "");
				version.valid = true;
			}
			catch
			{
				version.valid = false;
			}

			return version;
		}

		static VersionType GetVersionType(string type)
		{
			if( type.Equals("b") || type.Equals("B") )
				return VersionType.Beta;
			else if( type.Equals("p") || type.Equals("P") )
				return VersionType.Patch;

			return VersionType.Final;
		}
	}
}
