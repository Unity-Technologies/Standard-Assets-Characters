using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.FirstPerson;
using StandardAssets.Characters.ThirdPerson;
using UnityEngine;

namespace Demo
{
	public class PlayerManager : MonoBehaviour
	{
		[SerializeField]
		protected InputResponse changeViews;

		[SerializeField]
		protected ThirdPersonMotor thirdPersonMotor;

		[SerializeField]
		protected FirstPersonController firstPersonController;
		
		[SerializeField]
		protected GameObject[] firstPersonGameObjects, thirdPersonGameObjects;

		[SerializeField]
		protected Vector3 positionOffset;

		private GameObject firstPersonParent, thirdPersonParent;
		
		private void Awake()
		{
			SetupThirdPerson();
			SetupFirstPerson();
			
			
			changeViews.Init();
			
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
			thirdPersonMotor.transform.position = firstPersonController.transform.position + positionOffset;
			thirdPersonMotor.transform.rotation = firstPersonController.transform.rotation;
			firstPersonParent.SetActive(false);
		}

		private void SetFirstPerson()
		{
			//Set FPS
			thirdPersonParent.SetActive(false);
			firstPersonController.transform.position = thirdPersonMotor.transform.position + positionOffset;
			firstPersonController.transform.rotation = thirdPersonMotor.transform.rotation;
			firstPersonParent.SetActive(true);
		}

		private void SetupThirdPerson()
		{
			thirdPersonParent = SetupUnderParent("THIRD PERSON", thirdPersonGameObjects);
		}

		private void SetupFirstPerson()
		{
			firstPersonParent = SetupUnderParent("FIRST PERSON", firstPersonGameObjects);
			
		}

		private GameObject SetupUnderParent(string parentName, GameObject[] gameObjects)
		{
			GameObject parent = new GameObject {name = parentName};
			parent.transform.position = transform.position;
			parent.transform.rotation = transform.rotation;
			parent.transform.SetParent(transform, true);

			foreach (GameObject go in gameObjects)
			{
				go.transform.SetParent(parent.transform, true);
			}

			return parent;
		}
	}
}