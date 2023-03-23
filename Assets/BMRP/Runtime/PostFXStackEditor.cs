using UnityEditor;
using UnityEngine;

namespace BMRP.Runtime
{
    public partial class PostFXStack
    {
        private partial void ApplySceneViewState ();

#if UNITY_EDITOR
        private partial void ApplySceneViewState ()
        {
            if (Camera.cameraType != CameraType.SceneView) return;
            if (SceneView.currentDrawingSceneView.sceneViewState.showImageEffects) return;
            
            settings = null;
        }
#endif
    }
}