using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Util;

namespace StandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(RootMotionThirdPersonMotor))]
    public class ThirdPersonHeadLookController : MonoBehaviour
    {
        public float lookAtWeight;

        void OnAnimatorIK(int layerIndex)
        {
            float lookAtAngle = 0f;
            
            var avatar = GetComponent<Animator>();

            avatar.SetLookAtWeight(lookAtWeight);
            
            var lookAtPosition = new Vector3(transform.position.x + avatar.angularVelocity.y, transform.position.y, avatar.angularVelocity.z + 1f);
            var neutralLookAtPosition = new Vector3(transform.position.x, transform.position.y, avatar.angularVelocity.z + 1f);
            
            
            /* When using avatar.angularVelocity you can determine the direction of the PC by the steps below
            //Forward = avatar.angularVelocity = Vector3.Zero
            //Right = avatar.angularVelocity.y > 0
            //Left = avatar.angularVelocity.y < 0
            */
            

            
            if (avatar)
            {
                
                Vector3 playerPos = avatar.transform.position;
                Vector3 playerDirection = avatar.transform.forward;
                Quaternion playerRotation = avatar.transform.rotation;

                
                
                
                Debug.DrawLine(playerPos, playerPos + playerDirection * 5, Color.red);
                
                Debug.DrawLine(playerPos, new Vector3(playerPos.x + avatar.angularVelocity.y * 2, playerPos.y + 1f, playerPos.z) + playerDirection * 5, Color.blue);
                
                
//                avatar.SetLookAtPosition(!avatar.angularVelocity.Equals(Vector3.zero)
//                    ? lookAtPosition2
//                    : lookahead);
//                
                
                
                avatar.SetLookAtPosition(new Vector3(playerPos.x + avatar.angularVelocity.y * 2, playerPos.y + 1f, playerPos.z) + playerDirection * 5);
            }
        }
    }
}
