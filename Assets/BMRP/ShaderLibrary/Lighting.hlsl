#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/Surface.hlsl"

float3 IncomingLight (const Surface surface, const Light light)
{
	return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting (const Surface surface, const BRDF brdf, const Light light)
{
	return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
}

float3 GetLighting (const Surface surface, const BRDF brdf)
{
	float3 color = 0.0f;
	for (int i = 0; i < GetDirectionalLightCount(); i++)
	{
		color += GetLighting(surface, brdf, GetDirectionalLight(i));
	}
	return color;
}

#endif