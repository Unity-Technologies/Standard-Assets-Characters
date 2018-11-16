using StandardAssets.Characters.ThirdPerson;
using UnityEngine;
using UnityEngine.AI;

namespace StandardAssets.Characters.Examples.SimpleNavMeshInputController
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
		[SerializeField, Tooltip("Make input relative to the camera?")]
		bool m_InputRelativeToCamera = true;

		/// <summary>
		/// Character stops moving when it is this close to the agent.
		/// </summary>
		[SerializeField, Tooltip("Stop the Character moving when it is this close to the agent")]
		float m_NearDistance = 0.2f;
		
		/// <summary>
		/// Agent pauses movement when it is this distance away, to wait for the character
		/// </summary>
		[SerializeField, Tooltip("Agent pauses movement when it is this distance away, to wait for the character")]
		float m_FarDistance = 1.5f;

		/// <summary>
		/// Agent pauses movement when it reaches a corner/waypoint and it is this distance away, to wait for the character
		/// </summary>
		[SerializeField, Tooltip("Agent pauses movement when it reaches a corner/waypoint and it is this distance away, to wait for the character")]
		float m_CornerFarDistance = 0.5f;

		/// <summary>
		/// Reset the agent if it is further than this distance from the character.
		/// </summary>
		[SerializeField, Tooltip("Reset the agent if it is further than this distance from the character")]
		float m_ResetDistance = 3.0f;

		/// <summary>
		/// Reset the agent if its Y position is further than this distance from the character.
		/// </summary>
		[SerializeField, Tooltip("Reset the agent if its Y position is further than this distance from the character")]
		float m_ResetHeight = 3.0f;

		/// <summary>
		/// Assume character is stuck if it cannot reach the agent within this time.
		/// </summary>
		[SerializeField, Tooltip("Assume character is stuck if it cannot reach the agent within this time.")]
		float m_StuckDuration = 3.0f;

		NavMeshAgent m_NavMeshAgent;
		Transform m_CachedTransform;
		bool m_MovingToSimulatedPoint;
		float m_DynamicFarDistance;
		int m_LastCornerCount;
		float m_StuckTime;
		Camera m_CurrentCamera;

		/// <summary>
		/// Gets/sets look input vector
		/// </summary>
		public Vector2 lookInput { get; private set; }
		
		
		/// <summary>
		/// Gets/sets move input vector
		/// </summary>
		public Vector2 moveInput { get; private set; }
		
		/// <summary>
		/// Gets if there is movement input being applied
		/// </summary>
		public bool hasMovementInput
		{
			get { return moveInput != Vector2.zero; }
		}

		/// <summary>
		/// Set the camera to use, for making input relative to the camera. 
		/// </summary>
		public void SetCamera(Camera newCamera)
		{
			m_CurrentCamera = newCamera;
		}

		/// <summary>
		/// Initializes NavMeshAgent, caches transform and main camera
		/// </summary>
		void Awake()
		{
			m_CachedTransform = transform;
			m_CurrentCamera = Camera.main;
			m_NavMeshAgent = GetComponent<NavMeshAgent>();
			m_NavMeshAgent.updatePosition = false;
			m_NavMeshAgent.updateRotation = false;
			m_NavMeshAgent.updateUpAxis = false;
			lookInput = Vector2.zero;
		}

		/// <summary>
		/// Updates the move input.
		/// </summary>
		void Update()
		{		
			moveInput = Vector3.zero;

			if (!m_NavMeshAgent.isOnNavMesh ||
				BusyCalculatingPath())
			{
				m_MovingToSimulatedPoint = false;
				return;
			}

			// Started following a new path?
			if (!m_MovingToSimulatedPoint &&
				m_NavMeshAgent.hasPath)
			{
				m_MovingToSimulatedPoint = true;
				m_DynamicFarDistance = m_FarDistance;
				m_LastCornerCount = m_NavMeshAgent.path.corners.Length;
				m_StuckTime = 0.0f;
			}

			if (m_MovingToSimulatedPoint)
			{
				UpdateMoveToSimulatedPoint(Time.deltaTime);
				return;
			}

			CheckResetDistance();
		}

		/// <summary>
		/// Update the move input to move towards the agent.
		/// </summary>
		void UpdateMoveToSimulatedPoint(float dt)
		{
			if (CheckResetDistance())
			{
				return;
			}
			
			// Reached a corner?
			if (m_NavMeshAgent.hasPath &&
			    m_LastCornerCount != m_NavMeshAgent.path.corners.Length)
			{
				m_LastCornerCount = m_NavMeshAgent.path.corners.Length;
				m_DynamicFarDistance = m_CornerFarDistance;
			}
			
			var simulationReachedEndOfPath = ReachedEndOfPath();
			var direction = m_NavMeshAgent.nextPosition - m_CachedTransform.position;
			// Ignore height
			direction.y = 0.0f;
			var distance = direction.magnitude;
			direction.Normalize();
			var isTooFar = distance > m_DynamicFarDistance;
			var isTooNear = distance < m_NearDistance;

			if (isTooNear)
			{
				if (simulationReachedEndOfPath)
				{
					m_MovingToSimulatedPoint = false;
					ResetAgent();
				}
				return;
			}

			if (IsAgentMovingToCharacter(direction))
			{
				// Wait for the agent to reach the character
				if (m_NavMeshAgent.isStopped)
				{
					// Make sure agent is moving
					m_NavMeshAgent.isStopped = false;
				}
				return;
			}

			var checkStuck = false;
			if (isTooFar)
			{
				// Agent must wait for character to catch up
				if (!m_NavMeshAgent.isStopped)
				{
					m_NavMeshAgent.isStopped = true;
				}
				checkStuck = true;
			}
			else if (m_NavMeshAgent.isStopped)
			{
				m_NavMeshAgent.isStopped = false;
				m_DynamicFarDistance = m_FarDistance;
			}

			if (m_CurrentCamera != null &&
			    m_InputRelativeToCamera)
			{
				// To local, relative to camera
				direction = m_CurrentCamera.transform.InverseTransformDirection(direction);
			}
			moveInput = new Vector2(direction.x, direction.z);
			
			if (checkStuck)
			{
				m_StuckTime += dt;
				if (m_StuckTime > m_StuckDuration)
				{
					m_StuckTime = 0.0f;
					m_MovingToSimulatedPoint = false;
					ResetAgent();
				}
			}
			else
			{
				m_StuckTime = 0.0f;
			}
		}
		
		/// <summary>
		/// Check if the agent must be reset if it is too far from the character. If true then the method resets it.
		/// </summary>
		bool CheckResetDistance()
		{
			var reset = false;
			var direction = m_NavMeshAgent.nextPosition - m_CachedTransform.position;
			if (Mathf.Abs(direction.y) > m_ResetHeight)
			{
				reset = true;
			}
			else
			{
				// Ignore height
				direction.y = 0.0f;
				if (direction.sqrMagnitude > m_ResetDistance * m_ResetDistance)
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
		void ResetAgent()
		{
			m_NavMeshAgent.ResetPath();
			m_NavMeshAgent.Warp(m_CachedTransform.position);
			m_NavMeshAgent.nextPosition = m_CachedTransform.position;
		}
		
		/// <summary>
		/// Is the agent busy calculating a path?
		/// </summary>
		bool BusyCalculatingPath()
		{
			return m_NavMeshAgent.pathPending;
		}

		/// <summary>
		/// Has the agent reached the end of the path?
		/// </summary>
		bool ReachedEndOfPath()
		{		
			if (!BusyCalculatingPath())
			{
				if (m_NavMeshAgent.isOnNavMesh && 
				    m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
				{
					if (!m_NavMeshAgent.hasPath || 
					    Mathf.Approximately(m_NavMeshAgent.velocity.sqrMagnitude, 0.0f))
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
		bool IsAgentMovingToCharacter(Vector3 direction)
		{
			if (m_NavMeshAgent.path.corners.Length < 2)
			{
				return false;
			}

			var moveDirection = m_NavMeshAgent.path.corners[1] - m_NavMeshAgent.path.corners[0];
			// Ignore height
			moveDirection.y = 0.0f;
			moveDirection.Normalize();
			
			return Vector3.Dot(direction, moveDirection) < 0.0f;
		}
		
		#if UNITY_EDITOR
		void OnDrawGizmosSelected()
		{
			if (m_NavMeshAgent == null ||
			    !m_NavMeshAgent.isOnNavMesh ||
			    BusyCalculatingPath() ||
			    m_NavMeshAgent.path == null)
			{
				return;
			}

			// Draw the path
			var path = m_NavMeshAgent.path;
			var prevPoint = Vector3.zero;
			Gizmos.color = Color.green;
			for (int i = 0, len = path.corners.Length; i < len; i++)
			{
				var point = path.corners[i];
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