using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    [RequireComponent(typeof(Animator))]
    
    public class PlayerLegAnimator : NetworkBehaviour
    {
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
        private Animator _animator;
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
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