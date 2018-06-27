using UnityEngine;

namespace StandardAssets.Characters.Physics
{
	/// <summary>
	/// Physics motor properties for the Physics Third Person Motor. eg walk speed's proportion of max speed
	/// </summary>
	[CreateAssetMenu(fileName = "PhysicsMotorProperties", menuName = "Physics motor properties", order = 1)]
	public class PhysicsMotorProperties : ScriptableObject
	{
		[SerializeField, Range(0, 1f)] 
		protected float	walkSpeedProporitonOfMaxSpeed = 0.5f,
						runSpeedProportionOfMaxSpeed = 1.0f;
		
		[SerializeField, Range(0, 1f)] 
		protected float	walkAccelerationProporitonOfMaxSpeed = 0.5f,
						runAccelerationProportionOfMaxSpeed = 1.0f;

		public float walkSpeedProporiton
		{
			get { return walkSpeedProporitonOfMaxSpeed; }
		}

		public float runSpeedProportion
		{
			get { return runSpeedProportionOfMaxSpeed; }
		}
		
		public float walkAccelerationProporiton
		{
			get { return walkAccelerationProporitonOfMaxSpeed; }
		}

		public float runAccelerationProportion
		{
			get { return runAccelerationProportionOfMaxSpeed; }
		}
	}
}
