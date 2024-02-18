Shader "Cavrnus/Terrain"
{
    Properties
    {
        [Toggle(_LIGHTMAP)] Lightmap("Enable lightmap", Float) = 0

        [NoScaleOffset]_MainTex("Fallback AlbedoMap", 2D) = "white" {}
        [NoScaleOffset]_NormalMap("Fallback Normal", 2D) = "bump" {}
        [NoScaleOffset]_Splatmap ("Splat Map (RGBA)", 2D) = "black" {}
        _Layer_0_Color ("Layer 0 Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_Layer_0_AlbedoMap ("Layer 0 Albedo Map (R)", 2D) = "white" {}
        [NoScaleOffset]_Layer_0_NormalMap("Layer 0 Normal Map", 2D) = "bump" {}
        _Layer_0_NormalStrength("Layer 0 Normal Strength", Range(0, 10)) = 1
        _Layer_0_FresnelStrength("Fresnel Strength", Range(0, 1)) = .1

        [NoScaleOffset]_Layer_0_Mask ("Layer 0 Mask Map", 2D) = "white" {}
        _Layer_0_Scale("Layer 0 Scale", Range(0, 500)) = 100
        _Layer_0_Min("Layer 0 Mask Remap Min", Vector) = (0, 0, 0, 0)
        _Layer_0_Max("Layer 0 Mask Remap Max", Vector) = (1, 1, 1, 1)

        _Layer_1_Color ("Layer 1 Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_Layer_1_AlbedoMap ("Paint Map 1 (R)", 2D) = "white" {}
        [NoScaleOffset]_Layer_1_NormalMap("Bump", 2D) = "bump" {}
        _Layer_1_NormalStrength("Strength", Range(0, 10)) = 1
        [NoScaleOffset]_Layer_1_Mask ("Mask", 2D) = "white" {}
        _Layer_1_Scale("Layer 1 Scale", Range(0, 500)) = 100
        _Layer_1_Min("Mask Remap Min", Vector) = (0, 0, 0, 0)
        _Layer_1_Max("Mask Remap Max", Vector) = (1, 1, 1, 1)

        _Layer_2_Color ("Layer 2 Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_Layer_2_AlbedoMap ("Layer 2 Albedo (G)", 2D) = "white" {}
        [NoScaleOffset]_Layer_2_NormalMap("Layer 2 Bump", 2D) = "bump" {}
        _Layer_2_NormalStrength("Layer 2 Normal Strength", Range(0, 10)) = 1
        [NoScaleOffset]_Layer_2_Mask ("Layer 2 Mask Map", 2D) = "white" {}
        _Layer_2_Scale("Layer 2 Scale", Range(0, 500)) = 100
        _Layer_2_Min("Layer 2 Mask Remap Min", Vector) = (0, 0, 0, 0)
        _Layer_2_Max("Layer 2 Mask Remap Max", Vector) = (1, 1, 1, 1)

        _Layer_3_Color ("Layer 3 Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_Layer_3_AlbedoMap ("Layer 3 Albedo Map (B)", 2D) = "white" {}
        [NoScaleOffset]_Layer_3_NormalMap("Layer 3 Normal Map", 2D) = "bump" {}
        _Layer_3_NormalStrength("Layer 3 Normal Strength", Range(0,10)) = 1
        [NoScaleOffset]_Layer_3_Mask ("Layer 3 Mask Map", 2D) = "white" {}
        _Layer_3_Scale("Layer 3 Scale", Range(0, 500)) = 100
        _Layer_3_Min("Layer 3 Mask Remap Min", Vector) = (0, 0, 0, 0)
        _Layer_3_Max("Layer 3 Mask Remap Max", Vector) = (1, 1, 1, 1)

        _LightmapTex("Lightmap", 2D) = "white" {}
        _LightmapAoTex("Lightmap AO", 2D) = "white" {}
        _LightmapStrength("Lightmap Strength", Range(0,1)) = 1
        _LightmapAoStrength("Lightmap AO Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM
            #pragma target 4.5
            #pragma exclude_renderers gles gles3 glcore
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_local _ _LIGHTMAP
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS SHADERPASS_FORWARD

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Includes/splatmap.cginc"


            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS;
                float3 normalWS;
                float4 tangentWS;
                float4 texCoord0;
                float3 viewDirectionWS;
                float2 lightmapUV;
                float3 sh;
                float4 fogFactorAndVertexLight;
                float4 shadowCoord;

                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                   uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                   uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                  FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            struct SurfaceDescriptionInputs
            {
                float3 TangentSpaceNormal;
                float4 uv0;
                float2 lightmapUV;
            };

            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
            };

            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 interp0 : TEXCOORD0;
                float3 interp1 : TEXCOORD1;
                float4 interp2 : TEXCOORD2;
                float4 interp3 : TEXCOORD3;
                float3 interp4 : TEXCOORD4;
                float2 interp5 : TEXCOORD5;
                float3 interp6 : TEXCOORD6;
                float4 interp7 : TEXCOORD7;
                float4 interp8 : TEXCOORD8;

                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                output.interp0.xyz = input.positionWS;
                output.interp1.xyz = input.normalWS;
                output.interp2.xyzw = input.tangentWS;
                output.interp3.xyzw = input.texCoord0;
                output.interp4.xyz = input.viewDirectionWS;
                output.interp5.xy = input.lightmapUV;
                output.interp6.xyz = input.sh;
                output.interp7.xyzw = input.fogFactorAndVertexLight;
                output.interp8.xyzw = input.shadowCoord;

                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                #endif
                return output;
            }

            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                output.positionWS = input.interp0.xyz;
                output.normalWS = input.interp1.xyz;
                output.tangentWS = input.interp2.xyzw;
                output.texCoord0 = input.interp3.xyzw;
                output.viewDirectionWS = input.interp4.xyz;
                output.lightmapUV = input.interp5.xy;
                output.sh = input.interp6.xyz;
                output.fogFactorAndVertexLight = input.interp7.xyzw;
                output.shadowCoord = input.interp8.xyzw;

                #if UNITY_ANY_INSTANCING_ENABLED
                     output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                   output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                   output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                #endif

                return output;
            }

            CBUFFER_START(UnityPerMaterial)

            CBUFFER_END

            #pragma shader_feature_local _ _LAYER_COUNT_4
            #pragma shader_feature_local _LAYER_0_NORMAL
            #pragma shader_feature_local _LAYER_0_MASK
            #pragma shader_feature_local _LAYER_1_NORMAL
            #pragma shader_feature_local _LAYER_1_MASK
            #pragma shader_feature_local _LAYER_2_NORMAL
            #pragma shader_feature_local _LAYER_2_MASK
            #pragma shader_feature_local _LAYER_3_NORMAL
            #pragma shader_feature_local _LAYER_3_MASK


            struct VertexDescription
            {
                float3 Position;
                float3 Normal;
                float3 Tangent;
            };

            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription vertex = (VertexDescription)0;
                vertex.Position = IN.ObjectSpacePosition;
                vertex.Normal = IN.ObjectSpaceNormal;
                vertex.Tangent = IN.ObjectSpaceTangent;
                return vertex;
            }

            struct SurfaceDescription
            {
                float3 BaseColor;
                float3 NormalTS;
                float3 Emission;
                float Metallic;
                float Smoothness;
                float Occlusion;
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 uv = IN.uv0;
                float3 albedo;
                float3 normal;
                float metallic;
                float smoothness;
                float ao;
                #if defined(SHADERPASS_SHADOWCASTER)
		            albedoValue = 0;
		            alphaValue = 1;
		            normalValue = float3(0, 0, 1);
		            metallicValue = 0;
		            smoothnessValue = 0;
		            occlusionValue = 0;
                #else
                CalculateLayersBlend(uv.xy, albedo, normal, metallic, smoothness, ao);
                #endif

               /* #if _LIGHTMAP
				    float2 lightmapuv = IN.lightmapUV.xy * _LightmapTex_ST.xy + _LightmapTex_ST.zw;
				    half4 lightmapAoColor = SAMPLE_TEXTURE2D(_LightmapAoTex, sampler_LightmapTex, lightmapuv);
				    smoothness *= lightmapAoColor;
				    smoothness = lerp(1, smoothness, _LightmapAoStrength);
                #endif*/

                surface.BaseColor = albedo;
                surface.NormalTS = normal;
                surface.Emission = float3(0, 0, 0);
                surface.Metallic = metallic;
                surface.Smoothness = smoothness;
                surface.Occlusion = ao;

                return surface;
            }

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                output.ObjectSpaceNormal = input.normalOS;
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                output.ObjectSpacePosition = input.positionOS;

                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);

                output.uv0 = input.texCoord0;
                output.lightmapUV = input.lightmapUV;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign = IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                return output;
            }

            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On
            ColorMask 0


            HLSLPROGRAM
            #pragma target 4.5
            #pragma exclude_renderers gles gles3 glcore
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS SHADERPASS_SHADOWCASTER

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Includes/splatmap.cginc"


            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                  uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                  uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                   FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            struct SurfaceDescriptionInputs
            {
            };

            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
            };

            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                   uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;

                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                  output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                   output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                   output.cullFace = input.cullFace;
                #endif

                return output;
            }

            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                #if UNITY_ANY_INSTANCING_ENABLED
                  output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                #endif

                return output;
            }

            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END

            #pragma shader_feature_local _LAYER_0_NORMAL
            #pragma shader_feature_local _LAYER_1_NORMAL
            #pragma shader_feature_local _LAYER_2_NORMAL
            #pragma shader_feature_local _LAYER_3_NORMAL
            #pragma shader_feature_local _LAYER_0_MASK
            #pragma shader_feature_local _LAYER_1_MASK
            #pragma shader_feature_local _LAYER_2_MASK
            #pragma shader_feature_local _LAYER_3_MASK
            #pragma shader_feature_local _LAYER_4_MASK

            #define TERRAIN_TO_MESH_NEED_NORMAL
            #define TERRAIN_TO_MESH_NEED_METALLIC_SMOOTHNESS_OCCLUSION

            struct VertexDescription
            {
                float3 Position;
                float3 Normal;
                float3 Tangent;
            };

            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription vertex = (VertexDescription)0;
                vertex.Position = IN.ObjectSpacePosition;
                vertex.Normal = IN.ObjectSpaceNormal;
                vertex.Tangent = IN.ObjectSpaceTangent;
                return vertex;
            }

            struct SurfaceDescription
            {
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                return surface;
            }

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                output.ObjectSpaceNormal = input.normalOS;
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                output.ObjectSpacePosition = input.positionOS;

                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);


                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign = IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                return output;
            }

#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma target 4.5
            #pragma exclude_renderers gles gles3 glcore
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma vertex vert
            #pragma fragment frag

            #define _NORMALMAP 1
            #define _NORMAL_DROPOFF_TS 1
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define FEATURES_GRAPH_VERTEX
            #define SHADERPASS SHADERPASS_DEPTHONLY

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Includes/splatmap.cginc"


            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                #if UNITY_ANY_INSTANCING_ENABLED
                   uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                   uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            struct SurfaceDescriptionInputs
            {
            };

            struct VertexDescriptionInputs
            {
                float3 ObjectSpaceNormal;
                float3 ObjectSpaceTangent;
                float3 ObjectSpacePosition;
            };

            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                #endif
            };

            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                output.positionCS = input.positionCS;
                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                #endif

                return output;
            }

            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;
                output.positionCS = input.positionCS;
                #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                #endif
                #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                #endif
                #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                #endif
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                #endif

                return output;
            }

            CBUFFER_START(UnityPerMaterial)
            CBUFFER_END

            #pragma shader_feature_local _LAYER_0_NORMAL
            #pragma shader_feature_local _LAYER_1_NORMAL
            #pragma shader_feature_local _LAYER_2_NORMAL
            #pragma shader_feature_local _LAYER_3_NORMAL
            #pragma shader_feature_local _LAYER_0_MASK
            #pragma shader_feature_local _LAYER_1_MASK
            #pragma shader_feature_local _LAYER_2_MASK
            #pragma shader_feature_local _LAYER_3_MASK
            #pragma shader_feature_local _LAYER_4_MASK

            #define TERRAIN_TO_MESH_NEED_NORMAL
            #define TERRAIN_TO_MESH_NEED_METALLIC_SMOOTHNESS_OCCLUSION


            struct VertexDescription
            {
                float3 Position;
                float3 Normal;
                float3 Tangent;
            };

            VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
            {
                VertexDescription vertex = (VertexDescription)0;
                vertex.Position = IN.ObjectSpacePosition;
                vertex.Normal = IN.ObjectSpaceNormal;
                vertex.Tangent = IN.ObjectSpaceTangent;

                return vertex;
            }

            struct SurfaceDescription
            {
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;

                return surface;
            }

            VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
            {
                VertexDescriptionInputs output;
                ZERO_INITIALIZE(VertexDescriptionInputs, output);

                output.ObjectSpaceNormal = input.normalOS;
                output.ObjectSpaceTangent = input.tangentOS.xyz;
                output.ObjectSpacePosition = input.positionOS;

                return output;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign = IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

                return output;
            }
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}