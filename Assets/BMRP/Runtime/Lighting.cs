using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string BufferName = "Lighting";

    private readonly CommandBuffer buffer = new()
    {
        name = BufferName
    };

    private const int MaxDirLightCount = 4;

    private static readonly int
        DirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        DirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        DirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    private static readonly Vector4[]
        DirLightColors = new Vector4[MaxDirLightCount],
        DirLightDirections = new Vector4[MaxDirLightCount];

    private CullingResults cullingResults;
    
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(BufferName);
        SetupLights();
        buffer.EndSample(BufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
        SetupLights();
    }

    private void SetupLights()
    {
        var visibleLights = cullingResults.visibleLights;
        int i;
        for (i = 0; i < visibleLights.Length; i++)
        {
            var light = visibleLights[i];
            if (light.lightType == LightType.Directional) SetupDirectionalLight(i, ref light);
            if (i >= MaxDirLightCount) break;
        }
        
        buffer.SetGlobalInt(DirLightCountId, i);
        buffer.SetGlobalVectorArray(DirLightColorsId, DirLightColors);
        buffer.SetGlobalVectorArray(DirLightDirectionsId, DirLightDirections);
    }

    private void SetupDirectionalLight(int index, ref VisibleLight light)
    {
        DirLightColors[index] = light.finalColor;
        DirLightDirections[index] = -light.localToWorldMatrix.GetColumn(2); 
    }
}