using System.Collections;
using UnityEngine;

namespace up.AI.Combat
{
    [CreateAssetMenu(fileName = "CombatAct_Approch", menuName = "up/AI/Combat Actions/CombatAct_Approch")]

    public class CombatAct_Approch : CombatActionSO
    {
        public MoveMode moveMode;

        [Header("Preferred Distance (Score)")]
        public float preferredMinDistance = 0.8f;
        public float preferredMaxDistance = 3.1f;
        public float chaseTimeout = 0.2f;

        public override bool CanExecute(CombatContext ctx)
        {
            if(ctx == null) return false;
            if(ctx.enemyAI == false) return false;
            if(ctx.agent == null) return false;
            
            if(ctx.target == false) return false;   
            if(ctx.isAttacking == true) return false;
            if(ctx.isTakingHit == true) return false;
            if(ctx.isKnockbackRunning == true) return false;

            return true;
        }

        public override IEnumerator Execute(CombatContext ctx)
        {
            if (ctx == null) yield break;
            if (ctx.enemyAI == null) yield break;
            if (ctx.hasTarget == false) yield break;

            yield return ctx.enemyAI.ChaseTargetRoutine(ctx.target, preferredMinDistance, chaseTimeout, moveMode);
        }

        public override float Score(CombatContext ctx)
        {
            if(ctx == null) return float.NegativeInfinity;
            if(ctx.enemyAI == null) return float.NegativeInfinity;
            if(ctx.hasTarget == false) return float.NegativeInfinity;


            float score01 = 0;
            float dist = ctx.disanceToTarget;

            if (dist >= preferredMinDistance && dist <= preferredMaxDistance)
            {
                float mid = (preferredMinDistance + preferredMaxDistance) * 0.5f;
                float half = Mathf.Max(0.001f, (preferredMaxDistance - preferredMinDistance) * 0.5f);
                float diff = Mathf.Abs(mid - half);

                score01 = 1f - Mathf.InverseLerp(0f, half, diff);
            }
            else
            {
                score01 = 0;
            }


            return FinalizeScore(score01);
        }
    }
}