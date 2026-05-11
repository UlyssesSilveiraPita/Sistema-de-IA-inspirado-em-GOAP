using System.Collections;
using UnityEngine;
using up.AI.Utils;

namespace up.AI.Actions
{

    [CreateAssetMenu(fileName = "Act_Wander", menuName = "up/AI/Actions/Act_Wander")]
    public class Act_Wander : AIActionsSO
    {
        [Header("Distancia de exploracao por passo")]
        [Min(0.1f)]
        public float stepDistanceMin = 1f;

        [Min(0.1f)]
        public float stepDistanceMax = 4f;

        [Min(0.5f)]
        public float maxSampleRadius = 10f;

        public bool limitDistanceFromStart = true;

        public float maxDistanceFromStart = 10f;

        public float stopDistance = 0.2f;


        public override IEnumerator Execute(EnemyAI ai)
        {

            float explorationRadius = Random.Range(stepDistanceMin, stepDistanceMax);

            explorationRadius = Mathf.Min(explorationRadius, maxSampleRadius);

            yield return ai.WanderRoutine(explorationRadius, limitDistanceFromStart, maxDistanceFromStart, stopDistance);
        }

      
    }



}
