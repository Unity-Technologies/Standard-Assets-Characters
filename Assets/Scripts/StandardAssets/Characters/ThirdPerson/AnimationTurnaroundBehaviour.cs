using System;

namespace StandardAssets.Characters.ThirdPerson
{
	[Serializable]
	public class AnimationTurnaroundBehaviour : TurnaroundBehaviour
	{
		public string rapidTurnParameter = "RapidTurn";
		
		//TODO actually implement
		public override void Init(ThirdPersonBrain brain)
		{
			throw new System.NotImplementedException();
		}

		public override void Update()
		{
			throw new System.NotImplementedException();
		}

		protected override void FinishedTurning()
		{
			throw new System.NotImplementedException();
		}

		protected override void StartTurningAround(float angle)
		{
			throw new System.NotImplementedException();
		}
	}
}