using System;
using Attributes;
using Attributes.Types;
using StandardAssets.Characters.Common;
using StandardAssets.Characters.Effects;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class ThirdPersonBrain : CharacterBrain
	{
		[HelperBox(HelperType.Info,
			"Configurations are separate assets (ScriptableObjects). Click on the associated configuration to locate it in the Project View. Values can be edited here during runtime and not be lost. It also allows one to create different settings and swap between them. To create a new setting Right click -> Create -> Standard Assets -> Characters -> ...")]
		[SerializeField]
		protected ThirdPersonCameraAnimationManager cameraAnimationManager;
		
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

		private TurnaroundBehaviour[] turnaroundBehaviours;

		public RootMotionThirdPersonMotor rootMotionThirdPersonMotor
		{
			get { return rootMotionMotor; }
		}

		public ThirdPersonAnimationController animationControl
		{
			get { return animationController; }
		}

		public TurnaroundBehaviour turnaround
		{
			get { return currentTurnaroundBehaviour; }
		}

		public TurnaroundBehaviour[] turnaroundOptions
		{
			get
			{
				if (turnaroundBehaviours == null)
				{
					turnaroundBehaviours = new TurnaroundBehaviour[]
					{
						blendspaceTurnaroundBehaviour, 
						animationTurnaroundBehaviour
					};
				}
				return turnaroundBehaviours;
			}
		}

		public override MovementEventHandler movementEventHandler
		{
			get { return thirdPersonMovementEventHandler; }
		}

		public IThirdPersonMotor CurrentMotor { get; private set; }

		public ThirdPersonCameraAnimationManager thirdPersonCameraAnimationManager
		{
			get
			{
				return cameraAnimationManager;
			}
		}
		
		protected override void Awake()
		{
			base.Awake();
			characterBearing = new ThirdPersonCharacterBearing();
			blendspaceTurnaroundBehaviour.Init(this);
			animationTurnaroundBehaviour.Init(this);
			currentTurnaroundBehaviour = GetCurrentTurnaroundBehaviour();
			
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
		
			//Just for build testing
			if (Input.GetKeyDown(KeyCode.T))
			{
				turnaroundType = turnaroundType == TurnaroundType.Animation ? TurnaroundType.None : turnaroundType + 1;
				currentTurnaroundBehaviour = GetCurrentTurnaroundBehaviour();
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

		private void OnGUI()
		{
			GUI.Label(new Rect(Screen.width * 0.8f, 0, Screen.width * 0.2f, Screen.height * 0.1f), 
				string.Format("Turn around: {0}\nPress T to cycle", turnaroundType));
			
			
			GUI.Label(new Rect(Screen.width * 0.8f, Screen.height * 0.1f, Screen.width * 0.2f, Screen.height * 0.1f), 
				string.Format("Sprint: {0}", rootMotionMotor.sprint));
		}
		
		#if UNITY_EDITOR
		private void OnValidate()
		{
			currentTurnaroundBehaviour = GetCurrentTurnaroundBehaviour();
			if (turnaroundType == TurnaroundType.Animation)
			{
				animationTurnaroundBehaviour.OnValidate(GetComponent<Animator>());
			}
		}
		#endif
	}
}