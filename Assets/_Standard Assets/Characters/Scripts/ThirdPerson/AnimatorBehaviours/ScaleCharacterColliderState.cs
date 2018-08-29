using StandardAssets.Characters.Attributes;
using StandardAssets.Characters.Physics; 
using UnityEngine;


namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{

	public class ScaleCharacterColliderState : StateMachineBehaviour
	{
		// If true, use the current state's normalizeTime value (ideally, not looped animations),
		// otherwise, we will use our own time.
		[SerializeField] bool useNormalizedTime = false;
		
		/// <summary>
		/// Adjusts the speed in which the collider changes size.
		/// Relative to one second, a speed of 1 indicates it takes one second to complete state,
		/// a speed of 2 takes half a second.
		/// </summary>
		// Speed is is used when we are not using animation's time.
		[SerializeField, VisibleIf("useNormalizedTime", false)]
		private float speed = 2;
		 
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
	
		/// <summary>
		/// Adjusted character scale.
		/// </summary>
		[SerializeField, Range(0, 1)]
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
		//private CapsuleCollider bodyCollider;
	 
		
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
				// if another state does not reset the collider on exit, we can blend collider from it's existing state:
				currentScale  	= entryScale  = controller.GetHeight() / controller.defaultHeight;
				entryOffset 	= controller.GetCenter().y;
			} 
		}
	
		
		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			if (controller == null)
			{
				return;
			}
	  
			time = useNormalizedTime 
				? stateInfo.normalizedTime
				: Mathf.Clamp01(time + (Time.deltaTime * speed));
	
			currentScale 		= Mathf.Lerp(entryScale, heightScale, curve.Evaluate(time));
			float height 		= currentScale * controller.defaultHeight;
			float offset 		= Mathf.Lerp(entryOffset, Center(height), useNormalizedTime ? curve.Evaluate(time) : time);

			controller.SetHeight(height, false, false, false);
			controller.SetCenter(new Vector3(0, offset, 0), true, false);
		}
	
	
		float Center(float currHeight)
		{
			float charHeight = controller.GetHeight();
			// collider is centered on character:
			return   (((currHeight / charHeight) - (1.0f - heightOrigin)) * (charHeight / 2));  
		}
	
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
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
	 
		public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
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
		 
	}
}


