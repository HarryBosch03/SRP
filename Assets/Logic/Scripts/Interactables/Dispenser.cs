using System.Collections;
using BoschingMachine.Logic.Scripts.Animbites;
using BoschingMachine.Logic.Scripts.Utility;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Interactables
{
    public class Dispenser : Interactable
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform spawnpoint;
        [SerializeField] private string itemName;
        [SerializeField] private string itemPlural;

        [Space] 
        [SerializeField] private float spawnDelay;
        [SerializeField] private string animationName;
        
        [Space]
        [SerializeField] private bool limitDeployment;
        [SerializeField] private int capacity;
        [SerializeField] private int usesLeft;
        [SerializeField] private float rechargeTime;
        [SerializeField] private int rechargeCount;

        [Space]
        [SerializeField]
        private Bounds clogBounds;

        [Space]
        [SerializeField]
        private SquashAnimbite squash;

        private float timer;
        private bool cloged;
        private bool locked;
        private Animator animator;

        public override bool CanInteract
        {
            get
            {
                if (cloged) return false;
                if (locked) return false;
                if (usesLeft <= 0 && limitDeployment) return false;

                return true;
            }
        }

        protected override string InoperableAppend => $"Out of {Plural}";

        private string Plural => string.IsNullOrEmpty(itemPlural) ? $"{itemName}s" : itemPlural;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (!limitDeployment) return;

            if (usesLeft < capacity)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0.0f;
            }

            usesLeft += rechargeCount * (int)(timer / rechargeTime);
            timer %= rechargeTime;

            if (usesLeft > capacity) usesLeft = capacity;
        }

        private void FixedUpdate()
        {
            cloged = CheckIfCloged();
        }

        private bool CheckIfCloged()
        {
            var center = transform.position + transform.rotation * clogBounds.center;
            var extents = clogBounds.extents;
            var rotation = transform.rotation;

            var results = Physics.OverlapBox(center, extents, rotation);
            foreach (var result in results)
            {
                if (!result.transform.IsChildOf(transform))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void FinishInteract(Biped.Biped biped)
        {
            if (usesLeft > 0 || !limitDeployment)
            {
                if (animator) animator.Play(animationName, 0, 0.0f);
                else squash.Play(this, transform);
                StartCoroutine(DeferredSpawn());
            }
        }

        public IEnumerator DeferredSpawn()
        {
            locked = true;
            yield return new WaitForSeconds(spawnDelay);
            
            var instance = Instantiate(prefab, spawnpoint.position, spawnpoint.rotation);
            instance.name = prefab.name;
            usesLeft--;
            cloged = true;
            timer = 0.0f;
            
            locked = false;
        }
        
        public override string BuildInteractString(string passthrough = "")
        {
            if (locked)
            {
                return TMPUtil.Color("Working", Color.gray);
            }
            if (cloged)
            {
                return TMPUtil.Color("Dispenser Jammed", Color.gray);
            }
            if (limitDeployment && usesLeft > 0)
            {
                return base.BuildInteractString($"Dispense {itemName} [{usesLeft}/{capacity} Left]");
            }
            else
            {
                return base.BuildInteractString($"Dispense {itemName}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(clogBounds.center, clogBounds.size);
            Gizmos.color *= new Color(1.0f, 1.0f, 1.0f, 0.1f);
            Gizmos.DrawCube(clogBounds.center, clogBounds.size);
        }
    }
}
