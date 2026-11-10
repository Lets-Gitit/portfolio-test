using System.Collections;
using UnityEngine;

namespace SuperPhage
{
    [RequireComponent(typeof(SwimMotor))]
    public class AntiBodyAI : MonoBehaviour
    {
        [SerializeField] private float detectDistance = 5.0f;
        [SerializeField] private Transform target;
        private SwimMotor swimMotor;

        private Coroutine detectRutine;
        private float detectInterval = 0.5f;
        private bool isChasing = false;
        private bool isTouchingTarget = false;

        private void Awake()
        {
            swimMotor = GetComponent<SwimMotor>();
        }

        private void Update()
        {
            if (isTouchingTarget)
            {
                swimMotor.Brake();
                return;
            }

            if (isChasing)
            {
                Vector2 directionToTarget = target.position - transform.position;
                swimMotor.SetGoalDirection(directionToTarget.normalized);
            }
            else
            {
                swimMotor.Brake();
            }
        }

        private IEnumerator DetectPhage()
        {
            while (true) 
            {
                Vector3 offset = transform.position - target.transform.position;
                isChasing = offset.sqrMagnitude <= (detectDistance * detectDistance);
                
                yield return new WaitForSeconds(detectInterval);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsTarget(collision) == false) return;
            
            isTouchingTarget = true;
            swimMotor.Brake();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (IsTarget(collision) == false) return;
            
            isTouchingTarget = false;
        }

        private bool IsTarget(Collider2D collision)
        {
            if (target == null) return false;
            if (collision == null) return false;

            Transform other = collision.transform;
            if (other == null) return false;

            return other == target || other.IsChildOf(target);
        }

        private void OnEnable()
        {
            detectRutine = StartCoroutine(DetectPhage());
        }

        private void OnDisable()
        {
            if (detectRutine != null)
            {
                StopCoroutine(detectRutine);
                detectRutine = null;
            }
        }
    }
}

