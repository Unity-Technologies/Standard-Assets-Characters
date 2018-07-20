using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonBrain : MonoBehaviour
	{
		[SerializeField, Tooltip("Properties of the root motion motor")]
		protected RootMotionThirdPersonMotor rootMotionMotor;

		[SerializeField, Tooltip("Properties of the animation controller")]
		protected ThirdPersonAnimationController animationController;

		private IThirdPersonMotor currentMotor;

		public ThirdPersonAnimationController animationControl
		{
			get { return animationController; }
		}

		public RootMotionThirdPersonMotor rootMotionThirdPersonMotor
		{
			get { return rootMotionThirdPersonMotor; }
		}

		private void Awake()
		{
			currentMotor = rootMotionMotor;
			currentMotor.Init(this);
			if (animationController != null)
			{
				animationController.Init(this, currentMotor);
			}
		}

		private void OnEnable()
		{
			if (animationController != null)
			{
				animationController.Subscribe();
			}
			
			currentMotor.Subscribe();
		}
		
		private void OnDisable()
		{
			if (animationController != null)
			{
				animationController.Unsubscribe();
			}
			
			currentMotor.Unsubscribe();
		}

		private void Update()
		{
			if (animationController != null)
			{
				animationController.Update();
			}
			
			currentMotor.Update();
		}

		private void OnAnimatorMove()
		{
			currentMotor.OnAnimatorMove();
		}

		private void OnAnimatorIK(int layerIndex)
		{
			if (animationController != null)
			{
				animationController.HeadTurn();
			}
		}
	}
}