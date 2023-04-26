using System.Collections.Generic;
using BoschingMachine.Logic.Scripts.Vitallity;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Weapons
{
    [System.Serializable]
    public class Gun
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private float damage;
        [SerializeField] private float firerate;
        [SerializeField] private float spray;
        [SerializeField] private GameObject hitPrefab;

        [Space]
        [SerializeField]
        private List<ParticleSystem> fx;

        private float lastFireTime;

        public void Shoot (GameObject shooter)
        {
            if (Time.time > lastFireTime + 60.0f / firerate)
            {
                var ray = new Ray(muzzle.position, Quaternion.Euler(Random.insideUnitSphere * spray) * muzzle.forward);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.transform.TryGetComponent(out Health health))
                    {
                        health.Damage(new Health.DamageArgs(shooter, damage));
                    }

                    Object.Instantiate(hitPrefab, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
                }

                fx.ForEach(e => e.Play());

                lastFireTime = Time.time;
            }
        }
    }
}
