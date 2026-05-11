using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace up.AI.Combat
{
    [CreateAssetMenu(fileName = "Act_ChaseTarget", menuName = "up/AI/Combat Actions/Act_ChaseTarget")]


    public class Act_ChaseTarget : CombatActionSO
    {
        public MoveMode moveMode;

        [Header("Config")]
        public float desiredRange = 1.6f;
        public float extraRangeTolerance = 0.25f;
        public float chaseTimeout = 0.6f;
        public float maxConsiderDistance = 8f;


        public override bool CanExecute(CombatContext ctx)
        {
            if(ctx == null) return false;
            if(ctx.enemyAI == false) return false;
            if(ctx.agent == null) return false;
            
            if(ctx.target == false) return false;   
            if(ctx.isAttacking == true) return false;
            if(ctx.isTakingHit == true) return false;
            if(ctx.isKnockbackRunning == true) return false;

            float stopAt = desiredRange + extraRangeTolerance;
            if(ctx.disanceToTarget <= stopAt) return false; 

            return true;
        }

        public override IEnumerator Execute(CombatContext ctx)
        {
            if (ctx == null) yield break;
            if (ctx.enemyAI == null) yield break;
            if (ctx.hasTarget == false) yield break;

            yield return ctx.enemyAI.ChaseTargetRoutine(ctx.target, desiredRange, chaseTimeout, moveMode);
        }

        public override float Score(CombatContext ctx)
        {
            if(ctx == null) return float.NegativeInfinity;
            if(ctx.enemyAI == null) return float.NegativeInfinity;
            if(ctx.hasTarget == false) return float.NegativeInfinity;

            float d = ctx.disanceToTarget;
            float t = Mathf.Clamp01(Mathf.InverseLerp(desiredRange,maxConsiderDistance,d)); // 0.. 1


            return FinalizeScore(t);
        }
    }
}