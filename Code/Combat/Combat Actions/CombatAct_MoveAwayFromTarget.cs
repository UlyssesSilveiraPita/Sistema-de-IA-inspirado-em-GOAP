using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace up.AI.Combat
{
    [CreateAssetMenu(fileName = "CombatAct_MoveAwayFromTarget", menuName = "up/AI/Combat Actions/CombatAct_OpenRangeForRangeAttack")]


    public class CombatAct_MoveAwayFromTarget : CombatActionSO
    {
        [Header("Scoring")]
        [Range(0f, 0.2f)] public float baseScore = 0.02f;
        [Range(0f, 1f)] public float distanceWeight = 0.95f;
        //[Range(0f, 0.15f)] public float scoreJitter = 0.05f;

        [Header("Rules")]
        [Min(0f)] public float cooldown = 0.35f;

        [Header("Range Goal")]
        [Min(0.1f)] public float desiredMinDistance = 8f;
        [Min(0f)] public float arrivalTolerance = 0.25f;

        [Header("Timing")]
        [Min(0.1f)] public float timeOut = 1.5f;
        [Min(0.02f)] public float repathInterval = 0.12f;

        [Header("Move")]
        public MoveMode moveMode = MoveMode.Run;

        [Header("Rotation")]
        [Min(0f)] public float turnSpeedDegPerSec = 360f;
        public float maxTurnTime = 0.75f;

        [Header("Stamina Scoring")]
        public float minStaminaToScore = 0.15f;
        public float comfortableStamina = 0.60f;
        public float staminaScoreFloor = 0.30f;


        public override bool CanExecute(CombatContext ctx)
        {
            if(ctx == null) return false;
            if(ctx.enemyAI == false) return false;
            if(ctx.agent == null) return false;
            
            if(ctx.target == false) return false;   
            if(ctx.isAttacking == true) return false;
            if(ctx.isTakingHit == true) return false;
            if(ctx.isKnockbackRunning == true) return false;

            float now = Time.time;
            float cd = Mathf.Max(0f, cooldown);
            if(now <= ctx.lastRepositionTime + cd) { return false; }

            if(ctx.stamina01 <= minStaminaToScore) return false;   

            float d = ctx.disanceToTarget;
            float tol = Mathf.Max(0f, arrivalTolerance);
            if(d >= desiredMinDistance - tol)
            {
                return false;
            }    

            return true;
        }

        public override IEnumerator Execute(CombatContext ctx)
        {
            if (ctx == null) yield break;
            if (ctx.enemyAI == null) yield break;
            if (ctx.hasTarget == false) yield break;

            MoveRelativeToTargetData data = new MoveRelativeToTargetData
            {
                target = ctx.target,
                desiredMinDistance = desiredMinDistance,
                arrivalTolerance = arrivalTolerance,
                timeOut = timeOut,
                moveMode = moveMode,
                turnSpeedDegPerSec = turnSpeedDegPerSec,
                maxTurnTime = maxTurnTime,
                repathInterval = repathInterval,
                decisionDelay = decisionDelay,
            };


            yield return ctx.enemyAI.MoveAwayFromTargetRoutine(data);
        }

        public override float Score(CombatContext ctx)
        {
            if(ctx == null) return float.NegativeInfinity;
            if(ctx.enemyAI == null) return float.NegativeInfinity;
            if(ctx.hasTarget == false) return float.NegativeInfinity;

            if(ctx.isAttacking || ctx.isTakingHit || ctx.isKnockbackRunning) {  return 0f; }

            float d = ctx.disanceToTarget;
            float tol = Mathf.Max(0f, arrivalTolerance);

            if( d >= desiredMinDistance - tol) { return 0f; }

            float desired = Mathf.Max(0.1f, desiredMinDistance);
            float distance01 = Mathf.Clamp01(Mathf.InverseLerp(desired, 0f, d));

            float score01 = baseScore + (distanceWeight * distance01);

            // stamina factor
            float s = Mathf.Clamp01(ctx.stamina01);
            float stam01 = Mathf.InverseLerp(minStaminaToScore, comfortableStamina, s);

            stam01 *= stam01; // fator quadratico
            stam01 = Mathf.Clamp01(stam01);

            float stamMul = Mathf.Lerp(staminaScoreFloor, 1f, stam01);

            float awayQuality01 = ctx.awayEscapeQuality01;

            score01 *= stamMul *  awayQuality01;

            return FinalizeScore(score01);
        }
    }

    [System.Serializable]

    public struct MoveRelativeToTargetData
    {
        public Transform target;
        
        public float desiredMinDistance;
        public float arrivalTolerance;

        public float timeOut;
        public MoveMode moveMode;
        public float turnSpeedDegPerSec;
        public float maxTurnTime;

        public float repathInterval;

        public float decisionDelay;
        public RelativeMoveDirection moveDirection;

    }

}