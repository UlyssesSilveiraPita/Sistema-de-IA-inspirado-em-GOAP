using System;
using System.Collections;
using UnityEngine;
using up.AI.Utils;

namespace up.AI.Perception
{
    public class EnemyPerception : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private EnemyAI enemyAI;
        [SerializeField] private Transform eye;
        [SerializeField] private Transform player;



        public EnemyAI EnemyAI => enemyAI;
        public Transform Eye => eye;

        [Header("Scan")]
        [SerializeField, Min(0.02f)]
        private float scanInterval = 0.1f;

        //Eventos
        public event Action<Transform> OnPrimaySight;
        public event Action<Transform> OnPeripheralSight;
        public event Action LostSight;

        //Runtime
        private Coroutine _scanRoutine;
        public EnemyConfig cfg;

        private enum VisionState
        {
            None,
            Privary,
            Peripheral
        }

        private VisionState _currentVisionState = VisionState.None;

        private void Awake()
        {
            enemyAI ??= GetComponent<EnemyAI>();    

            cfg = enemyAI.config;

            if(player == null )
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if(go != null )
                {
                    player = go.transform;
                }
            }
        }

        private void OnEnable()
        {
            StartScan();
        }
        private void OnDisable()
        {
            StopScan();
        }

        private void StartScan()
        {
            if (enemyAI == null || enemyAI.config == null || eye == null) { return; }

            if (_scanRoutine != null) { return; }
            _scanRoutine = StartCoroutine(ScaneLoopCorroutine());
        }

        private void StopScan()
        {
            if(_scanRoutine == null) { return; }
            StopCoroutine(_scanRoutine);
            _scanRoutine = null;
        }

        private void UpdateVision()
        {
            if(player == null) 
            { 
                SetVisionState(VisionState.None);
                return; 
            }

            //pega a distancia e a Direcao para o alvo
            Vector3 toPlayer =  player.position - transform.position;

            float dist = toPlayer.magnitude;
            if(dist > cfg.viewRadius)
            {
                SetVisionState(VisionState.None);
                return;
            }

            Vector3 dirToPlayer = toPlayer.normalized;

            bool hasObstacle = Physics.Raycast(eye.position, dirToPlayer, dist, cfg.obstacleMasck, QueryTriggerInteraction.Ignore);
            
            if(hasObstacle == true)
            {
                SetVisionState(VisionState.None);
                return;
            }

            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            if(angle <= cfg.viewAlgle * 0.5f)
            {
                Debug.DrawRay(eye.position, dirToPlayer * dist, Color.green, scanInterval);   
                SetVisionState(VisionState.Privary);
            }
            else if(angle <= cfg.viewPeripheralAngle * 0.5f)
            {
                Debug.DrawRay(eye.position, dirToPlayer * dist, Color.white, scanInterval);
                SetVisionState(VisionState.Peripheral);
            }
            else
            {
                Debug.DrawRay(eye.position, dirToPlayer * dist, Color.red, scanInterval);
                SetVisionState(VisionState.None);
            }

        }

        private void SetVisionState(VisionState newState)
        {
            if(newState == _currentVisionState) {  return; }

            switch(newState)
            {
                case VisionState.None:
                    LostSight?.Invoke();

                    break;
                case VisionState.Privary:
                    OnPrimaySight?.Invoke(player);

                    break;

                case VisionState.Peripheral:
                    OnPeripheralSight?.Invoke(player);  

                    break;
            }

            _currentVisionState = newState;
        }

        private IEnumerator ScaneLoopCorroutine()
        {
            while(true)
            {
                UpdateVision();
                yield return new WaitForSeconds(scanInterval);
            }

        }

        public Vector3 DirForAngle(float angleInDegress)
        {
            angleInDegress += transform.eulerAngles.y;
            float rad = angleInDegress * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad), 0f ,Mathf.Cos(rad));   

        }

        /*
        private void OnDrawGizmosSelected()
        {
            if(enemyAI == null || enemyAI.config == null || eye == null) return;

            var cfg = enemyAI.config;

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(eye.position, cfg.viewRadius);

            //Cone Principal
            Vector3 angleA = DirForAngle(-cfg.viewAlgle * 0.5f);
            Vector3 angleB = DirForAngle(cfg.viewAlgle * 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(eye.position, eye.position + angleA * cfg.viewRadius);
            Gizmos.DrawLine(eye.position, eye.position + angleB * cfg.viewRadius);

            //Cone Periferico
            Vector3 angleAp = DirForAngle(-cfg.viewPeripheralAngle * 0.5f);
            Vector3 angleBp = DirForAngle(cfg.viewPeripheralAngle * 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eye.position, eye.position + angleAp * cfg.viewRadius);
            Gizmos.DrawLine(eye.position, eye.position + angleBp * cfg.viewRadius);
        }
        */

    }
}