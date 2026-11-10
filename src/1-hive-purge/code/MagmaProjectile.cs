using System;
using System.Collections.Generic;
using HybridShooter.Core.Contracts;
using HybridShooter.Core.Data.Defs;
using HybridShooter.Core.Services;
using UnityEngine;

namespace HybridShooter.Boss.Clypeosaurus
{
    [RequireComponent(typeof(Rigidbody))]
    public class MagmaProjectile : MonoBehaviour
    {
        [SerializeField] private LayerMask coverMask;
        [SerializeField] private Transform rangePrefab;
        [Tooltip("착탄 폭발음 (SoundTable.Key). 비우면 소리 없음. 취소·수명 초과로 사라질 때는 나지 않는다")]
        [SerializeField] private string explodeSfxKey = "mon_clypeosaurus_magma_impact";

        private SphereCollider sphere;
        private BossController owner;
        private ClypeosaurusDef data;
        
        private Transform targetRoot;
        private IDamageable target;
        private readonly BossAttackRange range = new();
        
        private Vector3 startPoint;
        private Vector3 landingPoint;
        private Vector3 initialVelocity;
        private Vector3 gravity;
        
        private float elapsed;
        
        public bool IsFinished { get; private set; } = true;

        private void Awake()
        {
            var body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            sphere = GetComponentInChildren<SphereCollider>(true);

            // 공의 직접 충돌과 피해 판정을 분리한다.
            foreach (var col in GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
            }
        }

        public void Launch(BossController owner, ClypeosaurusDef def, Vector3 destination, Transform targetRoot, IDamageable target)
        {
            this.owner = owner;
            data = def;
            this.targetRoot = targetRoot;
            this.target = target;

            startPoint = transform.position;
            landingPoint = destination;
            gravity = Physics.gravity;
            elapsed = 0f;
            
            Vector3 scale = sphere.transform.lossyScale;
            
            float worldRadius = sphere.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

            Vector3 arrival = landingPoint + Vector3.up * worldRadius;
            float duration = data.BarrageFlightTime;
            
            initialVelocity = (arrival - startPoint - 0.5f * gravity * duration * duration) / duration;
            
            IsFinished = false;
            
            range.Show(rangePrefab, landingPoint, data.BarrageRadius);
        }

        private void Update()
        {
            if (IsFinished) return;

            if (owner == null || owner.IsAlive == false || owner.isActiveAndEnabled == false || targetRoot == null)
            {
                Cancel();
                return;
            }

            float duration = data.BarrageFlightTime;
            elapsed = Mathf.Min(elapsed + Time.deltaTime, duration);

            transform.position = startPoint + initialVelocity * elapsed + 0.5f * gravity * elapsed * elapsed;

            if (elapsed >= duration) Impact();
        }
        
        private void Impact()
        {
            IsFinished = true;
            range.Clear();

            try
            {
                if ((string.IsNullOrEmpty(explodeSfxKey) == false) && ServiceLocator.TryGet<ISoundService>(out var sound))
                {
                    sound.PlaySfx(explodeSfxKey, landingPoint);
                }

                ApplyGroundDamage();
            }
            finally
            {
                Destroy(gameObject);
            }
        }
        
        private void ApplyGroundDamage()
        {
            if (owner == null || owner.IsAlive == false || owner.isActiveAndEnabled == false || targetRoot == null 
                || target == null)
                return;

            if (target.CanBeHitBy(owner.Faction) == false) return;

            // 플레이어의 수평 위치가 바닥 원 안인지 검사한다.
            Vector3 offset = targetRoot.position - landingPoint;
            offset.y = 0f;

            float radius = data.BarrageRadius;

            if (offset.sqrMagnitude > radius * radius) return;

            if (HasCover(landingPoint + Vector3.up * 0.03f, targetRoot.position))
                return;

            Vector3 direction = offset.sqrMagnitude > 0.001f
                ? offset.normalized
                : Vector3.up;

            target.TakeDamage(new DamageInfo(data.BarrageDamage, DamageKind.MonsterSkill, owner.Faction, owner.gameObject,
                targetRoot.position, direction));
        }
        
        private bool HasCover(Vector3 origin, Vector3 destination)
        {
            Vector3 difference = destination - origin;
            float distance = difference.magnitude;

            if (distance < 0.001f) return false;

            foreach (var hit in Physics.RaycastAll(origin, difference / distance, distance, coverMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.IsChildOf(owner.transform)) continue;

                if (hit.collider.TryGetDamageable(out var blocker) && object.ReferenceEquals(blocker, target))
                    continue;

                return true;
            }

            return false;
        }
        
        public void Cancel()
        {
            range.Clear();

            if (IsFinished) return;

            IsFinished = true;
            Destroy(gameObject);
        }
        private void OnDestroy()
        {
            range.Clear();
        }
    }
}
