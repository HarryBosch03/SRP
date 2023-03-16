#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

#include "Surface.hlsl"
#include "Light.hlsl"

struct BRDF {
    float3 diffuse;
    float3 specular;
    float roughness;
};

#define MIN_REFLECTIVITY 0.04
float OneMinusReflectivity (const float metallic)
{
    const float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

BRDF GetBRDF (const Surface surface, const bool premultiplyAlpha = false)
{
    BRDF brdf;

    brdf.diffuse = surface.color * OneMinusReflectivity(surface.metallic);
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    brdf.roughness = surface.roughness;

    if (premultiplyAlpha)
        brdf.diffuse *= surface.alpha;
    
    return brdf;
}

float SpecularStrength (const Surface surface, const BRDF brdf, const Light light)
{
    const float3 h = SafeNormalize(light.direction + surface.viewDirection);
    const float nh2 = Square(saturate(dot(surface.normal, h)));
    const float lh2 = Square(saturate(dot(light.direction, h)));
    const float r2 = Square(brdf.roughness);
    const float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    const float normalization = brdf.roughness * 4.0 + 2.0;
	return r2 / (d2 * max(0.1, lh2) * normalization);
}

float3 DirectBRDF (const Surface surface, const BRDF brdf, const Light light)
{
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

#endif