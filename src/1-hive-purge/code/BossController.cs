using System;
using HybridShooter.Core.Contracts;
using HybridShooter.Core.Data.Defs;
using HybridShooter.Core.Events;
using UnityEngine;

namespace HybridShooter.Boss
{
    [RequireComponent(typeof(BossMovement), typeof(BossView))]
    public class BossController : MonoBehaviour, IDamageable
    {
        private BossHealth health;
        private BossMovement movement;
        private BossView view;
        private BossAttack currentAttack;
        
        private float nextAttackTime;
        
        public BossDef Def { get; private set; }

        public Faction Faction => Faction.Monster;
        public bool IsAlive => health != null && health.IsAlive;
        public int MaxHp => health?.MaxHp ?? 0;
        public int CurrentHp => health?.CurrentHp ?? 0;
        public float HpRatio => health?.HpRatio ?? 0f;
        public bool IsAttacking => currentAttack != null;
        public bool CanAct => isActiveAndEnabled && IsAlive && IsAttacking == false && IsReacting == false;
        public bool CanStartAttack => CanAct && Time.time >= nextAttackTime;
        public bool IsInvulnerable { get; private set; }
        public bool IsReacting { get; private set; }
        
        public event Action<DamageInfo> Damaged;
        public event Action Died;

        private void Awake()
        {
            movement = GetComponent<BossMovement>();
            view = GetComponent<BossView>();
        }

        public void Initialize(BossDef def, IPlayerTarget target)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (target?.Root == null) throw new ArgumentException("플레이어 Root가 필요합니다.", nameof(target));
            
            currentAttack?.Cancel();
            
            Def = def;
            health = new BossHealth(def.MaxHp);
            nextAttackTime = Time.time;

            IsReacting = false;
            IsInvulnerable = false;
            
            movement.Initialize(def.MoveSpeed, target.Root);
            view.ResetState();
        }

        private void Update()
        {
            if (IsReacting == false || view.IsHitFinished == false) return;

            IsReacting = false;

            if (IsAlive)
            {
                view.PlayIdle();
                movement.Resume();
            }
        }
        
        public void TakeDamage(in DamageInfo info)
        {
            if (IsInvulnerable) return;
            if (this.CanBeHitBy(info.AttackerFaction) == false) return;
            if (health.ApplyDamage(info.Amount) == false) return;
            
            bool died = (health.IsAlive == false);
            
            if (died)
            {
                IsReacting = false;
                
                movement.Stop();
                currentAttack?.Cancel();
                
                view.PlayDeath();
            }
            
            Damaged?.Invoke(info);
            // 일반 몬스터와 같은 피격 사실 → 히트마커·타격음·데미지 숫자
            EventBus<DamageDealtEvent>.Publish(new DamageDealtEvent(gameObject, info, died));

            if (died) Died?.Invoke();
        }
        
        // 피격 경직 공통 처리
        public void StartHitReaction()
        {
            if (isActiveAndEnabled == false || IsAlive == false) return;
            if (IsInvulnerable) return;

            // 공격 취소 중 이동이 재개되지 않도록 먼저 설정한다.
            IsReacting = true;

            currentAttack?.Cancel();
            movement.Pause();

            view.PlayHit();
        }
        
        internal bool CanStartAttackFor(BossAttack attack)
        {
            return attack != null && CanAct && (attack.UsesPatternCooldown == false || Time.time >= nextAttackTime);
        }
        
        internal bool TryBeginAttack(BossAttack attack)
        {
            if (CanStartAttackFor(attack) == false) return false;

            currentAttack = attack;
            movement.Pause();

            return true;
        }

        internal void EndAttack(BossAttack attack, bool completed = false)
        {
            if (currentAttack != attack) return;

            currentAttack = null;

            // 일반 공격은 패턴 쿨타임과 별개로 동작한다.
            if (completed && attack.UsesPatternCooldown) nextAttackTime = Time.time + Def.PatternCooldown;

            if (IsAlive == false || isActiveAndEnabled == false)
            {
                movement.Stop();
            }
            else if (IsReacting == false)
            {
                movement.Resume();
            }
        }

        public void SetInvulnerable(bool value)
        {
            IsInvulnerable = value;
        }
    }
}
