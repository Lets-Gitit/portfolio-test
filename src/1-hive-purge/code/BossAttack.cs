using System;
using HybridShooter.Core.Contracts;
using HybridShooter.Core.GameFlow;
using UnityEngine;

namespace HybridShooter.Boss
{
    [RequireComponent(typeof(BossController))]
    public abstract class BossAttack : MonoBehaviour
    {
        protected BossController Boss { get; private set; }
        protected Transform TargetRoot { get; private set; }
        protected IDamageable TargetDamageable { get; private set; }
        
        public SkillPhase Phase { get; private set; } = SkillPhase.Idle;
        public bool IsRunning => Phase != SkillPhase.Idle;
        protected BossAttackRange Range { get; } = new();
        
        public virtual bool UsesPatternCooldown => true;

        /// <summary>
        /// 단계 변화 알림 (같은 보스의 BossSoundEmitter 등이 구독). Windup·Active 는 OnWindup·OnActiveStarted 가 끝난 뒤
        /// 아직 그 단계일 때만 알린다 → 준비 중 바로 취소된 공격(포격 낙하 지점 실패 등)의 예고는 알리지 않는다
        /// </summary>
        public event Action<BossAttack, SkillPhase> PhaseChanged;
        /// <summary>공격이 땅·대상에 닿은 순간과 지점 (지금은 짓밟기 착지)</summary>
        public event Action<BossAttack, Vector3> Impacted;

        private bool initialized;
        private float windupTime;
        private float recoveryTime;
        private float phaseEndTime;
        
        protected virtual void Awake()
        {
            Boss = GetComponent<BossController>();
        }
        
        protected virtual void OnDisable()
        {
            Cancel();
        }

        protected void InitializeAttack(IPlayerTarget target, float windupTime, float recoveryTime)
        {
            if (target?.Root == null) throw new ArgumentException("플레이어 Root가 필요합니다.", nameof(target));
            
            var receiver = target.Root.GetComponentInParent<IDamageable>();
            if (receiver == null) throw new ArgumentException("플레이어 Root 또는 부모에 IDamageable이 필요합니다.", nameof(target));

            Cancel();

            TargetRoot = target.Root;
            TargetDamageable = receiver;

            this.windupTime = windupTime;
            this.recoveryTime = recoveryTime;

            initialized = true;
        }

        public bool CanStart()
        {
            if (initialized == false || isActiveAndEnabled == false || IsRunning) return false;
            if (Boss == null || Boss.CanStartAttackFor(this) == false) return false;
            if (TargetRoot == null) return false;
            if (TargetDamageable.CanBeHitBy(Boss.Faction) == false) return false;

            return MeetsStartCondition();
        }
        
        public bool TryStart()
        {
            if (CanStart() == false) return false;
            // 다른 패턴이 실행 중이면 시작할 수 없다.
            if (Boss.TryBeginAttack(this) == false) return false;

            Phase = SkillPhase.Windup;
            phaseEndTime = Time.time + windupTime;

            OnWindup();
            if (Phase == SkillPhase.Windup) NotifyPhase();

            return IsRunning;
        }
        
        private void Update()
        {
            if (IsRunning == false) return;
            if (Boss.IsAlive == false || Boss.isActiveAndEnabled == false || TargetRoot == null)
            {
                Cancel();
                return;
            }

            switch (Phase)
            {
                case SkillPhase.Windup:
                    if (Time.time < phaseEndTime) return;
                    Phase = SkillPhase.Active;

                    OnActiveStarted();
                    if (Phase == SkillPhase.Active) NotifyPhase();
                    break;
                
                case SkillPhase.Active:
                    OnActiveTick(Time.deltaTime);
                    break;
                
                case SkillPhase.Recover:
                    if (Time.time < phaseEndTime) return;
                    
                    Phase = SkillPhase.Idle;
                    Boss.EndAttack(this, completed: true);
                    NotifyPhase();
                    break;
            }
        }
        
        protected void CompleteActive()
        {
            if (Phase != SkillPhase.Active) return;

            Range.Clear();

            Phase = SkillPhase.Recover;
            phaseEndTime = Time.time + recoveryTime;

            NotifyPhase();
        }
        
        public void Cancel()
        {
            // 이미 종료된 상태에서도 남은 표시를 정리한다.
            Range.Clear();

            if (IsRunning == false) return;

            Phase = SkillPhase.Idle;

            try
            {
                OnCancelled();
            }
            finally
            {
                Boss.EndAttack(this);
                NotifyPhase();
            }
        }

        protected virtual void OnDestroy()
        {
            Range.Clear();
        }
        
        /// <summary>하위 패턴이 타격 순간에 호출 (애니메이션 이벤트 등)</summary>
        protected void RaiseImpacted(Vector3 point) => Impacted?.Invoke(this, point);

        private void NotifyPhase() => PhaseChanged?.Invoke(this, Phase);

        protected virtual bool MeetsStartCondition() => true;

        protected virtual void OnWindup() { }

        protected abstract void OnActiveStarted();

        protected virtual void OnActiveTick(float deltaTime) { }

        protected virtual void OnCancelled() { }
    }
}
