using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace up.AI.Utils
{
    public class AIUtils 
    {
        
        public static Vector3 GetRandomPointOnNavMesh(Vector3 center, float radius)
        {
            Vector3 randonDirection = Random.insideUnitSphere * radius;
            randonDirection += center;

            NavMeshHit hit;
            if(NavMesh.SamplePosition(randonDirection, out hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return center;
        }

        public static void DrawNavPathGizmo(Vector3 start, Vector3 end, NavMeshPath reusablepath = null)
        {
            //se esqueda nao for null usa da esquerda se for null usa da direita (coalescencia nula, null coalescence)
            NavMeshPath path = reusablepath ?? new NavMeshPath();

            if(NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path) && path.corners != null && path.corners.Length > 1)
            {
                for(int i = 0; i < path.corners.Length - 1; i++  )
                {
                    Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                }
            }
            else
            {
                Gizmos.DrawLine(start, end);
            }
        }

        //metodo para retorno de posicao valida
        public static bool TryGetFreeDestination(Vector3 origin, Vector3 desired, int areaMask, out Vector3 finalDestination, float sampleRations = 0.3f, float stopBeforeObstacle = 0.15f, bool requiredCompletPath = true)
        {
            finalDestination = origin;
            Vector3 limitPoint = desired;

            Vector3 origenOnMesh = origin;

            if(NavMesh.Raycast(origin, desired, out var rayHit, areaMask) == true)
            {
                Vector3 dir = (desired - origin).normalized;
                limitPoint = rayHit.position - dir * Mathf.Max(0f, stopBeforeObstacle);
            }

            if(NavMesh.SamplePosition(limitPoint, out var finalHit, sampleRations, areaMask) == false)
            {
                return false;
            }

            Vector3 candidate = finalHit.position;

            if(requiredCompletPath == true)
            {
                var path = new NavMeshPath();
                if(NavMesh.CalculatePath(origin, candidate, areaMask, path) == false)
                {
                    return false;
                }
                if(path.status != NavMeshPathStatus.PathComplete)
                {
                    return false;
                }
            }

            finalDestination = candidate;
            return true;

            //if(NavMesh.SamplePosition(desired, out var desiredHit, sampleRations, areaMask) == false)
            //{
            //    return false;
            //}

            //Vector3 candidate = desiredHit.position;

            //if(NavMesh.Raycast(origin, candidate, out var rayHit, areaMask))
            //{
            //    Vector3 toCandidate = candidate - origin;
            //    toCandidate.y = 0f;

            //    if(toCandidate.sqrMagnitude < 0.0001f)
            //    {
            //        return false;
            //    }

            //    Vector3 dir = toCandidate.normalized;

            //    Vector3 before = rayHit.position - dir * Mathf.Max(0f, stopBeforeObstacle);

            //    if(NavMesh.SamplePosition(before, out var beforeHit, sampleRations, areaMask) == false)
            //    {
            //        return false;
            //    }

            //    candidate = beforeHit.position;

                
            //}

            //if (requiredCompletPath == true)
            //{
            //    var path = new NavMeshPath();
            //    if (NavMesh.CalculatePath(origin, candidate, areaMask, path) == false)
            //    {
            //        return false;
            //    }

            //    if (path.status != NavMeshPathStatus.PathComplete)
            //    {
            //        return false;
            //    }


            //}

            //finalDestination = candidate;


        }

    }


}
