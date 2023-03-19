using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using LightType = UnityEngine.LightType;

namespace BMRP.Runtime
{
    public partial class BmrPipeline
    {
        partial void InitializeForEditor();

#if UNITY_EDITOR

        private delegate void ExtractCallback<T>(Light light, ref T intermediate);
        private delegate void InitCallback<T>(ref T intermediate);
        private static void Init<T>(ref LightDataGI data, Light light, ExtractCallback<T> extract, InitCallback<T> init) where T : struct
        {
            var intermediate = new T();
            extract(light, ref intermediate);
            init(ref intermediate);
        }
        
        private static Lightmapping.RequestLightsDelegate lightDelegate =
            (Light[] lights, NativeArray<LightDataGI> output) =>
            {
                var lightData = new LightDataGI();
                for (var i = 0; i < lights.Length; i++)
                {
                    var light = lights[i];
                    switch (light.type)
                    {
                        case LightType.Directional:
                            Init<DirectionalLight>(ref lightData, light, LightmapperUtils.Extract, lightData.Init);
                            break;
                        case LightType.Point:
                            Init<DirectionalLight>(ref lightData, light, LightmapperUtils.Extract, lightData.Init);
                            break;
                        case LightType.Spot:
                        case LightType.Area:
                        case LightType.Disc:
                        default:
                            lightData.InitNoBake(light.GetInstanceID());
                            break;
                    }

                    output[i] = lightData;
                }
            };

#endif
}
}