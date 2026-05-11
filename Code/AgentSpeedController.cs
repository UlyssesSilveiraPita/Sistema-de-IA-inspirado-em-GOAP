using UnityEngine;
using UnityEngine.AI;

namespace up.AI
{
    [System.Serializable]
    public class AgentSpeedController
    {
        [Header("Rates (m/s)")]
        public float accel = 8f;
        public float decel = 12f;

        [Header("Stop behavior")]
        public bool setIsStoppedOnStop = true;
        public float stopSpeedEpsilon = 0.05f;

        public MoveMode CurrentMode { get; private set; } = MoveMode.Stop;
        public float CurrentSpeed { get; private set; } = 0f;
        public float TargetSpeed { get; private set; } = 0f;

        public void SetMode(MoveMode mode, EnemyMovementConfig cfg)
        {
            CurrentMode = mode;

            switch(mode)
            {
                case MoveMode.Stop:
                    TargetSpeed = 0f;
                    break;

                case MoveMode.Walk:
                    TargetSpeed = cfg.walkSpeed;
                     break;

                case MoveMode.Run:
                    TargetSpeed = cfg.runSpeed;
                    break;

                case MoveMode.Sprint:
                    TargetSpeed = cfg.sprintSpeed;
                    break;


            }

        }

        public void Tick(NavMeshAgent agent, float deltaTime)
        {
            if(agent == null) return;

            float rate = TargetSpeed >= CurrentSpeed ? accel : decel;
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, TargetSpeed, rate * deltaTime);
            agent.speed = CurrentSpeed;

            if(setIsStoppedOnStop == true)
            {
                bool shouldStop = TargetSpeed <= 0.0001f && CurrentSpeed <= stopSpeedEpsilon;
                agent.isStopped = shouldStop;
            }

        }

    }

}
