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
		protected Vector2 zeroAngleAxis = new Vector2(0, 1);

		private float previousTargetAngle;

		private float direction;

		protected float CalculateSign(float angle)
		{
			float angleDifference = angle - previousTargetAngle;
			float absoluteAngleDifference = Mathf.Abs(angleDifference);
			if (absoluteAngleDifference > angleThreshold)
			{
				previousTargetAngle = angle;
				float newDirection = Mathf.Sign(angleDifference);
				if (Math.Abs(direction - newDirection) > Mathf.Epsilon)
				{
					if (absoluteAngleDifference < changeThreshold)
					{
						direction = newDirection;
					}
				}
			}

			return direction;
		}
		
		public Quaternion GetNewRotation(Transform toRotate, Quaternion targetRotation, float turnSpeed)
		{
			float direction = CalculateSign(targetRotation.eulerAngles.y);
			Quaternion originalRotation = toRotate.rotation;
			Vector3 euler = toRotate.eulerAngles;
			float rotationAmount = Time.deltaTime * turnSpeed;
			euler.y = euler.y + rotationAmount * direction;
			Quaternion newRotation = Quaternion.Euler(euler);
			if (Quaternion.Angle(originalRotation, newRotation) > Quaternion.Angle(originalRotation, targetRotation))
			{
				return targetRotation;
			}

			return newRotation;
		}
	}
}