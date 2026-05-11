using System.Collections;
using UnityEngine;


namespace up.AI.Combat
{
    public abstract class CombatActionSO : ScriptableObject, ICombatAction
    {
        [Header("Meta")]
        public string actionName = "New Combat Action";
        [Header("Timing")]
        [Min(0f)] public float decisionDelay = 0f;

        [Header("Scoring")]
        [Min(0f)] public float baseCost = 1f;
        [Min(0f)] public float scoreMultiplier = 1f;

        [SerializeField] private float scoreJitter = 0.05f;
        [SerializeField] private bool useScoreJitter = true;

        public string Name => actionName;

        public float DecisionDelay => decisionDelay;

        public abstract bool CanExecute(CombatContext ctx);

        public abstract IEnumerator Execute(CombatContext ctx);


        public abstract float Score(CombatContext ctx);

        protected float FinalizeScore(float score01)
        {
            if (float.IsNaN(score01) || float.IsInfinity(score01))
            {
                return float.NegativeInfinity;
            }

            if (score01 <= 0f)
            {
                return 0f;
            }

            score01 = Mathf.Clamp01(score01);

            float score = score01 * baseCost * scoreMultiplier;

            if(useScoreJitter == true && scoreJitter > 0f)
            {
                float mul = UnityEngine.Random.Range(1f - scoreJitter, 1f + scoreJitter);
                score *= mul;
            }

            return score;
        }

        //protected float FinalizeScore(float score01)
        //{
        //    if(float.IsNaN(score01) || float.IsInfinity(score01))
        //    {
        //        return float.NegativeInfinity;
        //    }

        //    if(score01 < 0f) { return 0f; }

        //    float score = score01 * baseCost * scoreMultiplier;
        //    return score;

        //}


        
    }
}

