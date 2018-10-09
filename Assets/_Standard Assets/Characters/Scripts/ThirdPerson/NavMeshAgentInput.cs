using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.ThirdPerson
{
	/// <summary>
	/// Makes a character controller follow a NavMeshAgent. It sets input to move the character.
	/// </summary>
	[RequireComponent(typeof(NavMeshAgent))]
	public class NavMeshAgentInput : MonoBehaviour, IThirdPersonInput
	{
		/// <summary>
		/// Make input relative to the camera.
		/// </summary>
		[SerializeField, Tooltip("Make input relative to the camera.")]
		private bool inputRelativeToCamera = true;

		/// <summary>
		/// Character stops moving when it is this close to the agent.
		/// </summary>
		[SerializeField, Tooltip("Character stops moving when it is this close to the agent.")]
		private float nearDistance = 0.2f;
		
		/// <summary>
		/// Agent pauses movement when it is this distance away, to wait for the character
		/// </summary>
		[SerializeField, Tooltip("Agent pauses movement when it is this distance away, to wait for the character")]
		private float farDistance = 1.5f;

		/// <summary>
		/// Agent pauses movement when it reaches a corner/waypoint and it is this distance away, to wait for the character
		/// </summary>
		[SerializeField, Tooltip("Agent pauses movement when it reaches a corner/waypoint and it is this distance away, to wait for the character")]
		private float cornerFarDistance = 0.5f;

		/// <summary>
		/// Reset the agent if it is further than this distance from the character.
		/// </summary>
		[SerializeField, Tooltip("Reset the agent if it is further than this distance from the character.")]
		private float resetDistance = 3.0f;

		/// <summary>
		/// Reset the agent if its Y position is further than this distance from the character.
		/// </summary>
		[SerializeField, Tooltip("Reset the agent if its Y position is further than this distance from the character.")]
		private float resetHeight = 3.0f;

		/// <summary>
		/// Assume character is stuck if it cannot reach the agent within this time.
		/// </summary>
		[SerializeField, Tooltip("Assume character is stuck if it cannot reach the agent within this time.")]
		private float stuckDuration = 3.0f;
		
		private NavMeshAgent navMeshAgent;
		private Transform cachedTransform;
		private bool movingToSimulatedPoint;
		private float dynamicFarDistance;
		private int lastCornerCount;
		private float stuckTime;
		private Camera currentCamera;

		public Vector2 lookInput { get; private set; }
		public Vector2 moveInput { get; private set; }
		public bool hasMovementInput
		{
			get { return moveInput != Vector2.zero; }
		}
		public bool hasJumpInput { get; private set; }

		/// <summary>
		/// Set the camera to use, for making input relative to the camera. 
		/// </summary>
		public void SetCamera(Camera newCamera)
		{
			currentCamera = newCamera;
		}
		
		private void Awake()
		{
			cachedTransform = transform;
			
			currentCamera = Camera.main;
			
			navMeshAgent = GetComponent<NavMeshAgent>();
			navMeshAgent.updatePosition = false;
			navMeshAgent.updateRotation = false;
			navMeshAgent.updateUpAxis = false;
		}

		private void Update()
		{		
			UpdateInput(Time.deltaTime);
		}

		/// <summary>
		/// Update the move input.
		/// </summary>
		private void UpdateInput(float dt)
		{
			moveInput = Vector3.zero;

			if (!navMeshAgent.isOnNavMesh ||
			    BusyCalculatingPath())
			{
				movingToSimulatedPoint = false;
				return;
			}

			// Started following a new path?
			if (!movingToSimulatedPoint &&
			    navMeshAgent.hasPath)
			{
				movingToSimulatedPoint = true;
				dynamicFarDistance = farDistance;
				lastCornerCount = navMeshAgent.path.corners.Length;
				stuckTime = 0.0f;
			}

			if (movingToSimulatedPoint)
			{
				UpdateMoveToSimulatedPoint(dt);
				return;
			}

			CheckResetDistance();
		}

		/// <summary>
		/// Update the move input to move towards the agent.
		/// </summary>
		private void UpdateMoveToSimulatedPoint(float dt)
		{
			if (CheckResetDistance())
			{
				return;
			}
			
			// Reached a corner?
			if (navMeshAgent.hasPath &&
			    lastCornerCount != navMeshAgent.path.corners.Length)
			{
				lastCornerCount = navMeshAgent.path.corners.Length;
				dynamicFarDistance = cornerFarDistance;
			}
			
			bool simulationReachedEndOfPath = ReachedEndOfPath();
			Vector3 direction = navMeshAgent.nextPosition - cachedTransform.position;
			// Ignore height
			direction.y = 0.0f;
			float distance = direction.magnitude;
			direction.Normalize();
			bool isTooFar = distance > dynamicFarDistance;
			bool isTooNear = distance < nearDistance;

			if (isTooNear)
			{
				if (simulationReachedEndOfPath)
				{
					movingToSimulatedPoint = false;
					ResetAgent();
				}
				return;
			}

			if (IsAgentMovingToCharacter(direction))
			{
				// Wait for the agent to reach the character
				if (navMeshAgent.isStopped)
				{
					// Make sure agent is moving
					navMeshAgent.isStopped = false;
				}
				return;
			}

			bool checkStuck = false;
			if (isTooFar)
			{
				// Agent must wait for character to catch up
				if (!navMeshAgent.isStopped)
				{
					navMeshAgent.isStopped = true;
				}
				checkStuck = true;
			}
			else if (navMeshAgent.isStopped)
			{
				navMeshAgent.isStopped = false;
				dynamicFarDistance = farDistance;
			}

			if (currentCamera != null &&
			    inputRelativeToCamera)
			{
				// To local, relative to camera
				direction = currentCamera.transform.InverseTransformDirection(direction);
			}
			moveInput = new Vector2(direction.x, direction.z);
			
			if (checkStuck)
			{
				stuckTime += dt;
				if (stuckTime > stuckDuration)
				{
					stuckTime = 0.0f;
					movingToSimulatedPoint = false;
					ResetAgent();
				}
			}
			else
			{
				stuckTime = 0.0f;
			}
		}
		
		/// <summary>
		/// Check if the agent must be reset if it is too far from the character. If true then the method resets it.
		/// </summary>
		private bool CheckResetDistance()
		{
			bool reset = false;
			Vector3 direction = navMeshAgent.nextPosition - cachedTransform.position;
			if (Mathf.Abs(direction.y) > resetHeight)
			{
				reset = true;
			}
			else
			{
				// Ignore height
				direction.y = 0.0f;
				if (direction.sqrMagnitude > resetDistance * resetDistance)
				{
					reset = true;
				}
			}

			if (reset)
			{
				ResetAgent();
			}

			return reset;
		}

		/// <summary>
		/// Reset the agent's path and position
		/// </summary>
		private void ResetAgent()
		{
			navMeshAgent.ResetPath();
			navMeshAgent.Warp(cachedTransform.position);
			navMeshAgent.nextPosition = cachedTransform.position;
		}
		
		/// <summary>
		/// Is the agent busy calculating a path?
		/// </summary>
		private bool BusyCalculatingPath()
		{
			return navMeshAgent.pathPending;
		}

		/// <summary>
		/// Has the agent reached the end of the path?
		/// </summary>
		private bool ReachedEndOfPath()
		{		
			if (!BusyCalculatingPath())
			{
				if (navMeshAgent.isOnNavMesh && 
				    navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
				{
					if (!navMeshAgent.hasPath || 
					    Mathf.Approximately(navMeshAgent.velocity.sqrMagnitude, 0.0f))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Is the agent's simulated position moving towards the character?
		/// </summary>
		private bool IsAgentMovingToCharacter(Vector3 direction)
		{
			if (navMeshAgent.path.corners.Length < 2)
			{
				return false;
			}

			Vector3 moveDirection = navMeshAgent.path.corners[1] - navMeshAgent.path.corners[0];
			// Ignore height
			moveDirection.y = 0.0f;
			moveDirection.Normalize();
			
			return Vector3.Dot(direction, moveDirection) < 0.0f;
		}
		
		#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			if (navMeshAgent == null ||
			    !navMeshAgent.isOnNavMesh ||
			    BusyCalculatingPath() ||
			    navMeshAgent.path == null)
			{
				return;
			}

			// Draw the path
			NavMeshPath path = navMeshAgent.path;
			Vector3 prevPoint = Vector3.zero;
			Gizmos.color = Color.green;
			for (int i = 0, len = path.corners.Length; i < len; i++)
			{
				Vector3 point = path.corners[i];
				if (i > 0)
				{
					Gizmos.DrawLine(prevPoint, point);
				}
				prevPoint = point;
			}
		}
		#endif
	}
}