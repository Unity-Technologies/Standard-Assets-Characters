using System;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute for set minimum and maximum values on <see cref="Util.FloatRange"/>
	/// </summary>
	public class MinMaxRangeAttribute : Attribute
	{
		public float min { get; private set; }
		public float max { get; private set; }
		
		public MinMaxRangeAttribute(float minToUse, float maxToUse)
		{
			min = minToUse;
			max = maxToUse;
		}
	}
}