using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace StandardAssets.Characters.Input
{
	public class DefaultMouseLookInput : MonoBehaviour, ILookInput
	{
		Vector2 m_LookInput;

		public Vector2 lookInput
		{
			get { return m_LookInput; }
		}
		
		public bool isLookInput { get; private set; }

		void Update()
		{
			m_LookInput.Set(UnityInput.GetAxis("Mouse X"), UnityInput.GetAxis("Mouse Y"));
		}
	}
}