using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace up.AI.Actions
{
    [System.Serializable]
    public class ActionEntry 
    {
        public AIActionsSO action;

        [Header("Repeticoes")]
        [Min(1)] public int minRepeats = 1;
        [Min(1)] public int maxRepeats = 1;

        [Header("Probabilidade")]
        [Range(0.1f, 1f)]
        public float probability = 1f;

    }

}

