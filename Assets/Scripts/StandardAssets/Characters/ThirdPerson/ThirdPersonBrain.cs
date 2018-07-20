using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonBrain : MonoBehaviour
	{
		[SerializeField, Tooltip("Properties of the root motion motor")]
		protected RootMotionThirdPersonMotor rootMotionMotor;

		[SerializeField, Tooltip("Properties of the animation controller")]
		protected ThirdPersonAnimationController animationController;
		
		[SerializeField]
		protected TurnaroundType turnaroundType;

		//TODO remove these temp headers
		[Header("Waiting on attribute to hide")]
		[Header("the turnaroundbehaviour based")]
		[Header("above enum")]
		
		[SerializeField]
		protected BlendspaceTurnaroundBehaviour blendspaceTurnaroundBehaviour;

		[SerializeField]
		protected AnimationTurnaroundBehaviour animationTurnaroundBehaviour;

		private IThirdPersonMotor currentMotor;

		private TurnaroundBehaviour currentTurnaroundBehaviour;

		public ThirdPersonAnimationController animationControl
		{
			get { return animationController; }
		}

		public TurnaroundBehaviour turnaroundBehaviour
		{
			get { return currentTurnaroundBehaviour; }
		}

		private void Awake()
		{
			currentTurnaroundBehaviour = GetCurrentTurnaroundBehaviour();
			if (currentTurnaroundBehaviour != null)
			{
				currentTurnaroundBehaviour.Init(this);
			}
			
			currentMotor = GetCurrentMotor();
			currentMotor.Init(this);
			
			if (animationController != null)
			{
				animationController.Init(this, currentMotor);
			}
		}

		private TurnaroundBehaviour GetCurrentTurnaroundBehaviour()
		{
			switch (turnaroundType)
			{
					case TurnaroundType.None:
						return null;
					case TurnaroundType.Blendspace:
						return blendspaceTurnaroundBehaviour;
					case TurnaroundType.Animation:
						return animationTurnaroundBehaviour;
					default:
						return null;
			}
		}
		
		private IThirdPersonMotor GetCurrentMotor()
		{
			return rootMotionMotor;
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

			if (currentTurnaroundBehaviour != null)
			{
				currentTurnaroundBehaviour.Update();
			}
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