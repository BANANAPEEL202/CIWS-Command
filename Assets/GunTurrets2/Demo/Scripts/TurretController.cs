using System;
using UnityEngine;

namespace GT2.Demo
{
    public class TurretController : MonoBehaviour
    {
        [SerializeField] private TurretAim TurretAim = null;

        public Transform TargetPoint = null;

        private bool isIdle = false;

        private void Awake()
        {
            if (TurretAim == null)
                Debug.LogError(name + ": TurretController not assigned a TurretAim!");
        }

        private void Update()
        {
            if (TurretAim == null)
                return;

            FindTarget();

            if (TargetPoint == null)
                TurretAim.IsIdle = TargetPoint == null;
            else
                TurretAim.AimPosition = TargetPoint.position;

            if (Input.GetMouseButtonDown(0))
                TurretAim.IsIdle = !TurretAim.IsIdle;
        }

        private void FindTarget()
        {
            GameObject[] missiles = GameObject.FindGameObjectsWithTag("Missile");
            float closestDistance = float.MaxValue;
            Transform closestTarget = null;

            foreach (GameObject missile in missiles)
            {
                float distance = Vector3.Distance(transform.position, missile.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = missile.transform;
                }
            }
            TargetPoint = closestTarget;
        }
    }
}
