using BoschingMachine.Logic.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoschingMachine.Logic.Scripts.Interactables
{
    [System.Serializable]
    public class Interactor
    {
        [SerializeField] private float interactRange;
        [SerializeField] private InteractorUIHandler uiHandler;

        public Interactable CurrentInteractable { get; private set; }

        private Biped.Biped biped;

        public void FixedUpdate ()
        {
            uiHandler.FixedUpdate();
        }

        public void Update(Biped.Biped biped, bool use)
        {
            uiHandler.UpdateUI(this, biped);
            if (use)
            {
                if (CurrentInteractable)
                {
                    TryInteract(biped, CurrentInteractable);
                }
                else if (TryGetLookingAt(biped, out var interactable))
                {
                    TryInteract(biped, interactable);
                }
            }
            else
            {
                if (CurrentInteractable) CurrentInteractable.CancelInteract(biped);
                CurrentInteractable = null;
            }
        }

        public bool TryGetLookingAt(Biped.Biped biped, out Interactable interactable)
        {
            var ray = new Ray(biped.Head.position, biped.Head.forward);
            if (Physics.Raycast(ray, out var hit, interactRange))
            {
                interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable)
                {
                    return true;
                }
            }

            interactable = null;
            return false;
        }

        private bool TryInteract(Biped.Biped biped, Interactable interactable)
        {
            this.biped = biped;

            if ((interactable.transform.position - biped.Head.position).sqrMagnitude > interactRange * interactRange)
            {
                CurrentInteractable = null;
                return false;
            }

            if (interactable.TryInteract(biped, null, FinishInteractCallback))
            {
                CurrentInteractable = interactable;
                return true;
            }

            return false;
        }

        private void FinishInteractCallback()
        {
            biped.Unfreeze();
            CurrentInteractable = null;
        }
    }

    [System.Serializable]
    public class InteractorUIHandler
    {
        [SerializeField] private CanvasGroup hoverGroup;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image progressBar;
        [SerializeField] private SeccondOrderDynamicsF spring;

        private float springTarget;

        public void FixedUpdate ()
        {
            spring.Loop(springTarget, null, Time.deltaTime);
        }

        public void UpdateUI(Interactor interactor, Biped.Biped biped)
        {
            springTarget = 1.0f;

            var interactable = interactor.CurrentInteractable;
            if (!interactable) interactor.TryGetLookingAt(biped, out interactable);

            if (interactable)
            {
                var text = interactable.BuildInteractString();
                if (!string.IsNullOrEmpty(text))
                {
                    label.text = interactable.BuildInteractString();
                    progressBar.transform.localScale = new Vector3(interactable.GetInteractPercent(biped), 1.0f, 1.0f);
                }
                else springTarget = 0.0f;
            }
            else springTarget = 0.0f;

            hoverGroup.transform.localScale = Vector3.one * spring.Position;

            if (spring.Position < 0.0f)
            {
                spring.Position = 0.0f;
                spring.Velocity = 0.0f;
            }
        }
    }
}
