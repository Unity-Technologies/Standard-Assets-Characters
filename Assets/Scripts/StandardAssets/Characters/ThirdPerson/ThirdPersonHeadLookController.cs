using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(ThirdPersonMotor))]
    public class ThirdPersonHeadLookController : MonoBehaviour
    {
        public float lookAtWeight;

        void OnAnimatorIK(int layerIndex)
        {
            var avatar = GetComponent<Animator>();
            
            avatar.SetLookAtWeight(lookAtWeight);
            
            var targetRotation = GetComponent<ThirdPersonMotor>().targetRotation;

            var angle = Mathf.Clamp(targetRotation.eulerAngles.y - avatar.transform.eulerAngles.y, -60, 60);
            
            var lookAtPos = avatar.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * avatar.transform.forward * 100f;
            
            Vector3 playerPos = avatar.transform.position;
            
            if (avatar)
            {
                avatar.SetLookAtPosition(lookAtPos);
            }
        }
    }
}
