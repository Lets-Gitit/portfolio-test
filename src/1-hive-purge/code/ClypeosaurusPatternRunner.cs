using System.Collections.Generic;
using UnityEngine;

namespace HybridShooter.Boss.Clypeosaurus
{
    [RequireComponent(typeof(BossController))]
    public class ClypeosaurusPatternRunner : MonoBehaviour
    {
        [SerializeField] private BossAttack[] attacks;
        [SerializeField] private BossAttack basicAttack;

        [Header("짓밟기 사거리 밖 패턴 가중치")]
        [SerializeField, Min(0f)] private float barrageWeight = 4f;
        [SerializeField, Min(0f)] private float suctionWeight = 6f;
        
        private BossController boss;
        private readonly List<BossAttack> candidates = new();
        
        private void Awake()
        {
            boss = GetComponent<BossController>();
        }

        private void Update()
        {
            if (boss.CanAct == false) return;
            
            candidates.Clear();

            if (attacks != null)
            {
                foreach (var attack in attacks)
                {
                    if (attack == null || attack.gameObject != gameObject || attack.UsesPatternCooldown == false || attack.CanStart() == false || candidates.Contains(attack)) continue;

                    if (attack is StompAttack)
                    {
                        candidates.Clear();
                        candidates.Add(attack);
                        break;
                    }

                    if (GetWeight(attack) > 0f) candidates.Add(attack);
                }
            }
            
            if (candidates.Count > 0)
            {
                BossAttack selected = SelectPattern();

                if (selected.TryStart())
                {
                    Debug.Log($"[BossPattern] 시작: {selected.GetType().Name}", this);

                    return;
                }
            }
            
            if (basicAttack != null && basicAttack.gameObject == gameObject && basicAttack.UsesPatternCooldown == false)
            {
                basicAttack.TryStart();
            }
        }

        private BossAttack SelectPattern()
        {
            float totalWeight = 0f;
            foreach (var attack in candidates) totalWeight += GetWeight(attack);

            float roll = Random.Range(0f, totalWeight);
            foreach (var attack in candidates)
            {
                roll -= GetWeight(attack);
                if (roll <= 0f) return attack;
            }

            return candidates[candidates.Count - 1];
        }

        private float GetWeight(BossAttack attack)
        {
            if (attack is MagmaBarrageAttack) return Mathf.Max(0f, barrageWeight);
            if (attack is SuctionAttack) return Mathf.Max(0f, suctionWeight);
            return 1f;
        }
    }
}
