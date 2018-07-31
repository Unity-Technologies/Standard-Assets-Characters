using System.Collections.Generic;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.FirstPerson;
using StandardAssets.Characters.Physics;
using StandardAssets.Characters.ThirdPerson;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Demo
{
	public class PlayerManager : MonoBehaviour
	{

		[SerializeField]
		protected Text thirdPersonCameraModeText;

		
		[SerializeField]
		protected InputResponse changeViews;

		[SerializeField]
		protected ThirdPersonBrain thirdPersonBrain;

		[SerializeField, FormerlySerializedAs("firstPersonController")]
		protected FirstPersonBrain firstPersonBrain;
		
		[SerializeField]
		protected GameObject[] firstPersonGameObjects, thirdPersonGameObjects;

		[SerializeField]
		protected Vector3 positionOffset = new Vector3(0, 1, 0);

		private GameObject firstPersonParent, thirdPersonParent;

		private OpenCharacterController firstPersonCharacterController, thirdPersonCharacterController;
		

		[SerializeField] 
		protected bool parentObjects = true;

		private void Awake()
		{
			// TODO remove this when this stops getting nulled
			if (thirdPersonBrain == null)
			{
				thirdPersonBrain = FindObjectOfType<ThirdPersonBrain>();
			}
			
			if (parentObjects)
			{
				SetupThirdPerson();
				SetupFirstPerson();
			}
			
			changeViews.Init();
			
			SetFirstPerson();
			SetThirdPerson();
		}

		private void OnEnable()
		{
			if (changeViews != null)
			{
				changeViews.started += SetFirstPerson;
				changeViews.ended += SetThirdPerson;
			}
		}

		private void OnDisable()
		{
			if (changeViews != null)
			{
				changeViews.started -= SetFirstPerson;
				changeViews.ended -= SetThirdPerson;
			}
		}

		private void SetThirdPerson()
		{
			//Set Third Person
			thirdPersonParent.SetActive(true);
			if (firstPersonCharacterController.isGrounded)
			{
				thirdPersonCharacterController.SetPosition(firstPersonBrain.transform.position + positionOffset, true);
			}
			else
			{
				thirdPersonCharacterController.SetPosition(firstPersonBrain.transform.position, true);
			}
			thirdPersonBrain.transform.rotation = firstPersonBrain.transform.rotation;
			firstPersonParent.SetActive(false);

			if (thirdPersonCameraModeText !=null)
			{
				thirdPersonCameraModeText.gameObject.SetActive(true);
			}
		}

		private void SetFirstPerson()
		{
			//Set FPS
			thirdPersonParent.SetActive(false);
			if (thirdPersonCharacterController.isGrounded)
			{
				firstPersonCharacterController.SetPosition(thirdPersonBrain.transform.position + positionOffset, true);
			}
			else
			{
				firstPersonCharacterController.SetPosition(thirdPersonBrain.transform.position, true);
			}
			firstPersonBrain.transform.rotation = thirdPersonBrain.transform.rotation;
			firstPersonParent.SetActive(true);
			if (thirdPersonCameraModeText !=null)
			{
				thirdPersonCameraModeText.gameObject.SetActive(false);
			}

		}

		private void SetupThirdPerson()
		{
			thirdPersonCharacterController = thirdPersonBrain.GetComponent<OpenCharacterControllerPhysics>().GetOpenCharacterController();
			thirdPersonParent = SetupUnderParent("THIRD PERSON", thirdPersonGameObjects);
		}

		private void SetupFirstPerson()
		{
			firstPersonCharacterController = firstPersonBrain.GetComponent<OpenCharacterControllerPhysics>().GetOpenCharacterController();
			firstPersonParent = SetupUnderParent("FIRST PERSON", firstPersonGameObjects);
		}

		private GameObject SetupUnderParent(string parentName, IEnumerable<GameObject> gameObjects)
		{
			var parent = new GameObject(parentName);
			parent.transform.SetParent(transform, false);
			
			foreach (var go in gameObjects)
			{
				if (go != null)
				{
					go.transform.SetParent(parent.transform, true);
				}
			}
			return parent;
		}
	}
}