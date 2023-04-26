using BoschingMachine.Logic.Scripts.Signals;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Vitallity
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour
    {
        [SerializeField][Range(0.0f, 1.0f)] private float normalizedHealth;
        [SerializeField] private float maxHealth;

        [Space]
        [SerializeField]
        private bool destroyOnDeath;
        [SerializeField] private Signal deathSignal;

        public event System.Action<DamageArgs> DamageEvent;
        public event System.Action<DamageArgs> DeathEvent;

        public float CurrentHealth => normalizedHealth * maxHealth;
        public float MaxHealth => maxHealth;

        public bool Invulnerable { get; set; }

        private void OnEnable()
        {
            normalizedHealth = 1.0f;
        }

        public void Damage(DamageArgs args)
        {
            if (Invulnerable) return;

            normalizedHealth -= args.damage / MaxHealth;

            DamageEvent?.Invoke(args);

            if (normalizedHealth < 0.0f)
            {
                Die(args);
            }
        }

        private void Die(DamageArgs args)
        {
            DeathEvent?.Invoke(args);
            if (deathSignal) deathSignal.Raise(this, args);

            if (destroyOnDeath) Destroy(gameObject);
            else gameObject.SetActive(false);
        }

        public class DamageArgs : System.EventArgs
        {
            public GameObject damager;
            public float damage;

            public DamageArgs(GameObject damager, float damage)
            {
                this.damager = damager;
                this.damage = damage;
            }
        }
    }
}
