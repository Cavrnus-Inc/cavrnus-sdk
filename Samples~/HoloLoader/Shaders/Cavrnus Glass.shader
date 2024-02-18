﻿Shader "Cavrnus/Glass"
{
	Properties
	{
		[Toggle(_CUTOUT)] Cutout("Enable cutout", Float) = 0
		[Toggle(_REFRACTION)] Refraction("Enable refraction", Float) = 0

		_Color("Color", Color) = (0,0,0,0)
		_MainTex("Albedo/Alpha", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.2
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalStrength("Normal Map Strength", Range(0,1)) = 1
		_MetallicMap("Metallic Map", 2D) = "white" {}
		_MetallicStrength("Metallic Strength", Range(0,1)) = 0
		_SmoothnessStrength("Smoothness", Range(0 , 1)) = 1
		_SmoothnessMap("Smoothness Map", 2D) = "white" {}
		_AmbientOcclusionMap("AO Map", 2D) = "white" {}
		_AmbientOcclusionStrength("Ao Strength", Range(0,1)) = 1
		_IndexofRefraction("Index of Refraction", Range(1, 3)) = 1
		_FresnelStrength("Fresnel Strength", Range(0,1)) = 0.8
		_Transparency("Transparency", Range(0 , 1)) = 0

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
	}

		SubShader
		{
			Tags
			{
					"RenderPipeline" = "UniversalPipeline"
					"RenderType" = "Transparent"
					"Queue" = "Transparent"
			}

			HLSLINCLUDE
			#pragma target 2.0
			ENDHLSL

			Pass
			{
				Name "Forward"
				Tags
				{
					"LightMode" = "UniversalForward"
				}
				Blend One OneMinusSrcAlpha , One OneMinusSrcAlpha

				ZWrite On
				Cull Back
				ZTest LEqual
				ColorMask RGBA

				HLSLPROGRAM
				#pragma multi_compile_instancing
				#pragma shader_feature __ FOG_EXP2
				#define REQUIRE_OPAQUE_TEXTURE 1
				#define NEEDS_FRAG_SCREEN_POSITION
				#define _NORMALMAP 1
							#define _REFLECTION_PROBE_BLENDING 1
				#define _REFLECTION_PROBE_BOX_PROJECTION 1

				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x
				#pragma vertex vert
				#pragma fragment frag

				#pragma multi_compile_local _ _REFRACTION
				#pragma multi_compile_local _ _CUTOUT
				#pragma multi_compile_local _ _CUTTINGPLANE
				#pragma shader_feature __ FOG_EXP2

				#define _SPECULARHIGHLIGHTS_ON
				#define _ENVIRONMENTREFLECTIONS_ON

				#define SHADERPASS_FORWARD
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
				#include "Includes/CuttingPlane.cginc"

				struct VertexInput
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					float4 tangent : TANGENT;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct VertexOutput
				{
					float4 clipPos : SV_POSITION;
					float4 lightmapUVOrVertexSH : TEXCOORD0;
					half4 fogFactorAndVertexLight : TEXCOORD1;
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord : TEXCOORD2;
					#endif
					float4 tSpace0 : TEXCOORD3;
					float4 tSpace1 : TEXCOORD4;
					float4 tSpace2 : TEXCOORD5;
					#if defined(NEEDS_FRAG_SCREEN_POSITION)
					float4 screenPos : TEXCOORD6;
					#endif
					float4 texcoord7 : TEXCOORD7;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				CBUFFER_START(UnityPerMaterial)
				float4 _Color;
				float4 _MainTex_ST;
				float4 _NormalMap_ST;
				float4 _SmoothnessMap_ST;
				float4 _AmbientOcclusionMap_ST;
				float4 _MetallicMap_ST;
				float _SmoothnessStrength;
				float _MetallicStrength;
				float _Transparency;
				float _FresnelStrength;
				float _NormalStrength;
				float _IndexofRefraction;
				float _AmbientOcclusionStrength;
				float _Cutoff;

				#if defined(_CUTTINGPLANE)
					float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
					float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
					float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
					float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;
				#endif

				CBUFFER_END
				sampler2D _MainTex;
				sampler2D _NormalMap;
				sampler2D _SmoothnessMap;
				sampler2D _AmbientOcclusionMap;
				sampler2D _MetallicMap;

				VertexOutput VertexFunction(VertexInput v)
				{
					VertexOutput o = (VertexOutput)0;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.texcoord7.xyz = v.texcoord.xyz;
					o.texcoord7.w = 0;

					VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

					half3 positionWS = vertexInput.positionWS.xyz;
					half3 positionVS = vertexInput.positionVS.xyz;
					float4 positionCS = vertexInput.positionCS;

					VertexNormalInputs normalInput = GetVertexNormalInputs(v.normal, v.tangent);

					o.tSpace0 = float4(normalInput.normalWS, positionWS.x);
					o.tSpace1 = float4(normalInput.tangentWS, positionWS.y);
					o.tSpace2 = float4(normalInput.bitangentWS, positionWS.z);

					OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
					OUTPUT_SH(normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz);

					half3 vertexLight = VertexLighting(positionWS, normalInput.normalWS);
					half fogFactor = ComputeFogFactor(positionCS.z);
					o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					o.shadowCoord = GetShadowCoord(vertexInput);
					#endif

					o.clipPos = positionCS;
					#if defined(NEEDS_FRAG_SCREEN_POSITION)
					o.screenPos = ComputeScreenPos(positionCS);
					#endif
					return o;
				}

				VertexOutput vert(VertexInput v)
				{
					return VertexFunction(v);
				}

				half4 frag(VertexOutput IN) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(IN);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

					half3 WorldNormal = normalize(IN.tSpace0.xyz);
					half3 WorldTangent = IN.tSpace1.xyz;
					half3 WorldBiTangent = IN.tSpace2.xyz;
					half3 WorldPosition = half3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
					half3 WorldViewDirection = SafeNormalize(_WorldSpaceCameraPos.xyz - WorldPosition);
					float2 uv = IN.texcoord7.xy;

					#ifdef _CUTTINGPLANE
						bool withinBorder0 = handleCullingVolume(WorldPosition, _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorder0);
						bool withinBorder1 = handleCullingVolume(WorldPosition, _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorder1);
						bool withinBorder2 = handleCullingVolume(WorldPosition, _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorder2);
						if (withinBorder0)
							return _CullVolBorderColor0;
						else if (withinBorder1)
							return _CullVolBorderColor1;
						else if (withinBorder2)
							return _CullVolBorderColor2;
					#endif


					half4 albedoColor = tex2D(_MainTex, uv * _MainTex_ST.xy + _MainTex_ST.zw);

					#ifdef _CUTOUT
						clip(albedoColor.a - _Cutoff);
					#endif

					half4 tintedColor = _Color * albedoColor;

					half3 normal = UnpackNormalRGB(tex2D(_NormalMap, uv * _NormalMap_ST.xy + _NormalMap_ST.zw), _NormalStrength);
					half3 metallicColor = tex2D(_MetallicMap, uv * _MetallicMap_ST.xy + _MetallicMap_ST.zw);
					half3 smoothnessColor = tex2D(_SmoothnessMap, uv * _SmoothnessMap_ST.xy + _SmoothnessMap_ST.zw);
					half3 aoTex = tex2D(_AmbientOcclusionMap, uv * _AmbientOcclusionMap_ST.xy + _AmbientOcclusionMap_ST.zw);

					float dotProduct = dot(WorldNormal, WorldViewDirection);
					float fresnelFactor = saturate(dotProduct);
					fresnelFactor = 1 - fresnelFactor;
					fresnelFactor = pow(fresnelFactor, 1 + 4 * _FresnelStrength);
					float f0a = (_IndexofRefraction - 1) / (_IndexofRefraction + 1);
					float f0 = f0a * f0a;
					fresnelFactor = f0 + (1 - f0) * fresnelFactor;
					float clampedFresnel = clamp(fresnelFactor, 0.0, 1.0);
					float fresnelOpacity = clampedFresnel;
					float finalSmoothness = _SmoothnessStrength * smoothnessColor;

					float fadeAlpha = (1 - _Transparency) * tintedColor.a;
					float finalOpacity = fadeAlpha * fresnelOpacity;

					half3 Albedo = tintedColor.rgb;
					half3 Normal = normal;
					half3 Emission = 0;
					half3 Specular = 1;
					float Metallic = metallicColor * _MetallicStrength;
					float Smoothness = finalSmoothness;
					float Occlusion = lerp(1, aoTex, _AmbientOcclusionStrength);
					float Alpha = 1.0;

					InputData inputData;
					inputData.positionWS = WorldPosition;
					inputData.viewDirectionWS = WorldViewDirection;

					inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3(WorldTangent, WorldBiTangent, WorldNormal)));

					inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
					inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS);

					half4 color = UniversalFragmentPBR(inputData, Albedo, Metallic, Specular, Smoothness, Occlusion, Emission, 1.0);

					color.rgb *= finalOpacity;
					color.a = finalOpacity;

					color.rgb *= MixFog(color.rgb, IN.fogFactorAndVertexLight.x);

					#ifdef _REFRACTION
						float4 ScreenPos = IN.screenPos;
						float4 projScreenPos = ScreenPos / ScreenPos.w;
						half3 refractionOffset = (sqrt(_IndexofRefraction) - 1.0) * mul(UNITY_MATRIX_V, WorldNormal).xyz * (1.0 - dot(WorldNormal, WorldViewDirection));
						projScreenPos.xy += refractionOffset.xy;
						half3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR(projScreenPos) * Albedo;
						color.rgb += refraction * (1.0 - fresnelOpacity) * tintedColor.a;
						color.a = fadeAlpha; // remove blending; we're refractin'
					#endif

					return color;
				}
				ENDHLSL
			}


			Pass // depth
			{
				Name "DepthOnly"
				Tags
				{
					"LightMode" = "DepthOnly"
				}
				Blend One OneMinusSrcAlpha , One OneMinusSrcAlpha

				ZWrite On
				Cull Back
				ZTest LEqual
				ColorMask RGBA

				HLSLPROGRAM
				#pragma multi_compile_instancing

				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x

				#pragma vertex vert
				#pragma fragment frag

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
				#include "Includes/CuttingPlane.cginc"

				struct VertexInput
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					float4 tangent : TANGENT;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct VertexOutput
				{
					float4 clipPos : SV_POSITION;
					float4 tSpace0 : TEXCOORD3;
					float4 tSpace1 : TEXCOORD4;
					float4 tSpace2 : TEXCOORD5;
					float4 texcoord7 : TEXCOORD7;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				CBUFFER_START(UnityPerMaterial)
				float4 _Color;
				float4 _MainTex_ST;
				float4 _NormalMap_ST;
				float4 _SmoothnessMap_ST;
				float4 _AmbientOcclusionMap_ST;
				float4 _MetallicMap_ST;
				float _SmoothnessStrength;
				float _MetallicStrength;
				float _Transparency;
				float _FresnelStrength;
				float _NormalStrength;
				float _IndexofRefraction;
				float _AmbientOcclusionStrength;
				float _Cutoff;

				#if defined(_CUTTINGPLANE)
					float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
					float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
					float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
					float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;
				#endif

				CBUFFER_END


				VertexOutput vert(VertexInput IN)
				{
					VertexOutput output = (VertexOutput)0;
					UNITY_SETUP_INSTANCE_ID(IN);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
					output.texcoord7.xyz = IN.texcoord.xyz;
					output.texcoord7.w = 0;

					VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.vertex.xyz);

					half3 positionWS = vertexInput.positionWS.xyz;
					half3 positionVS = vertexInput.positionVS.xyz;
					float4 positionCS = vertexInput.positionCS;

					VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normal, IN.tangent);

					output.tSpace0 = float4(normalInput.normalWS, positionWS.x);
					output.tSpace1 = float4(normalInput.tangentWS, positionWS.y);
					output.tSpace2 = float4(normalInput.bitangentWS, positionWS.z);

					output.clipPos = positionCS;
					return output;
				}

				half4 frag(VertexOutput IN) : SV_Target
				{
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
					return 0;
				}
				ENDHLSL
			}

			Pass //depth normals
			{
				Name "DepthNormals"
				Tags
				{
					"LightMode" = "DepthNormals"
				}
				Blend One OneMinusSrcAlpha , One OneMinusSrcAlpha

				ZWrite On
				Cull Back
				ZTest LEqual
				ColorMask RGBA

				HLSLPROGRAM
				#pragma multi_compile_instancing
				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x
				#pragma vertex vert
				#pragma fragment frag

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
				#include "Includes/CuttingPlane.cginc"

				struct VertexInput
				{
					float4 vertex : POSITION;
					half3 normal : NORMAL;
					float4 tangent : TANGENT;
					float4 texcoord1 : TEXCOORD1;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct VertexOutput
				{
					float4 clipPos : SV_POSITION;
					float4 tSpace0 : TEXCOORD3;
					float4 tSpace1 : TEXCOORD4;
					float4 tSpace2 : TEXCOORD5;
					float3 normalWS : TEXCOORD6;
					float4 texcoord7 : TEXCOORD7;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				CBUFFER_START(UnityPerMaterial)
				float4 _Color;
				float4 _MainTex_ST;
				float4 _NormalMap_ST;
				float4 _SmoothnessMap_ST;
				float4 _AmbientOcclusionMap_ST;
				float4 _MetallicMap_ST;
				float _SmoothnessStrength;
				float _MetallicStrength;
				float _Transparency;
				float _FresnelStrength;
				float _NormalStrength;
				float _IndexofRefraction;
				float _AmbientOcclusionStrength;
				float _Cutoff;

				#if defined(_CUTTINGPLANE)
					float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
					float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
					float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
					float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;
				#endif

				CBUFFER_END

				VertexOutput vert(VertexInput IN)
				{
					VertexOutput o = (VertexOutput)0;
					UNITY_SETUP_INSTANCE_ID(IN);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.texcoord7.xyz = IN.texcoord.xyz;
					o.texcoord7.w = 0;

					VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.vertex.xyz);

					half3 positionWS = vertexInput.positionWS.xyz;
					half3 positionVS = vertexInput.positionVS.xyz;
					float4 positionCS = vertexInput.positionCS;

					VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normal, IN.tangent);

					o.tSpace0 = float4(normalInput.normalWS, positionWS.x);
					o.tSpace1 = float4(normalInput.tangentWS, positionWS.y);
					o.tSpace2 = float4(normalInput.bitangentWS, positionWS.z);

					o.clipPos = positionCS;

					return o;
				}

				half4 frag(VertexOutput IN) : SV_Target
				{
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

					return 0;
				}
				ENDHLSL
			}

		}
}
