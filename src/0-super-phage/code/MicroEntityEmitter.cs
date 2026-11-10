using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SuperPhage
{
    public class MicroEntityEmitter : MonoBehaviour
    {
        [Serializable]
        private struct EntityParentEntry
        {
            public EntityType entityType;
            public Transform parent;
        }

        [Header("MicroEntity/Settings")]
        [SerializeField] private Transform emitPoint;
        [SerializeField] private float emitSpeed = 3.0f;
        [SerializeField] private float emitDuration = 1.0f;
        [SerializeField] private int maxEntities = 10;
        private int currentEntities = 0;

        [Header("MicroEntity/Hierachy")]
        [SerializeField] private EntityParentEntry[] entityParents;

        protected void Emit(GameObject prefab)
        {
            Emit(prefab, GetEmitDirection());
        }

        protected void Emit(GameObject prefab, Vector2 direction)
        {
            if (prefab == null) return;
            if (currentEntities >= maxEntities) return;

            GameObject spawnedObject = Instantiate(prefab, emitPoint.position, Quaternion.identity);

            MicroEntity entity = spawnedObject.GetComponent<MicroEntity>();
            if (TryGetParent(entity.entityType, out Transform parent))
            {
                spawnedObject.transform.SetParent(parent);
            } 
            entity.Init(this);

            currentEntities += 1;

            // Emit(prefab, Vector2.zero)같이 호출하는 경우를 방지
            Vector2 emitDirection = direction.sqrMagnitude > 0f
                ? direction.normalized
                : GetEmitDirection();

            StartCoroutine(Eject(spawnedObject.transform, emitDirection));
        }

        private Vector2 GetEmitDirection()
        {
            float targetAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            return new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle));
        }

        private IEnumerator Eject(Transform target, Vector2 direction)
        {
            float elapsedTime = 0;

            float ejectSpeed = emitSpeed + Random.Range(-1f, 1f);

            while (elapsedTime < emitDuration)
            {
                if (target == null) yield break;

                elapsedTime += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsedTime / emitDuration);
                float currentSpeed = Mathf.Lerp(ejectSpeed, 0f, progress);

                target.position += (Vector3)(direction * currentSpeed * Time.deltaTime);

                yield return null;
            }
        }

        private bool TryGetParent(EntityType type, out Transform parent)
        {
            parent = null;

            foreach (var element in entityParents)
            {
                if (element.entityType == type)
                {
                    parent = element.parent;
                    return true;
                }
            }

            return false;
        }

        protected void SetEmitPoint(Vector3 position)
        {
            emitPoint.position = position;
        }

        public void DecreaseCount()
        {
            currentEntities -= 1;
        }
    }
}