using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using up.AI.Utils;
using Unity.VisualScripting;


namespace up.AI.Waypoints
{
    public class WaypointsGroup : MonoBehaviour
    {
        [SerializeField] private List<WaypointNode> _points = new List<WaypointNode>(); 
        public List<WaypointNode> Points => _points;

        [Header("Debug / Gizmos")]
        public bool drawPathGizmo = true;
        public Color pathColor = Color.cyan;
        public float waypointRadius = 0.35f;

        public bool drawLoopGrizmo = true;

        private void OnDrawGizmos()
        {
            if (drawPathGizmo == false) { return; } 

            if(_points == null || _points.Count == 0) { return; }    

            Gizmos.color = pathColor;   

            foreach(var wp in _points)
            {
                if (wp == null) { continue; }

                if(wp.isSubNode == false)
                {
                    Gizmos.color = pathColor;
                    Gizmos.DrawSphere(wp.transform.position, waypointRadius);

                }
                else
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(wp.transform.position, 0.15f);

                }

            }

            NavMeshPath path = new NavMeshPath();
            
            for(int i = 0; i < _points.Count -1; i++)
            {
                var from = _points[i];
                var to = _points[i + 1];
                if(from == null || to == null) { continue; }

                AIUtils.DrawNavPathGizmo(from.transform.position, to.transform.position, path);
            }

            if (drawLoopGrizmo == true)
            {
                var last = _points[_points.Count - 1];
                var first = _points[0];

                if(last != null && first != null)
                {
                    AIUtils.DrawNavPathGizmo(last.transform.position, first.transform.position, path);
                }

            }

        }


    }

}

