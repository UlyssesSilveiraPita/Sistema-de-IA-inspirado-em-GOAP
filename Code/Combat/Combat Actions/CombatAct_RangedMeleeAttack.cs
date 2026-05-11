using UnityEngine;
using System.Collections;

namespace up.AI.Combat
{

    [CreateAssetMenu(fileName = "CombatAct_RangedMeleeAttack", menuName = "up/AI/Combat Actions/CombatAct_RangedMeleeAttack")]

    public class CombatAct_RangedMeleeAttack : CombatActionSO
    {
        [Header("Attack")]
        public float executeRange = 1.5f;
        public float extraRangeTolerance = 0.2f;

        [Header("Anim")]
        public string animatorTrigger = "Attack_01";

        [Header("Rotation")]
        public float maxTurnTime = 0.75f;
        public float turnSpeedDegPerSec = 480f;


        [Header("Preferred Distance (Score)")]
        public float preferredMinDistance = 1.2f;
        public float preferredMaxDistance = 3.2f;

        [Header("Stamina Scoring")]
       
        [Range(0f, 1f)] public float staminaCritical = 0.35f;
        [Range(0f, 1f)] public float staminaComfort = 0.60f;


        public override bool CanExecute(CombatContext ctx)
        {
            if (ctx == null) { return false; }
            if (ctx.enemyAI == false) { return false; }
            if (ctx.agent == null) { return false; }

            if (ctx.target == false) { return false; }
            if (ctx.isAttacking == true) {return false; }
            if (ctx.isTakingHit == true) { return false; }
            if (ctx.isKnockbackRunning == true) { return false; }

            if(ctx.disanceToTarget < preferredMinDistance) return false;
            if(ctx.disanceToTarget > preferredMaxDistance) return false;

            //float maxRange = Mathf.Max(0.001f, executeRange + extraRangeTolerance);
            //if (ctx.disanceToTarget > maxRange) return false;

            return true;
        }

        public override IEnumerator Execute(CombatContext ctx)
        {
            if (ctx == null) { yield break; }
            if (ctx.enemyAI == null) { yield break; }
            if (ctx.hasTarget == false) { yield break; }

            float maxRange = Mathf.Max(0.001f, executeRange + extraRangeTolerance);
            //if (ctx.disanceToTarget > maxRange) return false;

            yield return ctx.enemyAI.RangedMeleeAttackRoutine(ctx.target, animatorTrigger, decisionDelay, maxTurnTime, turnSpeedDegPerSec, maxRange);

        }

        public override float Score(CombatContext ctx)
        {
            if (ctx == null) return float.NegativeInfinity;
            if (ctx.enemyAI == null) return float.NegativeInfinity;
            if (ctx.hasTarget == false) return float.NegativeInfinity;

            float score01 = 0f;
            float dist = ctx.disanceToTarget;

            //if (dist >= preferredMinDistance && dist <= preferredMaxDistance)
            //{
            //    float mid = (preferredMinDistance + preferredMaxDistance) * 0.5f;
            //    float half = Mathf.Max(0.001f, (preferredMaxDistance - preferredMinDistance) * 0.5f);
            //    float diff = Mathf.Abs(mid - half);

            //    score01 = 1f - Mathf.InverseLerp(0f, half, diff);
            //}
            //else
            //{
            //    score01 = 0;
            //}

            //float scoreDist01 = 0f;

            //if(dist < preferredMaxDistance)
            //{
            //    scoreDist01 = 0;
            //}
            //else if(dist > preferredMaxDistance)
            //{
            //    scoreDist01 = 0f;
            //}
            //else
            //{
            //    scoreDist01 = baseCost * scoreMultiplier;
            //}

            float scoreDist01 = 0f;

            if (dist >= preferredMinDistance && dist <= preferredMaxDistance)
            {
                float mid = (preferredMinDistance + preferredMaxDistance) * 0.5f;
                float half = (preferredMaxDistance - preferredMinDistance) * 0.5f;

                float diff = Mathf.Abs(dist - mid);

                scoreDist01 = 1f - Mathf.InverseLerp(0f, half, diff);
            }
            else
            {
                scoreDist01 = 0f;
            }

            scoreDist01 = Mathf.Clamp01(scoreDist01);

            float s = Mathf.Clamp01(ctx.stamina01);
            float lowStamina01 = 1f - Mathf.InverseLerp(staminaCritical, staminaComfort, s);

            float away = Mathf.Clamp01(ctx.awayEscapeQuality01);
            float left = 0; //Mathf.Clamp01(ctx.leftEscapeQuality01);
            float right = 0; //Mathf.Clamp01(ctx.rightEscapeQuality01);

            float bestEscape01 = Mathf.Max(away, left, right);
            float scoreTrapped01 = Mathf.Clamp01(1f - bestEscape01);

            float factor = Mathf.Max(lowStamina01, scoreTrapped01);
            score01 = scoreDist01 * factor;
            score01 = Mathf.Clamp01(score01);

            return FinalizeScore(score01);
        }
    }
}
