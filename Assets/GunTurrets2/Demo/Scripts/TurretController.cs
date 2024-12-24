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
        [SerializeField] private float projectileSpeed = 50f; // Speed of the projectile
        [SerializeField] private BulletPool bulletPool = null; // Reference to the BulletPool

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
            if (Input.GetMouseButtonDown(0)) {
                TurretAim.IsIdle = !TurretAim.IsIdle;
            }
            if (TurretAim.IsIdle)
                return;
            FindTarget();
            if (targetPoint != null) {
                Vector3 predictedPosition = PredictTargetPosition();
                TurretAim.AimPosition = predictedPosition;

                if (TurretAim.IsAimed)
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
            else {
                // Reset aim position
                TurretAim.AimPosition = TurretAim.transform.position + TurretAim.transform.forward * 100f;
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

            GameObject projectile = bulletPool.GetBullet();
            if (projectile == null) {
                return;
            }

            // Set its position and velocity
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (TurretAim.AimPosition - firePoint.position).normalized;
                rb.linearVelocity = direction * projectileSpeed; // Set the velocity
            }
        }

        private Vector3 PredictTargetPosition()
        {
            if (targetPoint == null || targetRigidbody == null)
                return firePoint.position;

            Vector3 targetPosition = targetPoint.position;
            Vector3 targetVelocity = targetRigidbody.linearVelocity;
            Vector3 firePointPosition = firePoint.position;

            // Calculate the vector from the firePoint to the target
            Vector3 toTarget = targetPosition - firePointPosition;

            // Calculate the quadratic coefficients for time to intercept
            float a = targetVelocity.sqrMagnitude - projectileSpeed * projectileSpeed;
            float b = 2 * Vector3.Dot(toTarget, targetVelocity);
            float c = toTarget.sqrMagnitude;

            // Compute the discriminant of the quadratic equation
            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0 || Mathf.Abs(a) < 0.001f) // No real solution or target is stationary
            {
                // If there's no real solution, just return the target's current position
                return targetPosition;
            }

            // Calculate the two possible solutions for time to intercept
            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b - sqrtDiscriminant) / (2 * a);
            float t2 = (-b + sqrtDiscriminant) / (2 * a);
            // Use the smallest positive time to ensure the projectile intercepts
            // Ensure both t1 and t2 are positive before selecting the smallest one
            float t = Mathf.Infinity; // Start with a large value for t (to find the smallest positive t)

            if (t1 > 0) t = Mathf.Min(t, t1); // Use t1 if it's positive
            if (t2 > 0) t = Mathf.Min(t, t2); // Use t2 if it's positive

            // If t is still Infinity, there was no valid positive time (return target position)
            if (t == Mathf.Infinity)
            {
                return targetPosition;
            }

            // Predict the target's future position based on its velocity and time to intercept
            return targetPosition + targetVelocity * t;
        }

    }
}
