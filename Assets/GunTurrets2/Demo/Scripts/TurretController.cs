using System;
using UnityEngine;

namespace GT2.Demo
{
    public class TurretController : MonoBehaviour
    {
        [SerializeField] private TurretAim TurretAim = null;
        [SerializeField] private Transform firePoint = null; // Firing point
        [SerializeField] private GameObject Bullet = null; // Projectile prefab
        [SerializeField] private float fireRate = 1f; // Fire rate in shots per second
        [SerializeField] private float projectileSpeed = 20f; // Speed of the projectile
        [SerializeField] private Transform Barrel = null; // Reference to the barrel's transform

        public Transform targetPoint = null;
        private Rigidbody targetRigidbody = null; // To track the target's velocity

        private float fireCooldown = 0f;

        private void Awake()
        {
            if (TurretAim == null)
                Debug.LogError(name + ": TurretController not assigned a TurretAim!");
            if (Bullet == null)
                Debug.LogError(name + ": TurretController not assigned a projectile prefab!");
        }

        private void Update()
        {
            if (TurretAim == null || Bullet == null)
                return;

            FindTarget();
            if (targetPoint == null) {
                TurretAim.IsIdle = targetPoint == null;
            }
            else {

                Vector3 predictedPosition = PredictTargetPosition();
                TurretAim.AimPosition = predictedPosition;

                // Check if the turret is aimed correctly at the predicted target position
                float aimAccuracy = 5f; // The maximum acceptable angle for firing (in degrees)
                Vector3 directionToTarget = predictedPosition - firePoint.position;
                float angle = Vector3.Angle(Barrel.forward, directionToTarget);

                if (angle <= aimAccuracy)
                {
                    // Attempt to shoot at the target
                    fireCooldown -= Time.deltaTime;
                    if (fireCooldown <= 0f)
                    {
                        Shoot();
                        fireCooldown = 1f / fireRate; // Reset cooldown
                    }
                }
                    }


                    if (Input.GetMouseButtonDown(0)) {
                        TurretAim.IsIdle = !TurretAim.IsIdle;
                    }
        }

        private void FindTarget()
        {
            GameObject[] missiles = GameObject.FindGameObjectsWithTag("Missile");
            float closestDistance = float.MaxValue;
            Transform closestTarget = null;
            Rigidbody closestRigedBody = null;

            foreach (GameObject missile in missiles)
            {
                float distance = Vector3.Distance(transform.position, missile.transform.position);

                // Calculate the angle to the target in the horizontal plane
                Vector3 targetDirection = missile.transform.position - transform.position;
                Vector3 flattenedDirection = Vector3.ProjectOnPlane(targetDirection, transform.up);
                float angleToTarget = Vector3.SignedAngle(transform.forward, flattenedDirection, transform.up);

                // Check if the target is within the traverse range
                if (distance < closestDistance && Mathf.Abs(angleToTarget) <= TurretAim.RightLimit && Mathf.Abs(angleToTarget) >= -TurretAim.LeftLimit)
                {
                    closestDistance = distance;
                    closestTarget = missile.transform;
                    closestRigedBody = missile.GetComponent<Rigidbody>();
                }
            }
            targetPoint = closestTarget;
            targetRigidbody = closestRigedBody;
        }

        private void Shoot()
        {
            if (targetPoint == null || TurretAim.IsIdle)
                return;

            // Instantiate a projectile and set its initial velocity toward the target
            GameObject projectile = Instantiate(Bullet, firePoint.position, firePoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (TurretAim.AimPosition - firePoint.position).normalized;
                rb.linearVelocity = direction * projectileSpeed;
            }
        }

        private Vector3 PredictTargetPosition()
        {
            if (targetPoint == null || targetRigidbody == null)
                return firePoint.position;

            Vector3 targetPosition = targetPoint.position;
            Vector3 targetVelocity = targetRigidbody.linearVelocity;
            Vector3 firePointPosition = firePoint.position;

            // Calculate time to intercept
            Vector3 toTarget = targetPosition - firePointPosition;
            float a = targetVelocity.sqrMagnitude - projectileSpeed * projectileSpeed;
            float b = 2 * Vector3.Dot(toTarget, targetVelocity);
            float c = toTarget.sqrMagnitude;
            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0 || Mathf.Abs(a) < 0.001f) // No real solution or target stationary
                return targetPosition;

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2 * a);
            float t2 = (-b + sqrtDiscriminant) / (2 * a);

            // Use the smallest positive time
            float t = Mathf.Max(0, Mathf.Min(t1, t2));

            // Predict future position
            return targetPosition + targetVelocity * t;
        }

    }
}
