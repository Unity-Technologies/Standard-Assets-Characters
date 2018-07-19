using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonBrain : MonoBehaviour
	{
		[SerializeField]
		protected ThirdPersonMotor rootMotionMotor;

		[SerializeField]
		protected ThirdPersonAnimationController animationController;

		private IThirdPersonMotor currentMotor;

		public ThirdPersonAnimationController animationControl
		{
			get { return animationController; }
		}

		public ThirdPersonMotor rootMotionThirdPersonMotor
		{
			get { return rootMotionThirdPersonMotor; }
		}

		private void Awake()
		{
			currentMotor = rootMotionMotor;
			currentMotor.Init(this);
			animationController.Init(gameObject, currentMotor);
		}

		private void OnEnable()
		{
			animationController.Subscribe();
			rootMotionMotor.Subscribe();
		}
		
		private void OnDisable()
		{
			animationController.Unsubscribe();
			rootMotionMotor.Unsubscribe();
		}

		private void Update()
		{
			animationController.Update();
			rootMotionMotor.Update();
		}

		private void OnAnimatorMove()
		{
			rootMotionMotor.OnAnimatorMove();
		}
	}
}