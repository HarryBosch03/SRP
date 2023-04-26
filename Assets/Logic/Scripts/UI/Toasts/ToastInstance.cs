using System.Collections;
using BoschingMachine.Logic.Scripts.Utility;
using TMPro;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.UI.Toasts
{
    public sealed class ToastInstance
    {
        private const float duration = 5.0f;
        private const float animateTime = 0.4f;
        
        public string message;
        public float time;

        public TMP_Text textObject;

        public ToastInstance(Toast toast, string message, TMP_Text prefab, System.Action<ToastInstance> finishCallback)
        {
            this.message = message;
            time = Time.time;

            textObject = Object.Instantiate(prefab, toast.transform);
            textObject.name = "Toast Instance";
            textObject.gameObject.SetActive(true);
            textObject.text = message;

            toast.StartCoroutine(Loop(finishCallback));
        }

        public IEnumerator Loop(System.Action<ToastInstance> finishCallback)
        {
            var percent = 0.0f;
            while (percent < 1.0f)
            {
                AnimateIn(percent);
                percent += Time.deltaTime / animateTime;
                yield return null;
            }

            yield return new WaitForSeconds(duration);

            percent = 0.0f;
            while (percent < 1.0f)
            {
                AnimateOut(percent);
                percent += Time.deltaTime / animateTime;
                yield return null;
            }

            finishCallback(this);
        }

        public void AnimateIn(float p)
        {
            var s = Curves.EaseOutBack(p);
            textObject.transform.localScale = Vector3.one * s;
        }

        public void AnimateOut(float p)
        {
            var s = Curves.EaseOutBack(1.0f - p);
            textObject.transform.localScale = Vector3.one * s;
        }

        public ToastInstance Destroy()
        {
            Object.Destroy(textObject);
            return this;
        }
    }
}
