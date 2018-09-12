using UnityEngine;

namespace StandardAssets.Characters.FirstPerson
{
	[CreateAssetMenu(fileName = "HeadBobProfile",
		menuName = "Standard Assets/Characters/Create First Person Head Bob Profile", order = 1)]
	public class HeadBobProfile : ScriptableObject
	{
		[SerializeField]
		protected HeadBobCollection footstepHeadBob;

		[SerializeField]
		protected HeadBobCollection fallHeadBob;
	}
}