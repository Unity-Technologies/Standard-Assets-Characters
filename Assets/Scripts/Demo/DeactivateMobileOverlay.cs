using UnityEngine;

namespace Demo
{
    public class DeactivateMobileOverlay: MonoBehaviour
    {
        // Use this for initialization
        void Start () {
#if UNITY_STANDALONE_WIN
		gameObject.active = false;
		#endif
		
        }
    }
}