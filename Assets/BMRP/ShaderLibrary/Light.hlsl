#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_OTHER_LIGHT_COUNT 64

CBUFFER_START(_CustomLight)
	int _DirectionalLightCount;
	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];

	int _OtherLightCount;
	float4 _OtherLightColors[MAX_OTHER_LIGHT_COUNT];
	float4 _OtherLightPositions[MAX_OTHER_LIGHT_COUNT];
	float4 _OtherLightDirections[MAX_OTHER_LIGHT_COUNT];
	float4 _OtherLightSpotAngles[MAX_OTHER_LIGHT_COUNT];

	float4 _OtherLightShadowData[MAX_OTHER_LIGHT_COUNT];
CBUFFER_END

struct Light
{
	float3 color;
	float3 direction;
	float attenuation;
};

int GetDirectionalLightCount ()
{
	return _DirectionalLightCount;
}

int GetOtherLightCount ()
{
	return _OtherLightCount;
}

DirectionalShadowData GetDirectionalShadowData (int lightIndex, const ShadowData shadowData)
{
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData[lightIndex].x;
	data.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex;
	data.normalBias = _DirectionalLightShadowData[lightIndex].z;
	data.shadowMaskChannel = _DirectionalLightShadowData[lightIndex].w;
	return data;
}

Light GetDirectionalLight (int index, const Surface surfaceWS, const ShadowData shadowData)
{
	Light light;
	light.color = _DirectionalLightColors[index].rgb;
	light.direction = _DirectionalLightDirections[index].xyz;
	const DirectionalShadowData dirShadowData = GetDirectionalShadowData(index, shadowData);
	light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, shadowData, surfaceWS);
	return light;
}

OtherShadowData GetOtherShadowData (int lightIndex)
{
	OtherShadowData data;
	data.strength = _OtherLightShadowData[lightIndex].x;
	data.tileIndex = _OtherLightShadowData[lightIndex].y;
	data.shadowMaskChannel = _OtherLightShadowData[lightIndex].w;
	data.lightPositionWS = 0.0;
	data.spotDirectionWS = 0.0;
	return data;
}

Light GetOtherLight (int index, const Surface surface, ShadowData shadowData)
{

	Light light;
	light.color = _OtherLightColors[index].rgb;
	float3 position = _OtherLightPositions[index].xyz;
	const float3 ray = position - surface.position;
	light.direction = normalize(ray);

	const float distanceSqr = max(dot(ray, ray), 0.00001);
	const float rangeAttenuation = Square(saturate(1.0 - Square(distanceSqr * _OtherLightPositions[index].w)));
	const float4 spotAngles = _OtherLightSpotAngles[index];
	float3 spotDirection = _OtherLightDirections[index].xyz;
	const float spotAttenuation = saturate(dot(spotDirection, light.direction) * spotAngles.x + spotAngles.y);

	OtherShadowData otherShadowData = GetOtherShadowData(index);
	otherShadowData.lightPositionWS = position;
	otherShadowData.spotDirectionWS = spotDirection;
	const float shadow = GetOtherShadowAttenuation(otherShadowData, shadowData, surface);
	light.attenuation = shadow * spotAttenuation * rangeAttenuation / distanceSqr;

	return light;
}

#endif