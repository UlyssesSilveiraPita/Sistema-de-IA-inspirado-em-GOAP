using System.Collections;
using UnityEngine;
using up.AI.Utils;

namespace up.AI.Actions
{
    public abstract class AIActionsSO : ScriptableObject
    {
        public abstract IEnumerator Execute(EnemyAI ai);
        
    }


}

