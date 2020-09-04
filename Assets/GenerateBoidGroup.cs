using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    [RequireComponent(typeof(BoidsGroup))]
    public class GenerateBoidGroup : MonoBehaviour
    {
        [SerializeField] private GameObject spawnBoidObject;
        [SerializeField] private int amount = 10;
        [SerializeField] private float generationArea = 5.0f;

        private void OnEnable()
        {
            var group = GetComponent<BoidsGroup>();

            for (var i = 0; i < amount; i++)
            {
                var disp = new Vector3(
                    Random.Range(-generationArea, generationArea),
                    Random.Range(-generationArea, generationArea),
                    Random.Range(-generationArea, generationArea));

                var go = Instantiate(spawnBoidObject, transform.position + disp, Quaternion.identity, transform);
                group.AddToGroup(go.GetComponent<Boid>());
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, generationArea);
        }
    }
}