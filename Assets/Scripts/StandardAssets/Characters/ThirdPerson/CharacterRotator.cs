using System;
using StandardAssets.Characters.CharacterInput;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class CharacterRotator
	{
		[SerializeField]
		protected float angleThreshold = 10f, changeThreshold = 270f;

		[SerializeField]
		protected Vector2 zeroAngleAxis = new Vector2(0,1);
		
		private float previousInputAngle;

		private ICharacterInput input;

		private float direction;

		public void Init(ICharacterInput inputToUse)
		{
			input = inputToUse;
		}

		public void Tick()
		{
			float angle = Vector2Utilities.Angle(zeroAngleAxis, input.moveInput);
			float angleDifference = angle - previousInputAngle;
			float absoluteAngleDifference = Mathf.Abs(angleDifference);
			if (absoluteAngleDifference > angleThreshold)
			{
				previousInputAngle = angle;
				float newDirection = Mathf.Sign(angleDifference);
				if (Math.Abs(direction - newDirection) > Mathf.Epsilon)
				{
					if (absoluteAngleDifference < changeThreshold)
					{
						direction = newDirection;
					}
				}
				Debug.Log(direction);
			}
		}
	}
}