using System;
using HybridShooter.Core.Contracts;
using HybridShooter.Core.Data.Defs;
using HybridShooter.Core.GameFlow;
using UnityEngine;
using UnityEngine.AI;

namespace  HybridShooter.Boss.Clypeosaurus
{
    [RequireComponent(typeof(BossView), typeof(UnityEngine.AI.NavMeshAgent))]
    public class EntranceAttack : BossAttack
    {
        private const string IdleState = "Base Layer.IdleBreathe";
        private const string RoarState = "Base Layer.RoarAntennaElectricAttack";
        
        private enum EntranceStage
        {
            Falling,
            Roaring
        }
        
        [Header("낙하 연출")]
        [SerializeField, Min(0.01f)] private float fallHeight = 30f;
        [SerializeField, Min(0.01f)] private float fallDuration = 0.8f;

        [Header("이펙트")]
        [SerializeField] private Transform shadowPrefab;
        [SerializeField] private GameObject shockwavePrefab;

        private BossView view;
        private NavMeshAgent agent;
        private ClypeosaurusDef data;

        private Vector3 landingPoint;
        private Vector3 fallStartPoint;
        private bool fixedLandingPoint;

        private float windupStatedAt;
        private float fallStartedAt;

        private EntranceStage stage;
        private bool used;
        private bool agentDetached;
        
        protected override void Awake()
        {
            base.Awake();

            view = GetComponent<BossView>();
            agent = GetComponent<NavMeshAgent>();
        }

        public void Initialize(ClypeosaurusDef def, IPlayerTarget target)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));

            if (shadowPrefab == null) throw new InvalidOperationException("Shadow Prefab을 연결해주세요.");

            if (fallHeight <= 0f || fallDuration <= 0f) throw new InvalidOperationException("낙하 높이와 시간은 0보다 커야 합니다.");

            InitializeAttack(target, def.EntranceTelegraphTime, def.EntranceRecoveryTime);

            data = def;
            used = false;
        }
        
        /// <summary>지정한 XZ와 지면 높이를 입장 연출의 착지점으로 유지한다.</summary>
        public void SetLandingPoint(Vector3 point)
        {
            landingPoint = point;
            fixedLandingPoint = true;
        }

        protected override bool MeetsStartCondition()
        {
            return (used == false) && agent.isActiveAndEnabled && agent.isOnNavMesh;
        }

        protected override void OnWindup()
        {
            Boss.SetInvulnerable(true);
            
            used = true;
            windupStatedAt = Time.time;

            if (fixedLandingPoint == false) landingPoint = transform.position;
            fallStartPoint = landingPoint + Vector3.up * fallHeight;

            agent.enabled = false;
            agentDetached = true;
            transform.position = fallStartPoint;
            
            Range.Show(shadowPrefab, landingPoint, 0f, 0.05f);
            view.PlayState(IdleState);
        }

        private void LateUpdate()
        {
            // 착지 후 Agent가 다시 켜져도 입장 연출 중에는 지정한 위치를 유지한다.
            if (fixedLandingPoint && IsRunning && agentDetached == false) transform.position = landingPoint;
            if (Phase != SkillPhase.Windup) return;
            float duration = data.EntranceTelegraphTime;
            
            float progress = duration > 0f ? Mathf.Clamp01((Time.time - windupStatedAt) / duration) : 1f;
            Range.SetRadius(data.EntranceRadius * progress);
        }

        protected override void OnActiveStarted()
        {
            stage = EntranceStage.Falling;
            fallStartedAt = Time.time;

            Range.SetRadius(data.EntranceRadius);
        }
        
        protected override void OnActiveTick(float deltaTime)
        {
            switch (stage)
            {
                case EntranceStage.Falling:
                    TickFall();
                    break;
                
                case EntranceStage.Roaring:
                    if (view.IsStateFinished(RoarState))
                    {
                        Boss.SetInvulnerable(false);
                        
                        CompleteActive();
                        view.PlayState(IdleState);
                    }
                    break;
            }
        }
        
        private void TickFall()
        {
            float progress = Mathf.Clamp01((Time.time - fallStartedAt) / fallDuration);
            transform.position = Vector3.Lerp(fallStartPoint, landingPoint, progress * progress);

            if (progress >= 1f) Land();
        }

        private void Land()
        {
            stage = EntranceStage.Roaring;

            if (RestoreAgent() == false)
            {
                Debug.LogError("[Entrance] 착지 지점에 navMesh가 없습니다.", this);

                Cancel();
                return;
            }

            Range.Clear();

            view.PlayState(RoarState);
            RaiseImpacted(landingPoint);

            if (shockwavePrefab != null)
            {
                var effect = Instantiate(shockwavePrefab, landingPoint, Quaternion.identity);
                var wave = effect.GetComponent<EntranceShockwave>();
                wave.Initialize(data.EntranceRadius);

                Destroy(effect, 3f);
            }

            ApplyLandingDamage();
        }

        private void ApplyLandingDamage()
        {
            if (TargetRoot == null || TargetDamageable.CanBeHitBy(Boss.Faction) == false) return;

            Vector3 offset = TargetRoot.position - landingPoint;
            offset.y = 0f;

            float radius = data.EntranceRadius;

            if (offset.sqrMagnitude > radius * radius) return;

            Vector3 direction = offset.sqrMagnitude > 0.001f
                ? offset.normalized
                : transform.forward;
            
            TargetDamageable.TakeDamage(new DamageInfo(data.EntranceDamage, DamageKind.MonsterSkill, Boss.Faction, Boss.gameObject, TargetRoot.position, direction));
        }

        private bool RestoreAgent()
        {
            if (agentDetached == false) return true;

            transform.position = landingPoint;
            agentDetached = false;
            agent.enabled = true;

            if (gameObject.activeInHierarchy == false) return true;

            bool restored = agent.isOnNavMesh && agent.Warp(landingPoint);
            if (fixedLandingPoint) transform.position = landingPoint;
            return restored;
        }

        protected override void OnCancelled()
        {
            RestoreAgent();
            
            if (Boss.IsAlive && gameObject.activeInHierarchy) view.PlayState(IdleState);
        }
    }

}
