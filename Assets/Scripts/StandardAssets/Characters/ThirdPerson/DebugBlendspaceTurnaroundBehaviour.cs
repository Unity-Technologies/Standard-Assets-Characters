using UnityEditor;
using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
	public class DebugBlendspaceTurnaroundBehaviour : BlendspaceTurnaroundBehaviour
	{
		[SerializeField]
		protected float[] times = new float[5]; 
		
		protected override void Update()
		{
			base.Update();
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				timeToTurn = times[0];
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				timeToTurn = times[1];
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				timeToTurn = times[2];
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				timeToTurn = times[3];
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				timeToTurn = times[4];
			}
		}
	}
}