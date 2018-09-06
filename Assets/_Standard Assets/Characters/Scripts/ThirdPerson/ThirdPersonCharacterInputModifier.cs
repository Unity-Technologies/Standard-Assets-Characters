using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Physics;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Smooths the input movement when rotating in fast circles. This makes the character run in a circle, instead of turning around on the spot.
	/// </summary>
	public class ThirdPersonCharacterInputModifier : MonoBehaviour, ILegacyCharacterInputModifier
	{
		/// <summary>
		/// Max angle ahead of the tail to rotate to.
		/// </summary>
		private const float k_MaxAngleToTailTarget = 30.0f;

		/// <summary>
		/// Recalculate the tail target when the angle to the current target is less than this. Do not make it too
		/// small, so that the target is calculated frequently enough.
		/// </summary>
		private const float k_AngleToRecalculateTailTarget = 40.0f;
		
		/// <summary>
		/// If angle from tail to head is less than this and the direction from tail to head changed, then assume the
		/// head went past the tail
		/// </summary>
		private const float k_WentPastAngle = 91.0f;
		
		/// <summary>
		/// If angle between current and previous input is greater than this, then do a fast snap turn (i.e. clear the
		/// input buffer). Used in conjunction with k_FastSnapTurnRate.
		/// </summary>
		private const float k_FastSnapTurnAngle = 30.0f;

		/// <summary>
		/// If average turn rate is greater than this, then do a fast snap turn (i.e. clear the input buffer). Used in
		/// conjunction with k_FastSnapTurnAngle.
		/// </summary>
		private const float k_FastSnapTurnRate = 500.0f;

		/// <summary>
		/// Reset the average turn rate after this number of updates.
		/// </summary>
		private const int k_MaxTurnRateCount = 100;

		/// <summary>
		/// The third person character brain.
		/// </summary>
		[SerializeField, Tooltip("The third person character brain.")]
		private ThirdPersonBrain characterBrain;
		
		/// <summary>
		/// Speed at which the tail of the buffer catches up to the head. It's recommended to make this slightly bigger
		/// than the character's turn speed.
		/// </summary>
		[SerializeField, Tooltip("Speed at which the tail of the buffer catches up to the head. It's recommended to make " +
		                         "this slightly bigger than the character's turn speed.")]
		private float turnSpeed = 305.0f;

		/// <summary>
		/// Stop turning when there is no input. (Otherwise try to face the last input vector.)
		/// </summary>
		[SerializeField, Tooltip("Stop turning when there is no input. (Otherwise try to face the last input vector.)")]
		private bool stopTurningWhenNoInput;

		/// <summary>
		/// Enable debug in the editor?
		/// </summary>
		[Header("Debug")]
		[SerializeField, Tooltip("Enable debug in the editor?")]
		private bool debugEnabled;

		[SerializeField, Tooltip("Draw vectors this Y offset from the character.")]
		private float debugOffsetY;
		
		/// <summary>
		/// Input at the head of the buffer.
		/// </summary>
		private Vector2? headInput;

		/// <summary>
		/// Previous head of the buffer.
		/// </summary>
		private Vector2? previousHeadInput;
		
		/// <summary>
		/// Input at the tail of the buffer.
		/// </summary>
		private Vector2? tailInput;
		
		/// <summary>
		/// Tail target input. The tail rotates to this value.
		/// </summary>
		private Vector2? tailTargetInput;

		/// <summary>
		/// The direction the buffer's tail rotated.
		/// </summary>
		private float tailRotateDirection;

		/// <summary>
		/// Previous angle direction between the buffer's head and tail.
		/// </summary>
		private float previousDirection;

		/// <summary>
		/// The average turning rate.
		/// </summary>
		private float turnRate;

		/// <summary>
		/// Time for calculating the turning rate.
		/// </summary>
		private float turnRateTime;

		/// <summary>
		/// Angle for calculating the turning rate.
		/// </summary>
		private float turnRateAngle;

		/// <summary>
		/// Counter for calculating the turning rate.
		/// </summary>
		private int turnRateCount;
		
		/// <summary>
		/// Character's transform
		/// </summary>
		private Transform characterTransform;

		/// <summary>
		/// The root motion motor.
		/// </summary>
		private RootMotionThirdPersonMotor rootMotionMotor;
		
		/// <summary>
		/// The character physics.
		/// </summary>
		private ICharacterPhysics characterPhysics;
		
		#if UNITY_EDITOR
		private Camera debugCamera;
		private Vector2 debugHeadInput;
		private Vector2 debugTailInput;
		private Vector2 debugTailTargetInput;
		private float debugHeadInputTime;
		private float debugTailInputTime;
		private float debugTailTargetInputTime;
		
		private bool debugShowHead = true;
		private Color debugHeadColor = Color.green;
		private bool debugShowTail = true;
		private Color debugTailColor = Color.red;
		private bool debugShowTailTarget = false;
		private Color debugTailTargetColor = Color.yellow;
		private bool debugShowCharacterForward = true;
		private Color debugCharacterForwardColor = Color.blue;
		#endif
		
		/// <inheritdoc />
		public void ModifyMoveInput(ref Vector2 moveInput)
		{
			if (!enabled)
			{
				return;
			}
			
			// Only modify when character is grounded, and not straffing
			if (!characterPhysics.isGrounded ||
			    rootMotionMotor.movementMode == ThirdPersonMotorMovementMode.Strafe)
			{
				return;
			}
			
			float dt = Time.deltaTime;
			bool updateInput = true;
			
			if (moveInput.sqrMagnitude > 0.0f)
			{
				float angle = 0.0f;
				if (headInput != null)
				{
					angle = Vector2.Angle(headInput.Value, moveInput);

					turnRateTime += dt;
					turnRateAngle += angle;
					turnRateCount++;
					turnRate = turnRateAngle / turnRateTime;
					
					if (turnRateCount > k_MaxTurnRateCount)
					{
						turnRateTime = 0.0f;
						turnRateAngle = 0.0f;
						turnRateCount = 0;
					}
				}
				else
				{
					// Start a new input buffer
					turnRateTime = 0.0f;
					turnRateAngle = 0.0f;
					turnRateCount = 0;
					turnRate = 0.0f;
				}
				
				// Rotated too far too fast?
				if (angle > k_FastSnapTurnAngle &&
				    turnRate > k_FastSnapTurnRate)
				{
					updateInput = false;
					ClearInputBuffer();
				}
				else
				{
					headInput = moveInput;
					if (tailInput == null)
					{
						// Start a new input buffer
						updateInput = false;
						tailTargetInput = moveInput;
						tailInput = moveInput;
					}
				}
				
				previousHeadInput = headInput;
			}
			else if (stopTurningWhenNoInput)
			{
				updateInput = false;
				ClearInputBuffer();
			}
			else
			{
				// Continue rotating until we reach the last head
				headInput = previousHeadInput;
			}
			
			if (updateInput && 
			    tailTargetInput != null &&  
			    tailInput != null)
			{
				// Rotate tail towards head (i.e. rotate to the "tail target" which may be the head or located between the tail and head)
				float turnSpeedRadians = Mathf.Deg2Rad * turnSpeed;
				Vector3 rotated = Vector3.RotateTowards(new Vector3(tailInput.Value.x, 0.0f, tailInput.Value.y),
				                                        new Vector3(tailTargetInput.Value.x, 0.0f, tailTargetInput.Value.y),
				                                        turnSpeedRadians * dt,
				                                        1.0f);
				tailInput = new Vector2(rotated.x, rotated.z);
				
				moveInput = new Vector2(tailInput.Value.x, tailInput.Value.y);
				
				// Tail near target, or reached targed?
				if (Vector2.Angle(tailInput.Value, tailTargetInput.Value) < k_AngleToRecalculateTailTarget ||
				    (Mathf.Approximately(tailTargetInput.Value.x, tailInput.Value.x) &&
				     Mathf.Approximately(tailTargetInput.Value.y, tailInput.Value.y)))
				{
					tailTargetInput = CalculateTailTarget();
					if (tailTargetInput == null)
					{
						ClearInputBuffer();
					}
				}
			}
			
			#if UNITY_EDITOR
			if (debugEnabled)
			{
				DebugUpdate();
			}
			#endif
		}

		private void Awake()
		{
			characterTransform = characterBrain.transform;
			rootMotionMotor = characterBrain.rootMotionThirdPersonMotor;
			characterPhysics = characterTransform.GetComponent<ICharacterPhysics>();
		}

		/// <summary>
		/// Clear the input buffer.
		/// </summary>
		private void ClearInputBuffer()
		{
			headInput = null;
			previousHeadInput = null;
			tailInput = null;
			tailTargetInput = null;
			tailRotateDirection = 0.0f;
			previousDirection = 0.0f;
		}

		/// <summary>
		/// Calculate where the tail should rotate towards.
		/// </summary>
		private Vector2? CalculateTailTarget()
		{
			if (headInput == null ||
			    tailInput == null)
			{
				return null;
			}
			
			if (Mathf.Approximately(headInput.Value.x, tailInput.Value.x) &&
			    Mathf.Approximately(headInput.Value.y, tailInput.Value.y))
			{
				// Reached the head (i.e. the end of the input buffer)
				return null;
			}

			// Head and tail vectors on the ground plane
			Vector3 head = new Vector3(headInput.Value.x, 0.0f, headInput.Value.y);
			Vector3 tail = new Vector3(tailInput.Value.x, 0.0f, tailInput.Value.y);
			float angle = Vector3.SignedAngle(tail, head, Vector3.up);
			float direction = angle > 0.0f
				? 1.0f
				: (angle < 0.0f ? -1.0f : 0.0f);
			Vector3 target = head;
			bool headWentPastTail = (Mathf.Abs(angle) <= k_WentPastAngle && 
			                         !Mathf.Approximately(previousDirection, 0.0f) && 
			                         !Mathf.Approximately(previousDirection, direction));
			
			previousDirection = direction;
			
			if (Mathf.Approximately(tailRotateDirection, 0.0f) || 
			    headWentPastTail)
			{
				// Tail starts rotating towards the head
				tailRotateDirection = direction;
			}
			else if (!Mathf.Approximately(tailRotateDirection, 0.0f) &&
					 !Mathf.Approximately(direction, 0.0f))
			{
				// Direction to head changed?
				if ((direction > 0.0f && tailRotateDirection < 0.0f) ||
				    (direction < 0.0f && tailRotateDirection > 0.0f))
				{
					direction = -direction;
				}
			}
			
			if (Mathf.Abs(angle) > k_MaxAngleToTailTarget)
			{
				// Limit the angle to prevent weirdness when angle is near 180 degrees
				if (direction > 0.0f)
				{
					target = Quaternion.Euler(0.0f, k_MaxAngleToTailTarget, 0.0f) * tail;
				}
				else
				{
					target = Quaternion.Euler(0.0f, -k_MaxAngleToTailTarget, 0.0f) * tail;
				}
			}

			return new Vector2(target.x, target.z);
		}
		
		#if UNITY_EDITOR
		/// <summary>
		/// DEBUG: Draw the input vectors.
		/// </summary>
		private void DebugUpdate()
		{
			if (debugCamera == null)
			{
				debugCamera = Camera.main;
				if (debugCamera == null)
				{
					debugCamera = Object.FindObjectOfType<Camera>();
				}
			}

			if (characterTransform == null)
			{
				return;
			}

			float offsetY = debugOffsetY;
			float duration = 0.0f;
			float durationAfterEnd = 5.0f;	// Show vectors for this long after input ends

			// Head
			if (debugShowHead)
			{
				if (headInput != null)
				{
					debugHeadInputTime = Time.time + durationAfterEnd;
					debugHeadInput = headInput.Value;
				}
				if (debugHeadInputTime > Time.time)
				{
					DebugDrawInput(debugHeadInput, offsetY, debugHeadColor, duration, null, 1.1f);
				}
			}

			// Tail target
			if (debugShowTailTarget)
			{
				if (tailTargetInput != null)
				{
					debugTailTargetInputTime = Time.time + durationAfterEnd;
					debugTailTargetInput = tailTargetInput.Value;
				}
				if (debugTailTargetInputTime > Time.time)
				{
					DebugDrawInput(debugTailTargetInput, offsetY, debugTailTargetColor, duration);
				}
			}

			// Tail
			if (debugShowTail)
			{
				if (tailInput != null)
				{
					debugTailInputTime = Time.time + durationAfterEnd;
					debugTailInput = tailInput.Value;
				}
				if (debugTailInputTime > Time.time)
				{
					DebugDrawInput(debugTailInput, offsetY, debugTailColor, duration, tailRotateDirection);
				}
			}
			
			// Character's forward vector
			if (debugShowCharacterForward)
			{
				DebugDrawInput(new Vector2(characterTransform.forward.x, characterTransform.forward.z), 
				               offsetY, debugCharacterForwardColor, duration);
			}
		}

		/// <summary>
		/// DEBUG: Draw the input vector.
		/// </summary>
		private void DebugDrawInput(Vector2 input, float offsetY, Color color, float duration,
		                            float? rotateDirection = null,
		                            float scale = 1.0f)
		{
			Vector3 vector = new Vector3(input.x, 0.0f, input.y);
			Vector3 point = characterTransform.position + new Vector3(0.0f, offsetY, 0.0f);
			if (debugCamera != null)
			{
				vector = debugCamera.transform.TransformVector(vector);
				
				// Project onto the ground
				vector = Vector3.ProjectOnPlane(vector, Vector3.up);
			}
			
			vector = vector.normalized * scale;
			
			Debug.DrawRay(point, 
			              vector, 
			              color, 
			              duration);

			if (rotateDirection != null &&
			    !Mathf.Approximately(rotateDirection.Value, 0.0f))
			{
				float sideLength = 0.2f;
				Vector3 side;
				if (rotateDirection > 0.0f)
				{
					side = Quaternion.AngleAxis(90.0f, Vector3.up) * vector;
				}
				else
				{
					side = Quaternion.AngleAxis(-90.0f, Vector3.up) * vector;
				}
				side.Normalize();
				Debug.DrawRay(point + (vector * 0.5f), 
				              side * sideLength, 
				              color, 
				              duration);
			}
		}
		#endif
	}
}