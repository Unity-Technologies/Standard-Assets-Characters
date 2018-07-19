using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(RootMotionThirdPersonMotor))]
    public class ThirdPersonHeadLookController : MonoBehaviour
    {
        public float lookAtWeight;

        void OnAnimatorIK(int layerIndex)
        {
            var avatar = GetComponent<Animator>();
            
            avatar.SetLookAtWeight(lookAtWeight);
            
            var targetRotation = GetComponent<ThirdPersonBrain>().rootMotionThirdPersonMotor.targetRotation;

            var angle = Mathf.Clamp(targetRotation.eulerAngles.y - avatar.transform.eulerAngles.y, -60, 60);
            
            var lookAtPos = avatar.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * avatar.transform.forward * 100f;
            
            if (avatar)
            {
                avatar.SetLookAtPosition(lookAtPos);
            }
        }
    }
}
