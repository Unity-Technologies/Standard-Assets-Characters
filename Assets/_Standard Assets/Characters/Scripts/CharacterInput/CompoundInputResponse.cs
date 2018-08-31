using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// An input response that reacts to multiple input responses and forwards on their callbacks
	/// Used for supporting multiple platforms
	/// </summary>
	[CreateAssetMenu(fileName = "InputResponse", menuName = "Standard Assets/Characters/Input/Create Compound Input Response",
		order = 1)]
	public class CompoundInputResponse : InputResponse
	{
		[SerializeField, Tooltip("Collection of different input responses - used for supporting multiple platforms")]
		protected InputResponse[] inputs;
		
		/// <inheritdoc />
		/// <summary>
		/// Iterates through all of the Input Responses and subscribe to their callbacks
		/// </summary>
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