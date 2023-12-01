//Some code taken from Unity Skybox-Cubed.shader under the MIT license
Shader "Cavrnus/Skybox Fog"
{
Properties 
    {
    _Tint ("Tint Colour", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex ("Cubemap   (HDR)", 2D) = "grey" {}
    _FogCol("Fog Colour", Color) = (.5, .5, .5, .5)
    _FogStart("Fog Begin", Range(0,1)) = 0
    _FogEnd("Fog End", Range(0,1)) = .4
    _FogDensity("Fog Density", Range(0,1)) = 0
    _MieIntensity("Mie Intensity", Range(0,2)) = 0
    _MieTint("Mie Tint", Color) = (.5, .5, .5, .5)
    _MieSize("Mie Size", Range(0,1)) = .8
    _SunDir("Sun Direction", Range(0,360)) = 0
    [Toggle]_FogBottom("Apply Fog To The Bottom Of The Sky?", float) = 0
    _XScale("X Scale", Range(-1, 1)) = 1
    _YScale("Y Scale", Range (-1, 1)) = 1
    }
    
    SubShader 
    {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"

        sampler2D _Tex;
        half4 _Tint;
        half _Exposure;
        float _Rotation;
        half4 _FogCol;
        half _FogStart;
        half _FogEnd;
        half _FogDensity;
        half _MieIntensity;
        half _SunDir;
        half4 _MieTint;
        fixed _FogBottom;
        half _MieSize;
        fixed _XScale;
        fixed _YScale;

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        float EaseOutInterop(float f)
        {
            return sqrt(1 - pow(f - 1, 2)); 
        }
        
        struct appdata
        {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert (appdata v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            float3 dir = normalize(i.texcoord);
            float2 longlat = float2(atan2(dir.x, dir.z) + UNITY_PI, acos(-dir.y));
            float2 uv = longlat / float2(2.0 * UNITY_PI, UNITY_PI);
            uv.x *= _XScale;
            uv.y *= _YScale;

            half4 tex = tex2Dlod (_Tex, float4(uv.x, uv.y, 0, 0));
            half3 c = tex;
            c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;

            //Calculate final fog colour
            half3 fogCol = _FogCol.rgb;
            float sina, cosa;
            sincos(_SunDir * UNITY_PI / 180.0, sina, cosa);
            fogCol *= _MieIntensity * smoothstep(1-_MieSize,1,saturate(dot(float2(cosa, sina), i.texcoord.xz)))*_MieTint.rgb * unity_ColorSpaceDouble.rgb+1;

            float remappedDensity = EaseOutInterop(_FogDensity);
            //Apply Fog
            c = lerp(c, fogCol, saturate((1-smoothstep(_FogStart*2, _FogEnd*2, (_FogBottom==0?abs(i.texcoord.y):i.texcoord.y))) * remappedDensity));

            c *= _Exposure;
            return half4(c, 1);
        }
        ENDCG
    }
}
Fallback Off

}
