using UnityEngine;
using System.Collections.Generic;

namespace up.AI.Actions
{
    [CreateAssetMenu(fileName = "ActionProfile", menuName = "up/AI/Profiles/ActionProfile")]
    public class ActionProfile : ScriptableObject
    {
        public List<ActionEntry> actions = new List<ActionEntry>();
        public bool randomOrder = false;
        
    }

}
