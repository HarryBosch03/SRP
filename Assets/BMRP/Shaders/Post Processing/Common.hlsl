#pragma once

float2 Downscale (float2 uv, float maxV, float4 texelSize)
{
    float aspect = texelSize.z / texelSize.w;
    float2 tRes = float2(aspect * maxV, maxV);
    return floor(uv * tRes) / tRes;
}