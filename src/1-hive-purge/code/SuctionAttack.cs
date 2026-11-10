using System;
using HybridShooter.Core.Contracts;
using HybridShooter.Core.Data.Defs;
using HybridShooter.Core.GameFlow;
using HybridShooter.Core.Services;
using UnityEngine;

namespace HybridShooter.Boss.Clypeosaurus
{
    [RequireComponent(typeof(BossView))]
    public class SuctionAttack : BossAttack
    {
        private const string WindupState = "Base Layer.IdleLookAround";
        private const string StartState = "Base Layer.SuctionStart";
        private const string AttackState = "Base Layer.ClawStrike";
        
        [SerializeField] private Transform inhaleCenter;
        [SerializeField, Min(0f)] private float stopRadius = 2.5f;
        [SerializeField, Min(0f)] private float fadeIn = 0.3f;
        
        [SerializeField] private Transform rangePrefab;
        [SerializeField] private Transform dangerRangePrefab;
        [SerializeField] private GameObject attackEffectPrefab;
        
        private readonly BossAttackRange dangerRange = new();
        
        private BossView view;
        private ClypeosaurusDef data;
        private InhaleSettings settings;
        private InhaleSource activeInhale;
        private IPlayerExternalVelocity receiver;

        private bool attacking;
        private bool hitApplied;
        private int attackStartedFrame;
        
        protected override void Awake()
        {
            base.Awake();
            view = GetComponent<BossView>();
        }
        
        public void Initialize(ClypeosaurusDef def, IPlayerTarget target)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (inhaleCenter == null) throw new InvalidOperationException("Inhale Center를 연결해주세요.");
            if (stopRadius < 0f || stopRadius >= def.SuctionNearRadius) 
                throw new InvalidOperationException("Stop Radius는 0 이상이고 SuctionNearRadius보다 작아야 합니다.");
            if (fadeIn < 0f) throw new InvalidOperationException("Fade In은 0 이상이어야 합니다.");
            
            InitializeAttack(target, def.SuctionTelegraphTime, def.SuctionRecoveryTime);
            data = def;

            settings = new InhaleSettings
            {
                NearRadius = def.SuctionNearRadius,
                StopRadius = stopRadius,
                FarExtraSpeed = def.SuctionFarExtraSpeed,
                NearExtraSpeed = def.SuctionNearExtraSpeed,
                MaxDuration = def.SuctionMaxDuration,
                NearDwellTime = def.SuctionNearDwellTime,
                FadeIn = fadeIn
            };
        }
        
        private void LateUpdate()
        {
            if (inhaleCenter == null) return;
            
            Vector3 center = inhaleCenter.position;
            
            Range.SetPosition(center);
            dangerRange.SetPosition(center);
        }
        
        protected override void OnWindup()
        {
            attacking = false;
            hitApplied = false;

            if (inhaleCenter == null)
            {
                Cancel();
                return;
            }

            Vector3 center = inhaleCenter.position;

            Range.Show(rangePrefab, center, data.SuctionNearRadius);
            dangerRange.Show(dangerRangePrefab, center, data.SuctionDamageRadius, 0.04f);

            view.PlayState(WindupState);
        }
        
        protected override void OnActiveStarted()
        {
            if (inhaleCenter == null || ServiceLocator.TryGet(out receiver) == false)
            {
                Debug.LogWarning("[Suction] 흡입 중심 또는 플레이어 API를 찾을 수 없습니다.", this);
                Cancel();
                return;
            }

            activeInhale = InhaleSource.Start(settings, inhaleCenter);

            if (activeInhale.IsRunning == false)
            {
                Debug.LogWarning($"[Suction] 시작 실패: {activeInhale.EndReason}", this);
                Cancel();
                return;
            }
            
            view.PlayState(StartState);
            
            Debug.Log("[Suction] 흡입 시작", this);
        }
        
        protected override void OnActiveTick(float deltaTime)
        {
            if (TargetDamageable.CanBeHitBy(Boss.Faction) == false)
            {
                Cancel();
                return;
            }

            if (attacking)
            {
                if (Time.frameCount <= attackStartedFrame) return;
                if (view.IsStateFinished(AttackState) == false) return;

                if (hitApplied == false)
                {
                    Debug.LogWarning("[Suction] OnSuctionAttackHit 이벤트를 확인해주세요.", this);
                }

                dangerRange.Clear();
                view.PlayIdle();
                CompleteActive();
                return;
            }
            
            if (activeInhale == null)
            {
                Cancel();
                return;
            }
            
            if (activeInhale.IsRunning) return;

            InhaleEndReason reason = activeInhale.EndReason;
            StopInhale();

            if (reason != InhaleEndReason.Timeout && reason != InhaleEndReason.NearDwell)
            {
                Cancel();
                return;
            }

            attacking = true;
            attackStartedFrame = Time.frameCount;

            view.PlayState(AttackState);
            
            Debug.Log($"[Suction] 흡입 종료: {reason}, 회복 {data.SuctionRecoveryTime}초", this);
        }
        
        public void OnSuctionAttackHit()
        {
            if (isActiveAndEnabled == false || Phase != SkillPhase.Active || attacking == false || hitApplied 
                || Boss == null || Boss.IsAlive == false || Boss.isActiveAndEnabled == false || inhaleCenter == null)
                return;

            hitApplied = true;

            Vector3 center = inhaleCenter.position;

            Range.Clear();
            dangerRange.Clear();

            if (attackEffectPrefab != null)
            {
                GameObject effect = Instantiate(attackEffectPrefab, center, Quaternion.identity);
                Destroy(effect, 3f);
            }

            RaiseImpacted(center);

            if (TargetRoot == null || TargetDamageable.CanBeHitBy(Boss.Faction) == false) return;

            Vector3 offset = TargetRoot.position - center;
            offset.y = 0f;

            float radius = data.SuctionDamageRadius;
            if (offset.sqrMagnitude > radius * radius) return;

            Vector3 direction = offset.sqrMagnitude > 0.001f
                ? offset.normalized
                : transform.forward;

            TargetDamageable.TakeDamage(new DamageInfo(data.SuctionDamage, DamageKind.MonsterSkill, Boss.Faction, Boss.gameObject, TargetRoot.position, direction));
        }
        
        protected override void OnCancelled()
        {
            StopInhale();
            dangerRange.Clear();

            attacking = false;
            hitApplied = false;
            
            if (view != null && Boss != null && Boss.IsAlive && gameObject.activeInHierarchy)
            {
                view.PlayIdle();
            }
        }

        protected override void OnDestroy()
        {
            StopInhale();
            dangerRange.Clear();

            base.OnDestroy();
        }
        
        private void StopInhale()
        {
            if (activeInhale != null)
            {
                activeInhale.Stop();
                receiver?.RemoveVelocitySource(activeInhale);
            }

            activeInhale = null;
            receiver = null;
        }
    }
}
