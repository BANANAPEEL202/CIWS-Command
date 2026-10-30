
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
        [SerializeField] private float maxEngagementRange = 300f; // Maximum range to engage targets
        [SerializeField] private float minimumEngagementRange = 25; // Max dispersion angle in degrees
        [SerializeField] private float dispersion = 1.0f; // Max dispersion angle in degrees
        [SerializeField] private Target Target;

        public Transform targetPoint = null;
        private Rigidbody targetRigidbody = null; // To track the target's velocity
        private SoundController soundController; // Reference to the Sound Controller
        
        private float fireCooldown = 1f;
        private bool isShooting = false; // Tracks whether the turret is actively shooting

        private void Awake()
        {
            Application.targetFrameRate = 30;
            if (TurretAim != null) TurretAim.AimOrigin = firePoint;
            if (TurretAim == null)
                Debug.LogError(name + ": TurretController not assigned a TurretAim!");
            if (Bullet == null)
                Debug.LogError(name + ": TurretController not assigned a projectile prefab!");
            // Automatically find the CIWSSoundController on the same GameObject
            soundController = GetComponent<SoundController>();
            if (soundController == null)
                Debug.LogError(name + ": CIWSSoundController script is missing on the same GameObject!");
        }
        

        private void Update()
        {
            if (TurretAim == null || Bullet == null || bulletPool == null ||
                soundController == null || firePoint == null || Target == null || fireRate <= 0)
            {
                ResetFiring();
                return;
            }

            TurretAim.destroyed = !Target.isAlive;
            if (!Target.isAlive || TurretAim.IsIdle)
            {
                ResetFiring();
                return;
            }

            Transform previousTarget = targetPoint;
            FindTarget();
            if (targetPoint != previousTarget) ResetFiring();

            if (targetPoint == null)
            {
                TurretAim.AimPosition = firePoint.position + transform.forward * 100f;
                TurretAim.isAimed = false;
                ResetFiring();
                return;
            }

            // Tracking stays stable; dispersion belongs to the projectiles, not the aim target.
            TurretAim.AimPosition = PredictTargetPosition();
        }

        private void OnDisable() { ResetFiring(); }

        private void LateUpdate()
        {
            // Both aiming and targeting Updates have finished, regardless of script order.
            if (!CanFire())
            {
                ResetFiring();
                return;
            }

            fireCooldown -= Time.deltaTime;
            while (fireCooldown <= 0f)
            {
                if (!Shoot()) break;
                fireCooldown += 1f / fireRate;
                if (!isShooting) StartFiringSound();
            }
        }

        private bool CanFire()
        {
            return Target != null && Target.isAlive && targetPoint != null &&
                targetPoint.gameObject.activeInHierarchy && TurretAim != null &&
                firePoint != null && Bullet != null && bulletPool != null &&
                soundController != null && fireRate > 0 &&
                TurretAim.IsAimedAt(TurretAim.AimPosition);
        }

        private void ResetFiring()
        {
            // Waiting for alignment must never accumulate a catch-up burst.
            fireCooldown = fireRate > 0 ? 1f / fireRate : 0;
            StopFiringSound();
        }

        private bool Shoot()
        {
            // Check again at the point of emission, including for a direct caller.
            if (!CanFire()) return false;

            Vector3 direction = firePoint.forward;
            float distance = Vector3.Distance(firePoint.position, TurretAim.AimPosition);
            if (distance < 50f)
            {
                direction = Quaternion.Euler(
                    Random.Range(-dispersion, dispersion),
                    Random.Range(-dispersion, dispersion),
                    Random.Range(-dispersion, dispersion)) * direction;
            }
            else
            {
                direction = (direction * distance + new Vector3(
                    Random.Range(-dispersion, dispersion),
                    Random.Range(-dispersion, dispersion),
                    Random.Range(-dispersion, dispersion))).normalized;
            }

            // Spawn along the actual barrel axis; never snap bullets toward an unaligned target.
            GameObject projectile = bulletPool.GetBullet(firePoint.position,
                Quaternion.LookRotation(direction, firePoint.up));
            if (projectile == null)
            {
                ResetFiring();
                return false;
            }
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = direction * projectileSpeed;
            return true;
        }

        private float FindTarget()
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
                if (minimumEngagementRange < distance && distance < maxEngagementRange &&
                    distance < closestDistance && (!TurretAim.HasLimitedTraverse ||
                    (angleToTarget >= -TurretAim.LeftLimit && angleToTarget <= TurretAim.RightLimit)))
                {
                    closestDistance = distance;
                    closestTarget = missile.transform;
                    closestRigedBody = missile.GetComponent<Rigidbody>();
                }
            }
            targetPoint = closestTarget;
            targetRigidbody = closestRigedBody;
            return closestDistance;
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

        private void StartFiringSound()
        {
            if (soundController != null)
            {
                soundController.StartFiring();
                isShooting = true;
            }
        }

        private void StopFiringSound()
        {
            if (soundController != null)
            {
                soundController.StopFiring();
                isShooting = false;
            }
        }

    }
    
}
