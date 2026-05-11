using UnityEngine;
using System.Collections;
namespace up.AI.Combat
{
    public interface ICombatAction
    {
        string Name {get;}

        bool CanExecute(CombatContext ctx);

        float Score(CombatContext ctx);

        IEnumerator Execute(CombatContext ctx);

        float DecisionDelay { get;}

    }

}


