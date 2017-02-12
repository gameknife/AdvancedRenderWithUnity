Shader "Character/BasicColor"
{
	Properties
	{
		#Group#Param("漫反射参数", Float) = 0
		_Color("叠加色", Color) = (1, 1, 1, 1)
		_MainTex("主颜色贴图#角色的颜色纹理", 2D) = "white" {}
		_MatCap("MATCAP#第一层环境映射", 2D) = "white" {}
		_Multi1("环境映射倍增", Range(0, 10)) = 1

		#Group#Param("高光参数", Float) = 0
		_SecondaryLayer("MATCAP#表面高光", 2D) = "black" {}
		_HLPower("二层高光强度", Range(0, 3)) = 1
	}

Subshader
{
	Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase" "Queue" = "Geometry+100" }
	Pass
	{
		//Tags{ "LightMode" = "Always" }
		Blend Off
		ColorMask RGBA

		CGPROGRAM
#pragma vertex vertBasic
#pragma fragment fragBasic
#pragma fragmentoption ARB_precision_hint_fastest
#pragma target 3.0

#pragma multi_compile_fwdbase

#include "UnityCG.cginc"
#include "WSShadow.cginc"
#include "GW_Basic.cginc"

		ENDCG
	}

	Pass
	{
	    		//Tags{ "LightMode" = "Always" }
        		Blend SrcAlpha OneMinusSrcAlpha
        		ColorMask RGB
        		Cull Front

        		CGPROGRAM
        #pragma vertex vertOutline
        #pragma fragment fragOutline
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma target 3.0

        #include "UnityCG.cginc"
        #include "WSShadow.cginc"
        #include "GW_Basic.cginc"

        		ENDCG
	}
}

Fallback "Diffuse"
CustomEditor "CustomShaderGUI"
}