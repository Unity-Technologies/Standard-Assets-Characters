using UnityEngine;

namespace StandardAssets.Characters.Examples.SimpleMovingPlatforms
{
	/// <summary>
	/// Basic moving platform that moves along waypoints.
	/// </summary>
	public class MovingPlatformController : MonoBehaviour
	{
		/// <summary>
		/// Movement path waypoints. Must have at least 2.
		/// </summary>
		[Header("Movement")]
		[SerializeField, Tooltip("Movement path waypoints. Must have at least 2.")]
		Transform[] m_Waypoints;

		/// <summary>
		/// Movement speed.
		/// </summary>
		[SerializeField, Tooltip("Movement speed.")]
		float m_Speed = 1.0f;

		/// <summary>
		/// Loop through the waypoints?
		/// </summary>
		[SerializeField, Tooltip("Loop through the waypoints?")]
		bool m_Loop;
		
		/// <summary>
		/// Pause for these seconds at the start and end waypoints.
		/// </summary>
		[SerializeField, Tooltip("Pause for these seconds at the start and end waypoints.")]
		float m_PauseAtEndDuration;

		/// <summary>
		/// Rotate to face towards the next waypoint?
		/// </summary>
		[Header("Rotation")]
		[SerializeField, Tooltip("Rotate to face towards the next waypoint?")]
		bool m_RotateToWaypoint;

		/// <summary>
		/// Rotate around the Y axis constantly?
		/// </summary>
		[SerializeField, Tooltip("Rotate around the Y axis constantly?")]
		bool m_RotateAroundY;

		/// <summary>
		/// Rotation speed (degrees per second).
		/// </summary>
		[SerializeField, Tooltip("Rotation speed (degrees per second).")]
		float m_RotateSpeed = 100.0f;
		
		/// <summary>
		/// Tilt on the local Z axis?
		/// </summary>
		[Header("Tilt")]
		[SerializeField, Tooltip("Tilt on the local Z axis?")]
		bool m_Tilt;
		
		/// <summary>
		/// Max tilt angle (degrees).
		/// </summary>
		[SerializeField, Tooltip("Max tilt angle (degrees).")]
		float m_MaxTiltAngle = 45.0f;

		/// <summary>
		/// Tilt speed (degrees per second).
		/// </summary>
		[SerializeField, Tooltip("Tilt speed (degrees per second).")]
		float m_TiltSpeed = 100.0f;

		/// <summary>
		/// Pause for these seconds at the end of each tilt.
		/// </summary>
		[SerializeField, Tooltip("Pause for these seconds at the end of each tilt.")]
		float m_PauseTileDuration;

		// Index of the current waypoint.
		int m_CurrentWaypoint;

		// Reversing along the path?
		bool m_Reversing;
		
		// Target tilt angle.
		float m_TiltAngle;

		// Current movement pause time.
		float m_PauseMoveTime;

		// Current tilt pause time.
		float m_PauseTiltTime;

		// Set the tilt angle
		void Awake()
		{
			m_TiltAngle = m_MaxTiltAngle;

#if UNITY_EDITOR
			// Check if there are at least 2 waypoints (exception: a tilting platform does not need waypoints).
			if (m_Waypoints.Length < 2 && !m_Tilt)
			{
				Debug.LogErrorFormat("Moving platform ({0}) must have at least 2 waypoints.", name);
			}
#endif
		}

		// Update the movement and rotation
		void FixedUpdate()
		{
			UpdatePlatform(Time.fixedDeltaTime);
		}

		// Update the movement and rotation
		void UpdatePlatform(float deltaTime)
		{
			if (m_PauseTiltTime > 0.0f)
			{
				m_PauseTiltTime -= deltaTime;
			}
			else if (m_Tilt)
			{
				var rotation = Quaternion.AngleAxis(m_TiltAngle, transform.forward);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, m_TiltSpeed * deltaTime);
				if (transform.rotation == rotation)
				{
					m_TiltAngle = -m_TiltAngle;
					m_PauseTiltTime = m_PauseTileDuration;
				}
			}
			
			if (m_Waypoints == null || m_Waypoints.Length <= 0)
			{
				return;
			}
			
			if (m_PauseMoveTime > 0.0f)
			{
				m_PauseMoveTime -= deltaTime;
				return;
			}
			
			var waypoint = m_Waypoints[m_CurrentWaypoint];
			var targetPosition = waypoint != null ? waypoint.position : transform.position;
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, m_Speed * deltaTime);
			if (m_RotateAroundY)
			{
				transform.RotateAround(transform.position, Vector3.up, m_RotateSpeed * deltaTime);
			}
			if (transform.position != targetPosition)
			{
				if (m_RotateToWaypoint && !m_RotateAroundY)
				{
					var rotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);
					transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, 
						m_RotateSpeed * deltaTime);
				}
				return;
			}
			if (m_Reversing)
			{
				m_CurrentWaypoint--;
				if (m_CurrentWaypoint < 0)
				{
					m_CurrentWaypoint = 1;
					m_Reversing = false;
					m_PauseMoveTime = m_PauseAtEndDuration;
				}
				return;
			}
			m_CurrentWaypoint++;
			if (m_CurrentWaypoint >= m_Waypoints.Length)
			{
				if (m_Loop)
				{
					m_CurrentWaypoint = 0;
				}
				else
				{
					m_CurrentWaypoint = m_CurrentWaypoint - 2;
					m_Reversing = true;
					m_PauseMoveTime = m_PauseAtEndDuration;
				}
			}
		}
		
#if UNITY_EDITOR
		// Draw the path waypoints
		void OnDrawGizmosSelected()
		{
			if (m_Waypoints == null || m_Waypoints.Length <= 1)
			{
				return;
			}
			
			// Draw the path
			Transform waypoint;
			var prevPoint = Vector3.zero;
			Gizmos.color = Color.green;

			var mesh = GetComponentInChildren<MeshCollider>(true);
			var boxCollider = GetComponentInChildren<BoxCollider>(true);
			Vector3 size = Vector3.one, centre = Vector3.one;
			if (mesh == null && boxCollider != null)
			{
				var lossyScale = boxCollider.transform.lossyScale;
				size = new Vector3(lossyScale.x * boxCollider.size.x, lossyScale.y * boxCollider.size.y,
					lossyScale.z * boxCollider.size.z);
				centre = new Vector3(lossyScale.x * boxCollider.center.x, lossyScale.y * boxCollider.center.y,
						lossyScale.z * boxCollider.center.z);
			}

			for (int i = 0, len = m_Waypoints.Length; i < len; i++)
			{
				waypoint = m_Waypoints[i];
				if (waypoint == null)
				{
					continue;
				}
				if (i > 0)
				{
					Gizmos.DrawLine(prevPoint, waypoint.position);
				}
				prevPoint = waypoint.position;

				if (mesh != null)
				{
					var offset = mesh.transform == transform ? Vector3.zero : mesh.transform.localPosition;
					var rotation = Quaternion.identity;
					if (m_RotateToWaypoint)
					{
						var previousWaypoint = m_Waypoints[i > 0 ? i - 1 : m_Waypoints.Length - 1];
						rotation = Quaternion.LookRotation(waypoint.position - previousWaypoint.position);
					}
					Gizmos.DrawWireMesh(mesh.sharedMesh, waypoint.position + offset, rotation);
				}
				else
				{
					Gizmos.DrawWireCube(waypoint.position + centre, size);
				}
			}
			
			// Draw line from the platform to the current waypoint
			waypoint = m_Waypoints[m_CurrentWaypoint];
			if (waypoint == null)
			{
				return;
			}
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(transform.position, waypoint.position);
		}

		// Check if a waypoint is selected then draw the whole path
		void OnDrawGizmos()
		{
			if (m_Waypoints == null || m_Waypoints.Length <= 1)
			{
				return;
			}
			
			// Check if one of the waypoints is selected
			for (int i = 0, len = m_Waypoints.Length; i < len; i++)
			{
				Transform waypoint = m_Waypoints[i];
				if (waypoint != null && UnityEditor.Selection.Contains(waypoint.gameObject))
				{
					OnDrawGizmosSelected();
					break;
				}
			}
		}
#endif
	}
}
