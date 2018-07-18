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
            float lookAtAngle = 0f;
            Vector3 lookAtAxis;
            GetComponent<ThirdPersonMotor>().targetRotation.ToAngleAxis(out lookAtAngle, out lookAtAxis);
            var avatarPosition = GetComponent<ThirdPersonMotor>().transform.position;
            var avatar = GetComponent<Animator>();

            avatar.SetLookAtWeight(lookAtWeight);

            var avatarForwardPosition = transform.forward;

//            var piotrVector = GetComponent<ThirdPersonMotor>().targetRotation * Vector3.forward;
//
//            var slepDerp = Vector3.Slerp(avatarForwardPosition, piotrVector, 0.5f);

            var direction = Quaternion.AngleAxis(lookAtAngle, avatarForwardPosition);
            
            Debug.Log("Hello Angle: " + MathUtilities.Wrap180(GetComponent<ThirdPersonMotor>().targetRotation.eulerAngles.y - transform.eulerAngles.y));

            if (avatar)
            {
                avatar.SetLookAtPosition(direction.eulerAngles);
            }

        }
    }
}
