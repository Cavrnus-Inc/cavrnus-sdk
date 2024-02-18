Shader "LoadingBounds"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_Fill1("Fill", Range( 0 , 1)) = 1
		_FillColor1("FillColor", Color) = (1,1,1,1)
		_BorderWidth1("Border Width", Float) = 0.5
		[HDR]_BorderColor1("BorderColor", Color) = (1,0.9626852,0,1)
		_BorderBackFaceAlpha1("BorderBackFaceAlpha", Range( 0 , 1)) = 0.5
	}

	SubShader
	{
		LOD 0
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Geometry" }
		Cull Off
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 2.0
		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 
		ENDHLSL
		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha, One Zero
			ZWrite On
			ZTest LEqual
			ColorMask RGBA
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#pragma multi_compile_instancing

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float3 ase_normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _BorderColor1;
			float4 _FillColor1;
			float _BorderBackFaceAlpha1;
			float _BorderWidth1;
			CBUFFER_END
			UNITY_INSTANCING_BUFFER_START(LoadingBounds)
				UNITY_DEFINE_INSTANCED_PROP(float, _Fill1)
			UNITY_INSTANCING_BUFFER_END(LoadingBounds)
						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.ase_texcoord3 = v.vertex;
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				o.ase_normal = v.ase_normal;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.zw = 0;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );
				o.worldPos = positionWS;
				o.clipPos = positionCS;
				return o;
			}

			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half4 frag ( VertexOutput IN , FRONT_FACE_TYPE ase_vface : FRONT_FACE_SEMANTIC ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float3 WorldPosition = IN.worldPos;
				float _Fill1_Instance = UNITY_ACCESS_INSTANCED_PROP(LoadingBounds,_Fill1);
				float3 object_scale = float3( length( GetObjectToWorldMatrix()[ 0 ].xyz ), length( GetObjectToWorldMatrix()[ 1 ].xyz ), length( GetObjectToWorldMatrix()[ 2 ].xyz ) );
				float4 transform62 = mul(GetObjectToWorldMatrix(),float4( IN.ase_texcoord3.xyz , 0.0 ));
				float switchResult85 = ase_vface>0?(0.0):(( 1.0 - step( -0.501 * object_scale.y + (_Fill1_Instance - 0.0) * (object_scale.y * 0.501 - ( -0.501 * object_scale.y )) / (1.0 - 0.0) , transform62.y ) ));
				float4 lerpResult95 = lerp( _BorderColor1 , _FillColor1 , switchResult85);
				float4 lerpResult98 = lerp( _FillColor1 , _BorderColor1 , _BorderBackFaceAlpha1);
				float3 ase_parentObjectScale = ( 1.0 / float3( length( GetWorldToObjectMatrix()[ 0 ].xyz ), length( GetWorldToObjectMatrix()[ 1 ].xyz ), length( GetWorldToObjectMatrix()[ 2 ].xyz ) ) );
				float2 appendResult7 = (float2(ase_parentObjectScale.x , ase_parentObjectScale.y));
				float2 texCoord22 = IN.ase_texcoord4.xy * appendResult7 + ( ( appendResult7 * -0.5 ) + 0.5 );
				float mulTime14 = _TimeParameters.x * -0.25;
				float2 appendResult19 = (float2(mulTime14 , 0.5));
				float temp_output_18_0 = radians( 45.0 );
				float cos23 = cos( temp_output_18_0 );
				float sin23 = sin( temp_output_18_0 );
				float2 rotator23 = mul( texCoord22 - appendResult19 , float2x2( cos23 , -sin23 , sin23 , cos23 )) + appendResult19;
				float2 appendResult10_g46 = (float2(0.5 , 1.0));
				float2 temp_output_11_0_g46 = ( abs( (frac( ( rotator23 * 6.0 ) )*2.0 + -1.0) ) - appendResult10_g46 );
				float2 break16_g46 = 1.0 - temp_output_11_0_g46 / fwidth( temp_output_11_0_g46 );
				float temp_output_30_0 = ase_parentObjectScale.x - _BorderWidth1;
				float temp_output_29_0 = ase_parentObjectScale.y - _BorderWidth1;
				float2 appendResult10_g42 = (float2(temp_output_30_0 , temp_output_29_0));
				float2 temp_output_11_0_g42 = ( abs( (texCoord22*2.0 + -1.0) ) - appendResult10_g42 );
				float2 break16_g42 = ( 1.0 - ( temp_output_11_0_g42 / fwidth( temp_output_11_0_g42 ) ) );
				float lerpResult60 = lerp( 0.0 , saturate( min( break16_g46.x , break16_g46.y ) ) , ( 1.0 - saturate( min( break16_g42.x , break16_g42.y ) ) ));
				float3 normalizeResult40 = normalize( IN.ase_normal );
				float3 break46 = normalizeResult40;
				float lerpResult77 = lerp( 0.0 , lerpResult60 , abs( break46.z ));
				float2 appendResult10 = (float2(ase_parentObjectScale.x , ase_parentObjectScale.z));
				float2 texCoord28 = IN.ase_texcoord4.xy * appendResult10 + ( ( appendResult10 * -0.5 ) + 0.5 );
				float cos34 = cos( temp_output_18_0 );
				float sin34 = sin( temp_output_18_0 );
				float2 rotator34 = mul( texCoord28 - appendResult19 , float2x2( cos34 , -sin34 , sin34 , cos34 )) + appendResult19;
				float2 appendResult10_g48 = (float2(0.5 , 1.0));
				float2 temp_output_11_0_g48 = ( abs( (frac( ( rotator34 * 6.0 ) )*2.0 + -1.0) ) - appendResult10_g48 );
				float2 break16_g48 = ( 1.0 - ( temp_output_11_0_g48 / fwidth( temp_output_11_0_g48 ) ) );
				float temp_output_42_0 = ( ase_parentObjectScale.z - _BorderWidth1 );
				float2 appendResult10_g45 = (float2(temp_output_30_0 , temp_output_42_0));
				float2 temp_output_11_0_g45 = ( abs( (texCoord28*2.0 + -1.0) ) - appendResult10_g45 );
				float2 break16_g45 = ( 1.0 - ( temp_output_11_0_g45 / fwidth( temp_output_11_0_g45 ) ) );
				float lerpResult78 = lerp( 0.0 , saturate( min( break16_g48.x , break16_g48.y ) ) , ( 1.0 - saturate( min( break16_g45.x , break16_g45.y ) ) ));
				float lerpResult82 = lerp( lerpResult77 , lerpResult78 , abs( break46.y ));
				float2 appendResult17 = (float2(ase_parentObjectScale.z , ase_parentObjectScale.y));
				float2 texCoord32 = IN.ase_texcoord4.xy * appendResult17 + ( ( appendResult17 * -0.5 ) + 0.5 );
				float cos39 = cos( radians( -45.0 ) );
				float sin39 = sin( radians( -45.0 ) );
				float2 rotator39 = mul( texCoord32 - appendResult19 , float2x2( cos39 , -sin39 , sin39 , cos39 )) + appendResult19;
				float2 appendResult10_g49 = (float2(0.5 , 1.0));
				float2 temp_output_11_0_g49 = ( abs( (frac( ( rotator39 * 6.0 ) )*2.0 + -1.0) ) - appendResult10_g49 );
				float2 break16_g49 = ( 1.0 - ( temp_output_11_0_g49 / fwidth( temp_output_11_0_g49 ) ) );
				float2 appendResult10_g47 = (float2(temp_output_42_0 , temp_output_29_0));
				float2 temp_output_11_0_g47 = ( abs( (texCoord32*2.0 + -1.0) ) - appendResult10_g47 );
				float2 break16_g47 = ( 1.0 - ( temp_output_11_0_g47 / fwidth( temp_output_11_0_g47 ) ) );
				float lerpResult81 = lerp( 0.0 , saturate( min( break16_g49.x , break16_g49.y ) ) , ( 1.0 - saturate( min( break16_g47.x , break16_g47.y ) ) ));
				float lerpResult87 = lerp( lerpResult82 , lerpResult81 , abs( break46.x ));
				float switchResult90 = (((ase_vface>0)?(0.0):(lerpResult87)));
				float4 lerpResult101 = lerp( lerpResult95 , lerpResult98 , ( switchResult85 * switchResult90 ));
				float switchResult102 = (((ase_vface>0)?(lerpResult87):(0.0)));
				float4 lerpResult104 = lerp( lerpResult101 , _BorderColor1 , switchResult102);
				
				float mulTime69 = _TimeParameters.x * 0.75;
				float temp_output_91_0 = ( lerpResult87 * (0.25 + (cos( ( mulTime69 * 5.0 ) ) - -1.0) * (1.0 - 0.25) / (1.0 - -1.0)) );
				float Alpha = (((ase_vface>0)?(temp_output_91_0):(( ( temp_output_91_0 * _BorderBackFaceAlpha1 ) + ( ( switchResult85 * saturate( ( distance( _WorldSpaceCameraPos , WorldPosition ) * ( 1.0 / ( object_scale.x + object_scale.y + object_scale.z + 5.0 ) ) ) ) ) * 0.5 ) ))));
				
				float3 Color = lerpResult104.rgb;

				float AlphaClipThreshold = 0.1;
				clip( Alpha - AlphaClipThreshold );

				return half4( Color, Alpha );
			}

			ENDHLSL
		}
	}
}
