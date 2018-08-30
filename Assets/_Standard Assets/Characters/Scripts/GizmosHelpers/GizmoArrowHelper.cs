using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.Common;
using UnityEngine;

namespace StandardAssets.Characters.GizmosHelpers
{
	/// <summary>
	/// Renders directional gizmos as well as arrows to determine 3 distinct directional vectors, namely:
	/// Forward Direction
	/// Intended rotational direction
	/// Input Direction
	/// </summary>
	public class GizmoArrowHelper : MonoBehaviour
	{
		private GameObject cylinderPrefab;
		private const string k_ArrowGizmoPath = "Gizmos/GizmoArrow";

		public bool enablePowerDebug;

		/// <summary>
		/// The Input implementation to be used
		/// e.g. Default unity input or (in future) the new new input system
		/// </summary>
		protected ICharacterInput characterInput;


		protected CharacterBrain characterMotor;

		public ICharacterInput inputForCharacter
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
			characterInput = GetComponent<ICharacterInput>();
			characterMotor = GetComponent<CharacterBrain>();
		}
		
		//Instances of arrow models
		private GameObject forwardDirection;
		private GameObject intendedRotation;
		private GameObject inputDirection;
		
		//Create arrows on start, if in editor
		private void Start()
		{
#if UNITY_EDITOR
			CreateGizmoArrow(transform.position, transform.position + transform.forward * 5, 0.5f, Color.green, "ForwardDirection", out forwardDirection);
			CreateGizmoArrow(transform.position, transform.position + transform.forward * 5, 0.5f, Color.blue, "InputDirection", out inputDirection);
			CreateGizmoArrow(transform.position, transform.position + transform.forward * 5, 0.5f, Color.red, "TargetRotation", out intendedRotation);
#endif
		}

		//Testing Code for arrow models
		void CreateGizmoArrow(Vector3 start, Vector3 end, float width, Color color, string name, out GameObject cylinderObject)
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
					arrowColorSelect.Color = color;
					arrowColorSelect.OnValidate();
				}
			}
		}

#if UNITY_EDITOR
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