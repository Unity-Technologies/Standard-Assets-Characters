using UnityEngine;

namespace StandardAssets.Characters.ThirdPerson.AnimatorBehaviours
{
    public class LandAnimatorState : StateMachineBehaviour 
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var animationController = animator.GetComponent<ThirdPersonBrain>().animationControl;
            if (animationController != null)
            {
                animationController.OnLandAnimationExit();
            }
        }
		
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var animationController = animator.GetComponent<ThirdPersonBrain>().animationControl;
            if (animationController != null)
            {
                animationController.OnLandAnimationEnter();
            }
        }
    }
}