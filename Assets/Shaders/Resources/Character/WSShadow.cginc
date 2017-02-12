#include "AutoLight.cginc"

#define CONST_RANGE 0.005
#define CONST_RANGE_1 0.003
uniform float _ShadowPhenomenon;

#define SHADOW_SOBEL_0 half2( 0,  1 )
#define SHADOW_SOBEL_1 half2(1,  0 )
#define SHADOW_SOBEL_2 half2(0, -1 )
#define SHADOW_SOBEL_3 half2(-1,  0 )
#define SHADOW_SOBEL_4 half2(0.707,  0.707 )
#define SHADOW_SOBEL_5 half2( 0.707,  -0.707 )
#define SHADOW_SOBEL_6 half2(-0.707,  0.707 )
#define SHADOW_SOBEL_7 half2(-0.707,  -0.707 )

sampler2D _JitMap;

#if defined (SHADOWS_DEPTH)
#elif defined (SHADOWS_SCREEN)
#else
UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);
#endif

#if !defined (SHADER_API_D3D11_9X)

inline fixed SoftSampleShadow(float4 shadowCoord, float2 screenpos)
{
	float3 coord = shadowCoord.xyz / shadowCoord.w;

	half4 jitRaw = tex2D(_JitMap, screenpos.xy);
	half2 jitRand1 = normalize(jitRaw.xy);
	half2 jitRand2 = normalize(jitRaw.zw);

#if defined (SHADOWS_NATIVE)
	fixed4 shadows;
	shadows.x = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + half4(reflect(SHADOW_SOBEL_0*CONST_RANGE*_ShadowPhenomenon, jitRand1), 0,0));
	shadows.y = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + half4(reflect(SHADOW_SOBEL_1*CONST_RANGE*_ShadowPhenomenon, jitRand2), 0, 0));
	shadows.z = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + half4(reflect(SHADOW_SOBEL_2*CONST_RANGE*_ShadowPhenomenon, jitRand1), 0, 0));
	shadows.w = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + half4(reflect(SHADOW_SOBEL_3*CONST_RANGE*_ShadowPhenomenon, jitRand2), 0, 0));
	shadows = _LightShadowData.rrrr + shadows * (1 - _LightShadowData.rrrr);

	fixed4 shadows1;
	shadows1.x = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + half4(reflect(SHADOW_SOBEL_4*CONST_RANGE_1*_ShadowPhenomenon, jitRand1), 0, 0));
	shadows1.y = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + half4(reflect(SHADOW_SOBEL_5*CONST_RANGE_1*_ShadowPhenomenon, jitRand2), 0, 0));
	shadows1.z = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + half4(reflect(SHADOW_SOBEL_6*CONST_RANGE_1*_ShadowPhenomenon, jitRand1), 0, 0));
	shadows1.w = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, coord + half4(reflect(SHADOW_SOBEL_7*CONST_RANGE_1*_ShadowPhenomenon, jitRand2), 0, 0));
	shadows1 = _LightShadowData.rrrr + shadows1 * (1 - _LightShadowData.rrrr);
#else
	fixed4 shadows = 0;
	fixed4 shadows1 = 0;
#endif
	// average-4 PCF
	fixed shadow = dot(shadows, fixed4(0.125f, 0.125f, 0.125f, 0.125f));

	//return shadow * 2.0;

	fixed shadow1 = dot(shadows1, fixed4(0.125f, 0.125f, 0.125f, 0.125f));
	return shadow + shadow1;
}

#endif

#if defined (SHADOWS_DEPTH) && defined (SPOT)
#define SHADOW_ATTENUATION_SOFT(a) SoftSampleShadow(a._ShadowCoord, a.post)
#elif defined (SHADOWS_SCREEN)
#define SHADOW_ATTENUATION_SOFT(a) SoftSampleShadow(a._ShadowCoord, a.post)
#else
#define SHADOW_ATTENUATION_SOFT(a) 1
#endif

#if defined (MCSHADOWMAP)
#define WS_SHADOW_COORDS(a) SHADOW_COORDS(a)
#define WS_TRANSFER_SHADOW(o) TRANSFER_SHADOW(o)
#define WS_SHADOW_ATTENUATION(a) SHADOW_ATTENUATION_SOFT(a);
#else
#define WS_SHADOW_COORDS(a)
#define WS_TRANSFER_SHADOW(o)
#define WS_SHADOW_ATTENUATION(a) 1.0
#endif