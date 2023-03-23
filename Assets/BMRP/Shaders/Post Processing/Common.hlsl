#pragma once

float2 Downscale (float2 uv, float2 size, int factor)
{
    return round(uv * size.xy / factor) / size.xy * factor;
}