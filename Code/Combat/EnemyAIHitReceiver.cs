using UnityEngine;
using up.AI.Utils;
using up.DamageSystem;

namespace up.AI
{
    public class EnemyAIHitReceiver : MonoBehaviour, IHitReceiver
    {
        [SerializeField] private EnemyAI enemyAI;

        public void OnHit(HitContext ctx)
        {
            if (enemyAI == null) return;

            enemyAI.ReceiveHit(ctx);
        }

        private void Awake()
        {
            enemyAI = GetComponent<EnemyAI>();
        }

    }
}