using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.GizmosHelpers
{
	/// <summary>
	/// Renders directional gizmos as well as arrows to determine 3 distinct directional vectors, namely:
	/// <list type="bullet">
	/// <item>
	/// <description>Forward Direction</description>
	/// </item>
	/// <item>
	/// <description>Intended Rotational Direction</description>
	/// </item>
	/// <item>
	/// <description>Input Direction</description>
	/// </item>
	/// </list>
	/// </summary>
	//[RequireComponent(typeof(ICharacterInput))]
	[RequireComponent(typeof(CharacterBrain))]
	public class GizmoArrowHelper : MonoBehaviour
	{
		/// <summary>
		/// Boolean used to enable or disable the drawing of these arrows in the scene
		/// </summary>
		public bool enablePowerDebug = true;
		
#if UNITY_EDITOR
		/// <summary>
		/// The Input implementation to be used
		/// e.g. Default unity input or (in future) the new new input system
		/// </summary>
		protected BaseInput characterInput;
		protected CharacterBrain characterMotor;

		private const string k_ArrowGizmoPath = "Gizmos/GizmoArrow";

		//Instances of arrow models
		private GameObject forwardDirection;
		private GameObject intendedRotation;
		private GameObject inputDirection;

		/// <summary>
		/// Gets the ICharacterInput, and the CharacterBrain when the Awake method is triggered
		/// </summary>
		protected virtual void Awake()
		{
			characterInput = GetComponent<BaseInput>();
			characterMotor = GetComponent<CharacterBrain>();
		}

		/// <summary>
		///When the script starts it will instantiate 3 GizmoArrow objects for:
		/// <list type="bullet">
		/// <item>
		/// <description>Forward Direction</description>
		/// </item>
		/// <item>
		/// <description>Intended Rotational Direction</description>
		/// </item>
		/// <item>
		/// <description>Input Direction</description>
		/// </item>
		/// </list>
		/// </summary>
		private void Start()
		{
			CreateGizmoArrow(transform.position, transform.position + transform.forward * 5, 0.5f, Color.green, "ForwardDirection", out forwardDirection);
			CreateGizmoArrow(transform.position, transform.position + transform.forward * 5, 0.5f, Color.blue, "InputDirection", out inputDirection);
			CreateGizmoArrow(transform.position, transform.position + transform.forward * 5, 0.5f, Color.red, "TargetRotation", out intendedRotation);
		}

		/// <summary>
		/// Creates an instance of a GizmoArrow given a start point, end point, color and name and draws it to the scene as a child object of this script's transform 
		/// </summary>
		private void CreateGizmoArrow(Vector3 start, Vector3 end, float width, Color color, string name, out GameObject cylinderObject)
		{
			var offset = end - start;
			//var scale = new Vector3(width, offset.magnitude / 2.0f, width);
			var position = start + (offset / 2.0f);
			cylinderObject = Instantiate(Resources.Load(k_ArrowGizmoPath)) as GameObject;

			if (cylinderObject != null)
			{
				cylinderObject.transform.forward = offset;
				cylinderObject.transform.position = position;
				cylinderObject.name = name;
				cylinderObject.transform.parent = transform;
				ArrowColorSelect arrowColorSelect = cylinderObject.GetComponentInChildren<ArrowColorSelect>();
				if (arrowColorSelect != null)
				{
					arrowColorSelect.color = color;
					arrowColorSelect.OnValidate();
				}
			}
		}

		/// <summary>
		/// Updates the arrow position, as well as the Debug lines drawn to scene, on every OnDrawGizmos call.
		/// </summary>
		private void OnDrawGizmos()
		{
			if (enablePowerDebug)
			{
				if (Application.isPlaying)
				{
					//Forward direction of the character transform
					Debug.DrawLine(transform.position, transform.position + transform.forward * 5, Color.green);
					forwardDirection.transform.localRotation = Quaternion.LookRotation(Vector3.forward);

					//Translate move input from vector2 into 3D space
					var translatedMoveInput = new Vector3(characterInput.moveInput.x, 0, characterInput.moveInput.y);
					if (translatedMoveInput.magnitude < 0.0001f)
					{
						translatedMoveInput = Vector3.forward;
					}

					//Find the direction of input relative to both the character and the main camera
					Debug.DrawLine(transform.position, transform.position + Camera.main.transform.TransformDirection(translatedMoveInput) * 10, Color.blue);

					//ArrowDrawer position for Input Direction
					Quaternion inputRotationQuaternion =
						Quaternion.LookRotation(Vector3.Scale(Camera.main.transform.forward , new Vector3(1,0,1)  ));

					inputDirection.transform.rotation = Quaternion.LookRotation( inputRotationQuaternion * translatedMoveInput.normalized);

					//Intended rotation by degrees
					float angle = characterMotor.targetYRotation;

					//Find vector rotated by given degrees
					Quaternion intendedRotationQuaternion = Quaternion.Euler(0, angle, 0); 
					Vector3 targetPoint = transform.position + (intendedRotationQuaternion * Vector3.forward);

					//Draw the line to the intended rotation
					Debug.DrawLine(transform.position, targetPoint, Color.red);

					//Arrow Drawer position for intended direction
					intendedRotation.transform.rotation = intendedRotationQuaternion;
				}
			}
		}
#endif
	}
}