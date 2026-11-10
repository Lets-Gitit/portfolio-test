using UnityEngine;
using UnityEngine.Rendering;

namespace SuperPhage
{
    public class EnergyOrb : MonoBehaviour
    {
        [SerializeField] private float despawnDistanceFromTarget = 12f;
        [SerializeField] private Transform despawnTarget;

        [SerializeField] private float offset = 0.2f;
        [SerializeField] private float speed = 6f;
        [SerializeField] private float energyAmount = 5f;
        private Transform target;
        private bool isAttracted = false;
        private PhageEnergy phageEnergy;
        private RandomAreaSpawner spawner;

        private const string PLAYER_TAG = "Player";

        private void Update()
        {
            DespawnIfTooFar();

            if (isAttracted == false || target == null) return;

            transform.position = Vector2.MoveTowards(
                transform.position, target.position, speed * Time.deltaTime);

            float sqrDist = (target.position - transform.position).sqrMagnitude;
            if (sqrDist < offset * offset)
            {
                phageEnergy.GainEnergy(energyAmount);
                Destroy(this.gameObject);
            }
        }

        

        private void DespawnIfTooFar()
        {
            if (despawnTarget == null) return;

            float sqrDistance = ((Vector2)despawnTarget.position - (Vector2)transform.position).sqrMagnitude;
            
            if (sqrDistance >= despawnDistanceFromTarget * despawnDistanceFromTarget)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(PLAYER_TAG))
            {
                target = collision.gameObject.GetComponent<Transform>();
                phageEnergy = target.GetComponent<PhageEnergy>();
                isAttracted = true;
            }
        }

        private void OnDestroy()
        {
            if (spawner == null)
            {
                return;
            }

            spawner.DecreaseCount();
        }

        public void RegisterSpawner(RandomAreaSpawner spawner)
        {
            this.spawner = spawner;
        }

        public void SetDespawnTarget(Transform target)
        {
            despawnTarget = target;
        }
    }
}

