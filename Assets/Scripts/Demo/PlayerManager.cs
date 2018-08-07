using System.Collections.Generic;
using Cinemachine;
using StandardAssets.Characters.CharacterInput;
using StandardAssets.Characters.FirstPerson;
using StandardAssets.Characters.ThirdPerson;
using UnityEditor;
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
		protected Vector3 positionOffset;

		private GameObject firstPersonParent, thirdPersonParent;

		[SerializeField]
		protected CinemachineStateDrivenCamera thirdPersonMainStateDrivenCamera;

		[SerializeField]
		protected CinemachineStateDrivenCamera firstPersonMainStateDrivenCamera;
		

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
			
			
			
		}
		
		
		private void Start()
		{
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
			thirdPersonBrain.transform.position = firstPersonBrain.transform.position + positionOffset;
			thirdPersonBrain.transform.rotation = firstPersonBrain.transform.rotation;

			
			
			firstPersonParent.SetActive(false);
			

			if (thirdPersonCameraModeText !=null)
			{
				thirdPersonCameraModeText.gameObject.SetActive(true);
			}

			if (thirdPersonMainStateDrivenCamera != null)
			{
				thirdPersonMainStateDrivenCamera.MoveToTopOfPrioritySubqueue();
			}
			
			
		}

		private void SetFirstPerson()
		{
			//Set FPS
			
			//If first person already set, then go to SetThirdPerson. 
			if (firstPersonParent != null && firstPersonParent.activeSelf)
			{
				SetThirdPerson();
				return;
			}
			thirdPersonParent.SetActive(false);
			firstPersonBrain.transform.position = thirdPersonBrain.transform.position - positionOffset;
			firstPersonBrain.transform.rotation = thirdPersonBrain.transform.rotation;
			firstPersonParent.SetActive(true);
			
			
			if (thirdPersonCameraModeText !=null)
			{
				thirdPersonCameraModeText.gameObject.SetActive(false);
			}

			if (firstPersonMainStateDrivenCamera != null)
			{
				/*
				 * firstPersonMainStateDrivenCamera
					.ChildCameras[0].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>()
					.m_HorizontalAxis.Value = LastKnownFreeLookCamPoisition();
				 */
				firstPersonMainStateDrivenCamera.MoveToTopOfPrioritySubqueue();
				
			}
			
			

		}

		private void SwitchCharacter()
		{
			if (firstPersonParent.activeSelf)
			{
				SetThirdPerson();
			}

			if (thirdPersonParent.activeSelf)
			{
				SetFirstPerson();
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
		
		/// <summary>
		/// This method will return the last Horizontal axis value for the
		/// Freelook camera that was active before player changed to Frist person
		/// </summary>
		/// <returns></returns>
		private float LastKnownFreeLookCamPoisition()
		{
			foreach (var childStateCamera in thirdPersonMainStateDrivenCamera.ChildCameras)
			{
				if (childStateCamera.ParentCamera.IsLiveChild(childStateCamera))
				{
					foreach (var childCamera in childStateCamera.GetComponent<CinemachineStateDrivenCamera>().ChildCameras)
					{
						if (childCamera.ParentCamera.IsLiveChild(childCamera))
						{
							return childCamera.GetComponent<CinemachineFreeLook>().m_XAxis.Value;
							
						}
					}
				}
			}

			return 0;
		}

	}
}