using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperPhage
{
    public enum RibosomeFactoryType
    {
        None,
        Phage,
        Nuclease
    }

    public class Ribosome : MicroEntityEmitter
    {
        [Header("Ribosome/FactoryOutputPrefabs")]
        [SerializeField] private GameObject PhagePrefab;
        [SerializeField] private GameObject NucleasePrefab;

        [Header("Ribosome/Settings")]
        [SerializeField] private float ProduceInterval = 3;
        [SerializeField] private float PhageProduceInterval = 1.2f;
        [SerializeField] private float MinimumPhageProduceInterval = 0.45f;
        [SerializeField] private float PhageProduceAcceleration = 0.08f;
        [SerializeField] private int RequiredAminoAcidForNuclease = 3;
        [SerializeField] private int RequiredAminoAcidForPhage = 3;
        [SerializeField] private int RequiredAGTCForPhage = 3;
        [SerializeField] private int ResourceAmountPerDrop = 3;
        [SerializeField] private int StartingAminoAcidForPhage = 0;
        [SerializeField] private int StartingAGTCForPhage = 0;
        [SerializeField] private int PhageOutputCount = 5;
        [SerializeField] private int NucleaseOutputCount = 2;
        [SerializeField] private SpriteRenderer mRNA;
        [SerializeField] private RibosomeView view;
        [SerializeField] private Sprite mRNA_phage;
        [SerializeField] private Sprite mRNA_nuclease;


        public int aminoAcidCount { get; private set; } = 0;
        public int AGTCCount { get; private set; } = 0;
        public float produceTimer { get; private set; } = 0;
        
        private RibosomeFactoryType FactoryType = RibosomeFactoryType.None;
        private int producedPhageCount = 0;

        private void Awake()
        {
            mRNA.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (FactoryType == RibosomeFactoryType.None) return;
            if (CanProduce() == false) return;

            float currentProduceInterval = GetCurrentProduceInterval();

            produceTimer += Time.deltaTime;
            view.RefreshGauge(produceTimer / currentProduceInterval);

            if (produceTimer >= currentProduceInterval)
            {
                produceTimer = 0f;
                view.RefreshGauge(0f);
                Produce();
            }
        }

        private bool CanProduce()
        {
            if (FactoryType == RibosomeFactoryType.Nuclease)
            {
                return aminoAcidCount >= RequiredAminoAcidForNuclease;
            }

            if (FactoryType == RibosomeFactoryType.Phage)
            {
                return (aminoAcidCount >= RequiredAminoAcidForPhage)
                    && (AGTCCount >= RequiredAGTCForPhage);
            }

            return false;
        }

        private float GetCurrentProduceInterval()
        {
            if (FactoryType != RibosomeFactoryType.Phage)
            {
                return ProduceInterval;
            }

            float acceleratedInterval = PhageProduceInterval - producedPhageCount * PhageProduceAcceleration;
            return Mathf.Max(MinimumPhageProduceInterval, acceleratedInterval);
        }

        private void Produce()
        {
            if (FactoryType == RibosomeFactoryType.Nuclease)
            {
                aminoAcidCount -= RequiredAminoAcidForNuclease;
                view.RefreshAminoCount(aminoAcidCount);
                for (int i = 0; i < NucleaseOutputCount; i++)
                {
                    base.Emit(NucleasePrefab);
                }
                
            }

            if (FactoryType == RibosomeFactoryType.Phage)
            {
                aminoAcidCount -= RequiredAminoAcidForPhage;
                AGTCCount -= RequiredAGTCForPhage;
                view.RefreshAminoCount(aminoAcidCount);
                view.RefreshAGTCCount(AGTCCount);
                for (int i = 0; i < PhageOutputCount; i++)
                {
                    base.Emit(PhagePrefab);
                }

                producedPhageCount += PhageOutputCount;
                 
            }
        }

        public void SelectFactoryType(RibosomeFactoryType type)
        {
            mRNA.gameObject.SetActive(true);

            if (type == RibosomeFactoryType.Nuclease)
            {
                FactoryType = RibosomeFactoryType.Nuclease;
                mRNA.sprite = mRNA_nuclease;
                view.NucleaseFactoryView();
            }

            if (type == RibosomeFactoryType.Phage)
            {
                FactoryType = RibosomeFactoryType.Phage;
                mRNA.sprite = mRNA_phage;
                AddAminoAcid(StartingAminoAcidForPhage);
                AddAGTC(StartingAGTCForPhage);
                view.PhageFactoryView();
            }

            if (type == RibosomeFactoryType.None)
            {
                mRNA.gameObject.SetActive(false);
                FactoryType = RibosomeFactoryType.None;
                mRNA.color = Color.white;
                view.EraseView();
            }
        }

        public void AddAminoAcid()
        {
            AddAminoAcid(ResourceAmountPerDrop);
        }

        private void AddAminoAcid(int amount)
        {
            aminoAcidCount += amount;
            view.RefreshAminoCount(aminoAcidCount);
        }

        public void AddAGTC()
        {
            AddAGTC(ResourceAmountPerDrop);
        }

        private void AddAGTC(int amount)
        {
            AGTCCount += amount;
            view.RefreshAGTCCount(AGTCCount);
        }
    }
}


