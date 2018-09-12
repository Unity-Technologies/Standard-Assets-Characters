using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[CreateAssetMenu(fileName = "HeadBobConfiguration",
		menuName = "Standard Assets/Characters/Create First Person Head Bob Configuration", order = 1)]
	public class HeadBobConfiguration : ScriptableObject
	{
		[SerializeField]
		protected AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, -1, 1, 1);

		[SerializeField]
		protected float movementRange;

		[SerializeField]
		protected float movementTime;

		[SerializeField]
		protected HeadBobMovementAxis movementAxis;

		public float Evaluate(float time, bool isNormalizedTime = false)
		{
			if (!isNormalizedTime)
			{
				time = time / movementTime;
			}
			
			return movementCurve.Evaluate(time) * movementRange;
		}
	}
}