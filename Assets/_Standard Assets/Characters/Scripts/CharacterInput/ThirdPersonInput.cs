namespace StandardAssets.Characters.CharacterInput
{
	public class ThirdPersonInput : BaseInput, IThirdPersonInput
	{
		protected override void RegisterAdditionalInputs()
		{
			
		}

		public void ResetSprint()
		{
			isSprinting = false;
		}
	}
}