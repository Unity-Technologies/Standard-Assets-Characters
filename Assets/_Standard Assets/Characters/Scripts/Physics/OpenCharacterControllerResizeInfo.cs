using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Resize info for OpenCharacterController (e.g. delayed resizig until it is safe to resize).
	/// </summary>
	public class OpenCharacterControllerResizeInfo
	{
		/// <summary>
		/// Intervals (seconds) in which to check if the capsule's height/center must be changed.
		/// </summary>
		private const float k_PendingUpdateIntervals = 1.0f;
		
		/// <summary>
		/// Height to set.
		/// </summary>
		public float? height { get; private set; }
		
		/// <summary>
		/// Center to set.
		/// </summary>
		public Vector3? center { get; private set; }
		
		/// <summary>
		/// Time.time when the height must be set.
		/// </summary>
		public float? heightTime { get; private set; }
		
		/// <summary>
		/// Time.time when the center must be set.
		/// </summary>
		public float? centerTime { get; private set; }

		/// <summary>
		/// Set the pending height.
		/// </summary>
		public void SetHeight(float newHeight)
		{
			height = newHeight;
			if (heightTime == null)
			{
				heightTime = Time.time + k_PendingUpdateIntervals;
			}
		}

		/// <summary>
		/// Set the pending center.
		/// </summary>
		public void SetCenter(Vector3 newCenter)
		{
			center = newCenter;
			if (centerTime == null)
			{
				centerTime = Time.time + k_PendingUpdateIntervals;
			}
		}

		/// <summary>
		/// Set the pending height and center.
		/// </summary>
		public void SetHeightAndCenter(float newHeight, Vector3 newCenter)
		{
			SetHeight(newHeight);
			SetCenter(newCenter);
		}

		/// <summary>
		/// Cancel the pending height.
		/// </summary>
		public void CancelHeight()
		{
			height = null;
			heightTime = null;
		}

		/// <summary>
		/// Cancel the pending center.
		/// </summary>
		public void CancelCenter()
		{
			center = null;
			centerTime = null;
		}

		/// <summary>
		/// Cancel the pending height and center.
		/// </summary>
		public void CancelHeightAndCenter()
		{
			CancelHeight();
			CancelCenter();
		}

		/// <summary>
		/// Clear the timers.
		/// </summary>
		public void ClearTimers()
		{
			heightTime = null;
			centerTime = null;
		}
	}
}