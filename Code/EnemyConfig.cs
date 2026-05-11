using UnityEngine;
using up.AI.Actions;

namespace up.AI
{

    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "up/AI/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public AIRationality rationality;
        public ActionProfile actionProfile;

        //============ Decision Timing =============\\
        [Header("Decision Timing")]
        public float minDecisionDelay = 0.5f;
        public float maxDecisionDelay = 2f;

        //========== Movement ==========\\
        [Header("Movement")]
        public EnemyMovementConfig movement;

        /*
        [Header("Look Around")]
        [Range(0f, 90f)] public float lookAroundYawMax = 25f;
        [Min(0f)] public float lookAroundDuration = 1.25f;
        [Range(0f, 5f)] public float maxDistanceToLookAraound = 2f;
        */

        //============ Perception =============\\
        [Header("Percepton")]
        [Range(3f, 30f)] public float viewRadius = 12f;
        [Range(0f, 360f)] public float viewAlgle = 45f;
        [Range(0f, 360f)] public float viewPeripheralAngle = 110f;

        [Min(0.1f)] public float confirmSightTime = 0.75f;
        public LayerMask tagertMask;
        public LayerMask obstacleMasck;


        //=========== Metodos Auxiliares ==\\
        public float GetDecisionDelay()
        {
            return Random.Range(minDecisionDelay, maxDecisionDelay);
        }



    }

    [System.Serializable]

    public class EnemyMovementConfig
    {
        public float walkSpeed;
        public float runSpeed;
        public float sprintSpeed;
    
    }


}
