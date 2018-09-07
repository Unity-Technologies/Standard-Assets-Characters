using UnityEngine;

namespace StandardAssets.Characters.CharacterInput
{
	/// <summary>
	/// Unity original input implementation
	/// </summary>
	/// <seealso cref="ICharacterInput"/>
	/// <seealso cref="LegacyCharacterInputBase"/>
	public class LegacyCharacterInput : LegacyCharacterInputBase
	{
		[SerializeField, Tooltip("Name of the Input Manager axis that controls the horizontal look")]
		protected string lookXAxisName = "LookX";

		[SerializeField, Tooltip("Name of the Input Manager axis that controls the vertical look")]
		protected string lookYAxisName = "LookY";

		[SerializeField, Tooltip("Ignores any attached controllers and forces the mouse look to be used as a priority")]
		protected bool useMouseLookOnly = false;

		[Header("Movement Input Axes")]
		[SerializeField, Tooltip("Name of the Input Manager axis that controls the horizontal movement")]
		protected string horizontalAxisName = "Horizontal";

		[SerializeField, Tooltip("Name of the Input Manager axis that controls the vertical movement")]
		protected string verticalAxisName = "Vertical";

		[SerializeField, Tooltip("Name of the Input Manager axis for jumping")]
		protected string keyboardJumpName = "Jump";
		
		/// <summary>
		/// Optional input modifier, which modifies the input at the end of the UpdateMoveVector method.
		/// </summary>
		protected ILegacyCharacterInputModifier inputModifier;

		private void Awake()
		{
			inputModifier = GetComponent<ILegacyCharacterInputModifier>();
		}
		
		/// <inheritdoc />
		/// <summary>
		/// Performs the base look and movement updates as well as checks for jumping
		/// </summary>
		protected override void Update()
		{
			base.Update();
			UpdateJump();
		}

		/// <inheritdoc />
		/// <summary>
		/// Updates the look vector
		/// </summary>
		/// <remarks>
		/// Checks if the mouse look is forced.
		/// If not resolves the input based on the controller at run-time
		/// </remarks>
		protected override void UpdateLookVector()
		{
			if (useMouseLookOnly)
			{
				lookInputVector.x = Input.GetAxis(lookXAxisName);
				lookInputVector.y = Input.GetAxis(lookYAxisName);
				return;
			}

			string lookX = LegacyCharacterInputDevicesCache.ResolveControl(lookXAxisName);
			string lookY = LegacyCharacterInputDevicesCache.ResolveControl(lookYAxisName);

			lookInputVector.x = Input.GetAxis(lookX);
			lookInputVector.y = Input.GetAxis(lookY);
		}

		/// <summary>
		/// Updates the movement vectors based on the axis
		/// </summary>
		/// <remarks>
		/// These axes are consistent across all controllers.
		/// That is why there is no need to resolve the controls
		/// </remarks>
		protected override void UpdateMoveVector()
		{
			moveInputVector.Set(Input.GetAxisRaw(horizontalAxisName), Input.GetAxisRaw(verticalAxisName));
			
			if (inputModifier != null)
			{
				inputModifier.ModifyMoveInput(ref moveInputVector);
			}
		}

		/// <summary>
		/// Handles the jump
		/// </summary>
		private void UpdateJump()
		{
			string resolvedJumpControl = LegacyCharacterInputDevicesCache.ResolveControl(keyboardJumpName);

			if (Input.GetButtonDown(resolvedJumpControl) ||
			    Input.GetButtonDown("Jump"))
			{
				OnJumpPressed();
			}

			hasJumpInput = (Input.GetButton(resolvedJumpControl) || Input.GetButton("Jump"));
		}
	}
}