using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Boid : MonoBehaviour
{
    public BoidsGroup belongGroup;
    public float speed = 0.5f;
    public Vector3 direction;
    private bool _turning = false;
    private SphereCollider _sphereCollider;

    private void Start()
    {
        direction = transform.forward;
        _sphereCollider = GetComponent<SphereCollider>();
    }

    private void FixedUpdate()
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, transform.forward * 50, out hit))
        {
            _turning = true;
            direction = Vector3.Reflect(transform.forward, hit.normal);
        }
        else
        {
            _turning = false;
        }
    }

    private void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
            belongGroup.rotationSpeed * Time.deltaTime);
        transform.position += Mathf.Clamp(direction.sqrMagnitude / 100, speed, belongGroup.maxSpeed) * Time.deltaTime *
                              transform.forward;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, belongGroup.distancing/2);
    }
}