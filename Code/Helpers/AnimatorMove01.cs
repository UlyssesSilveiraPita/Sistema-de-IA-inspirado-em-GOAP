using UnityEngine;
using UnityEngine.AI;

namespace up.AI.Helpers
{
    public static class AnimatorMove01
    {
        public static float CalcMove01(NavMeshAgent agente, float walkSpeed, float runSpeed, float sprintSpeed, float current01, float walkT = 0.33f, float runT = 0.66f, float damp = 12)
        {
            if(agente == null) return 0f;

            float speed = agente.velocity.magnitude;
            if(speed < 0.05f)
            {
                speed = 0f;    
            }
            float w = Mathf.Max(0.01f, walkSpeed);
            float r = Mathf.Max(0.01f, runSpeed);
            float s = Mathf.Max(0.01f, sprintSpeed);

            walkT = Mathf.Clamp01(walkT);
            runT = Mathf.Clamp(runT, walkT + 0.01f, 0.99f);

            float target01;

            if(speed <= w)
            {
                target01 = Mathf.InverseLerp(0f,w, speed) * walkT;
            }
            else if(speed <= r)
            {
                float t = Mathf.InverseLerp(w, r, speed);
                target01 = Mathf.Lerp(walkT, runT, t);
            }
            else
            {
                float t = Mathf.InverseLerp(r, s, speed);
                target01 = Mathf.Lerp(runT, 1f, t);
            }


            target01 = Mathf.Clamp01(target01);
            float lerpT = 1f - Mathf.Exp(-damp * Time.deltaTime);
            return Mathf.Lerp(current01, target01, lerpT);
        }

    }


}
