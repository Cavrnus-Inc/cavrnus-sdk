Shader "Cavrnus/Internal/Teleportation"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Color (RGBA)", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _Culling("Culling", Float) = 0
    }
    SubShader
    {
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100
		Cull[_Culling]
		BlendOp Add, Add
		Blend SrcAlpha OneMinusSrcAlpha, OneMinusDstAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
	        	UNITY_VERTEX_INPUT_INSTANCE_ID

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO  
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
		        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UNITY_SETUP_INSTANCE_ID(i)
                half4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
