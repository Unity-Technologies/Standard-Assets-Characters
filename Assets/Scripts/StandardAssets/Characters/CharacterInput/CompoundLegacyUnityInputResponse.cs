using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Input Response/Create Compound Unity Input Response",
		order = 1)]
	public class CompoundLegacyUnityInputResponse : InputResponse
	{
		[SerializeField]
		protected LegacyUnityInputResponse[] inputs;
		
		public override void Init()
		{
			foreach (LegacyUnityInputResponse legacyUnityInputResponse in inputs)
			{
				legacyUnityInputResponse.Init();
				legacyUnityInputResponse.started += OnInputStarted;
				legacyUnityInputResponse.ended += OnInputEnded;
			}
		}
	}
}