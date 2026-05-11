using System.Collections;
using UnityEngine;
using up.AI.Utils;

namespace up.AI.Actions
{

    [CreateAssetMenu(fileName = "Act_Static", menuName = "up/AI/Actions/Act_Static")]
    public class Act_Static : AIActionsSO
    {

        [Header("Tempo parado")]
        [Min(0f)]
        public float minDuration = 1f;

        [Min(0f)]
        public float maxDuration = 3f;

        

        public override IEnumerator Execute(EnemyAI ai)
        {
            float duration = Random.Range(minDuration, maxDuration);
            yield return ai.StaticRoutine(duration);
        }
    }


}

