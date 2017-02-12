Shader "Unlit/Transparent-Cutoff-Double" {
	Properties {	
		_TintColor ("Tint Color", Color) = (1.0,1.0,1.0,1.0)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_Cutoff("Cutoff", float) = 0.5
	}
	SubShader {
		Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }
		Blend Off
		Cull Off
		Lighting Off
		ZWrite On
		Fog { Mode Off }
		LOD 100
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _MainTex_ST;
			fixed4 _TintColor;
			fixed _Cutoff;

			struct appdata {
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _TintColor;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, i.uv);
				clip(color.a - _Cutoff);
				return i.color * color;
			}
			
			ENDCG 
		}
	} 
	FallBack Off
	CustomEditor "CustomShaderGUI"
}
