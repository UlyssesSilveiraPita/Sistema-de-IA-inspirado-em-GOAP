using System.Collections;
using UnityEngine;

namespace up.AI.Combat
{
    [CreateAssetMenu(fileName = "New Actions",menuName = "up/debug", order = 0)]

    public class CombatAct_Debug : CombatActionSO
    {
        public override bool CanExecute(CombatContext ctx)
        {
            return true;    
        }

        public override IEnumerator Execute(CombatContext ctx)
        {
            yield return ctx.enemyAI.DebugActions();
        }

        public override float Score(CombatContext ctx)
        {
            return FinalizeScore(1);
        }
    }

}
