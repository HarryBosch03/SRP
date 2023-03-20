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
        
        private static Lightmapping.RequestLightsDelegate _lightDelegate = (lights, output) =>
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
                            Init(ref lightData, light, LightmapperUtils.Extract, (ref SpotLight l) =>
                            {
                                l.innerConeAngle = light.innerSpotAngle * Mathf.Rad2Deg;
                                l.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
                                lightData.Init(ref l);
                            });
                            break;
                        case LightType.Area:
                            Init(ref lightData, light, LightmapperUtils.Extract, (ref RectangleLight l) =>
                            {
                                l.mode = LightMode.Baked;
                                lightData.Init(ref l);
                            });
                            break;
                        case LightType.Disc:
                            //Init<DiscLight>(ref lightData, light, LightmapperUtils.Extract, lightData.Init);
                            throw new System.Exception("What the Fuck is a Disc Light!");
                        default:
                            lightData.InitNoBake(light.GetInstanceID());
                            break;
                    }

                    lightData.falloff = FalloffType.InverseSquared;
                    output[i] = lightData;
                }
            };

        partial void InitializeForEditor()
        {
            Lightmapping.SetDelegate(_lightDelegate);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Lightmapping.ResetDelegate();
        }

#endif
}
}