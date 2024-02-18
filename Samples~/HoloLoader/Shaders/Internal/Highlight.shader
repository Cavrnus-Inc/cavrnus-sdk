Shader "Cavrnus/Highlight"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,.4)
		_EdgeColor("EdgeColor", Color) = (.5,1,1,.2)
		_Coverage("Coverage", Range(0,1)) = .1
		_Spacing("Spacing", Range(0, 5)) = .2
		_Speed("Speed", Range(0,10)) = 1
		_Edging("Edging", Range(0,5)) = .5
		_StartTime("StartTime", Float) = 0
		_RiseTime("RiseTime", Range(0,4)) = .1
		_HoldTime("HoldTime", Range(-1,100)) = -1
		_FallTime("FallTime", Range(0,4)) = 0
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            	float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(4)
                float4 vertex : SV_POSITION;
            	float3 normal : TEXCOORD2;
				float3 viewDir : TEXCOORD1;
				float3 worldPos : TEXCOORD3;
            };

		half4 _Color;
		half4 _EdgeColor;
		float _Coverage;
		float _Spacing;
		float _Speed;
		float _Edging;
		float _StartTime;
		float _RiseTime;
		float _HoldTime;
		float _FallTime;

            v2f vert (appdata v)
            {
            	v2f o;
            	UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

			float3 n = normalize(i.normal);
			// compute edge color and stripe color, add together.
			float edgeDot = pow(1.0-saturate(abs(dot(n, normalize(i.viewDir)))), 1.0 / (_Edging+.02));
			half4 edgeOut = _EdgeColor * edgeDot;

			float3 wpos = i.worldPos + _Time.yyy*_Speed;
			float stripeSum =
				pow(1.0 - abs(n.x), 5.0)*pow(2.0*abs(.5 - frac(wpos.x / _Spacing)), 1.0 / (_Coverage + .02)) +
				pow(1.0 - abs(n.y), 5.0)*pow(2.0*abs(.5 - frac(wpos.y / _Spacing)), 1.0 / (_Coverage + .02)) +
				pow(1.0 - abs(n.z), 5.0)*pow(2.0*abs(.5 - frac(wpos.z / _Spacing)), 1.0 / (_Coverage + .02));
			stripeSum = saturate(stripeSum);
			half4 stripeOut = _Color * stripeSum;

			half4 final = saturate(edgeOut + stripeOut);

			float timeEnvelope = 1.0;
			float timeSince = _Time.y - _StartTime;
			if (timeSince < _RiseTime)
			{
				timeEnvelope = saturate(timeSince / _RiseTime);
			}
			else if (_HoldTime < 0.0 || timeSince < _RiseTime + _HoldTime)
			{
				timeEnvelope = 1.0;
			}
			else
			{
				timeEnvelope = 1.0 - saturate((timeSince - _RiseTime - _HoldTime) / _FallTime);
			}			

			return fixed4(final.rgb, final.a*timeEnvelope);
            }
            ENDCG
        }
    }
}
