using System;

namespace StandardAssets.Characters.Attributes
{
	/// <summary>
	/// Attribute for set minimum and maximum values on <see cref="Util.FloatRange"/>
	/// </summary>
	public class FloatRangeSetupAttribute : Attribute
	{
		public float min { get; private set; }
		public float max { get; private set; }
		public int decimalPoints { get; private set; }
		
		public FloatRangeSetupAttribute(float minToUse, float maxToUse, int decimalPointsToUse = 2)
		{
			min = minToUse;
			max = maxToUse;
			decimalPoints = decimalPointsToUse;
		}
	}
}