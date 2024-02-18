Shader "Custom/PointCloud"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Color (RGB)", 2D) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100
		Cull Off

		Pass
		{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct Input
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float4 color : COLOR;
		};
		struct VertexOut
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 color : TEXCOORD1;
		};

		half4 _Color;
		sampler2D _MainTex;
		float4 _MainTex_ST;

		VertexOut vert(Input v)
		{
			VertexOut o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.color = v.color;
			return o;
		}

		fixed4 frag(VertexOut i) : SV_Target
		{
			// sample the texture
			fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
			return col;
		}

		ENDCG
		}
	}
		Fallback Off
}