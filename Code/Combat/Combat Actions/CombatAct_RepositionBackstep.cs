using System.Collections;
using UnityEngine;

namespace up.AI.Combat
{
    [CreateAssetMenu(fileName = "CombatAct_RepositionBackstep", menuName = "up/AI/Combat Actions/CombatAct_RepositionBackstep")]


    public class CombatAct_RepositionBackstep : CombatActionSO
    {
        [Range(0f, 0.2f)] public float desireBase = 0.02f;

        [Min(0.1f)] public float triggerDistance = 1.6f;
        [Min(0.1f)] public float safeDistance = 2.2f;

        [Range(0f, 1f)] public float desireWeight = 0.4f;
        [Range(0f, 1f)] public float distanceWeight = 0.30f;
        [Range(0f, 1f)] public float chainWeight = 0.30f;

        [Min(1)] public int attackToMaxChainFactor = 4;

        [Min(0f)] public float cooldown = 1.5f;

        [Min(0.1f)] public float backstepDistanceMin = 0.9f;
        [Min(0.1f)] public float backstepDistanceMax = 1.6f;

        [Min(0.1f)] public float timeOut = 1.1f;

        public MoveMode moveMode = MoveMode.Walk;

        [Min(30f)] public float turnSpeedDegPerSec = 360f;

        //[Range(0f, 0.15f)] public float scoreJitter = 0.05f;

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
            float cd = Mathf.Max(0, cooldown); // so para n ter valor negatico

            if(now < ctx.lastRepositionTime + cd) return false;

            return true;
        }

        public override IEnumerator Execute(CombatContext ctx)
        {
            if (ctx == null) yield break;
            if (ctx.enemyAI == null) yield break;
            if (ctx.hasTarget == false) yield break;

            float backstep = Random.Range(backstepDistanceMin, backstepDistanceMax);

            RepositionBackstepData data = new RepositionBackstepData
            {
                target = ctx.target,
                backstep = backstep,
                timeOut = timeOut,
                moveMode = moveMode,
                turnSpeedDegPerSec = turnSpeedDegPerSec,
                decisionDelay = decisionDelay
            };

            yield return ctx.enemyAI.RepositionBackstepRoutine(data);
        }

        public override float Score(CombatContext ctx)
        {
            if(ctx == null) return float.NegativeInfinity;
            if(ctx.enemyAI == null) return float.NegativeInfinity;
            if(ctx.hasTarget == false) return float.NegativeInfinity;

            float score01 = 0; 

            float now = Time.time;
            float desire01 = (now <= ctx.repositonDesireUntil) ? 1 : 0;

            float dist = ctx.disanceToTarget;
            float distance01 = Mathf.Clamp01(Mathf.InverseLerp(safeDistance, triggerDistance, dist));

            float chain01 = 0f;
            int chain = Mathf.Clamp(ctx.consecutiveMeleeCount -1, 0, attackToMaxChainFactor);
            if (chain > 0)
            {
                chain01 = Mathf.Clamp01( chain * (1f / attackToMaxChainFactor));
            }

            score01 = desireBase + (desireWeight * desire01) + (distanceWeight * distance01) + (chainWeight * chain01); 

            //if(scoreJitter > 0)
            //{
            //    score01 *= Random.Range(1f - scoreJitter, 1f + scoreJitter);
            //}

            return FinalizeScore(score01);
        }
    }

    [System.Serializable]

    public struct RepositionBackstepData
    {
        public Transform target;
        public float backstep;
        public float timeOut;
        public MoveMode moveMode;
        public float turnSpeedDegPerSec;
        public float decisionDelay;


    }

}