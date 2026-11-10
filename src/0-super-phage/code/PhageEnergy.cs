using UnityEngine;

namespace SuperPhage
{
    public enum EnergyStage
    {
        High,
        Medium,
        Low
    }

    [RequireComponent(typeof(SwimMotor))]
    public class PhageEnergy : MonoBehaviour
    {
        [SerializeField] private float energyUse = 1f;
        [SerializeField] private float currentEnergy = 100f;
        private float maxEnergy = 100f;
        private SwimMotor swimMotor;
        private EnergyStage currentStage = EnergyStage.High;
        private bool isMoving = false;

        public float EnergyRatio => ((maxEnergy <= 0f) ? 0f : currentEnergy / maxEnergy);

        private void Awake()
        {
            swimMotor = GetComponent<SwimMotor>();
        }

        private void Update()
        {
            currentEnergy = Mathf.Clamp(currentEnergy - energyUse * Time.deltaTime, 0f, maxEnergy);
            UpdateEnergyState();
        }

        private void UpdateEnergyState()
        {
            EnergyStage nextStage = EvaluateEnergyStage();

            if (currentStage == nextStage) return;

            currentStage = nextStage;

            switch (currentStage)
            {
                case EnergyStage.High:
                    swimMotor.SetMoveSpeed(5f);
                    break;
                case EnergyStage.Medium:
                    swimMotor.SetMoveSpeed(3f);
                    break;
                case EnergyStage.Low:
                    swimMotor.SetMoveSpeed(2f);
                    break;
            }
        }

        private EnergyStage EvaluateEnergyStage()
        {
            if (currentEnergy > 40)
            {
                return EnergyStage.High; 
            }
            else if (currentEnergy > 20)
            {
                return EnergyStage.Medium;
            }

            return EnergyStage.Low;
        }

        public void GainEnergy(float energyAmount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + energyAmount, 0f, maxEnergy);
        }
    }

}
