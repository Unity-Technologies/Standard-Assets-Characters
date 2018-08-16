using System;
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
		
		

		/*
		 *
		 
		 [SerializeField, FormerlySerializedAs("firstPersonController")]
		protected FirstPersonBrain firstPersonBrain;

		//[SerializeField]
		//protected GameObject[] firstPersonGameObjects, thirdPersonGameObjects;
		
		[SerializeField]
		protected Text thirdPersonCameraModeText;

		[SerializeField]
		protected InputResponse changeViews;

		[SerializeField]
		protected Vector3 positionOffset;
		
		[SerializeField]
		protected bool parentObjects = true;

		private GameObject firstPersonParent, thirdPersonParent;

		 */
		[SerializeField]
		protected ThirdPersonBrain thirdPersonBrain;

		[SerializeField]
		protected CinemachineStateDrivenCamera thirdPersonMainStateDrivenCamera;

		[SerializeField]
		protected CinemachineStateDrivenCamera firstPersonMainStateDrivenCamera;

		[SerializeField]
		protected Transform[] warpPositions;

		private int warpPositionIndex = 0;

		[SerializeField]
		protected InputResponse warpPlayerInput;

		[SerializeField]
		protected GameObject thirdPersonGameObject;

		[SerializeField]
		protected GameObject firstPersonGameObject;

		[SerializeField]
		protected Boolean thirdPersonMode = true;

		private void Awake()
		{
			warpPlayerInput.Init();
			
			if (thirdPersonMode)
			{
				firstPersonGameObject.active = false;
				thirdPersonGameObject.active = true;
				thirdPersonMainStateDrivenCamera.Priority = 10;
				firstPersonMainStateDrivenCamera.Priority = 0;
			}
			else
			{
				firstPersonGameObject.active = true;
				thirdPersonGameObject.active = false;
				thirdPersonMainStateDrivenCamera.Priority = 0;
				firstPersonMainStateDrivenCamera.Priority = 10;
					
				
			}
			
			/*
			 * 
			if (thirdPersonBrain == null)
			{
				thirdPersonBrain = FindObjectOfType<ThirdPersonBrain>();
			}

			if (parentObjects)
			{
				//SetupThirdPerson();
				//SetupFirstPerson();
			}
			 */
			
			
		}

		

		private void OnEnable()
		{
			if (warpPlayerInput != null)
			{
				warpPlayerInput.started += WarpToNextPoint;
			}
		}

		private void OnDisable()
		{
			if (warpPlayerInput != null)
			{
				warpPlayerInput.started -= WarpToNextPoint;
			}
		}
		
		void WarpToNextPoint()
		{
			if (warpPositionIndex >= warpPositions.Length)
			{
				warpPositionIndex = 0;
			}

			thirdPersonBrain.transform.position = warpPositions[warpPositionIndex++].position;
		}
		

		/*
		 void PrioritiseCamera(CinemachineStateDrivenCamera camera)
		{
			if (camera != null)
			{
				camera.MoveToTopOfPrioritySubqueue();
			}
		}

		private void Start()
		{
			//SetThirdPerson(false);
		}
		
		 private void SetThirdPerson()
		{
			SetThirdPerson(true);
		}

		private void SetThirdPerson(bool setPosition)
		{
			//Set Third Person
			thirdPersonParent.SetActive(true);
			if (setPosition)
			{
				thirdPersonBrain.transform.position = firstPersonBrain.transform.position + positionOffset;
				thirdPersonBrain.transform.rotation = firstPersonBrain.transform.rotation;
			}

			firstPersonParent.SetActive(false);

			if (thirdPersonCameraModeText != null)
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
			SetFirstPerson(true);
		}

		private void SetFirstPerson(bool setPosition)
		{
			//Set FPS

			//If first person already set, then go to SetThirdPerson. 
			if (firstPersonParent != null && firstPersonParent.activeSelf)
			{
				SetThirdPerson();
				return;
			}

			thirdPersonParent.SetActive(false);
			if (setPosition)
			{
				firstPersonBrain.transform.position = thirdPersonBrain.transform.position - positionOffset;
				firstPersonBrain.transform.rotation = thirdPersonBrain.transform.rotation;
			}

			firstPersonParent.SetActive(true);

			if (thirdPersonCameraModeText != null)
			{
				thirdPersonCameraModeText.gameObject.SetActive(false);
			}

			if (firstPersonMainStateDrivenCamera != null)
			{
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
		 * private void SetupThirdPerson()
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
					foreach (var childCamera in childStateCamera
					                            .GetComponent<CinemachineStateDrivenCamera>().ChildCameras)
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
	*/
		
	}
}