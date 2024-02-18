Shader "Cavrnus/Lit"
{
	Properties
	{
		// Specular vs Metallic workflow
		_WorkflowMode("WorkflowMode", Float) = 1.0
		_MainTex("Albedo/Alpha", 2D) = "white" {}
		[Toggle(_CUTOUT)] Cutout("Enable cutout", Float) = 0
		[Toggle(_FADE)] Fade("Enable Dithering Fade", Float) = 0
		[Toggle(_LIGHTMAP)] Lightmap("Enable lightmap", Float) = 0
		[Toggle(_DETAIL)] Detail("Enable Detail Maps", Float) = 0
		[Toggle(_INVERTSMOOTHNESS)] InvertSmoothness("Invert Smoothness", Float) = 0
		[HideInInspector][Toggle(_CUTTINGPLANE)] CuttingPlane("Enable Cutting Plane", Float) = 0
		[Toggle(_SURFACE_TYPE_TRANSPARENT)] Transparent("Enable Transparency", Float) = 0
		SpecularSetup("Enable Specular Workflow Mode", Float) = 0

		_SrcBlend("Src Blend", Float) = 1 //default opaque settings.
		_DstBlend("Dst Blend", Float) = 0
		_Surface("surface Type", Float) = 0
		 _ZWrite("Zwrite", Float) = 1.0
		 _Blend("Blend Mode", Float) = 3

		_Color("Base Color", Color) = (.75, .75, .75, 1)
		_Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
		_NormalMap("Normal", 2D) = "bump" {}
		_NormalStrength("Normal Strength", Range(0,1)) = 0
		_MetallicMap("Metallic Map", 2D) = "white" {}
		_MetallicStrength("Metallic Strength", Range(0,1)) = 0
		_SmoothnessMap("Smoothness Map", 2D) = "white" {}
		_SmoothnessStrength("Smoothness Strength", Range(0,1)) = 0.5
		_SpecularColor("Specular reflection color tint", Color) = (1, 1, 1)
		_SpecularMap("Specular Map", 2D) = "white" {}
		_AmbientOcclusionMap("Ao map", 2D) = "white" {}
		_AoStrength("Ambient Occlusion Strength", Range(0,1)) = 0
		_EmissionMap("Emission Map", 2D) = "white" {}
		_EmissionStrength("Emission Strength", Range(0,10)) = 0
		_EmissionColor("Emission Color", Color) = (1,1,1,0)
		[Enum(Flip, 0, Mirror, 1, None, 2)] _DoubleSidedNormalMode("Double sided normal mode", Float) = 0
		[Enum(Off, 0, Back, 1, Front, 2)] _Cull("Cull Mode", Float) = 0

		_UseDetailMaskMap("Enable detail mask map", Range(0,1)) = 0
		_DetailMaskMap("Detail Mask Map", 2D) = "white"{}
		_DetailTiling("Detail scale", Range(1, 100)) = 5
		_DetailNormalStrength("Detail Normal Scale", Range(0, 1)) = 1
		_DetailAoStrength("Detail Ambient Occlusion Strength", Range(0,1)) = 0
		_DetailAlbedoStrength("Detail Albedo Strength", Range(0,1)) = 1
		[NoScaleOffset]_DetailAlbedoMap("Detail Albedo Map", 2D) = "white" {}
		[NoScaleOffset]_DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
		[NoScaleOffset]_DetailAmbientOcclusionMap("Detail Ambient Occlusion Map", 2D) = "white" {}

		[HideInInspector] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[HideInInspector] _EnvironmentReflections("Environment Reflections", Float) = 1.0

		_FresnelStrength("Fresnel Strength", Range(0, 1)) = .1
		_Transparency("Transparency", Range(0,1)) = 0

		_LightmapTex("Lightmap", 2D) = "white" {}
		_LightmapAoTex("Lightmap AO", 2D) = "white" {}
		_LightmapStrength("Lightmap Strength", Range(0,1)) = 1
		_LightmapAoStrength("Lightmap AO Strength", Range(0,1)) = 1

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
					"RenderType" = "Opaque"
					"UniversalMaterialType" = "Lit"
					"Queue" = "Geometry"
				}

				Pass
				{
					Name "Universal Forward"
					Tags
					{
						"LightMode" = "UniversalForward"
					}
					ZTest LEqual
					ZWrite[_ZWrite]
					Blend[_SrcBlend][_DstBlend]
					Cull[_Cull]

					HLSLPROGRAM
					#pragma vertex Vert
					#pragma fragment Frag
					#pragma target 4.5
					#pragma prefer_hlslcc gles
					#pragma exclude_renderers d3d11_9x
					#pragma shader_feature_local_fragment __ FOG_EXP2

			// Keywords
			#pragma multi_compile _ _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
			#pragma multi_compile_local_fragment _ _DETAIL
			#pragma multi_compile_local_fragment _ _CUTOUT
			#pragma multi_compile_local_fragment _ _LIGHTMAP
			#pragma multi_compile_local_fragment _ _CUTTINGPLANE
			#pragma shader_feature_local_fragment _ _SPECULAR_SETUP _INVERTSMOOTHNESS
			#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT _FADE
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
					#pragma shader_feature __ FOG_EXP2

			#ifdef _ADDITIONAL_LIGHTS
				#define _ADDITIONAL_LIGHT_SHADOWS 1
			#endif

			#if defined SHADER_API_MOBILE || SHADER_API_VULKAN
				#define _SCREEN_SPACE_OCCLUSION 0
				#define _REFLECTION_PROBE_BLENDING 0
				#define _REFLECTION_PROBE_BOX_PROJECTION 0
			#else
				#define _SCREEN_SPACE_OCCLUSION 1
				#define _REFLECTION_PROBE_BLENDING 1
				#define _REFLECTION_PROBE_BOX_PROJECTION 1
			#endif

			#define NEED_FACING 1
			#define _SPECULARHIGHLIGHTS_ON
			#define _ENVIRONMENTREFLECTIONS_ON
			#define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
			#define _SHADOWS_SOFT 1
			#define _MAIN_LIGHT_SHADOWS_CASCADE 1


			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Includes/CuttingPlane.cginc"
			#include "Includes/Utilities.cginc"

			#undef WorldNormalVector
			#define WorldNormalVector(data, normal) mul(normal, data.TBNMatrix)
			#define UnityObjectToWorldNormal(normal) mul(GetObjectToWorldMatrix(), normal)
			#define _WorldSpaceLightPos0 _MainLightPosition
			#define UNITY_DECLARE_TEX2D(name) TEXTURE2D(name); SAMPLER(sampler##name);
			#define UNITY_SAMPLE_TEX2D(tex, coord)                SAMPLE_TEXTURE2D(tex, sampler##tex, coord)
			#define UNITY_SAMPLE_TEX2D_SAMPLER(tex, samp, coord)  SAMPLE_TEXTURE2D(tex, sampler##samp, coord)
			#define UNITY_INITIALIZE_OUTPUT(type,name) name = (type)0;
			#define sampler2D_float sampler2D
			#define sampler2D_half sampler2D

		struct VertexToPixel
		{
			float4 pos : SV_POSITION;
			float3 worldPos : TEXCOORD0;
			float3 worldNormal : TEXCOORD1;
			float4 worldTangent : TEXCOORD2;
			float4 texcoord0 : TEXCOORD3;
			float4 texcoord1 : TEXCOORD4;
			float4 screenPos : TEXCOORD7;
			float3 sh : TEXCOORD9;
			float fogFactor : TEXCOORD5;
			float4 shadowCoord : TEXCOORD11;

			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO

			#if defined(SHADER_STAGE_FRAGMENT)
			FRONT_FACE_TYPE facing : FRONT_FACE_SEMANTIC;
			#endif
		};

		struct Surface
		{
			half3 Albedo;
			half3 Normal;
			half Smoothness;
			half3 Specular;
			half3 Emission;
			half Metallic;
			half Occlusion;
			half Alpha;
			float outputDepth;
			float3 DiffuseGI;
		};

		struct ShaderData
		{
			float4 clipPos;
			float3 worldSpacePosition;
			float3 worldSpaceNormal;
			float3 worldSpaceTangent;
			float tangentSign;
			float3 worldSpaceViewDir;
			float3 tangentSpaceViewDir;
			float4 texcoord0;
			float4 texcoord1;
			float2 screenUV;
			float4 screenPos;
			bool isFrontFace;
			float3x3 TBNMatrix;
		};

		struct VertexData
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			float4 texcoord0 : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		float3 WorldToTangentSpace(ShaderData d, float3 normal)
		{
			return mul(d.TBNMatrix, normal);
		}

		float3 TangentToWorldSpace(ShaderData d, float3 normal)
		{
			return mul(normal, d.TBNMatrix);
		}

		float GetLinearEyeDepth(float2 uv)
		{
			return LinearEyeDepth(shadergraph_LWSampleSceneDepth(uv), _ZBufferParams);
		}

		float3 GetWorldPositionFromDepthBuffer(float2 uv, float3 worldSpaceViewDir)
		{
			float eye = GetLinearEyeDepth(uv);
			float3 camView = mul((float3x3)GetObjectToWorldMatrix(),
								 transpose(mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V))[2].xyz);

			float dt = dot(worldSpaceViewDir, camView);
			float3 div = worldSpaceViewDir / dt;
			float3 wpos = (eye * div) + _WorldSpaceCameraPos;
			return wpos;
		}

		CBUFFER_START(UnityPerMaterial)

			half4 _Color;
			float4 _MainTex_ST;
			float4 _NormalMap_ST;
			float4 _EmissionMap_ST;
			float4 _SmoothnessMap_ST;
			float4 _SpecularMap_ST;
			float4 _MetallicMap_ST;
			float4 _AmbientOcclusionMap_ST;
			half _SmoothnessStrength;
			half _NormalStrength;
			half _EmissionStrength;
			half _AoStrength;
			half _Cutoff;
			half4 _EmissionColor;
			float _DoubleSidedNormalMode;
			half _FresnelStrength;
			half _Transparency;
			half _Surface;

			half3 _SpecularColor;
			half _MetallicStrength;

			half _LightmapStrength;
			half _LightmapAoStrength;
			float4 _LightmapTex_ST;

			half _DetailTiling;
			half _DetailNormalStrength;
			half _DetailSmoothnessStrength;
			half _DetailAoStrength;
			float4 _DetailMaskMap_ST;

			float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
			float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
			float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
			float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;

		CBUFFER_END

		#define unity_ObjectToWorld GetObjectToWorldMatrix()
		#define unity_WorldToObject GetWorldToObjectMatrix()

		TEXTURE2D(_MainTex);					SAMPLER(sampler_MainTex);
		TEXTURE2D(_NormalMap);					SAMPLER(sampler_NormalMap);
		TEXTURE2D(_AmbientOcclusionMap);		SAMPLER(sampler_AmbientOcclusionMap);
		TEXTURE2D(_EmissionMap);				SAMPLER(sampler_EmissionMap);
		TEXTURE2D(_SmoothnessMap);				SAMPLER(sampler_SmoothnessMap);
		TEXTURE2D(_SpecularMap);				SAMPLER(sampler_SpecularMap);
		TEXTURE2D(_MetallicMap);				SAMPLER(sampler_MetallicMap);

		#if defined(_LIGHTMAP)
			TEXTURE2D(_LightmapTex);			SAMPLER(sampler_LightmapTex);
			TEXTURE2D(_LightmapAoTex);
		#endif

		#if defined(_DETAIL)
			TEXTURE2D(_DetailMaskMap);
			TEXTURE2D(_DetailAlbedoMap);
			TEXTURE2D(_DetailNormalMap);
			TEXTURE2D(_DetailSmoothnessMap);
			TEXTURE2D(_DetailAmbientOcclusionMap);
			SAMPLER(sampler_DetailMaskMap);
		#endif

		#if (_READY_TO_UPGRADE)

			//
			   // // Used for scaling detail albedo. Main features:
			   // // - Depending if detailAlbedo brightens or darkens, scale magnifies effect.
			   // // - No effect is applied if detailAlbedo is 0.5.
			   // half3 ScaleDetailAlbedo(half3 detailAlbedo, half scale)
			   // {
			   //     return half(2.0) * detailAlbedo * scale - scale + half(1.0);
			   // }
			//
			   // half3 ApplyDetailAlbedo(float2 detailUv, half3 albedo, half detailMask)
			   // {
			   // #if defined(_DETAIL)
			   //     half3 detailAlbedo = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, detailUv).rgb;
			//
			   //     // In order to have same performance as builtin, we do scaling only if scale is not 1.0 (Scaled version has 6 additional instructions)
			   // #if defined(_DETAIL_SCALED)
			   //     detailAlbedo = ScaleDetailAlbedo(detailAlbedo, _DetailAlbedoMapScale);
			   // #else
			   //     detailAlbedo = half(2.0) * detailAlbedo;
			   // #endif
			//
			   //     return albedo * LerpWhiteTo(detailAlbedo, detailMask);
			   // #else
			   //     return albedo;
			   // #endif
			   // }
			//
			   // half3 ApplyDetailNormal(float2 detailUv, half3 normalTS, half detailMask)
			   // {
			   // #if defined(_DETAIL)
			   // #if BUMP_SCALE_NOT_SUPPORTED
			   //     half3 detailNormalTS = UnpackNormal(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv));
			   // #else
			   //     half3 detailNormalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detailUv), _DetailNormalMapScale);
			   // #endif
			//
			   //     // With UNITY_NO_DXT5nm unpacked vector is not normalized for BlendNormalRNM
			   //     // For visual consistancy we going to do in all cases
			   //     detailNormalTS = normalize(detailNormalTS);
			//
			   //     return lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask); // todo: detailMask should lerp the angle of the quaternion rotation, not the normals
			   // #else
			   //     return normalTS;
			   // #endif
			   // }
			//
				   #endif

			   float FixSpecularHighlights(inout float smoothness, ShaderData d)
			   {
				   half3 worldNormalFace = d.worldSpaceNormal.xyz;
				   half roughness = 1.0h - smoothness;
				   half3 deltaU = ddx(worldNormalFace);
				   half3 deltaV = ddy(worldNormalFace);
				   half variance = 0.1h * (dot(deltaU, deltaU) + dot(deltaV, deltaV));  //might need to be a shader prop?
				   half kernelSquaredRoughness = min(2.0h * variance , 0.2h); //same here. To be figured out when tested across assets.
				   half squaredRoughness = saturate(roughness * roughness + kernelSquaredRoughness);
				   smoothness = 1.0h - sqrt(squaredRoughness);
				   return smoothness;
			   }

			   float3 UnpackNormalRGBScale(real4 packedNormal, float scale)
			   {
				   real3 normal;
				   normal.xyz = packedNormal.rgb * 2.0 - 1.0;
				   normal.xy *= scale;
				   return normal;
			   }

			   void DoubleSidedNormal(inout Surface o, ShaderData d)
			   {
				   o.Normal *= d.isFrontFace ? 1 : -1;
			   }

			   void Fresnel(inout Surface o, ShaderData d)
			   {
				   float dotProduct = dot(d.worldSpaceNormal, d.worldSpaceViewDir);
				   float fresnel = Pow4(1.0 - saturate(dotProduct));
					   fresnel *= lerp(o.Specular, _FresnelStrength, fresnel);
					   o.Specular += d.isFrontFace ? fresnel : 0;

					   fresnel *= lerp(o.Smoothness, _FresnelStrength, fresnel);
					   o.Smoothness += d.isFrontFace ? fresnel : 0;

			   }

			   void ApplyDitherCrossFadeVSP(float2 vpos, float fade)
			   {
				   float dither = Dither8x8Bayer(fmod(vpos.x, 8), fmod(vpos.y, 8));
				   clip(dither - fade);
			   }

			   void DitheringFade(inout Surface o, ShaderData d)
			   {
				   float4 screenPosNorm = d.screenPos / d.screenPos.w;
				   float2 clipScreen = screenPosNorm.xy * _ScreenParams.xy;
				   float trans = lerp(0, 1.01, _Transparency);
				   ApplyDitherCrossFadeVSP(clipScreen,trans);
			   }

			   void NormalFinalizer(inout Surface o, ShaderData d)
			   {
				   o.Normal = TangentToWorldSpace(d, o.Normal);
			   }

			   void DetailSurface(inout Surface o, ShaderData d)
			   {
				   #if defined (_DETAIL)
					   float2 uv = d.texcoord0.xy * _DetailTiling;
					   half4 albedoTex = SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_MainTex, uv);
					   half4 aoTex = SAMPLE_TEXTURE2D(_DetailAmbientOcclusionMap, sampler_MainTex, uv);
					   half3 normal = UnpackNormalRGB(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_MainTex, uv), _DetailNormalStrength);
					   normal = BlendNormalRNM(o.Normal, normal);
					   albedoTex = sqrt(albedoTex);
					   o.Albedo *= albedoTex;
					   o.Normal = normal;
					   o.Metallic = _MetallicStrength;
					   o.Occlusion *= lerp(1, aoTex, _DetailAoStrength);
				   #endif
			   }

			   void SpecularSurface(inout Surface o, ShaderData d)
			   {
				   half metallic;
				   half smoothness;
				   half3 finalSpec;
				   half3 specColor;
				   float2 uv;
				   uv = d.texcoord0.xy * _SmoothnessMap_ST.xy + _SmoothnessMap_ST.zw;
				   half3 smoothnessTex = SAMPLE_TEXTURE2D(_SmoothnessMap, sampler_SmoothnessMap, uv);

				   #if _SPECULAR_SETUP
					   uv = d.texcoord0.xy * _SpecularMap_ST.xy + _SpecularMap_ST.zw;
					   half3 specularTex = SAMPLE_TEXTURE2D(_SpecularMap, sampler_SpecularMap, uv);
					   metallic = half(1.0);
					   finalSpec = specularTex * _SpecularColor;
					   smoothness = smoothnessTex * _SmoothnessStrength;

				   #else
					   uv = d.texcoord0.xy * _MetallicMap_ST.xy + _MetallicMap_ST.zw;
					   half3 metallicTex = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, uv);

					   #if _INVERTSMOOTHNESS
					   smoothnessTex = 1 - smoothnessTex;
					   #endif

					   smoothness = smoothnessTex * _SmoothnessStrength;
					   metallic = metallicTex * _MetallicStrength;
					   finalSpec = half3(0,0,0);

				   #endif

				   o.Metallic = metallic;
				   o.Specular = finalSpec;
				   o.Smoothness = smoothness;
			   }

			   void LitBaseSurface(inout Surface o, ShaderData d)
			   {
				   #if _CUTTINGPLANE
					   bool withinBorder0 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorder0);
					   bool withinBorder1 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorder1);
					   bool withinBorder2 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorder2);
				   #endif

				   float2 uv = d.texcoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				   half4 albedoTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

				   #if _CUTOUT
						clip(albedoTex.a - _Cutoff);
				   #endif
				   #if _FADE
					   DitheringFade(o, d);
				   #endif
				   uv = d.texcoord0.xy * _AmbientOcclusionMap_ST.xy + _AmbientOcclusionMap_ST.zw;
				   half3 aoTex = SAMPLE_TEXTURE2D(_AmbientOcclusionMap, sampler_AmbientOcclusionMap, uv);
				   uv = d.texcoord0.xy * _EmissionMap_ST.xy + _EmissionMap_ST.zw;
				   half3 emissionTex = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv);
				   half3 color = half3(albedoTex.rgb * _Color.rgb);
				   uv = d.texcoord0.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				   half3 normal = UnpackNormalRGB(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv), _NormalStrength);

				   o.Albedo = color;
				   o.Normal = normal;
				   o.Emission = emissionTex * _EmissionStrength * _EmissionColor;
				   o.Occlusion = lerp(1, aoTex, _AoStrength);
				   o.Alpha = albedoTex.a * _Color.a * (1 - _Transparency);
			   }

			   void ChainSurface(inout Surface l, inout ShaderData d)
			   {
				   LitBaseSurface(l, d);
				   SpecularSurface(l,d);
				   DoubleSidedNormal(l, d);
				   Fresnel(l, d);

				   #if _DETAIL
					   DetailSurface(l, d);
				   #endif

			   }

			   ShaderData CreateShaderData(VertexToPixel i)
			   {
				   ShaderData data = (ShaderData)0;
				   data.clipPos = i.pos;
				   data.worldSpacePosition = i.worldPos;
				   data.worldSpaceNormal = normalize(i.worldNormal);
				   data.worldSpaceTangent = normalize(i.worldTangent.xyz);
				   data.tangentSign = i.worldTangent.w;
				   float3 bitangent = cross(i.worldTangent.xyz, i.worldNormal) * data.tangentSign * -1;
				   data.TBNMatrix = float3x3(data.worldSpaceTangent, bitangent, data.worldSpaceNormal);
				   data.worldSpaceViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				   data.tangentSpaceViewDir = mul(data.TBNMatrix, data.worldSpaceViewDir);
				   data.texcoord0 = i.texcoord0;
				   data.texcoord1 = i.texcoord1;
				   #if defined(SHADER_STAGE_FRAGMENT)
					   data.isFrontFace = i.facing;
				   #endif
				   data.screenPos = i.screenPos;
				   data.screenUV = (i.screenPos.xy / i.screenPos.w);

				   return data;
			   }

			   VertexToPixel Vert(VertexData v)
			   {
				   VertexToPixel o = (VertexToPixel)0;

				   UNITY_SETUP_INSTANCE_ID(v);
				   UNITY_TRANSFER_INSTANCE_ID(v, o);
				   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				   o.texcoord0 = v.texcoord0;
				   o.texcoord1 = v.texcoord1;

				   VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				   o.worldPos = vertexInput.positionWS;
				   o.worldNormal = TransformObjectToWorldNormal(v.normal);
				   o.worldTangent = float4(TransformObjectToWorldDir(v.tangent.xyz), v.tangent.w);
				   o.pos = vertexInput.positionCS;
				   o.screenPos = ComputeScreenPos(o.pos, _ProjectionParams.x);
				   OUTPUT_SH(o.worldNormal, o.sh);

				   half3 vertexLight = VertexLighting(o.worldPos, o.worldNormal);
				   o.fogFactor = ComputeFogFactor(o.pos.z);

				   return o;
				 }

			   half4 Frag(VertexToPixel IN) : SV_Target
			   {
				   UNITY_SETUP_INSTANCE_ID(IN);
				   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				   ShaderData d = CreateShaderData(IN);
				   Surface l = (Surface)0;
				   l.Albedo = half3(0.5, 0.5, 0.5);
				   l.Normal = float3(0, 0, 1);
				   l.Occlusion = 1;
				   l.Alpha = 1;

				   ChainSurface(l, d);

				   InputData inputData;
				   inputData.positionWS = IN.worldPos;
				   inputData.normalWS = normalize(TangentToWorldSpace(d, l.Normal));
				   inputData.viewDirectionWS = SafeNormalize(d.worldSpaceViewDir);
				   inputData.shadowCoord = TransformWorldToShadowCoord(IN.worldPos);
				   inputData.fogCoord = IN.fogFactor;
				   //inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw; //remove in Forward+, always pixel lighting.

				   float3 sh = SAMPLE_GI(IN.lightmapUV, IN.sh, inputData.normalWS);

				   #if _LIGHTMAP
					   float2 lightMapUV = d.texcoord1.xy * _LightmapTex_ST.xy + _LightmapTex_ST.zw;
					   float3 bakedGI = SampleSingleLightmap(TEXTURE2D_ARGS(_LightmapTex,  sampler_LightmapTex), lightMapUV,float4(1.0, 1.0, 0.0, 0.0),false, float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0, 0.0));
					   inputData.bakedGI = lerp(sh, bakedGI, _LightmapStrength);
				   #else
					   inputData.bakedGI = sh;
				   #endif

				   inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.pos);
				   inputData.shadowMask = float4(0,0,0,0);

				   SurfaceData surface = (SurfaceData)0;
				   surface.albedo = l.Albedo;
				   surface.metallic = saturate(l.Metallic);
				   surface.specular = l.Specular;
				   surface.smoothness = saturate(l.Smoothness);
				   surface.occlusion = l.Occlusion;
				   surface.emission = l.Emission;
				   surface.alpha = saturate(l.Alpha);

				   half4 color = UniversalFragmentPBR(inputData, surface);

				   color.rgb = MixFog(color.rgb, inputData.fogCoord);
				   color.a = OutputAlpha(_Transparency, _Surface);
				   color.a = surface.alpha;

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

				   ZWrite[_ZWrite]
				   Blend[_SrcBlend][_DstBlend]
				   ZTest LEqual
				   Cull[_Cull]
				   HLSLPROGRAM
				   #pragma vertex Vert
				   #pragma fragment Frag
				   #pragma target 3.0
				   #pragma prefer_hlslcc gles
				   #pragma exclude_renderers d3d11_9x

				   #define _NORMAL_DROPOFF_TS 1
				   #define ATTRIBUTES_NEED_NORMAL
				   #define ATTRIBUTES_NEED_TANGENT
				   #define SHADERPASS_SHADOWCASTER
				   #define _PASSSHADOW 1
				   #define NEED_FACING 1

				   #pragma shader_feature_local_fragment _ _CUTOUT
				   #pragma shader_feature_local_fragment _ _CUTTINGPLANE
				   #pragma shader_feature_local_fragment _ _FADE
				   // #pragma shader_feature_local_fragment _ALPHATEST_ON

				   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
				   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
				   #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

				   #undef WorldNormalVector
				   #define WorldNormalVector(data, normal) mul(normal, data.TBNMatrix)
				   #define UnityObjectToWorldNormal(normal) mul(GetObjectToWorldMatrix(), normal)
				   #define _WorldSpaceLightPos0 _MainLightPosition
				   #define UNITY_DECLARE_TEX2D(name) TEXTURE2D(name); SAMPLER(sampler##name);
				   #define UNITY_SAMPLE_TEX2D(tex, coord)                SAMPLE_TEXTURE2D(tex, sampler##tex, coord)
				   #define UNITY_SAMPLE_TEX2D_SAMPLER(tex, samp, coord)  SAMPLE_TEXTURE2D(tex, sampler##samp, coord)
				   #define UNITY_INITIALIZE_OUTPUT(type,name) name = (type)0;

				   #define sampler2D_float sampler2D
				   #define sampler2D_half sampler2D

				   struct VertexToPixel
				   {
					   float4 pos : SV_POSITION;
					   float3 worldPos : TEXCOORD0;
					   float3 worldNormal : TEXCOORD1;
					   float4 worldTangent : TEXCOORD2;
					   float4 texcoord0 : TEXCOORD3;
					   float4 texcoord1 : TEXCOORD4;
					   float4 screenPos : TEXCOORD7;
					   float4 shadowCoord : TEXCOORD11;
					   UNITY_VERTEX_INPUT_INSTANCE_ID
					   UNITY_VERTEX_OUTPUT_STEREO
					   #if defined(SHADER_STAGE_FRAGMENT)
					   FRONT_FACE_TYPE facing : FRONT_FACE_SEMANTIC;
					   #endif
				   };

				   struct Surface
				   {
					   half3 Albedo;
					   half Alpha;
					   float outputDepth;
					   float4 ShadowMask;
				   };

				   struct ShaderData
				   {
					   float4 clipPos;
					   float3 worldSpacePosition;
					   float3 worldSpaceNormal;
					   float3 worldSpaceTangent;
					   float tangentSign;
					   float3 worldSpaceViewDir;
					   float3 tangentSpaceViewDir;
					   float4 texcoord0;
					   float2 screenUV;
					   float4 screenPos;
					   #if defined (SHADER_STAGE_FRAGMENT)
					   bool isFrontFace;
					   #endif
					   float3x3 TBNMatrix;
				   };

				   struct VertexData
				   {
					   float4 vertex : POSITION;
					   float3 normal : NORMAL;
					   float4 tangent : TANGENT;
					   float4 texcoord0 : TEXCOORD0;
					   float4 texcoord1 : TEXCOORD1;
					   float4 texcoord2 : TEXCOORD2;
					   UNITY_VERTEX_INPUT_INSTANCE_ID
				   };


				   float3 WorldToTangentSpace(ShaderData d, float3 normal)
				   {
					   return mul(d.TBNMatrix, normal);
				   }

				   float3 TangentToWorldSpace(ShaderData d, float3 normal)
				   {
					   return mul(normal, d.TBNMatrix);
				   }


				   float3 GetCameraWorldPosition()
				   {
					   return _WorldSpaceCameraPos;
				   }

				   half3 GetSceneColor(float2 uv)
				   {
					   return SHADERGRAPH_SAMPLE_SCENE_COLOR(uv);
				   }


				   float GetSceneDepth(float2 uv) { return SHADERGRAPH_SAMPLE_SCENE_DEPTH(uv); }
				   float GetLinear01Depth(float2 uv) { return Linear01Depth(GetSceneDepth(uv), _ZBufferParams); }
				   float GetLinearEyeDepth(float2 uv) { return LinearEyeDepth(GetSceneDepth(uv), _ZBufferParams); }

				   float3 GetWorldPositionFromDepthBuffer(float2 uv, float3 worldSpaceViewDir)
				   {
					   float eye = GetLinearEyeDepth(uv);
					   float3 camView = mul((float3x3)GetObjectToWorldMatrix(),
											transpose(mul(GetWorldToObjectMatrix(), UNITY_MATRIX_I_V))[2].xyz);

					   float dt = dot(worldSpaceViewDir, camView);
					   float3 div = worldSpaceViewDir / dt;
					   float3 worldPos = (eye * div) + GetCameraWorldPosition();
					   return worldPos;
				   }

			   CBUFFER_START(UnityPerMaterial)

				   half4 _Color;
				   float4 _MainTex_ST;
				   float4 _NormalMap_ST;
				   float4 _EmissionMap_ST;
				   float4 _SmoothnessMap_ST;
				   float4 _SpecularMap_ST;
				   float4 _MetallicMap_ST;
				   float4 _AmbientOcclusionMap_ST;
				   half _SmoothnessStrength;
				   half _NormalStrength;
				   half _EmissionStrength;
				   half _AoStrength;
				   half _Cutoff;
				   half4 _EmissionColor;
				   float _DoubleSidedNormalMode;
				   half _FresnelStrength;
				   half _Transparency;
				   half _Surface;

				   half3 _SpecularColor;
				   half _MetallicStrength;

				   half _LightmapStrength;
				   half _LightmapAoStrength;
				   float4 _LightmapTex_ST;

				   half _DetailTiling;
				   half _DetailNormalStrength;
				   half _DetailSmoothnessStrength;
				   half _DetailAoStrength;
				   float4 _DetailMaskMap_ST;

				   float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
				   float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
				   float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
				   float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;

			   CBUFFER_END

				   #define unity_ObjectToWorld GetObjectToWorldMatrix()
				   #define unity_WorldToObject GetWorldToObjectMatrix()

				   TEXTURE2D(_MainTex);			SAMPLER(sampler_MainTex);
				   float3 _LightDirection;

				   float Dither8x8Bayer(int x, int y)
				   {
					   const float dither[64] = {
						   1, 49, 13, 61, 4, 52, 16, 64,
						   33, 17, 45, 29, 36, 20, 48, 32,
						   9, 57, 5, 53, 12, 60, 8, 56,
						   41, 25, 37, 21, 44, 28, 40, 24,
						   3, 51, 15, 63, 2, 50, 14, 62,
						   35, 19, 47, 31, 34, 18, 46, 30,
						   11, 59, 7, 55, 10, 58, 6, 54,
						   43, 27, 39, 23, 42, 26, 38, 22
					   };
					   int r = y * 8 + x;
					   return dither[r] / 64;
				   }

				   void ApplyDitherCrossFadeVSP(float2 vpos, float fade)
				   {
					   float dither = Dither8x8Bayer(fmod(vpos.x, 8), fmod(vpos.y, 8));
					   clip(dither - (fade));
				   }

				   void DitheringFade(inout Surface o, ShaderData d)
				   {
						float4 screenPosNorm = d.screenPos / d.screenPos.w;
						float2 clipScreen = screenPosNorm.xy * _ScreenParams.xy;
						float trans = lerp(0, 1.01, _Transparency);
						ApplyDitherCrossFadeVSP(clipScreen, trans);
				   }

				   ShaderData CreateShaderData(VertexToPixel i)
				   {
					   ShaderData d = (ShaderData)0;
					   d.clipPos = i.pos;
					   d.worldSpacePosition = i.worldPos;
					   d.worldSpaceNormal = normalize(i.worldNormal);
					   d.worldSpaceTangent = normalize(i.worldTangent.xyz);
					   d.tangentSign = i.worldTangent.w;
					   float3 bitangent = cross(i.worldTangent.xyz, i.worldNormal) * d.tangentSign * -1;
					   d.TBNMatrix = float3x3(d.worldSpaceTangent, bitangent, d.worldSpaceNormal);
					   d.worldSpaceViewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
					   d.tangentSpaceViewDir = mul(d.TBNMatrix, d.worldSpaceViewDir);
					   d.texcoord0 = i.texcoord0;

					   #if defined(SHADER_STAGE_FRAGMENT)
						   d.isFrontFace = i.facing;
					   #endif

					   d.screenPos = i.screenPos;
					   d.screenUV = (i.screenPos.xy / i.screenPos.w);

					   return d;
				   }


				   VertexToPixel Vert(VertexData v)
				   {
					   VertexToPixel o = (VertexToPixel)0;

					   UNITY_SETUP_INSTANCE_ID(v);
					   UNITY_TRANSFER_INSTANCE_ID(v, o);
					   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

					   o.texcoord0 = v.texcoord0;
					   VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
					   o.worldPos = TransformObjectToWorld(v.vertex.xyz);
					   o.worldNormal = TransformObjectToWorldNormal(v.normal);
					   o.worldTangent = float4(TransformObjectToWorldDir(v.tangent.xyz), v.tangent.w);
					   o.pos = TransformWorldToHClip(ApplyShadowBias(o.worldPos, o.worldNormal, _LightDirection));

					   #if UNITY_REVERSED_Z
						   o.pos.z = min(o.pos.z, o.pos.w * UNITY_NEAR_CLIP_VALUE);
					   #else
						   o.pos.z = max(o.pos.z, o.pos.w * UNITY_NEAR_CLIP_VALUE);
					   #endif

					   o.screenPos = ComputeScreenPos(o.pos, _ProjectionParams.x);
					   o.shadowCoord = GetShadowCoord(vertexInput);

					   return o;
				   }


				   half Alpha(half albedoAlpha, half4 color, half cutoff)
				   {
					   half alpha = albedoAlpha * color.a * _Transparency;
					   return alpha;
				   }

				   half4 Frag(VertexToPixel IN) : SV_Target
				   {
					   UNITY_SETUP_INSTANCE_ID(IN);

					   ShaderData d = CreateShaderData(IN);
					   Surface l = (Surface)0;

					   float2 uv = d.texcoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					   half4 albedoTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

					   #if _CUTOUT
					   clip(albedoTex.a - _Cutoff);
					   #endif

					   #if _FADE
						   DitheringFade(l, d);
					   #endif

					   Alpha(albedoTex.a, _Color, _Cutoff);
					   return 0;
				   }
				   ENDHLSL
			   }

			   Pass
			   {
				   Name "DepthOnly"
				   Tags
				   {
					   "LightMode" = "DepthOnly"
				   }

				   ZWrite[_ZWrite]
				   Blend[_SrcBlend][_DstBlend]
				   Cull[_Cull]


				   HLSLPROGRAM
				   #pragma vertex Vert
				   #pragma fragment Frag

				   #pragma shader_feature_local_fragment _ALPHATEST_ON
				   #pragma shader_feature_local_fragment _CUTOUT
				   #pragma shader_feature_local_fragment _CUTTINGPLANE

				   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				   #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
			#include "Includes/CuttingPlane.cginc"

				   struct VertexToPixel
				   {
					   float4 positionCS   : SV_POSITION;
					   float4 uv           : TEXCOORD1;
					   float3 normalWS     : TEXCOORD2;
					   float3 positionWS	: TEXCOORD3;
					   float4 screenPos    : TEXCOORD4;

					   UNITY_VERTEX_INPUT_INSTANCE_ID
					   UNITY_VERTEX_OUTPUT_STEREO
				   };

				   struct VertexData
				   {
					   float4 positionOS     : POSITION;
					   float4 tangentOS      : TANGENT;
					   float4 texcoord     : TEXCOORD0;
					   float3 normal       : NORMAL;
					   float3 worldPos : TEXCOORD1;
					   UNITY_VERTEX_INPUT_INSTANCE_ID
				   };

				   struct ShaderData
				   {
					   float4 clipPos;
					   float3 worldSpacePosition;
					   float4 texcoord0;
					   float4 screenPos;
				   };

				   CBUFFER_START(UnityPerMaterial)

					   half4 _Color;
					   float4 _MainTex_ST;
					   float4 _NormalMap_ST;
					   float4 _EmissionMap_ST;
					   float4 _SmoothnessMap_ST;
					   float4 _SpecularMap_ST;
					   float4 _MetallicMap_ST;
					   float4 _AmbientOcclusionMap_ST;
					   half _SmoothnessStrength;
					   half _NormalStrength;
					   half _EmissionStrength;
					   half _AoStrength;
					   half _Cutoff;
					   half4 _EmissionColor;
					   float _DoubleSidedNormalMode;
					   half _FresnelStrength;
					   half _Transparency;
					   half _Surface;

					   half3 _SpecularColor;
					   half _MetallicStrength;

					   half _LightmapStrength;
					   half _LightmapAoStrength;
					   float4 _LightmapTex_ST;

					   half _DetailTiling;
					   half _DetailNormalStrength;
					   half _DetailSmoothnessStrength;
					   half _DetailAoStrength;
					   float4 _DetailMaskMap_ST;

					   float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
					   float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
					   float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
					   float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;

				   CBUFFER_END

				   #define unity_ObjectToWorld GetObjectToWorldMatrix()
				   #define unity_WorldToObject GetWorldToObjectMatrix()

				   TEXTURE2D(_MainTex);					SAMPLER(sampler_MainTex);

				   VertexToPixel Vert(VertexData IN)
				   {
					   VertexToPixel output = (VertexToPixel)0;
					   UNITY_SETUP_INSTANCE_ID(IN);
					   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

					   output.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
					   output.uv = IN.texcoord;
					   output.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
					   output.screenPos = ComputeScreenPos(output.positionCS);

					   VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normal, IN.tangentOS);
					   output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);

					   return output;
				   }

				   ShaderData CreateShaderData(VertexToPixel i)
				   {
					   ShaderData d = (ShaderData)0;
					   d.clipPos = i.positionCS;
					   d.worldSpacePosition = i.positionWS;
					   d.screenPos = i.screenPos;
					   d.texcoord0 = i.uv;
					   return d;
				   }

				   half4 Frag(VertexToPixel IN) : SV_Target
				   {
#if _CUTTINGPLANE || _CUTOUT
					ShaderData d = CreateShaderData(IN);

#endif

					   #if _CUTTINGPLANE
						   bool withinBorder0 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorder0);
						   bool withinBorder1 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorder1);
						   bool withinBorder2 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorder2);
					   #endif

					   UNITY_SETUP_INSTANCE_ID(IN);
					   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

					   #if _CUTOUT
						   float2 uv = d.texcoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
						   half4 albedoTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
						   clip(albedoTex.a - _Cutoff);
					   #endif

						   // #if _FADE
						   // 	float4 screenPosNorm = d.screenPos / d.screenPos.w;
						   // 	float2 clipScreen = screenPosNorm.xy * _ScreenParams.xy;
						   // 	float fade = lerp(0, 1.01, _Transparency);
						   // float dither = Dither8x8Bayer(fmod(clipScreen.x, 8), fmod(clipScreen.y, 8));
						   // 	clip(dither - fade);
						   // #endif

						   return 0;
					   }

					   ENDHLSL
				   }

					   //commenting out depth normal.. DN Prepass in profiler is using nearly 2ms!
					   //remember to Fix AO to depth normal in RendererAO feature if I ever figure out this one...

						   Pass
						   {
							   Name "DepthNormals"
							   Tags{"LightMode" = "DepthNormals"}

							   Cull[_Cull]
							   ZWrite[_ZWrite]
							   Cull[_Cull]

							   HLSLPROGRAM
							   #pragma vertex Vert
							   #pragma fragment Frag
							   #pragma target 3.0
							   #pragma prefer_hlslcc gles
							   #pragma exclude_renderers d3d11_9x

						   // Keywords
						   #pragma shader_feature_local_fragment _ _CUTOUT
						   #pragma shader_feature_local_fragment _ _FADE
						   #pragma shader_feature_local_fragment _ _CUTTINGPLANE
						   #pragma shader_feature_local_fragment _ALPHATEST_ON

						   #define NEED_FACING 1

						   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
						   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
						   #include "Includes/CuttingPlane.cginc"
						   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
						   #include "Includes/Utilities.cginc"

						   struct VertexData
						   {
							   float4 positionOS     : POSITION;
							   float4 tangentOS      : TANGENT;
							   float4 texcoord     : TEXCOORD0;
							   float3 normal       : NORMAL;
							   float3 worldPos : TEXCOORD1;
							   UNITY_VERTEX_INPUT_INSTANCE_ID
						   };

						   struct VertexToPixel
						   {
							   float4 positionCS   : SV_POSITION;
							   float4 uv           : TEXCOORD1;
							   float3 normalWS     : TEXCOORD2;
							   float3 positionWS	: TEXCOORD3;
							   float4 screenPos    : TEXCOORD4;

							   UNITY_VERTEX_INPUT_INSTANCE_ID
							   UNITY_VERTEX_OUTPUT_STEREO
						   };

						   struct Surface
						   {
							   half3 Normal;
							   half3 Emission;
							   half Alpha;
						   };

						   struct ShaderData
						   {
							   float4 clipPos;
							   float3 worldSpacePosition;
							   float4 uv;
							   float4 screenPos;
						   };

						   CBUFFER_START(UnityPerMaterial)

							   half4 _Color;
							   float4 _MainTex_ST;
							   float4 _NormalMap_ST;
							   float4 _EmissionMap_ST;
							   float4 _SmoothnessMap_ST;
							   float4 _SpecularMap_ST;
							   float4 _MetallicMap_ST;
							   float4 _AmbientOcclusionMap_ST;
							   half _SmoothnessStrength;
							   half _NormalStrength;
							   half _EmissionStrength;
							   half _AoStrength;
							   half _Cutoff;
							   half4 _EmissionColor;
							   float _DoubleSidedNormalMode;
							   half _FresnelStrength;
							   half _Transparency;
							   half _Surface;

							   half3 _SpecularColor;
							   half _MetallicStrength;

							   half _LightmapStrength;
							   half _LightmapAoStrength;
							   float4 _LightmapTex_ST;

							   half _DetailTiling;
							   half _DetailNormalStrength;
							   half _DetailSmoothnessStrength;
							   half _DetailAoStrength;
							   float4 _DetailMaskMap_ST;

							   float4 _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorderColor0;
							   float4 _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorderColor1;
							   float4 _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorderColor2;
							   float _CullVolBorder0, _CullVolBorder1, _CullVolBorder2;

						   CBUFFER_END

						   TEXTURE2D(_MainTex);		SAMPLER(sampler_MainTex);
						   TEXTURE2D(_NormalMap);      SAMPLER(sampler_NormalMap);

						   ShaderData CreateShaderData(VertexToPixel i)
						   {
							   ShaderData d = (ShaderData)0;
							   d.clipPos = i.positionCS;
							   d.worldSpacePosition = i.positionWS;
							   d.screenPos = i.screenPos;
							   d.uv = i.uv;
							   return d;
						   }

						   VertexToPixel Vert(VertexData IN)
						   {
							   VertexToPixel output = (VertexToPixel)0;
							   UNITY_SETUP_INSTANCE_ID(IN);
							   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

							   output.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
							   output.uv = IN.texcoord;
							   output.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
							   output.screenPos = ComputeScreenPos(output.positionCS);

							   VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normal, IN.tangentOS);
							   output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);

							   return output;
						   }

						   half4 Frag(VertexToPixel IN) : SV_TARGET
						   {
							   UNITY_SETUP_INSTANCE_ID(IN);
							   UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

							   ShaderData d = CreateShaderData(IN);

							   #if _CUTTINGPLANE
								   bool withinBorder0 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter0, _CullVolX0, _CullVolZ0, _CullVolSize0, _CullVolBorder0);
								   bool withinBorder1 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter1, _CullVolX1, _CullVolZ1, _CullVolSize1, _CullVolBorder1);
								   bool withinBorder2 = handleCullingVolume(d.worldSpacePosition, _CullVolCenter2, _CullVolX2, _CullVolZ2, _CullVolSize2, _CullVolBorder2);
							   #endif

							   #if _CUTOUT
							   float2 uv = d.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
							   half4 albedoTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
							   clip(albedoTex.a - _Cutoff);
							   #endif

							   #if _FADE
								   float4 screenPosNorm = d.screenPos / d.screenPos.w;
								   float2 clipScreen = screenPosNorm.xy * _ScreenParams.xy;
								   float fade = lerp(0, 1.01, _Transparency);
								   float dither = Dither8x8Bayer(fmod(clipScreen.x, 8), fmod(clipScreen.y, 8));
								   clip(dither - fade);
							   #endif

							   return half4(NormalizeNormalPerPixel(IN.normalWS), 0.0);
						   }

						   ENDHLSL

					   }


		}
			//    CustomEditor "CavLitGUI"
}
