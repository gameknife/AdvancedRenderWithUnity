Shader "Scene/SceneObject_MultiLayer_CM"
{
	Properties
	{
		#Group#Param("漫反射参数", Float) = 0
		_Color("叠加色", Color) = (1, 1, 1, 1)
		_MainTex("主颜色贴图#角色的颜色纹理", 2D) = "white" {}
		_DetailNormal("反射扰动", 2D) = "white" {}

		_MatCap("MATCAP#第一层环境映射", Cube) = "white" {}
		_Multi1("环境映射倍增", Range(0, 10)) = 1

		_MaskTex("Mask贴图#未Mask区域为Unlit", 2D) = "white" {}

		_IllumColor("自发光色", Color) = (1, 1, 1, 1)
	}

Subshader
{
	Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
	Pass
	{
		Blend Off
        ColorMask RGBA
		CGPROGRAM
#pragma vertex vertBasic
#pragma fragment fragBasic
#pragma fragmentoption ARB_precision_hint_fastest
#pragma target 3.0

#include "UnityCG.cginc"

		struct v2f_Basic
		{
			float4 pos	: SV_POSITION;
			float2 uv	: TEXCOORD1;
			float3 vU : TEXCOORD2;
			float3 vNW : TEXCOORD3;
			float3 vPW : TEXCOORD4;
			float3 hpos	: TEXCOORD5;
		};

		uniform float4 _MainTex_ST;
		uniform fixed _DiffuseAdj;
		uniform float4 _AddColor;

		v2f_Basic vertBasic(appdata_full v)
		{
			v2f_Basic o;

			float3 worldpos = mul(_Object2World, float4(v.vertex.xyz,1)).xyz;
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			float3 n = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal.xyz));
			o.vNW = normalize(mul((float3x3)_Object2World, v.normal.xyz));
			o.vPW = worldpos;
            o.hpos.xyz = o.pos.xyw;
            o.hpos.x = -o.hpos.x;

            #if UNITY_UV_STARTS_AT_TOP
            o.hpos.y = -o.hpos.y;
            #endif

            o.hpos.xy = (o.hpos.zz * 1.0 - o.hpos.xy) * 0.5;
			return o;
		}


		uniform fixed4 _Color;
		uniform fixed4 _IllumColor;
		uniform float _Multi1;

		uniform sampler2D _MainTex;
		uniform sampler2D _MaskTex;
		uniform samplerCUBE _MatCap;
		uniform sampler2D _MatCap2;
		uniform sampler2D _DetailNormal;
		uniform sampler2D ReflectionTex;

		fixed4 fragBasic(v2f_Basic i) : COLOR
		{
			fixed4 albeto = tex2D(_MainTex, i.uv) * (1.0 + _DiffuseAdj);
			fixed3 normal_scatter = tex2D(_DetailNormal, i.uv * 30.0) * 2 - 1;
			normal_scatter *= 4.0;
			fixed3 mask = tex2D(_MaskTex, i.uv).rgb;

			i.hpos.xy = i.hpos.xy / i.hpos.z;
            fixed4 refl = tex2D(ReflectionTex, i.hpos.xy + normal_scatter.xy * 0.025);


			float3 viewDir = normalize(i.vPW - _WorldSpaceCameraPos);
			float3 normalDir = normalize(i.vNW);

			float ndote = 1.0 - saturate( dot(-viewDir, normalDir) );
			ndote = pow(ndote, 2) * 2;

			// basic matcap lighting
			fixed3 lighting = texCUBE(_MatCap, reflect(i.vNW, viewDir)).rgb * _Multi1;
		
			// lighting formula
			fixed3 base = lerp(0, lighting, mask.g);

			// multi - albeto
			base += lerp(albeto.rgb * _Color, refl, ndote * mask.g);

            // illum
            base.rgb += _IllumColor.rgb * mask.r;

            //return fixed4(refl.rgb, 1);
			return fixed4(base.rgb, mask.r);
		}



		ENDCG
	}


	pass
	{

        		Blend SrcAlpha OneMinusSrcAlpha
        		ColorMask RGB
        		Cull Front

        		CGPROGRAM
        #pragma vertex vertOutline
        #pragma fragment fragOutline
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma target 3.0

        #include "UnityCG.cginc"

#include "UnityCG.cginc"

       struct v2f_Outline
       {
       	float4 pos	: SV_POSITION;
       	fixed4 color : COLOR;
       };

       v2f_Outline vertOutline(appdata_full v)
       {
       	v2f_Outline o;

       	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
       	half3 n = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal.xyz));
       	// projectspace expanse
           o.pos.xy += normalize(n.xy) * 0.002 * o.pos.w;;
           o.color.r = (o.pos.z - _ProjectionParams.y - 0.0) / 5.0;
       	return o;
       }

       fixed4 fragOutline(v2f_Outline i) : COLOR
       {
       	return fixed4(0,0,0,1.0 - i.color.r);
       }

        		ENDCG




	}
}

Fallback "Diffuse"
CustomEditor "CustomShaderGUI"
}