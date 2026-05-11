using System.Collections;
using UnityEngine;

namespace up.AI.Combat
{
    [CreateAssetMenu(fileName = "CombatAct_MeleeAttack", menuName = "up/AI/Combat Actions/CombatAct_MeleeAttack")]


    public class CombatAct_MeleeAttack : CombatActionSO
    {
        [Header("Attack")]
        public float executeRange = 1.5f;
        public float extraRangeTolerance = 0.2f;

        [Header("Anim")]
        public string animatorTrigger = "Attack_01";

        [Header("Rules")]
        public bool reguiresLos = true;

        [Header("Backstep")]
        public float backstepChance = 0.35f;
        public float backstepDesireDuration = 0.9f;

        [Header("Preferred Distance (Score)")]
        public float preferredMinDistance = 0.0f;
        public float preferredMaxDistance = 1.6f;


        public override bool CanExecute(CombatContext ctx)
        {
            if(ctx == null) return false;
            if(ctx.enemyAI == false) return false;
            if(ctx.agent == null) return false;
            
            if(ctx.target == false) return false;   
            if(ctx.isAttacking == true) return false;
            if(ctx.isTakingHit == true) return false;
            if(ctx.isKnockbackRunning == true) return false;

            float maxRange = Mathf.Max(0.001f, executeRange + extraRangeTolerance);
            if(ctx.disanceToTarget >  maxRange) return false;   

            if(reguiresLos == true && ctx.hasLos == false) return false;    

            return true;
        }

        public override IEnumerator Execute(CombatContext ctx)
        {
            if (ctx == null) yield break;
            if (ctx.enemyAI == null) yield break;
            if (ctx.hasTarget == false) yield break;

            yield return ctx.enemyAI.MeleeAttackRoutine(ctx.target, animatorTrigger, decisionDelay, backstepChance, backstepDesireDuration);
        }

        public override float Score(CombatContext ctx)
        {
            if(ctx == null) return float.NegativeInfinity;
            if(ctx.enemyAI == null) return float.NegativeInfinity;
            if(ctx.hasTarget == false) return float.NegativeInfinity;

            float score01 = 0;
            float dist = ctx.disanceToTarget;

            if(dist >= preferredMinDistance && dist <= preferredMaxDistance)
            {
                float mid = (preferredMinDistance + preferredMaxDistance) * 0.5f;
                float half = Mathf.Max(0.001f, (preferredMaxDistance - preferredMinDistance) * 0.5f);
                float diff = Mathf.Abs(mid - half);

                score01 = 1f - Mathf.InverseLerp(0f,half,diff);
            }
            else
            {
                score01 = 0;
            }


            return FinalizeScore(score01);
        }
    }
}