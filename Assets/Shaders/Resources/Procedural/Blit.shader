 Shader "Hidden/Blit" {
	Properties {
		_MainTex ("-", 2D) = "black" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};	
	
	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;

	v2f vert( appdata_full v ) 
	{
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		o.uv.xy = v.texcoord.xy;
		
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1-o.uv.y;
		#endif			
		
		return o;
	} 

	void fragGeneralGrey (v2f i
	, out fixed4 ocol : SV_Target
	)
	{	
		fixed4 color = tex2D(_MainTex, i.uv.xy);
		ocol.rgba = color;
	} 
		
	ENDCG
	
Subshader 
	{

		// pass 0

		Pass {
			 ZTest Off Cull Off ZWrite Off
			 Fog { Mode off }

			 CGPROGRAM

			 #pragma fragmentoption ARB_precision_hint_fastest
			 #pragma glsl_no_auto_normalization
			 #pragma vertex vert
			 #pragma fragment fragGeneralGrey
			 #pragma exclude_renderers d3d11_9x flash

			 ENDCG
		   }
	}
  
Fallback off

}