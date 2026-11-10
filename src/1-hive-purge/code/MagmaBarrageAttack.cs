using System;
using System.Collections.Generic;
using HybridShooter.Core.Contracts;
using HybridShooter.Core.Data.Defs;
using HybridShooter.Core.GameFlow;
using UnityEngine;

namespace HybridShooter.Boss.Clypeosaurus
{
    [RequireComponent(typeof(BossView))]
    public class MagmaBarrageAttack : BossAttack
    {
        // Animation
        private const string WindupState = "Base Layer.IdleLookAround";
        private const string BarrageState = "Base Layer.SpittersAttack";
        private const float PlacementRetryDelay = 1f;
        
        private const int PlacementAttempts = 100;
        
        [Header("Prefab·fire")] 
        [SerializeField] private MagmaProjectile projectilePrefab;
        [SerializeField] private Transform[] firePoints;

        [Header("fall point")] 
        [SerializeField] private LayerMask groundMask;
        // Raycast 설정, Height: 시작 높이 / Distance: 탐색 거리
        [SerializeField, Min(0f)] private float groundProbeHeight = 50f;
        [SerializeField, Min(0f)] private float groundProbeDistance = 100f;
        
        // 발사관 리스트 
        private readonly List<Transform> sources = new();
        private readonly List<Vector3> landingPoints = new();
        private readonly List<MagmaProjectile> projectiles = new();
        
        private ClypeosaurusDef data;
        private BossView view;
        
        private int nextShot;
        private float nextShotTime;
        private int totalShots;
        private int animationStartedFrame;
        private float nextPlacementRetryTime;
        
        private bool fireStarted;
        private bool animationFinished;

        protected override void Awake()
        {
            base.Awake();
            view = GetComponent<BossView>();
        }
        
        public void Initialize(ClypeosaurusDef def, IPlayerTarget target)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (projectilePrefab == null) throw new InvalidOperationException("Projectile Prefab을 연결해주세요.");
            if (firePoints == null || firePoints.Length == 0)
                throw new InvalidOperationException("Fire Points를 연결해주세요.");

            InitializeAttack(target, def.BarrageTelegraphTime, def.BarrageRecoveryTime);

            data = def;
        }

        protected override void OnWindup()
        {
            sources.Clear();
            
            foreach (var point in firePoints)
            {
                if (point != null && point.gameObject.activeInHierarchy)
                    sources.Add(point);
            }
            
            nextShot = 0;
            totalShots = sources.Count * data.BarrageVolleyCount;
            fireStarted = false;
            animationFinished = false;

            if (BuildVolleyPlan() == false)
            {
                Debug.LogWarning("[MagmaBarrage] 발사구 또는 착탄 지점을 확인해주세요.", this);

                nextPlacementRetryTime = Time.time + PlacementRetryDelay;

                Cancel();
                return;
            }

            view.PlayState(WindupState);
        }

        private bool BuildVolleyPlan()
        {
            landingPoints.Clear();

            if (sources.Count == 0 || TargetRoot == null) return false;
            
            Vector3 center = TargetRoot.position;            
            float minimumDistance = data.BarrageRadius * 0.5f;
            
            for (int i = 0; i < sources.Count; i++)
            {
                bool placed = false;

                for (int attempt = 0; attempt < PlacementAttempts; attempt++)
                {
                    Vector2 offset = UnityEngine.Random.insideUnitCircle * data.BarrageSpreadRadius;
                    Vector3 origin = center + new Vector3(offset.x, groundProbeHeight, offset.y);

                    if (Physics.Raycast(origin, Vector3.down, out var hit, groundProbeDistance, groundMask,
                            QueryTriggerInteraction.Ignore) == false) continue;
                    
                    // Damageable 객체를 제외하여 착탄 높이를 지면으로 고정
                    if (hit.collider.TryGetDamageable(out _)) continue;
                    if (HasOverlap(hit.point, minimumDistance)) continue;

                    landingPoints.Add(hit.point);
                    placed = true;
                    break;
                }
                if (placed == false) return false;
            }
            
            return true;
        }

        private bool HasOverlap(Vector3 position, float minimumDistance)
        {
            foreach (var previous in landingPoints)
            {
                Vector3 difference = position - previous;
                difference.y = 0f;

                if (difference.sqrMagnitude < minimumDistance * minimumDistance) return true;
            }

            return false;
        }
        
        protected override void OnActiveStarted()
        {
            PlayBarrageAnimation();
        }
        
        private void PlayBarrageAnimation()
        {
            animationStartedFrame = Time.frameCount;
            view.PlayState(BarrageState);
        }
        
        public void OnBarrageFire()
        {
            if (isActiveAndEnabled == false || Phase != SkillPhase.Active || fireStarted || animationFinished 
                || Boss.IsAlive == false || Boss.isActiveAndEnabled == false || TargetRoot == null)
                return;

            // 이벤트가 중복 호출되어도 발사는 한 번만 시작한다.
            fireStarted = true;
            FireNext();
        }

        protected override void OnActiveTick(float deltaTime)
        {
            if (fireStarted && nextShot < totalShots && Time.time >= nextShotTime)
            {
                FireNext();
                
                if (Phase != SkillPhase.Active) return;
            }

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                if (projectiles[i] == null || projectiles[i].IsFinished)
                {
                    projectiles.RemoveAt(i);
                } 
            }

            bool clipFinished = Time.frameCount > animationStartedFrame && view.IsStateFinished(BarrageState);
            
            if (animationFinished == false && clipFinished)
            {
                // 이벤트가 누락되어도 Active 상태에 갇히지 않도록 처리.
                if (fireStarted == false)
                {
                    Debug.LogWarning("[MagmaBarrage] OnBarrageFire 이벤트가 호출되지 않았습니다.", this);
                    
                    view.PlayIdle();
                    CompleteActive();
                    return;
                }
                
                if (nextShot < totalShots)
                {
                    PlayBarrageAnimation();
                }
                else
                {
                    animationFinished = true;
                    view.PlayIdle();
                }
            }
            
            if (nextShot >= totalShots && animationFinished && projectiles.Count == 0)
            {
                CompleteActive();
            }
        }
        
        private void FireNext()
        {
            int sourceIndex = nextShot % sources.Count;
            
            if (BuildVolleyPlan() == false)
            {
                Debug.LogWarning("[MagmaBarrage] 다음 묶음의 착탄 지점 확보 실패.", this);

                nextPlacementRetryTime = Time.time + PlacementRetryDelay;

                Cancel();
                return;
            }
            
            Transform source = sources[sourceIndex];
            Vector3 destination = landingPoints[sourceIndex];

            nextShot++;
            nextShotTime = Time.time + data.BarrageInterval;

            if (source == null || source.gameObject.activeInHierarchy == false) return;

            var projectile = Instantiate(projectilePrefab, source.position, source.rotation);
            
            projectile.Launch(Boss, data, destination, TargetRoot, TargetDamageable);

            projectiles.Add(projectile);
        }

        protected override bool MeetsStartCondition()
        {
            return Time.time >= nextPlacementRetryTime;
        }
        
        protected override void OnCancelled()
        {
            foreach (var projectile in projectiles)
            {
                if (projectile != null)
                    projectile.Cancel();
            }

            projectiles.Clear();
            sources.Clear();
            landingPoints.Clear();

            fireStarted = false;
            animationFinished = false;

            if (Boss.IsAlive && gameObject.activeInHierarchy) view.PlayIdle();
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying == false ||
                data == null ||
                IsRunning == false)
                return;

            UnityEditor.Handles.color = Color.yellow;

            foreach (var point in landingPoints)
            {
                UnityEditor.Handles.DrawWireDisc(
                    point, Vector3.up, data.BarrageRadius);
            }
        }
#endif
    }
}
