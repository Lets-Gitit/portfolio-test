using UnityEngine;
using Random = UnityEngine.Random;

namespace SuperPhage
{
    [RequireComponent(typeof(SwimMotor))]
    public class PhageAI : MicroEntity
    {
        [SerializeField] private Vector2 wanderArea = new Vector2(10f, 6f);

        private SwimMotor swimMotor;
        private Vector2 moveDirection;
        private float changeDirectionTimer = 0f;
        private float changeDirectionInterval = 3f;
        private float intervalOffset = 0.5f;

        private void Awake()
        {
            swimMotor = GetComponent<SwimMotor>();
        }

        private void Start()
        {
            ChangeDirection();
        }

        public override void Init(MicroEntityEmitter emitter)
        {
            base.Init(emitter);
            PredationProcessManager.Instance.IncreasePhageCount();
        }

        private void Update()
        {
            changeDirectionTimer += Time.deltaTime;

            if (isOutsideWanderArea())
            {
                MoveTowardCenter();
            }

            if (changeDirectionTimer >= changeDirectionInterval)
            {
                ChangeDirection();
            }

            swimMotor.SetGoalDirection(moveDirection);
        }

        private void ChangeDirection()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            changeDirectionInterval = Random.Range(changeDirectionInterval - intervalOffset, changeDirectionInterval + intervalOffset);
            changeDirectionTimer = 0f;
        }

        private bool isOutsideWanderArea()
        {
            Vector2 position = transform.position;

            return Mathf.Abs(position.x) > wanderArea.x * 0.5f
                || Mathf.Abs(position.y) > wanderArea.y * 0.5f;
        }

        private void MoveTowardCenter()
        {
            moveDirection = (Vector2.zero - (Vector2)transform.position).normalized;
            changeDirectionTimer = 0f;
        }

        public void SetWanderArea(Vector2 area)
        {
            wanderArea = area;
        }
    }   
}
