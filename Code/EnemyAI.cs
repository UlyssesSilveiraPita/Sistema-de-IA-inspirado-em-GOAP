using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using up.AI.Actions;
using up.AI.Waypoints;
using up.AI.Perception;
using up.AI.Combat;
using up.AI.Helpers;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEditor;
using up.DamageSystem;


namespace up.AI.Utils
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("Ref")]
        public EnemyConfig config;
        public AIMode startAIMode = AIMode.World;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private EnemyPerception _Perception;
        [SerializeField] private Animator _animator;
        private AgentSpeedController _speedCtrl = new AgentSpeedController();

        [Header("World & Pathing")]
        [SerializeField] private WaypointManager _waypointManager;

        [Header("Rotation Controller")]
        [SerializeField] private RotationMode _rotationMode = RotationMode.AgentDriven;
        [SerializeField] private float _turnSpeed = 180f; // graus por segundo ( akuste fino )
        [SerializeField] private float _minLookDistance = 0.15f;
        [SerializeField] private float _manualLookDistance = 2.5f;
        private Vector3 _lookAtPath;
        private float _turnSpeedToPath;

        // Stamina Controller (Inspector)\\
        [Header("Stamina Controller")]
        [SerializeField] private float staminaMax = 100f;
        [SerializeField] private float staminaRegenPerSec = 5f;
        [SerializeField] private float staminaDrainRunPerSec = 10f;
        [SerializeField] private float staminaDrainSprintPerSec = 18f;

        [SerializeField] private float EscapeProbeDistance = 3f; // distacia a ser testada
        [SerializeField] private int EscapeProbeSteps = 3; // passos de fuga


        // "Hit / Knockback" (Inspector)\\
        [Header("Hit / Knockback")]
        [SerializeField] private string hitAnimatorTrigger = "Hit";
        private HitContext hitContext;


        [Header("RunTime")]
        private Vector3 startPosition;
        private Vector3 targetPosition;

        // Percepton / Target RunTime
        [Header("perception Runtime")]
        [SerializeField] private Transform _target;
        [SerializeField] private bool hasLos;
        [SerializeField] private bool hasPeripheralSight;

        //Combat Runtime
        bool _isTakingHit;
        bool _isKnockbackRunning;
        bool _isAttacking;
        float move01;

        [SerializeField] float _stamina;
        public float Stamina01 => Mathf.Clamp01( _stamina / staminaMax); // validacao clamo entre 0 e 1

        // EscapeQualities\\
        private float _awayEscapeQuality01;
        public float AwayEscapeceQuality01 => _awayEscapeQuality01;
        private float _rightEscapeQuality01;
        public float RightEscapeceQuality01 => _rightEscapeQuality01;
        private float _leftEscapeQuality01;
        public float LeftEscapeceQuality01 => _leftEscapeQuality01;

        //usado pela rotina de investigar
        private bool hasSuspicionPoint;
        private Vector3 lastSuspicionPoint;

        CombatPlanner combatPlanner;
        //public List<ICombatAction> actions = new List<ICombatAction>();

        public Transform Target => _target;
        public bool HasLos => hasLos;

        // Propriedades
        public bool IsTakingHit => _isTakingHit;
        public bool IsKnockbackRunning;
        public bool IsAttacking => _isAttacking;

        private AIMode _currentMode = AIMode.None;
        private Coroutine _currentCorroutine;

        //== COMBAT (COAP - like) == \\
        [Header("Combat (GOAP - like)")]
        [SerializeField] private List<CombatActionSO> combatActions = new List<CombatActionSO>();
        private float combatDecisionInterval = 0.25f;
        private CombatContext _combatContext;
        private CombatPlanner _combatPlanner;
        private float _nextDecisionTime;
        [SerializeField] private float combatStopDistance = 0.2f;

        private float _repositionBackstepDesireUntil;
        public float RepositionBackstepDesireUntil => _repositionBackstepDesireUntil;

        private float _lastRepositionTime;
        public float LastRepositionTime => _lastRepositionTime;

        private int _consecutiveMeleeCount;
        public int ConsecutiveMeleeCount => _consecutiveMeleeCount;

        private MoveMode _currentMoveMode;


        #region UNITY


        private void Awake()
        {
            _agent ??= GetComponent<NavMeshAgent>(); 
            _waypointManager ??= GetComponent<WaypointManager>();
            _Perception ??= GetComponent<EnemyPerception>();
        }

        void Start()
        {
            //StartCoroutine(Idle());
            startPosition = transform.position;
            // StartCoroutine(RunActionProfile(config.actionProfile));
            BuildCombatPlanner();
            SwitchMode(startAIMode);

            _stamina = staminaMax;

            //combatPlanner = new CombatPlanner();
        }

        private void OnEnable()
        {
            if(_Perception != null)
            {
                _Perception.OnPrimaySight += HandlePrimarySight;
                _Perception.OnPeripheralSight += HandlePeripheralSight;
                _Perception.LostSight += HandleLostSight;
            }
        }
        private void OnDisable()
        {
            if(_Perception != null)
            {
                _Perception.OnPrimaySight -= HandlePrimarySight;
                _Perception.OnPeripheralSight -= HandlePeripheralSight;
                _Perception.LostSight -= HandleLostSight;
            }
        }

        void Update()
        {
            _speedCtrl.Tick(_agent, Time.deltaTime);
            TickStamina(Time.deltaTime);

            move01 = AnimatorMove01.CalcMove01(_agent, config.movement.walkSpeed, config.movement.runSpeed, config.movement.sprintSpeed, move01);
            _animator.SetFloat("Move01", move01);

            if(_rotationMode == RotationMode.ManualLookAt || _rotationMode == RotationMode.LookAtTarget)
            {
                FaceTarget(_target, _turnSpeed);
            }
            else if(_rotationMode == RotationMode.LookAtPath)
            {
                FacePath(_lookAtPath, _turnSpeedToPath);
            }
        }

        private void LateUpdate()
        {
            if(Target == null || _agent == null) return;
            if(_rotationMode == RotationMode.None || _rotationMode == RotationMode.LookAtPath || _rotationMode == RotationMode.LookAtTarget) return;

            float dist = Vector3.Distance(transform.position, _target.position);

            if (dist <= _manualLookDistance)
            {
                if (_rotationMode != RotationMode.ManualLookAt)
                {
                    SetRotationMode(RotationMode.ManualLookAt);
                }
            }
            else
            {
                if (_rotationMode != RotationMode.AgentDriven)
                {
                    SetRotationMode(RotationMode.AgentDriven);
                }
            }

        }

        private void FixedUpdate()
        {
            UpdateEscapeQualities();


        }

        #endregion

        #region Movement & Rotation Public Controls & Stamina

        public void SetMoveMode(MoveMode mode)
        {
            _currentMoveMode = mode;
            _speedCtrl.SetMode(mode, config.movement);
        }

        public void SetRotationMode(RotationMode mode)
        {
            
            _rotationMode = mode;
            
            if(_agent != null)
            {
                _agent.updateRotation = (mode == RotationMode.AgentDriven);
            }
        }

        private void TickStamina(float dt)
        {
            if(dt <= 0) return;
            if(_agent == null) return;

            bool isMoving = _agent.velocity.sqrMagnitude > 0.0025f; // sqrMagnitude valores ao quadrado
            float drainPerSec = 0;

            if(isMoving == true)
            {
                switch (_currentMoveMode)
                { 
                    case MoveMode.Run:
                        drainPerSec = staminaDrainRunPerSec;
                        break;

                    case MoveMode.Sprint:
                        drainPerSec = staminaDrainRunPerSec;
                        break ;
                
                }

            }
            else
            {
                drainPerSec = 0;
            }

            float delta = 0;

            if(isMoving == true && (_currentMoveMode == MoveMode.Run || _currentMoveMode == MoveMode.Sprint))//consome
            {
                delta = -drainPerSec * dt;
            }
            else
            {
                delta = Mathf.Max(0f, staminaRegenPerSec) * dt;
            }

            _stamina = Mathf.Clamp (_stamina + delta, 0f, staminaMax);



        }


        #endregion

        private async Task UpdateEscapeQualities()
        {
            if(_agent == null || _target == null)
            {
                _awayEscapeQuality01 = 0f;
                _rightEscapeQuality01 = 0f;
                _leftEscapeQuality01 = 0f;
                return;
            }

            Vector3 myPos = transform.position;   
            Vector3 away = myPos - _target.position;
            away.y = 0;

            if(away.sqrMagnitude < 0.0001f) 
            {
                _awayEscapeQuality01 = 0f;
                _rightEscapeQuality01 = 0f;
                _leftEscapeQuality01 = 0f;
                return;
            }

            Vector3 awayDir = away.normalized;
            Vector3 leftDir = Vector3.Cross(Vector3.up, awayDir).normalized; // calcula nossa direcao direita
            Vector3 righDir = -leftDir;

            _awayEscapeQuality01 = ComputerEscapeQuality(myPos, awayDir);
            _rightEscapeQuality01 = ComputerEscapeQuality(myPos, righDir);
            _leftEscapeQuality01 = ComputerEscapeQuality(myPos, leftDir);

        }

        private float ComputerEscapeQuality(Vector3 myPos, Vector3 dir)
        {
            float  maxDist = Mathf.Max(0.5f, EscapeProbeDistance);
            float  steps = Mathf.Clamp(EscapeProbeSteps, 1, 8);

            float sampleRadius = 0.20f;
            float maxSnapFromProbe = 0.25f;
            float minDot = 0.95f;

            float bestContigousOkDist = 0;

            for(int i = 1; i <= steps ; i++)
            {
                float d = (maxDist / steps) * i;
                Vector3 probe = myPos + (dir * d);

                //Teste 1
                if(NavMesh.SamplePosition(probe, out var hit, sampleRadius, _agent.areaMask) == false)
                {
                    break;
                }

                //Test 2
                if((hit.position -  probe).sqrMagnitude > (maxSnapFromProbe * maxSnapFromProbe))
                {
                    break ;
                }

                //Test 3
                Vector3 toHit = hit.position - myPos;
                toHit.y = 0;

                if (toHit.sqrMagnitude < 0.0001f) break;

                float dot = Vector3.Dot(toHit.normalized, dir);

                if (dot < minDot) break;
                
                //Teste 4

                if(NavMesh.Raycast(myPos, hit.position, out var rayHit, _agent.areaMask) == true)
                {
                    break;
                }

                //Teste 5 - Regra mais dura ( Verificar se esta andando em linha reta)

                var path = new NavMeshPath();
                if(NavMesh.CalculatePath(myPos, hit.position, _agent.areaMask, path) == false || path.status != NavMeshPathStatus.PathComplete)
                {
                    break;
                }

                var corners = path.corners;
                if(corners == null || corners.Length > 2)
                {
                    break;
                }

                bestContigousOkDist = d;
            }

            return Mathf.Clamp01(bestContigousOkDist / maxDist);

        }


        private void FaceTarget(Transform target, float turnSpeedDegPerSec)
        {
            if(target == null) return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0;

            if(toTarget.sqrMagnitude < (_minLookDistance * _minLookDistance))
            {
                return;
            }

            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired , turnSpeedDegPerSec * Time.deltaTime);

        }

        private void FacePath(Vector3 target, float turnSpeedDegPerSec)
        {
            if (target == null) return;

            Vector3 toTarget = target - transform.position;
            toTarget.y = 0;

            if (toTarget.sqrMagnitude < (_minLookDistance * _minLookDistance))
            {
                return;
            }

            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turnSpeedDegPerSec * Time.deltaTime);

        }

        public void MarkReposition()
        {
            _lastRepositionTime = Time.time;
        }

        public void MarkMeleeAttack()
        {
            _consecutiveMeleeCount +=1;
        }

        public void ResetMeleeChain()
        {
            _consecutiveMeleeCount = 0;
        }

        private void RollRepositionBackstepDesire(float chance, float desiredDuration)
        {
            float c = Mathf.Clamp01(chance);
            float d = Mathf.Max(0f, desiredDuration);

            if(c <= 0 || d <= 0) return;

            if(Random.value <= c)
            {
                _repositionBackstepDesireUntil = Time.time + d;
            }
        }

        private void ResetBackstepDesire()
        {
            _repositionBackstepDesireUntil = 0;
        }

        #region Animation Event Callback

        public void OnAttackAnimationEnd()
        {
            _isAttacking = false;
        }

        public void OnHitAnimationEnd()
        {
            _isTakingHit = false;
        }

        #endregion

        #region Hit / Knockback

        public void ReceiveHit(HitContext ctx)
        {
            if (_agent == null) return;

            if (_isTakingHit == false)
            {
                _isTakingHit = true;
                _animator.SetTrigger(hitAnimatorTrigger);
            }
        }

        #endregion

        #region Perception




        private void HandlePrimarySight(Transform target)
        {
            if(target == null) {  return; }

            _target = target;
            hasLos = true;
            hasPeripheralSight = false;
        }

        private void HandlePeripheralSight(Transform target)
        {
            if (target == null) { return; }

            _target = target;
            hasLos = false;
            hasPeripheralSight = true;

            hasSuspicionPoint = true;
            lastSuspicionPoint = target.position;
        }

        private void HandleLostSight()
        {
            hasLos = false;
            hasPeripheralSight = false;
        }

        #endregion

        //== MODE SWTICH (WORLD <-> COMBAT) ==\\

        #region Mode Switch
        private void SwitchMode(AIMode newMode)
        {
            if(_currentMode == newMode)
            {
                return;
            }
            if(_currentCorroutine != null)
            {
                StopCoroutine(_currentCorroutine);
                _currentCorroutine = null;
            }
            _currentMode = newMode;

            switch (_currentMode)
            {
                case AIMode.World:
                    _currentCorroutine = StartCoroutine(RunActionProfile(config.actionProfile));
                    break;

                case AIMode.Combat:
                    _nextDecisionTime = 0f;
                    _currentCorroutine = StartCoroutine(Combat());
                    break;
            }


        }

        private void BuildCombatPlanner()
        {
            _combatContext = new CombatContext(this, _agent);

            List<ICombatAction> actions = new List<ICombatAction>();

            for(int i = 0; i < combatActions.Count ;i ++)
            {
                if(combatActions[i] != null)
                {
                    actions.Add(combatActions[i]);
                }
            }

            _combatPlanner = new CombatPlanner(actions);
            _nextDecisionTime = 0f;


        }

        #endregion

        //== STATE MACHINE & STATES ==\\

        #region STATE MACHINE - WORLD

        private IEnumerator RunActionProfile(ActionProfile profile)
        {
            if(profile == null)
            {
                yield break;
            }

            int index = 0;

            Debug.Log($"Iniciando profile com {profile.actions.Count} acoes");
            if(profile.randomOrder == true)
            {
                Debug.Log("Modo Shuffle");
            }


            while(true)
            {
                ActionEntry entry;

                if(profile.randomOrder == true)
                {
                    int randIndex = Random.Range(0, profile.actions.Count);
                    entry = profile.actions[randIndex]; 
                }
                else
                {
                    entry = profile.actions[index];
                }


                Debug.Log($"Acao selecionada {entry.action.name} com {entry.probability * 100}% de chance de ativacao por repeticao");

                int min = Mathf.Max(1, entry.minRepeats);
                int max = Mathf.Max(min, entry.maxRepeats);
                int repeats = Random.Range(min, max + 1);

                for(int i = 0; i < repeats; i++)
                {
                    float waitTime = config.GetDecisionDelay();
                    if (waitTime > 0) { yield return new WaitForSeconds(waitTime);}

                    if (entry.action != null && Random.value <= entry.probability) 
                    {
                        yield return entry.action.Execute(this);
                        
                    }

                }

                if(profile.randomOrder == false)
                {
                    index++;
                    if (index >= profile.actions.Count)
                    {
                        index = 0;
                    }
                }
                
            }
        }

        public IEnumerator StaticRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        /*
        private IEnumerator Idle()
        {
            while (true)
            {
                float waitTime = Config.GetDecisionDelay();
                
                yield return new WaitForSeconds(waitTime);

                yield return WanderRoutine(Config.maxDecisionDelay);
   
            }
        }
        */
        public IEnumerator LookAroundRoutine(float yaw, float rotationSpeed, float pauseAtExtremes)
        {
            /*
            Quaternion startRot = transform.rotation;
            float yaw = Config.lookAroundYawMax;

            Quaternion lefRot = Quaternion.Euler(0f, -yaw, 0f) * startRot;
            Quaternion rigthRot = Quaternion.Euler(0f, yaw, 0f) * startRot;

            float duration = Config.lookAroundDuration;

            // OLHAR PARA A ESQUERDA
            float t = 0;

            while ( t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01 (t / duration);
                transform.rotation = Quaternion.Slerp(startRot , lefRot, normalized);
                yield return null;
            }

            yield return new WaitForSeconds (duration);

            //VOLTAR AO CENTRO
             t = 0;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);
                transform.rotation = Quaternion.Slerp(lefRot, startRot, normalized);
                yield return null;
            }

            yield return new WaitForSeconds(duration);

            // OLHAR PARA A DIREITA
             t = 0;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);
                transform.rotation = Quaternion.Slerp(startRot, rigthRot, normalized);
                yield return null;
            }

            yield return new WaitForSeconds(duration);

            //VOLTAR AO CENTRO A PARTIR DA DIREITA  
             t = 0;

            while (t < duration)
            {
                t += Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);
                transform.rotation = Quaternion.Slerp(rigthRot, startRot, normalized);
                yield return null;
            }

            */

            Transform t = transform;
            Quaternion centerRot = t.rotation;
            Quaternion lefRot = Quaternion.Euler(0f, -yaw, 0f) * centerRot;
            Quaternion rigthRot = Quaternion.Euler(0f, yaw, 0f) * centerRot;

            //Esquerda
            yield return RotateTo(t,lefRot,rotationSpeed);
            if (pauseAtExtremes > 0) { yield return new WaitForSeconds(pauseAtExtremes); }

            //Center
            yield return RotateTo(t, centerRot, rotationSpeed);
            if (pauseAtExtremes > 0) { yield return new WaitForSeconds(pauseAtExtremes); }

            //Direita
            yield return RotateTo(t, rigthRot, rotationSpeed);
            if (pauseAtExtremes > 0) { yield return new WaitForSeconds(pauseAtExtremes); }

            //Center
            yield return RotateTo(t, centerRot, rotationSpeed);
 
           
        }

        private IEnumerator RotateTo(Transform t, Quaternion target, float rotationSpeed)
        {
            while(Quaternion.Angle(t.rotation, target) > 0.5f)
            {
                t.rotation = Quaternion.RotateTowards(t.rotation, target, rotationSpeed * Time.deltaTime);
                yield return null;
            }

        }

        public IEnumerator WanderRoutine(float explorationRadius, bool limitDistanceFromStart, float maxDistanceFromStart, float stopDistance )
        {
            /*
            targetPosition = AIUtils.GetRandomPointOnNavMesh(transform.position, maxDistanceToLookAround);

            if(Vector3.Distance(startPosition, targetPosition) > Config.maxDistanceToLookAraound)
            {
                targetPosition = startPosition;
            }

            _agent.SetDestination(targetPosition);

            yield return new WaitUntil(() => Vector3.Distance(transform.position, targetPosition) <= 0.5f);

            yield return new WaitForSeconds(Config.lookAroundDuration);
            */

            SetMoveMode(MoveMode.Walk);

            Vector3 target = AIUtils.GetRandomPointOnNavMesh(transform.position, explorationRadius);
            float distaceFromStart = Vector3.Distance(startPosition, target);
            if(limitDistanceFromStart == true && distaceFromStart > maxDistanceFromStart)
            {
                target = startPosition;
            }

            _agent.SetDestination(target);

            yield return new WaitUntil(() => Vector3.Distance(transform.position, target) <= stopDistance);


            //yield return LookAroundRoutine();
        }

        public IEnumerator WaypointNavigationRoutine(WaypointNavigationConfig navigationConfig)
        {
            if(_waypointManager == null || _waypointManager.PointCount == 0)
            {
                yield break;
            }

            _waypointManager.LoadWaypointNavigationConfig(navigationConfig);

            var wp = _waypointManager.GetCurrentPoint();
            _agent.SetDestination(wp.transform.position);

            if(navigationConfig.traversalMode == RouteTraversalMode.Once)
            {
                for(int i = 0; i < _waypointManager.PointCount; i++)
                {
                    yield return new WaitUntil(() => Vector3.Distance(transform.position, wp.transform.position) < 0.2f);

                    if(wp.isSubNode == false)
                    {
                        //Verificar se ha um perfil de acoes

                        if(wp.onReachWaypointProfile != null && wp.onReachWaypointProfile.actions.Count > 0 && navigationConfig.allowNodeActionOverride == true)
                        {
                            yield return RunOneShotActionProfile(wp.onReachWaypointProfile);

                        }
                        else
                        {
                            if (navigationConfig.onReachWaypointProfile != null)
                            {
                                yield return RunOneShotActionProfile(navigationConfig.onReachWaypointProfile);
                            }

                        }

                    }

                    //yield return LookAroundRoutine(30f, 20f, 1f);

                    wp = _waypointManager.Advance();
                    _agent.SetDestination(wp.transform.position);
                }

                yield return new WaitUntil(() => Vector3.Distance(transform.position, wp.transform.position) < 0.2f);

            }
            else
            {
                while (true)
                {
                    yield return new WaitUntil(() => Vector3.Distance(transform.position, wp.transform.position) < 0.2f);

                    if(wp.isSubNode == false)
                    {
                        //yield return LookAroundRoutine(50f, 20f, 1f);

                        if (navigationConfig.onReachWaypointProfile != null)
                        {
                            yield return RunOneShotActionProfile(navigationConfig.onReachWaypointProfile);
                        }

                    }

                    wp = _waypointManager.Advance();
                    _agent.SetDestination(wp.transform.position);
                    yield return null;
                }


            }

        }

        private IEnumerator RunOneShotActionProfile(ActionProfile profile)
        {
            if(profile  == null || profile.actions == null || profile.actions.Count == 0)
            {
                yield break;

            }

            float waitTime = 0;

            //== MODO RANDOM ESCOLHE APENAS UMA ACAO ==\\

            if (profile.randomOrder == true)
            {
                var valid = new List<ActionEntry>();
                foreach(var e in profile.actions)
                {
                    if(e != null && e.action != null)
                    {
                        valid.Add(e);
                    }
                }

                if(valid.Count == 0)
                {
                    yield break ;
                }

                int idx = Random.Range  (0, valid.Count);
                var entry = valid[idx];

                int min = Mathf.Max(1, entry.minRepeats);
                int max = Mathf.Max(min, entry.maxRepeats);
                int repeats = Random.Range(min, max + 1);

                for (int i = 0; i < repeats; i++)
                {
                    waitTime = config.GetDecisionDelay();
                    if (waitTime > 0) { yield return new WaitForSeconds(waitTime); }

                    if (Random.value <= entry.probability)
                    {
                        yield return entry.action.Execute(this);

                    }

                }

                waitTime = config.GetDecisionDelay();
                if (waitTime > 0) { yield return new WaitForSeconds(waitTime); }

                yield break ;
            }

            //== MODO NORMAL EXECUTA TODAS AS ACOES ==\\

            foreach(var entry in profile.actions)
            {
                if (entry == null || entry.action == null) { continue; }
                
                int min = Mathf.Max(1, entry.minRepeats);
                int max = Mathf.Max(min, entry.maxRepeats);
                int repeats = Random.Range(min, max + 1);

                waitTime = config.GetDecisionDelay();

                for (int i = 0; i < repeats; i++)
                {

                    waitTime = config.GetDecisionDelay();
                    if (waitTime > 0) { yield return new WaitForSeconds(waitTime); }

                    if (Random.value <= entry.probability)
                    {
                        yield return entry.action.Execute(this);

                    }

                }

                waitTime = config.GetDecisionDelay();
                if (waitTime > 0) { yield return new WaitForSeconds(waitTime); }
            }

        }

        #endregion

        //== COMBAT (GOAP - like loop + Rotinas de Combate ==\\

        #region COMBAT

        private IEnumerator Combat()
        {

            while (true)
            {
                if(_target == null)
                {
                    yield return null;
                    continue;
                }

                if(Time.time > _nextDecisionTime)
                {
                    _combatContext.Refresh();

                    ICombatAction chosen = _combatPlanner.ChooseBestAction(_combatContext);

                    if(chosen != null)
                    {

                        //Debug.Log($"Acao Selecionada {chosen.Name}");

                        yield return chosen.Execute(_combatContext);

                        float delay = chosen.DecisionDelay;
                        if(delay > 0)
                        {
                            _nextDecisionTime = Time.time + delay;  
                        }
                        else
                        {
                            _nextDecisionTime = Time.time + combatDecisionInterval;
                        }
                    }
                    else
                    {
                        _nextDecisionTime += Time.time + combatDecisionInterval;
                    }

                }

                yield return null;
            }

        }

        public IEnumerator ChaseTargetRoutine(Transform target, float desiredRange, float timeOut, MoveMode moveMode)
        {
            if(target == null) yield break;
            
            float endTime = Time.time + Mathf.Max(0.1f, timeOut);

            _agent.isStopped = false;

            SetMoveMode(moveMode);

            //_agent.speed = moveSpeed;
            _agent.stoppingDistance = Mathf.Max(0f, desiredRange);
            _agent.ResetPath();

            while(Time.time < endTime)
            {
                if (target == null) yield break;

                float dist = Vector3.Distance(transform.position, target.position);
                if(dist <= desiredRange + combatStopDistance)
                {
                    _agent.isStopped = true;
                    yield break;
                }

                _agent.isStopped = false;
                _agent.SetDestination(target.position);

                yield return null;
            }

            yield break;
        }

        public IEnumerator MeleeAttackRoutine(Transform target, string triggerName, float delay, float backstepChance = 0.35f, float backstepDesiredDuration = 0.9f)
        {
            if(target == null) yield break;

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();

            _animator.SetTrigger(triggerName);

            SetRotationMode(RotationMode.None);
            
            _isAttacking = true;

            MarkMeleeAttack();

            // chamada da animacao
            //StartCoroutine(nameof(AtaqueTeste));

            yield return new WaitUntil(() => _isAttacking == false);

            RollRepositionBackstepDesire(backstepChance, backstepDesiredDuration);

            SetRotationMode(RotationMode.Auto);

            //Debug.Log("Atack Finalizado");
            yield return new WaitForSeconds(delay);
        }

        //public IEnumerator AtaqueTeste()
        //{
        //    yield return new WaitForSeconds(3f);
        //    _isAttacking = false;
        //}

        public IEnumerator RangedMeleeAttackRoutine(Transform target, string triggerName, float delay, float maxTurnTime, float turnSpeedDegPerSec, float maxRange)
        {
            if (target == null) yield break;

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();


            float startTime = Time.time;

            SetRotationMode(RotationMode.None);

            while(true)
            {
                Vector3 dir = target.position - transform.position;
                dir.y = 0;

                if(dir.sqrMagnitude < 0.0001f)
                {
                    break;
                }

                Quaternion desiredRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                float angle = Quaternion.Angle(transform.rotation, desiredRot);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, turnSpeedDegPerSec * Time.deltaTime);

                if(Time.time - startTime >= maxTurnTime)
                {
                    break;
                }

                yield return null;
            }

            float d = Vector3.Distance(transform.position, target.position);

            if(d >= maxRange)
            {
                _agent.stoppingDistance = maxRange;
                _agent.SetDestination(target.position);
                SetRotationMode(RotationMode.AgentDriven);
                //yield break;
            }    

            yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position) <= maxRange + 0.2f);

            SetRotationMode(RotationMode.None);

            _animator.SetTrigger(triggerName);

            _isAttacking = true;
           
            yield return new WaitUntil(() => _isAttacking == false);

            _agent.stoppingDistance = 0f;
            SetRotationMode(RotationMode.Auto);

            yield return new WaitForSeconds(delay);
            
        }


        public IEnumerator RepositionBackstepRoutine(RepositionBackstepData data)
        {
            if (data.target == null) yield break;
            if (_agent == null) yield break;

            _agent.isStopped = false;
            _agent.ResetPath();
            _agent.stoppingDistance = 0;   

            SetMoveMode(data.moveMode);

            SetRotationMode(RotationMode.None);

            float timeout = Mathf.Max(0.1f, data.timeOut);
            float endtime = Time.time + timeout;

            float desireDistance = Mathf.Max(0.01f, data.backstep);
            float turnSpeed = Mathf.Max(0f, data.turnSpeedDegPerSec);

            while (true)
            {
                if (data.target == null) break;
                if (Time.time > endtime) break;

                Vector3 myPos = transform.position;
                Vector3 targetPos = data.target.position;

                Vector3 toTarget = targetPos - myPos;
                toTarget.y = 0f;

                float sqrDist = toTarget.sqrMagnitude;
                float sqrDesired = desireDistance * desireDistance;

                if (sqrDist >= sqrDesired)
                {
                    break;
                }

                if (sqrDist > 0.0001f)
                {
                    Quaternion desiredRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, turnSpeed * Time.deltaTime);
                }

                Vector3 awayDir;
                if (sqrDist > 0.0001f)
                {
                    awayDir = (-toTarget).normalized;
                }
                else
                {
                    awayDir = -transform.forward;
                    awayDir.y = 0;
                    if (awayDir.sqrMagnitude > 0.0001f)
                    {
                        awayDir.Normalize();
                    }
                }

                Vector3 desireDestination = myPos + awayDir * desireDistance;
                _agent.SetDestination(desireDestination);

                if (_agent.speed != 0)
                {
                    _animator.SetBool("isBackstep", true);
                }
                else
                {
                    _animator.SetBool("isBackstep", false);

                }

                yield return null;
            }

            if(data.decisionDelay > 0)
            {
                yield return new WaitForSeconds(data.decisionDelay);
            }

            _agent.ResetPath();
            _agent.isStopped = true;
            _animator.SetBool("isBackstep", false);
            SetRotationMode(RotationMode.Auto);

            MarkReposition();
            ResetMeleeChain();
            ResetBackstepDesire();
        }

        public IEnumerator RangeAttackRoutine (Transform targe, string triggerName, float delay)
        {
            if(targe == null) { yield break; }

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();

            float maxTurnTime = 1f;
            float startTime = Time.time;

            SetRotationMode(RotationMode.None);

            while (true)
            {
                Vector3 dir = targe.position - transform.position;
                dir.y = 0;

                if (dir.magnitude < 0.0001f)
                {
                    break;
                }

                Quaternion desiredRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                float angle = Quaternion.Angle(transform.rotation, desiredRot);

                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, 480f * Time.deltaTime);

                if (Time.time - startTime >= maxTurnTime)
                {
                    break;
                }


                yield return null;
            }

            _animator.SetTrigger(triggerName);
            _isAttacking = true;
            SetRotationMode(RotationMode.LookAtTarget);

            yield return new WaitUntil(() => _isAttacking == false);


            yield return new WaitForSeconds(delay);
        }

        public IEnumerator MoveAwayFromTargetRoutine(MoveRelativeToTargetData data)
        {
            if(data.target == null) { yield break; }
            if(_agent == null) { yield break; }

            float desiredMin = Mathf.Max(0.1f, data.desiredMinDistance);
            float timeOut = Mathf.Max(0.1f, data.timeOut);

            float stopTol = Mathf.Max(0f, data.arrivalTolerance);

            float repathInterval = Mathf.Max(0.02f, data.repathInterval);
            float nextRepathAt = 0;

            _agent.isStopped = false;
            _agent.ResetPath();
            _agent.stoppingDistance = 0f;

            _lookAtPath = data.target.position;
            _turnSpeedToPath = Mathf.Max(0f, data.turnSpeedDegPerSec);

            SetMoveMode(data.moveMode);
            SetRotationMode(RotationMode.LookAtPath);

            float startTime = Time.time;

            while(true)
            {
                if (data.target == null) { break; }

                Vector3 myPos = transform.position;
                Vector3 targetPos = data.target.position;

                Vector3 toTarget = targetPos - myPos;
                toTarget.y = 0f;

                float dist = toTarget.magnitude;

                if(dist >= (desiredMin - stopTol))
                {
                    break;
                }

                if(Time.time - startTime >= timeOut)
                {
                    SetRotationMode(RotationMode.Auto);
                    yield break; // parar a rotina - Dar o controle ao planner
                }

                if(Time.time >= nextRepathAt)
                {
                    nextRepathAt = Time.time + repathInterval;

                    Vector3 awayDir;
                    if(toTarget.sqrMagnitude > 0.0001f)
                    {
                        awayDir = (-toTarget).normalized; // negatinvo pq quer ir ao sentido oposto em fuga
                    }
                    else
                    {
                        awayDir = -transform.forward;
                        awayDir.y = 0;
                        if(awayDir.sqrMagnitude > 0.00001f) { awayDir.Normalize(); }
                    }

                    Vector3 desiredDestination = targetPos + awayDir * desiredMin;

                    if(AIUtils.TryGetFreeDestination(transform.position, desiredDestination, _agent.areaMask, out var safeDest))
                    {
                        _lookAtPath = safeDest;
                        _agent.SetDestination(safeDest);
                    }
                    else
                    {
                        SetRotationMode(RotationMode.Auto);
                        yield break;
                    }

                    
                }

                yield return null;
            }

            _agent.isStopped = true;
            _agent.ResetPath();

            SetRotationMode(RotationMode.Auto);

            if(data.target != null)
            {
                float maxTurnTime = data.maxTurnTime;
                startTime = Time.time;

                SetRotationMode(RotationMode.None);

                while (true)
                {
                    Vector3 dir = data.target.position - transform.position;    
                    dir.y = 0;

                    if(dir.magnitude < 0.0001f)
                    {
                        break;
                    }

                    Quaternion desiredRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    float angle = Quaternion.Angle(transform.rotation, desiredRot);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, data.turnSpeedDegPerSec * Time.deltaTime);

                    if(Time.time - startTime >= maxTurnTime)
                    {
                        break;
                    }


                    yield return null;
                }

                SetRotationMode(RotationMode.Auto);
                MarkReposition();

                if(data.decisionDelay > 0)
                {
                    yield return new WaitForSecondsRealtime(data.decisionDelay);
                }

            }

        }

        public IEnumerator MoveSidewaysFromTargetRoutine(MoveRelativeToTargetData data)
        {
             if(data.target == null) yield break;
             if(_agent == null) yield break;

            float desiredMin = Mathf.Max(0.1f, data.desiredMinDistance);
            float timeout = Mathf.Max(0.1f, data.timeOut);
            float stopTol = Mathf.Max(0.1f, data.arrivalTolerance);

            _agent.isStopped = false;
            _agent.ResetPath();
            _agent.stoppingDistance = 0f;

            SetMoveMode(data.moveMode); 
            SetRotationMode(RotationMode.LookAtPath); // olhar para direcao do caminho
            _turnSpeedToPath = Mathf.Max(0f, data.turnSpeedDegPerSec);

            Vector3 myPos = transform.position;
            Vector3 targetPos = data.target.position;
            Vector3 toTarget = targetPos - myPos;
            toTarget.y = 0f;

            Vector3 awayDir;

            if(toTarget.sqrMagnitude > 0.0001f)
            {
                awayDir = (-toTarget).normalized;
            }
            else
            {
                awayDir = -transform.forward;
                awayDir.y = 0;
            }

            Vector3 leftDir = Vector3.Cross(Vector3.up, awayDir).normalized;
            Vector3 rightDir = -leftDir;

            Vector3 lateralDir = data.moveDirection == RelativeMoveDirection.Left ? leftDir : rightDir;

            Vector3 desiredDestination = myPos + lateralDir * desiredMin;

            if(AIUtils.TryGetFreeDestination(myPos, desiredDestination, _agent.areaMask, out var safeDest) == false)
            {
                SetRotationMode(RotationMode.Auto);
                yield break;
            }


            _lookAtPath = safeDest;
            _agent.SetDestination(safeDest);

            float startTime = Time.time;

            while(true)
            {
                Vector3 p = transform.position;
                Vector3 t = data.target.position;

                Vector3 tot = t - p;
                tot.y = 0;

                float dist = tot.magnitude;
                if(dist >= desiredMin - stopTol)
                {
                    break;
                }

                if(Vector3.Distance(transform.position, safeDest) <= Mathf.Max(0.05f, stopTol))
                {
                    break;
                }

                if(Time.time - startTime >= timeout)
                {
                    SetRotationMode(RotationMode.Auto);
                    yield break;
                }
                yield return null;
            }

            _agent.isStopped = true;
            SetRotationMode(RotationMode.None);
            _agent.ResetPath();

            float maxTurnTime = Mathf.Max(0f, data.maxTurnTime);
            float turnStart = Time.time;    

            while(true)
            {
                Vector3 dir = data.target.position - transform.position;
                dir.y = 0;

                if (dir.magnitude < 0.00001f) break;

                Quaternion desiredRot = Quaternion.LookRotation(dir.normalized, Vector3.up);    
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, data.turnSpeedDegPerSec * Time.deltaTime);    

                if(Time.time - turnStart >= maxTurnTime)
                {
                    break;
                }

                yield return null;
            }
            SetRotationMode(RotationMode.Auto);

            if(data.decisionDelay > 0f)
            {
                yield return new WaitForSeconds(data.decisionDelay);
            }
        }
        public IEnumerator DebugActions()
        {
            _agent.isStopped = true;
            Debug.Log("Acao Debug Iniciada");
            yield return new WaitForSeconds(2);
            _agent.isStopped = false;
            Debug.Log("Acao Debuf Finalizada");
        }

        #endregion

        
    }
}
