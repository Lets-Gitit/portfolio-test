using UnityEngine;

namespace SuperPhage
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SwimMotor : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float rotationSpeed = 180.0f;
        [SerializeField] private float backwardMoveSpeed = 2.0f;
        [SerializeField] private float backwardRotationSpeed = 50.0f;
        [SerializeField] private float deceleration = 5.0f;
        private Rigidbody2D rb;
        private Vector2 currentDirection;
        private Vector2 goalDirection;
        private float currentAngle;
        private float currentSpeed;
        private float currentRotationSpeed;
        private bool hasGoalDirection = false;
        private bool isBackwardMove = false;

        private float angleOffset = -90f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            currentAngle = rb.rotation - angleOffset;
        }

        private void FixedUpdate()
        {
            if (hasGoalDirection)
            {
                Vector2 targetHeadDirection = isBackwardMove ? -goalDirection : goalDirection;

                float targetAngle = Mathf.Atan2(goalDirection.y, goalDirection.x) * Mathf.Rad2Deg;
                currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, currentRotationSpeed * Time.fixedDeltaTime);
            }

            float currentRadian = currentAngle * Mathf.Deg2Rad;
            Vector2 headDirection = new Vector2(
                Mathf.Cos(currentRadian),
                Mathf.Sin(currentRadian)
            );

            currentDirection = isBackwardMove ? -headDirection : headDirection;

            Vector2 movePosition = rb.position + (currentDirection * currentSpeed * Time.fixedDeltaTime);

            float visualAngle = currentAngle + angleOffset;

            rb.MovePosition(movePosition);
            rb.MoveRotation(visualAngle);

            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        public void SetGoalDirection(Vector2 dir)
        {
            SetGoalDirection(dir, false);
        }

        /// <summary>
        /// 후진 전용 메서드
        /// </summary>
        public void SetGoalDirection(Vector2 dir, bool isBackward)
        {
            goalDirection = dir.normalized;
            hasGoalDirection = true;
            isBackwardMove = isBackward;
            currentSpeed = isBackward ? backwardMoveSpeed : moveSpeed;
            currentRotationSpeed = isBackward ? backwardRotationSpeed : rotationSpeed;
        }

        public void Brake()
        {
            hasGoalDirection = false;
        }

        /// <summary>
        /// If speed is under 0, set moveSpeed to 0
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
        }
    }

}