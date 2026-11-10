using UnityEngine;

namespace SuperPhage
{
    public enum EntityType
    {
        AminoAcid,
        AGTC,
        mRNA_Phage,
        mRNA_Nuclease,
        PhageAI,
        Nuclease
    }

    public class MicroEntity : MonoBehaviour
    {
        [field: SerializeField] public EntityType entityType { get; private set; }
        private MicroEntityEmitter emitter;
        private RandomAreaSpawner spawner;

        public virtual void Init(MicroEntityEmitter emitter)
        {
            this.emitter = emitter;
        }

        public void RegisterSpawner(RandomAreaSpawner spawner)
        {
            this.spawner = spawner;
        }

        private void OnDestroy()
        {
            if (emitter != null)
            {
                emitter.DecreaseCount();
            }

            if (spawner != null)
            {
                spawner.DecreaseCount();
            }
        }
    }
}