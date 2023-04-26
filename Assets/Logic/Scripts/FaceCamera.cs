using UnityEngine;
using UnityEngine.Rendering;

namespace BoschingMachine.Logic.Scripts
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class FaceCamera : MonoBehaviour
    {
        [SerializeField] private bool flip;

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraaRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraaRendering;
        }

        private void OnBeginCameraaRendering(ScriptableRenderContext context, Camera cam)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.forward * (flip ? 1 : -1), Vector3.up);
        }
    }
}
