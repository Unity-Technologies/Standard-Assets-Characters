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
			animationController.Init(this, currentMotor);
		}

		private void OnEnable()
		{
			animationController.Subscribe();
			currentMotor.Subscribe();
		}
		
		private void OnDisable()
		{
			animationController.Unsubscribe();
			currentMotor.Unsubscribe();
		}

		private void Update()
		{
			animationController.Update();
			currentMotor.Update();
		}

		private void OnAnimatorMove()
		{
			currentMotor.OnAnimatorMove();
		}
	}
}