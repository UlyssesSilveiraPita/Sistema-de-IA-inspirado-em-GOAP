using UnityEngine;
using up.AI.Utils;

namespace up.AI
{

    public class OnAnimationExit : StateMachineBehaviour
    {
        public enum AnimationType { Attack, Hit}

        [SerializeField] private AnimationType animationType = AnimationType.Attack;
        private EnemyAI _enemyAI;


        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_enemyAI != null) return;

            if (animator.TryGetComponent(out _enemyAI) == false || _enemyAI == null)
            {
                _enemyAI = animator.GetComponentInParent<EnemyAI>();
            }
        }


        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_enemyAI == null)
            {
                if (animator.TryGetComponent(out _enemyAI) == false || _enemyAI == null)
                {
                    _enemyAI = animator.GetComponentInParent<EnemyAI>();
                }
            }

            if (_enemyAI == null) return;

            switch (animationType)
            {
                case AnimationType.Attack:
                    _enemyAI.OnAttackAnimationEnd();
                    break;

                case AnimationType.Hit:
                    _enemyAI.OnHitAnimationEnd();
                    break;

            }
        }

    }
}