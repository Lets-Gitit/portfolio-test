using System.Collections.Generic;
using UnityEngine;

namespace HybridShooter.Boss
{
    [RequireComponent(typeof(BossController))]
    public class BossPatternRunner : MonoBehaviour
    {
        [SerializeField] private BossAttack[] attacks;
        [SerializeField] private BossAttack basicAttack;
        
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
                    if (attack == null || attack.gameObject != gameObject || attack.UsesPatternCooldown == false 
                        || attack.CanStart() == false || candidates.Contains(attack)) 
                        continue;

                    candidates.Add(attack);
                }
            }
            
            if (candidates.Count > 0)
            {
                int index = Random.Range(0, candidates.Count);
                BossAttack selected = candidates[index];

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
    }
}
