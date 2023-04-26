using BoschingMachine.Logic.Scripts.Vitallity;
using UnityEngine;
using UnityEngine.Rendering;

namespace BoschingMachine.Logic.Scripts.Player
{
    public class PlayerHealthFX : MonoBehaviour
    {
        [SerializeField] private Volume damageVolume;
        [SerializeField] private float decay;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void Update()
        {
            damageVolume.weight -= damageVolume.weight * decay * Time.deltaTime;
            damageVolume.weight = Mathf.Max(damageVolume.weight, 0.0f);
        }

        private void OnEnable()
        {
            health.DamageEvent += OnDamage;
        }

        private void OnDisable()
        {
            health.DamageEvent -= OnDamage;
        }

        private void OnDestroy()
        {
            health.DamageEvent -= OnDamage;
        }

        private void OnDamage(Health.DamageArgs args)
        {
            damageVolume.weight += args.damage / 12.5f;
        }
    }
}
