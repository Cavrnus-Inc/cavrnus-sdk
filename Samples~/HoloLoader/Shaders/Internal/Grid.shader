Shader "Custom/Grid"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,.7)
		_SubColor("SubColor", Color) = (.8,.8,.8,.7)
		_Spacing("Spacing", Range(0, 5)) = 1
		_SubPtsCount("SubPtsCount", Range(1, 100)) = 10
		_LineWidthMult("LineWidthMultiplier", Range(0, 10)) = 1
	}
		SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+100" }
		LOD 100
		Cull Off
		Blend SrcAlpha One
		ZWrite Off

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct Input
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	struct VertexOut
	{
		float4 vertex : POSITION;
		float3 normal : TEXCOORD0;
		float3 viewDir : TEXCOORD1;
		float3 worldPos : TEXCOORD2;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	half4 _Color;
	half4 _SubColor;
	float _Coverage;
	float _Spacing;
	float _SubPtsCount;
	float _LineWidthMult;

	VertexOut vert(Input v)
	{
		VertexOut o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.normal = UnityObjectToWorldNormal(v.normal);
		o.viewDir = WorldSpaceViewDir(v.vertex);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		return o;
	}

	fixed stripeValue(float3 pos, float3 n, float spacing, float linewidth, float screenDiff)
	{
		float screenDiffLineWidthMult = saturate(smoothstep(linewidth * 2.0, linewidth * 8.0, screenDiff));
		float screenDiffIntensityMult = saturate(smoothstep(linewidth * 32.0, linewidth * 2.0, screenDiff));

		float lineWidthAdjusted = linewidth * (1.0 + screenDiffLineWidthMult) * 5.0;

		float3 distanceFromLine = spacing * 2.0 * abs(.5 - frac((pos - spacing.xxx*.5) / spacing));
		float3 planarLimiter = pow(1 - float3(abs(n.x), abs(n.y), abs(n.z)), 5.0);

		float3 lineIntensityPerAxis = smoothstep(lineWidthAdjusted.xxx, (lineWidthAdjusted.xxx)*.25, distanceFromLine) * planarLimiter;

		return screenDiffIntensityMult * (lineIntensityPerAxis.x + lineIntensityPerAxis.y + lineIntensityPerAxis.z);
	}

	fixed4 frag(VertexOut i) : SV_Target
	{
		float3 n = normalize(i.normal);

		float3 wpos = i.worldPos;
		float2 wposDeriv = float2(length(ddx(wpos)), length(ddy(wpos)));
		float screenSpaceSkew = length(wposDeriv);

		float mainStripe = stripeValue(wpos, n, (_Spacing / _SubPtsCount), (_Spacing / _SubPtsCount) * .0125 * _LineWidthMult, screenSpaceSkew);
		float bigStripe = stripeValue(wpos, n, _Spacing, (_Spacing / _SubPtsCount)*.1 * _LineWidthMult, screenSpaceSkew);

		return fixed4(_Color * mainStripe + _SubColor * bigStripe);
	}

		ENDCG
	}
	}
		Fallback Off
}