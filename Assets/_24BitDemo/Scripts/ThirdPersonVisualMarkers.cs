using System;
using System.Security.Permissions;
using UnityEngine;
using StandardAssets.Characters.Physics;
using StandardAssets.Characters.Common;

namespace Demo
{
	[RequireComponent(typeof(CharacterPhysics))]
	[RequireComponent(typeof(CharacterInput))]
	[RequireComponent(typeof(CharacterBrain))]
	public class ThirdPersonVisualMarkers : MonoBehaviour
	{
		public bool enablePowerDebug;
		
		/// <summary>
		/// The Input implementation to be used
		/// e.g. Default unity input or (in future) the new new input system
		/// </summary>
		protected CharacterInput characterInput;


		protected CharacterBrain characterMotor;

		public CharacterInput inputForCharacter
		{
			get { return characterInput; }
		}
		
		public CharacterBrain motorForCharacter
		{
			get { return characterMotor; }
		}
		
		/// <summary>
		/// Get physics and input on Awake
		/// </summary>
		protected virtual void Awake()
		{
			characterInput = GetComponent<CharacterInput>();
			characterMotor = GetComponent<CharacterBrain>();
			
			
		}
		
		/// <summary>
		/// Rotate around an implied circle to give the vector at a certain point along the circumference 
		/// </summary>
		public Vector3 RotateByDegrees(Vector3 centre, float radius, float angle)
		{
			float centreX = centre.x;
			float centreY = centre.z;

			angle = angle * Mathf.Deg2Rad;

			var rotationPoint = new Vector3
			{
				x = (Mathf.Sin(angle) * radius) + centreX, y = centre.y, z = (Mathf.Cos(angle) * radius) + centreY
			};

			return rotationPoint;
		}
		
		#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (enablePowerDebug)
			{
				if (Application.isPlaying)
				{
					//Forward direction of the character transform
					Debug.DrawLine(transform.position, transform.position + transform.forward * 5.0f, Color.green);
					
					//Translate move input from vector2 into 3D space
					var translatedMoveInput = new Vector3(characterInput.moveInput.x, 0.0f, characterInput.moveInput.y);
					
					//Find the direction of input relative to both the character and the main camera
					Debug.DrawLine(transform.position, transform.position + Camera.main.transform.TransformDirection(translatedMoveInput) * 10.0f, Color.blue);
					
					//Intended rotation by degrees
					float angle = characterMotor.targetYRotation;
					
					//Find vector rotated by given degrees
					Vector3 targetPoint = RotateByDegrees(transform.position, 1.0f, angle);
					
					//Draw the line to the intended rotation
					Debug.DrawLine(transform.position, targetPoint, Color.red);
				}
			}
		}
		#endif
	}
}


