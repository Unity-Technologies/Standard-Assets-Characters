using Attributes;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonBrain : CharacterBrain
	{
		[SerializeField, Tooltip("Properties of the root motion motor")]
		protected RootMotionThirdPersonMotor rootMotionMotor;

		[SerializeField, Tooltip("Properties of the animation controller")]
		protected ThirdPersonAnimationController animationController;
		
		[SerializeField]
		protected TurnaroundType turnaroundType;

		[SerializeField]
		[ConditionalInclude("turnaroundType", TurnaroundType.Blendspace)]
		protected BlendspaceTurnaroundBehaviour blendspaceTurnaroundBehaviour;

		[SerializeField]
		[ConditionalInclude("turnaroundType", TurnaroundType.Animation)]
		protected AnimationTurnaroundBehaviour animationTurnaroundBehaviour;
		
		[SerializeField]
		protected ThirdPersonMovementEventHandler thirdPersonMovementEventHandler;

		private TurnaroundBehaviour currentTurnaroundBehaviour;

		public ThirdPersonAnimationController animationControl
		{
			get { return animationController; }
		}

		public TurnaroundBehaviour turnaroundBehaviour
		{
			get { return currentTurnaroundBehaviour; }
		}

		public override MovementEventHandler movementEventHandler
		{
			get { return thirdPersonMovementEventHandler; }
		}

		public IThirdPersonMotor CurrentMotor { get; private set; }
		
		protected override void Awake()
		{
			base.Awake();
			
			currentTurnaroundBehaviour = GetCurrentTurnaroundBehaviour();
			if (currentTurnaroundBehaviour != null)
			{
				currentTurnaroundBehaviour.Init(this);
			}
			
			CurrentMotor = GetCurrentMotor();
			CurrentMotor.Init(this);
			
			if (animationController != null)
			{
				animationController.Init(this, CurrentMotor);
			}
			
			thirdPersonMovementEventHandler.Init();
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
			
			CurrentMotor.Subscribe();
			thirdPersonMovementEventHandler.Subscribe();
		}
		
		private void OnDisable()
		{
			if (animationController != null)
			{
				animationController.Unsubscribe();
			}
			
			CurrentMotor.Unsubscribe();
			thirdPersonMovementEventHandler.Unsubscribe();
		}

		private void Update()
		{
			if (animationController != null)
			{
				animationController.Update();
			}
			
			CurrentMotor.Update();

			if (currentTurnaroundBehaviour != null)
			{
				currentTurnaroundBehaviour.Update();
			}
		}

		private void OnAnimatorMove()
		{
			CurrentMotor.OnAnimatorMove();
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