using UnityEngine;
using UnityEngine.AI;
using up.AI.Utils;

namespace up.AI.Combat
{
    public class CombatContext
    {
        public EnemyAI enemyAI;
        public NavMeshAgent agent;

        // Target
        public Transform target;
        public bool hasTarget;

        //visao / combat
        public bool hasLos;
        public bool isTakingHit;
        public bool isKnockbackRunning;

        public bool isAttacking;
        public float nextAttackAt;

        public float disanceToTarget;

        public float stamina01;

        public float awayEscapeQuality01;
        public float rightEscapeQuality01;
        public float leftEscapeQuality01;

        public float lastRepositionTime;
        public float repositonDesireUntil;
        public int consecutiveMeleeCount;


        public CombatContext(EnemyAI enemyAI, NavMeshAgent agent)
        {
            this.enemyAI = enemyAI;
            this.agent = agent; 
        }

        public void Refresh()
        {
            if(enemyAI == null) 
            { 
                target = null;
                hasTarget = false;
                hasLos = false;
                disanceToTarget = float.PositiveInfinity;
                isTakingHit = false;
                isKnockbackRunning = false;
                isAttacking = false;
                nextAttackAt = 0f;
                lastRepositionTime = 0;
                consecutiveMeleeCount = 0;
                repositonDesireUntil = 0;
                stamina01 = 0;
                awayEscapeQuality01 = 0;
                rightEscapeQuality01 = 0;
                leftEscapeQuality01 = 0;

                return; 
            
            }

            target = enemyAI.Target;
            hasTarget = target != null;
            hasLos = enemyAI.HasLos;

            isTakingHit = enemyAI.IsTakingHit;
            isKnockbackRunning = enemyAI.IsKnockbackRunning;
            isAttacking = enemyAI.IsAttacking;

            if(hasTarget == true)
            {
                disanceToTarget = Vector3.Distance(enemyAI.transform.position, target.position);
            }
            else
            {
                disanceToTarget = float.PositiveInfinity;
            }

            lastRepositionTime = enemyAI.LastRepositionTime;
            consecutiveMeleeCount = enemyAI.ConsecutiveMeleeCount;
            repositonDesireUntil = enemyAI.RepositionBackstepDesireUntil;
            stamina01 = enemyAI.Stamina01;
            awayEscapeQuality01 = enemyAI.AwayEscapeceQuality01;
            rightEscapeQuality01 = enemyAI.RightEscapeceQuality01;
            leftEscapeQuality01 = enemyAI.LeftEscapeceQuality01;

        }

    }

}
