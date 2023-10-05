using UnityEngine;

namespace PlayerLogic
{
    
    public class PlayerLegAnimator
    {
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
        private readonly Animator _animator;

        public PlayerLegAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void PlayMove()
        {
            _animator.SetBool(IsMoving, true);
        }

        public void PlayIdle()
        {
            _animator.SetBool(IsMoving, false);
        }

        public void PlayCrouch()
        {
            _animator.SetBool(IsCrouching, true);
        }

        public void StopCrouch()
        {
            _animator.SetBool(IsCrouching, false);
        }
    }
}