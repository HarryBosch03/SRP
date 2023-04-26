using System.Collections;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Animbites
{
    [System.Serializable]
    public class SquashAnimbite
    {
        [SerializeField] private AnimationCurve curve;
        [SerializeField][Range(0.0f, 1.0f)] private float strength;
        [SerializeField] private float duration;
        [SerializeField] private Vector3 axis;

        public void Play (MonoBehaviour behaviour, Transform target)
        {
            behaviour.StartCoroutine(Play(target));
        }

        public IEnumerator Play (Transform target)
        {
            var originalScale = target.localScale;
            var axis = this.axis.normalized;

            var percent = 0.0f;
            while (percent < 1.0f)
            {
                var vScale = 1.0f + curve.Evaluate(percent) * strength;
                var hScale = Mathf.Sqrt(1 / vScale);

                var scale = Vector3.one * hScale;
                scale += axis * (vScale - Vector3.Dot(axis, scale));

                target.localScale = Vector3.Scale(originalScale, scale);

                percent += Time.deltaTime / duration;
                yield return null;
            }

            target.localScale = originalScale;
        }
    }
}
