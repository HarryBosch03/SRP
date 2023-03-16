#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#include "../ShaderLibrary/Surface.hlsl"

float3 GetLighting (Surface surface) {
    return surface.normal.y * surface.color;
}

#endif