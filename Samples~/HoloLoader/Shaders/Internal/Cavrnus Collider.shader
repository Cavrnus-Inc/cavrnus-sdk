Shader "Cavrnus/Internal/Collider"
{

  SubShader
    {
        Pass
        {
            Tags { "Queue"="Transparent" "RenderType"="None" "IgnoreProjector"="True" }
            ColorMask 0
            ZWrite Off
         
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            float4 vert () : SV_POSITION { return float4(0,0,0,0); }
            fixed4 frag() : SV_Target { return fixed4(0,0,0,0); }
            ENDCG
        }
    }
}