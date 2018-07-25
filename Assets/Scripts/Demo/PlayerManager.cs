using System.Collections.Generic;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.FirstPerson;
using StandardAssets.Characters.ThirdPerson;
using UnityEngine;
using UnityEngine.Serialization;

namespace Demo
{
	public class PlayerManager : MonoBehaviour
	{
		[SerializeField]
		protected InputResponse changeViews;

		[SerializeField]
		protected ThirdPersonBrain thirdPersonBrain;

		[SerializeField, FormerlySerializedAs("firstPersonController")]
		protected FirstPersonBrain firstPersonBrain;
		
		[SerializeField]
		protected GameObject[] firstPersonGameObjects, thirdPersonGameObjects;

		[SerializeField]
		protected Vector3 positionOffset;

		private GameObject firstPersonParent, thirdPersonParent;
		
		[SerializeField]
		protected CinemachineStateDrivenCamera thirdPersonStateCameras;

		[SerializeField] 
		protected bool parentObjects = true;

		private void Awake()
		{
			if (parentObjects)
			{
				SetupThirdPerson();
				SetupFirstPerson();
			}
			
			changeViews.Init();
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
			thirdPersonBrain.transform.position = firstPersonBrain.transform.position + positionOffset;
			thirdPersonBrain.transform.rotation = firstPersonBrain.transform.rotation;
			firstPersonParent.SetActive(false);
			if (thirdPersonStateCameras != null)
			{
				thirdPersonStateCameras.Priority = 11;
			}
		}

		private void SetFirstPerson()
		{
			//Set FPS
			thirdPersonParent.SetActive(false);
			firstPersonBrain.transform.position = thirdPersonBrain.transform.position + positionOffset;
			firstPersonBrain.transform.rotation = thirdPersonBrain.transform.rotation;
			firstPersonParent.SetActive(true);
			if (thirdPersonStateCameras != null)
			{
				thirdPersonStateCameras.Priority = 1;
			}
		}

		private void SetupThirdPerson()
		{
			thirdPersonParent = SetupUnderParent("THIRD PERSON", thirdPersonGameObjects);
		}

		private void SetupFirstPerson()
		{
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