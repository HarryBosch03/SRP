using System.Collections;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.UI.Toasts
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class ToastOnEnable : MonoBehaviour
    {
        [SerializeField][TextArea] private string message;
        [SerializeField] private Toast.ToastLocation location;

        private void OnEnable()
        {
            StartCoroutine(DeferToast());
        }

        public IEnumerator DeferToast ()
        {
            yield return null;
            Toast.RaiseToast(message, location);
        }
    }
}
