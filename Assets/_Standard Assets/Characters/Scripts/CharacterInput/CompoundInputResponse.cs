using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Input Response/Create Compound Input Response",
		order = 1)]
	public class CompoundInputResponse : InputResponse
	{
		[SerializeField]
		protected InputResponse[] inputs;
		
		public override void Init()
		{
			foreach (InputResponse inputResponse in inputs)
			{
				inputResponse.Init();
				inputResponse.started += OnInputStarted;
				inputResponse.ended += OnInputEnded;
			}
		}
	}
}