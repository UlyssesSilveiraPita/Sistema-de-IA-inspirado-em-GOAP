using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using up.AI.Utils;
using up.AI.Actions;

namespace up.AI.Waypoints
{
    public class WaypointNode : MonoBehaviour
    {
        public List<RouteLink> routeLinks = new List<RouteLink>();
        public WaypointsGroup ownerGroup;

        [Header("Sub-acoes por waypoint (opcional)")]
        public ActionProfile onReachWaypointProfile;

        public bool isSubNode;

        private void Awake()
        {
            ownerGroup ??= GetComponentInParent<WaypointsGroup>();

        }

#if UNITY_EDITOR

        [Header("Debug / Gizmos")]
        public Color linksColor = Color.black;

        private void OnDrawGizmos()
        {
            if(ownerGroup.drawPathGizmo == false) { return; }

            if(routeLinks == null || routeLinks.Count == 0 ) { return; }

            Gizmos.color = linksColor;
            NavMeshPath path = new NavMeshPath();

            foreach(var link in routeLinks)
            {
                if(link == null || link.targetGroup == null) { continue; }

                var points = link.targetGroup.Points;
                if(points == null || points.Count == 0) { continue; }

                int idx = Mathf.Clamp(link.targetIndex, 0, points.Count - 1);
                var targetNode = points[idx];

                if(targetNode == null) { continue; }

                Vector3 start = transform.position;
                Vector3 end = targetNode.transform.position;

                AIUtils.DrawNavPathGizmo(start, end, path);
                DrawArrow(start, end);

            }

        }

        private void DrawArrow(Vector3 start, Vector3 end, float headLength = 0.5f, float headAngle = 25f)
        {
            Vector3 direction = (end - start).normalized;
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + headAngle, 0) * Vector3.forward;

            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - headAngle, 0) * Vector3.forward;

            Gizmos.DrawLine(end, end + right * headLength);
            Gizmos.DrawLine(end, end + left * headLength);
        }


#endif

    }
}