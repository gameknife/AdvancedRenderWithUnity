Shader "Character/ShadowReceiver"
{
	Properties
	{	
	#Group#Param("数据纹理", Float) = 0

		[HideInInspector]_JitMap("抖动", 2D) = "white" {}
		_ShadowPhenomenon("半影大小", Range(0, 2)) = 0.5
		_ShadowStength("阴影强度", Range(0, 1)) = 0.8
		_DepthLayer("深度层级", Range(0, 100)) = 50
	}

Subshader
{
	Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase" "Queue"="Geometry+50"}
	Blend SrcAlpha OneMinusSrcAlpha
	ZWrite Off
	Pass
	{
		//Tags{ "LightMode" = "Always" }

		CGPROGRAM
#pragma vertex vertLite
#pragma fragment fragLite
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_fwdbase
#pragma target 3.0
#define MCSHADOWMAP
#include "UnityCG.cginc"
#include "WSShadow.cginc"
		struct v2f
	{
		float4 pos	: SV_POSITION;
		WS_SHADOW_COORDS(3)
		float2 post : TEXCOORD4;
	};

	uniform float _DepthLayer;

	v2f vertLite(appdata_full v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		//o.pos.z += (_DepthLayer - 50) * 0.0001;
		WS_TRANSFER_SHADOW(o);

		float4 p = o.pos; // equivalent to commented line above but without actual multiplication
		p.xy /= p.w;
		o.post.xy = p.xy;

		return o;
	}
	uniform float _ShadowStength;

	fixed4 fragLite(v2f i) : COLOR
	{
		i.post.xy = 0.5*(i.post.xy + 1.0) * (_ScreenParams.xy / float2(64,64));

		// jitered shadow map
		fixed attenraw = WS_SHADOW_ATTENUATION(i);
		fixed atten = _ShadowStength - attenraw * _ShadowStength;

		return fixed4(0,0,0, atten);
	}
		ENDCG
	}
}

Fallback "Diffuse"
CustomEditor "CustomShaderGUI"
}