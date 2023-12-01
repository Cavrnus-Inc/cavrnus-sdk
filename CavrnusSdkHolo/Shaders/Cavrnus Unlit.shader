Shader "Cavrnus/Unlit"
{

    Properties
    {
    	[Toggle(_CUTOUT)] Cutout ("Enable cutout", Float) = 0
    	[Toggle(_FADE)] Fade ("Enable dithering fade", Float) = 0
    	[Toggle(_SMOOTHFADE)] SmoothFade ("Enable smooth fade", Float) = 0
    	[HideInInspector][Toggle(_CUTTINGPLANE)] CuttingPlane ("Enable Cutting Plane", Float) = 0

        [MainColor]_Color("Color", Color) = (1,1,1,1)
        [MainTexture] _MainTex("Base Map", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Float) = 0.2
        _Transparency("Transparency", Range(0,1)) = 0
        _EmissionStrength("Emission Strength", Range(0, 100)) = 0
        [HideInInspector][PerRendererData] _CullVolCenter0("Culling Volume Center Point 0", Vector) = (0,0,0,-1)
		[HideInInspector][PerRendererData] _CullVolX0("Culling Volume X Vector 0", Vector) = (1,0,0,-1)
		[HideInInspector][PerRendererData] _CullVolZ0("Culling Volume Z Vector 0", Vector) = (0,0,1,-1)
		[HideInInspector][PerRendererData] _CullVolSize0("Culling Volume Size 0", Vector) = (0,0,1,-1)
		[HideInInspector][PerRendererData] _CullVolBorder0("Culling Volume Border Size 0", Range(0,.1)) = .025
		[HideInInspector][PerRendererData] _CullVolBorderColor0("Culling Volume Border Size 0", Color) = (.8, .2, 0, 1)
		[HideInInspector][PerRendererData] _CullVolCenter1("Culling Volume Center Point 1", Vector) = (0,0,0,-1)
		[HideInInspector][PerRendererData] _CullVolX1("Culling Volume X Vector 1", Vector) = (1,0,0,-1)
		[HideInInspector][PerRendererData] _CullVolZ1("Culling Volume Z Vector 1", Vector) = (0,0,1,-1)
		[HideInInspector][PerRendererData] _CullVolSize1("Culling Volume Size 1", Vector) = (0,0,1,-1)
		[HideInInspector][PerRendererData] _CullVolBorder1("Culling Volume Border Size 1", Range(0,.1)) = .025
		[HideInInspector][PerRendererData] _CullVolBorderColor1("Culling Volume Border Size 1", Color) = (.8, .2, 0, 1)
		[HideInInspector][PerRendererData] _CullVolCenter2("Culling Volume Center Point 2", Vector) = (0,0,0,-1)
		[HideInInspector][PerRendererData] _CullVolX2("Culling Volume X Vector 2", Vector) = (1,0,0,-1)
		[HideInInspector][PerRendererData] _CullVolZ2("Culling Volume Z Vector 2", Vector) = (0,0,1,-1)
		[HideInInspector][PerRendererData] _CullVolSize2("Culling Volume Size 2", Vector) = (0,0,1,-1)
		[HideInInspector][PerRendererData] _CullVolBorder2("Culling Volume Border Size 2", Range(0,.1)) = .0025
		[HideInInspector][PerRendererData] _CullVolBorderColor2("Culling Volume Border Size 2", Color) = (.8, .2, 0, 1)

        // Hidden properties - Generic
        [HideInInspector] _Surface("Surface", Float) = 1.0
        [HideInInspector] _Blend("Blend MOde", Float) = 0.0
        [HideInInspector] _AlphaClip("Alpha Clip", Float) = 0.0
        [HideInInspector] _BlendOp("BlendOp", Float) = 0.0
        [HideInInspector] _SrcBlend("Src Blend", Float) = 1.0
        [HideInInspector] _DstBlend("Dst Blend", Float) = 0.0
        [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
        [HideInInspector] _Cull("Cull Mode", Float) = 0.0


         }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry"  "RenderPipeline" = "UniversalPipeline" }
		ZWrite [_ZWrite]
		Blend [_SrcBlend] [_DstBlend]
		Cull[_Cull]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Includes/CuttingPlane.cginc"
            #include "Includes/Utilities.cginc"

            #pragma multi_compile_local _ _CUTOUT
            #pragma multi_compile_local _ _SMOOTHFADE _FADE
            #pragma multi_compile_local _ _CUTTINGPLANE
            #pragma shader_feature __ FOG_EXP2

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            	float4 worldSpacePos : TEXCOORD1;
                UNITY_FOG_COORDS(3)
				UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half _Cutoff;
                float _EmissionStrength;
            	float4 _Color;
                half _Transparency;
                sampler2D _MainTex;
                float4 _MainTex_ST;

                #if defined(_CUTTINGPLANE)
				    float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
				    float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
				    float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
				    float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;
                #endif

            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_OUTPUT(Varyings, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = UnityObjectToClipPos(IN.positionOS.xyz);
            	OUT.worldSpacePos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                UNITY_TRANSFER_FOG(OUT, OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {

                #if _CUTTINGPLANE
            	    bool withinBorder0 = handleCullingVolume(IN.worldSpacePos, _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorder0);
				    bool withinBorder1 = handleCullingVolume(IN.worldSpacePos, _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorder1);
				    bool withinBorder2 = handleCullingVolume(IN.worldSpacePos, _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorder2);
                    if (withinBorder0)
                        return _CullVolBorderColor0;
                    else if (withinBorder1)
                        return _CullVolBorderColor1;
                    else if (withinBorder2)
                        return _CullVolBorderColor2;
            	#endif

                half4 color = tex2D(_MainTex, IN.uv);
            	color *= _Color;

                #if _CUTOUT
                    if (color.a < _Cutoff)
                       discard;
                #endif

                half factor = 1 + _EmissionStrength;
                color = half4(color.r * factor, color.g * factor, color.b * factor, color.a);

                #if _FADE
                    float dither = Dither8x8Bayer( fmod(IN.positionHCS.x, 8), fmod(IN.positionHCS.y, 8) );
                    _Transparency = lerp(0, 1.01, _Transparency);
                    clip(dither - (_Transparency));
                #elif _SMOOTHFADE
                    color.a *= 1.0 - _Transparency;
                #else
                    color.a = 1.0;
                #endif

                UNITY_APPLY_FOG(IN.fogCoord, color);

                return color;

            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Includes/CuttingPlane.cginc"
            #include "Includes/Utilities.cginc"


            #pragma multi_compile_local _ _CUTOUT
            #pragma multi_compile_local _ _SMOOTHFADE _FADE
            #pragma multi_compile_local _ _CUTTINGPLANE

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 worldSpacePos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half _Cutoff;
                float _EmissionStrength;
            	float4 _Color;
                half _Transparency;
                sampler2D _MainTex;
                float4 _MainTex_ST;

                #if defined(_CUTTINGPLANE)
				    float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
				    float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
				    float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
				    float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;
                #endif

            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN)
                UNITY_TRANSFER_INSTANCE_ID(Varyings, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = UnityObjectToClipPos(IN.positionOS.xyz);
                OUT.worldSpacePos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN)

            	#if _CUTTINGPLANE
                    bool withinBorder0 = handleCullingVolume(IN.worldSpacePos, _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorder0);
                    bool withinBorder1 = handleCullingVolume(IN.worldSpacePos, _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorder1);
                    bool withinBorder2 = handleCullingVolume(IN.worldSpacePos, _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorder2);
                    if (withinBorder0)
                        return _CullVolBorderColor0;
                    else if (withinBorder1)
                        return _CullVolBorderColor1;
                    else if (withinBorder2)
                        return _CullVolBorderColor2;
                #endif

                half4 color = tex2D(_MainTex, IN.uv);

                #if _CUTOUT
                   if (color.a < _Cutoff)
                       discard;
                #endif

                #if _FADE
                    float dither = Dither8x8Bayer(fmod(IN.positionHCS.x, 8), fmod(IN.positionHCS.y, 8));
                    _Transparency = lerp(0, 1.01, _Transparency);
                    clip(dither - _Transparency);
                #elif _SMOOTHFADE
                    float alpha = color.a;
                    float dither = Dither8x8Bayer(fmod(IN.positionHCS.x, 8), fmod(IN.positionHCS.y, 8));
                    alpha *= dither;
                    _Transparency = lerp(0.01, 1.01, _Transparency);
                    clip(alpha - _Transparency);
                #endif

                return 1;
            }
            ENDHLSL
        }
    }
//    CustomEditor "CavUnlitGUI"
}