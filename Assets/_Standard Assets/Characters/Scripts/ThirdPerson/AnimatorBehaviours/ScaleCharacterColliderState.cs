using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Physics; 
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
	/// <summary>
	/// Allows an animation state to change the OpenCharacterController's capsule height and offset.
	/// </summary>
	public class ScaleCharacterColliderState : StateMachineBehaviour
	{
		private const float k_MinHeightScale = 0.0f;	
		private const float k_MaxHeightScale = 2.0f;
		
		private enum NormalizedMode
		{
			Once,
			Loop,
			PingPong
		}
		
		// If true, use the current state's normalizeTime value (ideally, not looped animations),
		// otherwise, we will use our own time.
		[SerializeField] bool useNormalizedTime = true;
		 
		// We are using curve only for normalized time from the animation,
		// but for looping animations it is useless (looping collider size...).
		[SerializeField, VisibleIf("useNormalizedTime", true)]
		private AnimationCurve curve = new AnimationCurve()
		{ 
			keys = new Keyframe[]
			{
				new Keyframe(0, 0), 
				new Keyframe(1, 1),
			}
		};
		
		[SerializeField, VisibleIf("useNormalizedTime", true)]
		private NormalizedMode animationMode;
		
		/// <summary>
		/// How many seconds it will take to change the height.
		/// </summary>
		[SerializeField, VisibleIf("useNormalizedTime", false)]
		private float duration = 1.0f;
	
		/// <summary>
		/// Adjusted character scale.
		/// </summary>
		[SerializeField, VisibleIf("useNormalizedTime", false)]
		private float heightScale = 1;
		
		/// <summary>
		/// Scale character's collider and offset relative to character's height. 0.5 is center.
		/// </summary>
		[SerializeField, Range(0, 1)]
		private float heightOrigin = 0.5f;
		
		// if false, we will not restore collider on exit
		// (allows another state behavior to use the last state of the collider)
		[SerializeField] bool resetOnExit = true;
		
		/// <summary>
		/// Using own time as normalized time seems to be looping.
		/// </summary>
		private float time;
		private float currentScale, entryScale;
		private float entryOffset;
		private OpenCharacterControllerPhysics physics;
		private OpenCharacterController controller;
		
		// OnStateEnter is called before OnStateEnter is called on any state inside this state machine
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (physics == null)
			{
				physics = animator.GetComponentInChildren<OpenCharacterControllerPhysics>();
				if (physics == null)
				{
					// TODO: show warning in inspector?
					return;
				} 
				controller   = physics.GetOpenCharacterController();
				if (controller == null)
				{
					return;
				}
			}

			if (controller == null)
			{
				return;
			}
			
			// if another state does not reset the collider on exit, we can blend collider from it's existing state:
			currentScale  	= entryScale  = controller.GetHeight() / controller.defaultHeight;
			entryOffset 	= controller.GetCenter().y;
		}
	
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (controller == null)
			{
				return;
			}
			
			if (useNormalizedTime)
			{
				switch (animationMode)
				{
					default:
						time = stateInfo.normalizedTime;
						break;
					case NormalizedMode.Loop:
						time = Mathf.Repeat(stateInfo.normalizedTime, 1);
						break;
					case NormalizedMode.PingPong:
						time = Mathf.PingPong(stateInfo.normalizedTime, 1);
						break; 
				}  
				currentScale 		=  curve.Evaluate(time);
			}
			else
			{
				if (duration <= 0.0f)
				{
					time = 1.0f;
				}
				else
				{
					time = Mathf.Clamp01(time + (Time.deltaTime * (1.0f / duration)));
				}
				currentScale 		=  Mathf.Lerp(entryScale, heightScale, time);
			}
			
			float height 		= currentScale * controller.defaultHeight;
			
			height = controller.SetHeight(height, false, false, false);

			float offset;

			if (useNormalizedTime)
			{
				offset 		= Mathf.Lerp(entryOffset, Center(height), curve.Evaluate(time));
			}
			else
			{
				offset 		= Mathf.Lerp(entryOffset, Center(height), time);
			}
			
			controller.SetCenter(new Vector3(0, offset, 0), true, false);
		}
	
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			HandleStateExit();
		}
	 
		public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
		{ 		
			HandleStateExit();
		}
		
		private void HandleStateExit()
		{
			if (controller == null)
			{
				return;
			}
	
			if (resetOnExit)
			{
				controller.ResetHeightAndCenter(true, false);
			}
	
			time = 0;
		}
		
		private void OnValidate()
		{
			heightScale = Mathf.Clamp(heightScale, k_MinHeightScale, k_MaxHeightScale);
			duration = Mathf.Max(0.0f, duration);
		}
		
		private float Center(float currHeight)
		{
			float charHeight = controller.defaultHeight;
			// collider is centered on character:
			return   (((currHeight / charHeight) - (1.0f - heightOrigin)) * (charHeight / 2));  
		}
	}
}


