struct v2f_Basic
{
	float4 pos	: SV_POSITION;
	half2 uv	: TEXCOORD1;
	half3 vU    : TEXCOORD2;
};

uniform fixed4 _MainTex_ST;
uniform fixed _DiffuseAdj;
uniform fixed4 _AddColor;

v2f_Basic vertBasic(appdata_full v)
{
	v2f_Basic o;

	half3 vU = normalize(mul(UNITY_MATRIX_MV, float4(v.vertex.xyz, 1.0)).xyz);
	float3 worldpos = mul(_Object2World, float4(v.vertex.xyz,1)).xyz;
	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

	half3 n = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal.xyz));
	o.vU = 0;
	half3 r = reflect(vU, n);
	fixed m = 2.0 * sqrt(r.x * r.x + r.y * r.y + (r.z + 1.0) * (r.z + 1.0));
	o.vU.xy = float2(r.x / m + 0.5, r.y / m + 0.5);

	return o;
}

uniform fixed4 _Color;
uniform fixed _Multi1;
uniform sampler2D _MainTex;
uniform sampler2D _MatCap;
uniform sampler2D _SecondaryLayer;
uniform fixed _HLPower;
uniform fixed _GlowFade;


fixed4 fragBasic(v2f_Basic i) : COLOR
{
	fixed4 albeto = tex2D(_MainTex, i.uv) * (1.0 + _DiffuseAdj);

	fixed alpha = 1;
	if (albeto.a < 0.02)
	{
		discard;
	}

#if WRITEALPHA
	alpha = albeto.a * 0.98;
#else
	alpha = 0.98 * _GlowFade;
#endif
	albeto = albeto * albeto;

	// basic matcap lighting
	fixed3 lighting = tex2D(_MatCap, i.vU.xy).rgb * _Multi1;
	fixed3 lightingSecondary = tex2D(_SecondaryLayer, i.vU.xy).rgb;

	// lighting formula
	fixed3 base = lighting;

	// multi - albeto
	base *= albeto.rgb * _Color *  (_AddColor + 1);

	// add - highlight
	base.rgb += lightingSecondary.ggg * _HLPower;

	base = sqrt(base);

	fixed brightness = dot(base.rgb, fixed3(0.33, 0.33, 0.33));
	base = base + pow(brightness, 1.5) * base * 0.3;

	return fixed4(base, alpha);
}

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
	#if UNITY_UV_STARTS_AT_TOP
	n.y *= -1;
	o.pos.xy += normalize(n.xy) * 0.003 * o.pos.w;;
	#else
	o.pos.xy += normalize(n.xy) * 0.003 * o.pos.w;;
	#endif

    o.color.r = (o.pos.z - _ProjectionParams.y - 0.0) / 5.0;
	return o;
}

fixed4 fragOutline(v2f_Outline i) : COLOR
{
	return fixed4(0,0,0,1.0 - i.color.r);
}

