Shader "Custom/GridSnap"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,.7)
		_SubColor("SubColor", Color) = (.8,.8,.8,.7)
		_SnapColor("SnapColor", Color) = (0,1,0,.7)
		_Spacing("Spacing", Range(0, 5)) = 1
		_SubPtsCount("SubPtsCount", Range(1, 100)) = 10
		_LineWidthMult("LineWidthMultiplier", Range(0, 10)) = 1
		_DistanceFadeBeginScreenPercent("DistanceFadeBeginScreenPercent", Range(0, 1)) = .3
		_DistanceFadeSourcePoint("DistanceFadeSourcePoint", Vector) = (0,0,0)
		_DistanceFadeMin("DistanceFadeMin", Range(0,1)) = .2
		_SnapLineXPos("SnapLineXPos", Vector) = (0, 0, 0)
		_SnapLineXDir("SnapLineXDir", Vector) = (1, 0, 0)
		_SnapLineYPos("SnapLinYPos", Vector) = (0, 0, 0)
		_SnapLineYDir("SnapLineYDir", Vector) = (0, 1, 0)
		_SnapLineZPos("SnapLineZPos", Vector) = (0, 0, 0)
		_SnapLineZDir("SnapLineZDir", Vector) = (0, 0, 1)
		_HackySnapLineShutoff("HackySnapLineShutoff", Range(0, 1)) = 1
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
			float3 centerPtScreenPos : TEXCOORD3;
			float3 ptScreenPos : TEXCOORD4;

			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};

		half4 _Color;
		half4 _SubColor;
		half4 _SnapColor;
		float _Coverage;
		float _Spacing;
		float _SubPtsCount;
		float _LineWidthMult;
		float3 _SnapLineXPos;
		float3 _SnapLineXDir;
		float3 _SnapLineYPos;
		float3 _SnapLineYDir;
		float3 _SnapLineZPos;
		float3 _SnapLineZDir;
		float _HackySnapLineShutoff;
		float _DistanceFadeBeginScreenPercent;
		float3 _DistanceFadeSourcePoint;
		float _DistanceFadeMin;

		VertexOut vert(Input v)
		{
			VertexOut o;

			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_OUTPUT(VertexOut, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.vertex = UnityObjectToClipPos(v.vertex);
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.viewDir = WorldSpaceViewDir(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			float4 centerPtScreenPos = mul(UNITY_MATRIX_V, float4(_DistanceFadeSourcePoint, 1));
			o.centerPtScreenPos = centerPtScreenPos.xyz;
			float3 ptScreenPos = UnityObjectToViewPos(v.vertex);
			o.ptScreenPos = ptScreenPos.xyz;
			return o;
		}

		fixed stripeValue(float3 pos, float3 n, float spacing, float linewidth, float screenDiff)
		{
			float screenDiffLineWidthMult = saturate(smoothstep(linewidth * 2.0, linewidth * 8.0, screenDiff));
			float screenDiffIntensityMult = saturate(smoothstep(linewidth * 32.0, linewidth * 2.0, screenDiff));

			float lineWidthAdjusted = linewidth * (1.0 + screenDiffLineWidthMult) * 5.0;

			float3 distanceFromLine = spacing * 2.0 * abs(.5 - frac((pos-spacing.xxx*.5) / spacing));
			float3 planarLimiter = pow(1 - float3(abs(n.x), abs(n.y), abs(n.z)), 2.0);

			float3 lineIntensityPerAxis = smoothstep(lineWidthAdjusted.xxx, (lineWidthAdjusted.xxx)*.25, distanceFromLine) * planarLimiter;

			return screenDiffIntensityMult * (lineIntensityPerAxis.x + lineIntensityPerAxis.y + lineIntensityPerAxis.z);
		}

		fixed planesStripeValue(float3 pos, float3 xPos, float3 xDir, float3 yPos, float3 yDir, float3 zPos, float3 zDir, float linewidth, float screenDiff)
		{
			float screenDiffLineWidthMult = saturate(smoothstep(linewidth * 2.0, linewidth * 8.0, screenDiff));
			float screenDiffIntensityMult = saturate(smoothstep(linewidth * 32.0, linewidth * 2.0, screenDiff));

			float lineWidthAdjusted = linewidth * (1.0 + screenDiffLineWidthMult) * 5.0;

			float3 distanceFromX = length(cross(xDir, pos - xPos));
			float3 distanceFromY = length(cross(yDir, pos - yPos));
			float3 distanceFromZ = length(cross(zDir, pos - zPos));

			float3 distanceFromLine = min(distanceFromX, min(distanceFromY, distanceFromZ)) + (_HackySnapLineShutoff * 100);

			float3 lineIntensityPerAxis = smoothstep(lineWidthAdjusted.xxx, (lineWidthAdjusted.xxx)*.25, distanceFromLine);

			return screenDiffIntensityMult * (lineIntensityPerAxis.x + lineIntensityPerAxis.y + lineIntensityPerAxis.z);
		}

		fixed4 frag(VertexOut i) : SV_Target
		{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

			float3 n = normalize(i.normal);
			 
			float3 wpos = i.worldPos;
			float2 wposDeriv = float2(length(ddx(wpos)), length(ddy(wpos)));
			float screenSpaceSkew = length(wposDeriv);

			float mainStripe = stripeValue(wpos, n, (_Spacing / _SubPtsCount), (_Spacing / _SubPtsCount) * .0125 * _LineWidthMult, screenSpaceSkew);
			float bigStripe = stripeValue(wpos, n, _Spacing, (_Spacing/_SubPtsCount)*.1 * _LineWidthMult, screenSpaceSkew);
			float snapStripe = planesStripeValue(wpos, _SnapLineXPos, _SnapLineXDir, _SnapLineYPos, _SnapLineYDir, _SnapLineZPos, _SnapLineZDir, (_Spacing / _SubPtsCount)*.1 * _LineWidthMult, screenSpaceSkew);

			mainStripe = min(1, mainStripe);
			bigStripe = min(1, bigStripe);
			snapStripe = min(1, snapStripe);

			mainStripe = max(0, mainStripe - snapStripe);
			bigStripe = max(0, bigStripe - snapStripe);
			
			fixed4 final = fixed4(_Color * mainStripe + _SubColor * bigStripe + _SnapColor * snapStripe);

			float alpha = saturate(bigStripe + snapStripe);

			float finalA = saturate(1 - (1 - alpha) * 2);

			float distanceFromCenter = length(i.centerPtScreenPos.xy/i.centerPtScreenPos.z - i.ptScreenPos.xy/i.ptScreenPos.z);
			float distanceFade = _DistanceFadeMin + (1.0 - _DistanceFadeMin) * (1.0 - saturate((distanceFromCenter - _DistanceFadeBeginScreenPercent) / (1.5*_DistanceFadeBeginScreenPercent + .01)));

			return fixed4(final.rgb * finalA, final.a*distanceFade);
		}

		ENDCG
		}
	}
	Fallback Off
}