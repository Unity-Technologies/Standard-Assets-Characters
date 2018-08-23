using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using StandardAssets.Characters.Physics;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Animations;
using Attributes;
using Attributes.Types;

/*
 * To Implement this, I added GetBodyCollider() to OpenCharacterController.
 * It seems controller doesnt adjust it's collider once initialized, making animation of collider safe and easy.
 * The only risk is two animator states fighting over each other to manipulate the character's collider.
 *
 * TODOs: (OpenCharacterController needs...), can update here later.
 *  -- p
 *  TODO: OpenCharacterController.GetCapsuleColliderSize() or GetColliderHeight()
 *  TODO: OpenCharacterController.SetCapsuleColliderSize() or SetColliderHight()
 *  TODO: OpenCharacterController.GetCapsuleColliderOffset() 
 *  TODO: OpenCharacterController.SetCapsuleColliderOffset()
 *  TODO: OpenCharacterController.ResetCapsuleCollider() (reset collider back to character size)
 *
 *  TODO: We can create a simple ResetCharacterCollider behaviour which just resets character's collider on entry.
 *  
 */

public class ScaleCharacterColliderState : StateMachineBehaviour
{
	// If true, use the current state's normalizeTime value (ideally, not looped animations),
	// otherwise, we will use our own time.
	[SerializeField] bool normalizeTime = false;
	
	/// <summary>
	/// Adjusts the speed in which the collider changes size.
	/// Relative to one second, a speed of 1 indicates it takes one second to complete state,
	/// a speed of 2 takes half a second.
	/// </summary>
	// Speed is is used when we are not using animation's time.
	[SerializeField, ConditionalInclude("normalizeTime", false)]
	private float speed = 2;
	 
	// We are using curve only for normalized time from the animation,
	// but for looping animations it is useless (looping collider size...).
	[HelperBox(HelperType.Info, "TEST TEST")]
	[SerializeField, ConditionalInclude("normalizeTime", true)]
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
	private CapsuleCollider bodyCollider;
 
	
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
			bodyCollider = physics != null ? controller.GetCapsuleCollider() : null;

			if (bodyCollider == null)
			{
				// Codie: Is this possible, or character not fully initialized?
				// I am just guessing here, I don't know how this whole package works.
				// - assuming someone didn't add a collider or has not done it yet.
				// TODO: if this can happen, a warning in inspector?
				return;
			}
 
			// if another state does not reset the collider on exit, we can blend collider from it's existing state:
			currentScale  	= entryScale  = bodyCollider.height / controller.GetHeight();
			entryOffset 	= bodyCollider.center.y;
		} 
	}

	
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (bodyCollider == null)
		{
			return;
		}
  
		time = normalizeTime 
			? stateInfo.normalizedTime
			: Mathf.Clamp01(time + (Time.deltaTime * speed));

		currentScale 		= Mathf.Lerp(entryScale, heightScale, curve.Evaluate(time));
		float height 		= currentScale * controller.GetHeight();
		float offset 		= Mathf.Lerp(entryOffset, Center(height), normalizeTime ? curve.Evaluate(time) : time);
		bodyCollider.center = new Vector3(0, offset, 0);
		bodyCollider.height = height; 
	}


	float Center(float currHeight)
	{
		float charHeight = controller.GetHeight();
		// collider is centered on character:
		return   (((currHeight / charHeight) - (1.0f - heightOrigin)) * (charHeight / 2));  
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (bodyCollider == null)
		{
			return;
		} 
		// Attempting to restore by animation doesnt work.
		//bodyCollider.height = Mathf  .Lerp(currentHeight, preserveHeight, stateInfo.normalizedTime);
		//bodyCollider.center = Vector3.Lerp(currentCenter, preserveCenter, stateInfo.normalizedTime); 
		if (resetOnExit)
		{
			bodyCollider.height = controller.GetHeight();
			bodyCollider.center = bodyCollider.center = default(Vector3); 
		}
		time = 0;
	}
 
	public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
	{ 		
		if (bodyCollider == null)
		{
			return;
		}

		if (resetOnExit)
		{
			bodyCollider.height = controller.GetHeight();
			bodyCollider.center = bodyCollider.center = default(Vector3); 
		}

		time = 0;
	}
	 
}
