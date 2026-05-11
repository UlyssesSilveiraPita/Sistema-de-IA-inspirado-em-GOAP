using NUnit.Framework.Internal.Commands;
using System.Collections;
using UnityEngine;


namespace up.AI.Combat
{
    [CreateAssetMenu(fileName = "CombatAct_BowAttack", menuName = "up/AI/Combat Actions/CombatAct_BowAttack")]

    public class CombatAct_BowAttack : CombatActionSO
    {

        #region Inspector - Ranged Setup

        [Header("Ranges")]
        [Min(0.1f)] public float minBowRange = 3.0f; // Distancai minima confortavel
        [Min(0.1f)] public float idealBowRange = 6.0f; // pico do score
        [Min(0.1f)] public float maxBowRange = 12.0f; // distancia maxima (acima disso o score cai para 0 )
        [Min(0f)] public float extraRangeTolerance = 0.25f; // tolerancia para evitar falhas por micro variacao

        [Header("LOS")]
        public bool requireLos = true; //versao simples: so atira com LOS

        [Header("Anim")]
        public string animatorTrigger = "BowShot"; // triggger do disparo no animator
        //[Min(0f)] public float releaseDelay = 0.2f; // tempo ate o release ( se voce usar animatio Event)
        //[Min(0f)] public float recoverTime = 0.1f; // Recuperacao apos disparo ( alem do decisionDelay)

        //[Header("Execution")]
        //[Min(0.05f)] public float maxExecutionTime = 1.0f; // Timeout de seguranca para nao travar caso algo de

        #endregion

        #region ICombatAction

        public override bool CanExecute(CombatContext ctx)
        {
            if (ctx == null) { return false; }
            if (ctx.enemyAI == null) { return false; }
            if (ctx.hasTarget == false) { return false; }

            // bloqueios basicos ( mesmo padrao conceitual do seu sistema
            if (ctx.isKnockbackRunning) { return false; }
            if (ctx.isTakingHit) { return false; }
            if (ctx.isAttacking) { return false; }

            if (requireLos && ctx.hasLos == false) { return false; }

            float d = ctx.disanceToTarget;

            // nao tenta atirar colado ( deixa keepDistance ou Melee assumir)
            if (d < minBowRange) { return false; }

            // nao tenta atirar muito longe (deixa chase/approach assumir)
            if (d > maxBowRange) { return false; }

            return true;

        }

        public override IEnumerator Execute(CombatContext ctx)
        {
            if (ctx == null) { yield break; }
            if (ctx.enemyAI == null) { yield break; }
            if (ctx.hasTarget == false) { yield break; }

            // revalida pre condicoes na execucao (robustez)
            if (requireLos && ctx.hasLos == false) { yield break; }

            float d = ctx.disanceToTarget;
            if (d < (minBowRange - extraRangeTolerance)) { yield break; }
            if (d > (maxBowRange + extraRangeTolerance)) { yield break; }

            yield return ctx.enemyAI.RangeAttackRoutine(ctx.target, animatorTrigger, decisionDelay);


            //// Interrompe movimento para estabilizar o Tiro
            //if (ctx.agent != null)
            //{
            //    ctx.agent.isStopped = true;
            //    ctx.agent.velocity = Vector3.zero;
            //    ctx.agent.ResetPath();
            //}

            //// matem rotacao sob contro do sistema ( LateUpdate decide agentDriven vs ManualLookAt)
            //ctx.enemyAI.SetRotationMode(RotationMode.ManualLookAt);

            //// dispara animacao ( projetil pode sair via animacao event)
            ////ctx.enemyAI.Animator.SetTrigger(animatorTrigger);

            //// Timeout de seguranca



            //throw new System.NotImplementedException();
        }

        public override float Score(CombatContext ctx)
        {
            if (ctx == null) { return float.NegativeInfinity; }
            if (ctx.enemyAI == null) { return float.NegativeInfinity; }
            if (ctx.hasTarget == false) { return float.NegativeInfinity; }

            // se nao pode executar, score deve ser - ( ou -inf dependendo do seu padrao).
            if (requireLos && ctx.hasLos == false) { return 0f; }
            if (ctx.isKnockbackRunning || ctx.isTakingHit || ctx.isAttacking) { return 0f; }

            float d = ctx.disanceToTarget;

            // faixa rigida ( versao simples )
            if (d < minBowRange) { return 0f; }
            if (d > maxBowRange) { return 0f; }

            // curva "sino" com pico em idealBowRange (similar ao Melee/aprouch)
            float min = Mathf.Max(0.01f, minBowRange);
            float max = Mathf.Max(min + 0.01f, maxBowRange);

            float ideal = Mathf.Clamp(idealBowRange, min,max);
            float half = Mathf.Max(0.001f, (max - min) * 0.5f);

            // usa um mid/half para normalizar a distancia em torno do idial
            // se ideal esta fora do centro, isso ainda funciona como uma campanula simples

            float diff = Mathf.Abs(d - ideal);

            float score01 = 1f - Mathf.InverseLerp(0f, half, diff);

            return FinalizeScore(score01);

        }

        #endregion

    }



}
