using HybridShooter.Core.Contracts;
using HybridShooter.Core.Data;
using HybridShooter.Core.Data.Defs;
using HybridShooter.Core.Events;
using HybridShooter.Core.Services;
using UnityEngine;

namespace HybridShooter.Boss.Clypeosaurus
{
    public class ClypeosaurusController : MonoBehaviour, IStaggerable
    {
        private static readonly float[] HitHpRatios = { 0.75f, 0.50f, 0.25f };
        [SerializeField] private MonoBehaviour playerSource;

        [SerializeField] private CameraShakeSettings footstepShake = CameraShakePresets.BossFootstep.WithIntensity(0.25f);
        [SerializeField, Min(0.1f)] private float footstepShakeRadius = 40f;

        [Header("환경 폭발 경직")]
        [Tooltip("연료 셀 폭발 등으로 경직된 뒤 다시 경직될 수 있기까지(초)")]
        [SerializeField, Min(0f)] private float staggerCooldown = 8f;
        [Tooltip("경직될 때 소리 (SoundTable 키, 비우면 없음)")]
        [SerializeField] private string staggerSfxKey = "mon_clypeosaurus_hurt";

        private BossController boss;
        private int nextHitIndex;
        private bool initialized;
        private float nextStaggerTime;

        /// <summary>등장 착지 중(무적)에는 환경 충격을 받지 않는다</summary>
        public bool IsInCinematic => boss == null || boss.IsInvulnerable;

        /// <summary>환경 폭발 경직. 무적·이미 경직 중·쿨다운이면 거절한다</summary>
        public bool TryStagger(Vector3 source)
        {
            if (boss == null || boss.IsAlive == false || boss.IsInvulnerable || boss.IsReacting) return false;
            if (Time.time < nextStaggerTime) return false;

            nextStaggerTime = Time.time + staggerCooldown;
            boss.StartHitReaction();
            if (string.IsNullOrEmpty(staggerSfxKey) == false && ServiceLocator.TryGet<ISoundService>(out var sound))
                sound.PlaySfx(staggerSfxKey, transform.position);
            EventBus<CameraShakeRequestedEvent>.Publish(new CameraShakeRequestedEvent(CameraShakePresets.Impact.At(transform.position, footstepShakeRadius)));
            return true;
        }
        
        private void Awake()
        {
            boss = GetComponent<BossController>();
        }
        
        private void Start()
        {
            // 라운드에서 먼저 초기화했다면 다시 실행하지 않는다.
            if (initialized) return;

            // 씬에 직접 배치해서 테스트하는 경우의 초기화.
            var target = playerSource as IPlayerTarget;

            if (target?.Root == null)
            {
                Debug.LogError("[Clypeosaurus] IPlayerTarget을 구현한 플레이어를 연결해주세요.", this);

                enabled = false;
                return;
            }

            var attackData = DataManager.Clypeosaurus;
            var bossData = DataManager.Bosses.Get(attackData.BossId);

            if (bossData == null)
            {
                enabled = false;
                return;
            }

            Initialize(bossData, target);
        }
        
        public void Initialize(BossDef def, IPlayerTarget target)
        {
            if (initialized) return;

            var attackData = DataManager.Clypeosaurus;

            nextHitIndex = 0;
            boss.Initialize(def, target);

            var entrance = GetComponent<EntranceAttack>();
            entrance.Initialize(attackData, target);

            GetComponent<ClypeosaurusBasicAttack>().Initialize(attackData, target);
            GetComponent<StompAttack>().Initialize(attackData, target);
            GetComponent<MagmaBarrageAttack>().Initialize(attackData, target);
            GetComponent<SuctionAttack>().Initialize(attackData, target);
            
            initialized = true;

            if (entrance.TryStart() == false)
            {
                Debug.LogError("[Clypeosaurus] 등장 공격 시작 실패. 초기화·NavMesh를 확인해주세요.", this);
            }

            Debug.Log($"[Boss] 초기화 | ID: {boss.Def.Id} | " + $"HP: {boss.CurrentHp}/{boss.MaxHp}", boss);
        }
        
        private void OnDamaged(DamageInfo info)
        {
            if (boss.IsAlive == false) return;

            bool crossed = false;

            while (nextHitIndex < HitHpRatios.Length && boss.HpRatio <= HitHpRatios[nextHitIndex])
            {
                nextHitIndex++;
                crossed = true;
            }

            if (crossed)
            {
                boss.StartHitReaction();
            }
        }
        
        public void LeftFoot()
        {
            Debug.Log(
                $"[DEBUG-boss-footstep] LeftFoot | frame: {Time.frameCount}",
                this);

            ShakeFootstep();
        }

        public void RightFoot()
        {
            Debug.Log(
                $"[DEBUG-boss-footstep] RightFoot | frame: {Time.frameCount}",
                this);

            ShakeFootstep();
        }
        
        private void ShakeFootstep()
        {
            if (isActiveAndEnabled == false || boss == null) return;
            if (boss.IsAlive == false || boss.IsAttacking || boss.IsReacting) return;

            var settings = footstepShake.At(transform.position, footstepShakeRadius);

            EventBus<CameraShakeRequestedEvent>.Publish(new CameraShakeRequestedEvent(settings));
        }
        
        private void OnEnable()
        {
            boss.Damaged += OnDamaged;
        }
        
        private void OnDisable()
        {
            boss.Damaged -= OnDamaged;
        }
        
        
    }
}