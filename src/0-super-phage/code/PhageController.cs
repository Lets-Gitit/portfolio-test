using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperPhage
{
    [RequireComponent(typeof(SwimMotor))]
    public class PhageController : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveActionRefence;
        [SerializeField] private InputActionReference backwardActionRefence;
        private SwimMotor swimMotor;
        private PhageEnergy phageEnergy;

        private void Awake()
        {
            swimMotor = GetComponent<SwimMotor>();
            phageEnergy = GetComponent<PhageEnergy>();
        }

        private void Update()
        {
            Vector2 moveDirection = moveActionRefence.action.ReadValue<Vector2>().normalized;
            
            if (moveDirection.sqrMagnitude <= 0.001f)
            {
                swimMotor.Brake();
                return;
            }

            bool isBackward = backwardActionRefence.action.IsPressed();
            swimMotor.SetGoalDirection(moveDirection, isBackward);
        }

        private void OnEnable()
        {
            moveActionRefence.action.Enable();
            backwardActionRefence.action.Enable();
        }

        private void OnDisable()
        {
            moveActionRefence.action.Disable();
            backwardActionRefence.action.Disable();
        }
    }
}
