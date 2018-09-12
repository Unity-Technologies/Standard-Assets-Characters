using System;
using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[Serializable]
	public class HeadBobCollection
	{
		[SerializeField]
		protected HeadBobConfiguration[] rotationHeadBobConfigurations;
		
		[SerializeField]
		protected HeadBobConfiguration[] translationHeadBobConfigurations;

		[SerializeField]
		protected bool appliesRotationFirst = true;

	}
}