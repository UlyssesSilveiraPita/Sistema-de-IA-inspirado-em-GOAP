using UnityEngine;
using System.Collections;
using up.AI.Utils;
using up.AI.Waypoints;

namespace up.AI.Actions
{

    [CreateAssetMenu(fileName = "Act_WaypointNavigation", menuName = "up/AI/Actions/Act_WaypointNavigation")]
    public class Act_WaypointNavigation : AIActionsSO
    {
        public WaypointNavigationConfig navigationConfig;

        public override IEnumerator Execute(EnemyAI ai)
        {

            yield return ai.WaypointNavigationRoutine(navigationConfig);
        }
    }


}
