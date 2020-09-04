using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;
using Vector3 = UnityEngine.Vector3;

namespace DefaultNamespace
{
    public class BoidsGroup : MonoBehaviour
    {
        [SerializeField] public float distancing = 10.0f;
        [SerializeField] public float maxSpeed = 5f;
        [SerializeField] private float minSpeed = 1f;
        [SerializeField] private Transform tendToPlace;
        [SerializeField] private Transform fleeFromPlace;
        [SerializeField] private float applyRulesTimelapse = 0.2f;
        [SerializeField] private float centerSignificance = 1.0f;
        [SerializeField] private float distancingSignificance = 1.0f;
        public float rotationSpeed = 1.0f;

        private List<Boid> _boids = new List<Boid>();
        private WaitForSeconds _applyRulesTimelapseWaiter;
        private Vector3 _cachePositionSum;
        private Vector3 _cacheDirectionSum;
        private float _cacheVelocitySum;
        private Collider[] nearObjects;

        private void Start()
        {
            nearObjects = new Collider[_boids.Count];
            _applyRulesTimelapseWaiter = new WaitForSeconds(applyRulesTimelapse);
        }

        private void Update()
        {
            Profiler.BeginSample("Getting Cache Values");
            GetCacheValues();
            Profiler.EndSample();
            Profiler.BeginSample("Distancing");
            foreach (var boid in _boids)
            {
                boid.direction = new Vector3();
                DistancingBasedOnPhysics(boid);
            }

            Profiler.EndSample();
            Profiler.BeginSample("Centre of Mass");
            foreach (var boid in _boids)
            {
                GetCentreOfMass(boid);
            }

            Profiler.EndSample();
            Profiler.BeginSample("Match Speed");
            foreach (var boid in _boids)
            {
                MatchVelocity(boid);
            }

            Profiler.EndSample();
            Profiler.BeginSample("Limit Speed");
            foreach (var boid in _boids)
            {
                LimitSpeed(boid);
            }
            Profiler.EndSample();
            Profiler.BeginSample("Match Direction");
            foreach (var boid in _boids)
            {
                MatchDirection(boid);
                TendToPlace(boid);
                FleeFromPlace(boid);
            }
            Profiler.EndSample();
        }

        IEnumerator UpdateBoidGroup()
        {
            var even = false;
            while (true)
            {
                // Debug.Log("UpdateBoid");
                GetCacheValues();
                for (int i = 0; i < _boids.Count; i++)
                {
                    //GetCentreOfMass(_boids[i], centerSignificance);
                    Distancing(_boids[i], distancingSignificance);
                    // MatchVelocity(_boids[i]);
                    // TendToPlace(_boids[i]);
                }

                yield return _applyRulesTimelapseWaiter;
            }
        }

        public void AddToGroup(Boid boid)
        {
            if (boid.belongGroup != null) boid.belongGroup.RemoveFromGroup(boid);
            boid.belongGroup = this;
            _boids.Add(boid);
        }

        private void RemoveFromGroup(Boid boid)
        {
            _boids.Remove(boid);
        }

        private void GetCacheValues()
        {
            _cacheVelocitySum = 0;
            _cachePositionSum = new Vector3();
            _cacheDirectionSum = new Vector3();
            foreach (var boid in _boids)
            {
                _cachePositionSum += boid.transform.position;
                _cacheVelocitySum += boid.speed;
                _cacheDirectionSum += boid.direction;
            }
        }

        private void GetCentreOfMass(Boid boid, float significance = 1.0f)
        {
            if (significance == 0.0f) return;

            var bPosition = boid.transform.position;
            Vector3 centre = (_cachePositionSum - bPosition) / (_boids.Count - 1);
            var distance = (centre - bPosition) * significance;

            boid.direction += distance;
        }

        private void DistancingBasedOnPhysics(Boid boid, float significance = 1.0f)
        {
            if (significance == 0.0f) return;

            Vector3 c = new Vector3();
            var amount = Physics.OverlapSphereNonAlloc(boid.transform.position, distancing, nearObjects);
            if (amount > 0)
            {
                for (int i = 0; i < amount; i++)
                {
                    var outDirection = boid.transform.position - nearObjects[i].gameObject.transform.position;
                    var distance = Vector3.Magnitude(outDirection);
                    if (distance < distancing && distance != 0)
                    {
                        // If there are too near make them go different directions
                        c += (distancing / (distancing - distance)) * outDirection;
                    }
                }
            }
            boid.direction += c;
        }

        private void Distancing(Boid boid, float significance = 1.0f)
        {
            if (significance == 0.0f) return;

            Vector3 c = new Vector3();
            foreach (var b in _boids)
            {
                if (b != boid)
                {
                    var outDirection = boid.transform.position - b.transform.position;
                    var distance = Vector3.Magnitude(outDirection);
                    if (distance < distancing)
                    {
                        c += (1 / (distancing - distance))  * outDirection;
                    }
                }
            }

            boid.direction += c;
        }

        private void MatchVelocity(Boid boid)
        {
            var matchingVelocity = (_cacheVelocitySum - boid.speed) / (_boids.Count - 1);
            boid.speed = (matchingVelocity + boid.speed) / 2;
        }

        private void LimitSpeed(Boid boid)
        {
            boid.speed = Mathf.Clamp(boid.speed, minSpeed, maxSpeed);
        }

        private void MatchDirection(Boid boid)
        {
            var matchingDirection = (_cacheDirectionSum - boid.direction) / (_boids.Count - 1);
            boid.direction = (matchingDirection + boid.direction) / 2;
        }

        private void TendToPlace(Boid boid, float significance = 1.0f)
        {
            boid.direction += (tendToPlace.position - boid.transform.position) * significance;
        }

        private void FleeFromPlace(Boid boid, float significance = 10.0f)
        {
            var direction = boid.transform.position - fleeFromPlace.position;
            var sqrDistancing = distancing * distancing * significance;
            if (direction.sqrMagnitude < sqrDistancing)
            {
                var percent = (sqrDistancing - direction.sqrMagnitude) / sqrDistancing;
                boid.direction = percent * direction + (percent - 1) * boid.direction;
                boid.speed = percent * maxSpeed;
            }
        }
    }
}