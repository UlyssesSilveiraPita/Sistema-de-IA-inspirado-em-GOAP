using UnityEngine;
using System.Collections;
using up.AI.Utils;

namespace up.AI.Actions
{

    [CreateAssetMenu(fileName = "Act_LookAround", menuName = "up/AI/Actions/Act_LookAround")]
    public class Act_LookAround : AIActionsSO
    {
        [Header("Rotacao")]
        public float yaw = 60f;
        public float rotationSpeed = 180f; // graus/s
        public float pauseAtExtremes = 0.2f;

        public override IEnumerator Execute(EnemyAI ai)
        {
            yield return ai.LookAroundRoutine(yaw, rotationSpeed, pauseAtExtremes);
        }

    }


}
