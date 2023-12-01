// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skybox/Horizontal Skybox"
{
    Properties
    {
        _Color1 ("Top Color", Color) = (1, 1, 1, 0)
        _Color2 ("Horizon Color", Color) = (1, 1, 1, 0)
        _Color3 ("Bottom Color", Color) = (1, 1, 1, 0)
        _Exponent1 ("Exponent Factor for Top Half", Float) = 1.0
        _Exponent2 ("Exponent Factor for Bottom Half", Float) = 1.0
        _Intensity ("Intensity Amplifier", Float) = 1.0
        _Rotation ("Rotation", Range(0,360)) = 0
        _FogCol("Fog Colour", Color) = (.5, .5, .5, .5)
        _FogStart("Fog Begin", Range(0,1)) = 0
        _FogEnd("Fog End", Range(0,1)) = .4
        _FogDensity("Fog Density", Range(0,1)) = 0
        _MieIntensity("Mie Intensity", Range(0,2)) = 0
        _MieTint("Mie Tint", Color) = (.5, .5, .5, .5)
        _MieSize("Mie Size", Range(0,1)) = .8
        _SunDir("Sun Direction", Range(0,360)) = 0
        [Toggle]_FogBottom("Apply Fog To The Bottom Of The Sky?", float) = 0
    }


    SubShader
    {
        Tags
        {
            "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"


            half4 _Color1;
            half4 _Color2;
            half4 _Color3;
            half _Intensity;
            half _Exponent1;
            half _Exponent2;
            half4 _FogCol;
            half _FogStart;
            half _FogEnd;
            half _FogDensity;
            fixed _FogBottom;
            half _MieIntensity;
            half _SunDir;
            half4 _MieTint;
            half _MieSize;


            struct appdata
            {
                float4 position : POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float EaseOutInterop(float f)
            {
                return sqrt(1 - pow(f - 1, 2)); 
            }

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.position = UnityObjectToClipPos(v.position);
                o.texcoord = v.texcoord;
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                float p = normalize(i.texcoord).y;
                float p1 = 1.0f - pow(min(1.0f, 1.0f - p), _Exponent1);
                float p3 = 1.0f - pow(min(1.0f, 1.0f + p), _Exponent2);
                float p2 = 1.0f - p1 - p3;

                half3 c = (_Color1 * p1 + _Color2 * p2 + _Color3 * p3);
                half3 fogCol = _FogCol.rgb;
                float sina, cosa;
                sincos(_SunDir * UNITY_PI / 180.0, sina, cosa);
                fogCol *= _MieIntensity * smoothstep(1-_MieSize,1,saturate(dot(float2(cosa, sina), i.texcoord.xz)))*_MieTint.rgb * unity_ColorSpaceDouble.rgb+1;
                
                //remap fog density.
                
                float remappedDensity = EaseOutInterop(_FogDensity);
                //Apply Fog
                c = lerp(c, fogCol, saturate((1-smoothstep(_FogStart*2, _FogEnd*2, (_FogBottom==0?abs(i.texcoord.y):i.texcoord.y))) * remappedDensity));

                c *= _Intensity;
                return half4(c, 1);


            }
            ENDCG
        }
    }

    Fallback Off
}