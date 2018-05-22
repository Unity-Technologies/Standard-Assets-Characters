namespace StandardAssets.Characters.Effects
{
	/// <summary>
	/// Uses ColliderMovementDetection objects to broadcast movement events. i.e. footfalls
	/// </summary>
	public class ColliderMovementEventBroadcaster : MovementEventBroadcaster
	{
		/// <summary>
		/// The movement detections
		/// </summary>
		public ColliderMovementDetection[] movementDetections;

		/// <summary>
		/// Subscribe to the movement detection events
		/// </summary>
		void Awake()
		{
			foreach (ColliderMovementDetection colliderMovementDetection in movementDetections)
			{
				colliderMovementDetection.detection += OnMoved;
			}
		}
	}
}