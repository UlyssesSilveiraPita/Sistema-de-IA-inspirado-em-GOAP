using UnityEngine;
using System.Collections.Generic;


namespace up.AI.Combat
{
    public class CombatPlanner
    {
       //readonly garante que a lista nao possa ser auterada sua referencia
        private readonly List<ICombatAction> _actions = new List<ICombatAction>();

        public CombatPlanner(IEnumerable<ICombatAction> actions)
        {


            if(actions == null) { return; }

            _actions.AddRange(actions); 

        }

        public ICombatAction ChooseBestAction(CombatContext ctx)
        {
            ICombatAction best = null;
            float bestScore = float.NegativeInfinity;    
            
            for(int i = 0; i < _actions.Count; i++)
            {
                ICombatAction action = _actions[i];

                if (action == null) { continue; }

                if(action.CanExecute(ctx) == false)
                {
                    continue;
                }

                float score = action.Score(ctx);

                Debug.Log($"Action {action.Name} - Score: {score}");

                if(score > bestScore)
                {
                    bestScore = score;
                    best = action;
                }
            }

            return best;
        }


    }

}
