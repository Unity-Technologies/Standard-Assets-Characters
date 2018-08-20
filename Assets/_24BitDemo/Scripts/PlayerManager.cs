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
		[SerializeField]
		protected ThirdPersonBrain thirdPersonBrain;

		[SerializeField]
		protected CinemachineStateDrivenCamera thirdPersonMainStateDrivenCamera;

		[SerializeField]
		protected CinemachineStateDrivenCamera firstPersonMainStateDrivenCamera;

		[SerializeField]
		protected Transform[] warpPositions;

		private int warpPositionIndex = 0;

		[SerializeField] protected InputResponse warpPlayerInput,
			warpPlayerPreviousInput;

		[SerializeField]
		protected GameObject thirdPersonGameObject;

		[SerializeField]
		protected GameObject firstPersonGameObject;

		[SerializeField]
		protected Boolean thirdPersonMode = true;

		private void Awake()
		{
			warpPlayerInput.Init();
			warpPlayerPreviousInput.Init();
			
			if (thirdPersonMode)
			{
				firstPersonGameObject.SetActive(false);
				thirdPersonGameObject.SetActive(true);
				thirdPersonMainStateDrivenCamera.Priority = 10;
				firstPersonMainStateDrivenCamera.Priority = 0;
			}
			else
			{
				firstPersonGameObject.SetActive(true);
				thirdPersonGameObject.SetActive(false);
				thirdPersonMainStateDrivenCamera.Priority = 0;
				firstPersonMainStateDrivenCamera.Priority = 10;
					
				
			}
		}

		private void OnEnable()
		{
			if (warpPlayerInput != null)
			{
				warpPlayerInput.started += WarpToNextPoint;
			}
			if (warpPlayerPreviousInput != null)
			{
				warpPlayerPreviousInput.started += WarpToPreviousPoint;
			}
		}

		private void OnDisable()
		{
			if (warpPlayerInput != null)
			{
				warpPlayerInput.started -= WarpToNextPoint;
			}
			if (warpPlayerPreviousInput != null)
			{
				warpPlayerPreviousInput.started -= WarpToPreviousPoint;
			}
		}

		void WarpToNextPoint()
		{
			if (++warpPositionIndex >= warpPositions.Length)
			{
				warpPositionIndex = 0;
			}
			thirdPersonBrain.transform.position = warpPositions[warpPositionIndex].position;
		}
		
		void WarpToPreviousPoint()
		{
			if (thirdPersonBrain.transform.position == warpPositions[warpPositionIndex].position)
			{
				if (--warpPositionIndex <= 0)
				{
					warpPositionIndex = warpPositions.Length - 1;
				}
			}
			thirdPersonBrain.transform.position = warpPositions[warpPositionIndex].position;
		}
	}
}