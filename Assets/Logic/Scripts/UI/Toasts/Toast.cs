using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.UI.Toasts
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class Toast : MonoBehaviour
    {
        [SerializeField] private TMP_Text template;
        [SerializeField] private ToastLocation location;
        [SerializeField] private int maxToasts;

        private List<ToastInstance> toasts;

        private static event System.Action<string, ToastLocation> ToastRaisedEvent;

        private void Awake()
        {
            toasts = new List<ToastInstance>();
            template.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            ToastRaisedEvent += OnToastRaised;
        }

        private void OnDisable()
        {
            ToastRaisedEvent -= OnToastRaised;
        }

        private void OnDestroy()
        {
            ToastRaisedEvent -= OnToastRaised;
        }

        private void OnToastRaised(string message, ToastLocation location)
        {
            if (this.location != location) return;
            toasts.Add(new ToastInstance(this, message, template, ToastFinishedCallback));

            if (maxToasts <= 0) return;

            if (toasts.Count > maxToasts)
            {
                toasts[0].Destroy();
                toasts.RemoveAt(0);
            }
        }

        private void ToastFinishedCallback(ToastInstance toast)
        {
            toasts.Remove(toast.Destroy());
        }

        public static void RaiseToast (string message, ToastLocation location)
        {
            ToastRaisedEvent?.Invoke(message, location);
        }

        public enum ToastLocation
        {
            List,
            Title
        }
    }
}
