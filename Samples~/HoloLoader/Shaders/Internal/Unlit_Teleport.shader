Shader "Custom/TeleportLineShader"
{

	    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_PulseColor("Pulse Color", Color) = (1,1,1,1)
		_MainTex("Color (RGBA)", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)] _Culling("Culling", Float) = 0
		_StartPoint("Start Point", Vector) = (0, 0, 0, 0)
		_Velocity("Velocity", Vector) = (5, 5, 5, 0)
		_Acceleration("Acceleration", Vector) = (0, -1, 0, 0)
		_EndT("End T", Float) = 10
		_Thickness("Thickness", Float) = .02
		_TimeSpeed("Speed", Range(-10,10)) = 1
		_SegmentInverse("Segment Inverse", Range(-100,100)) = 30
		[Toggle] _Fadeout("Fadeout", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"  "Queue" = "Transparent+2" }
		Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

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
            	float distanceDownLine : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
				half4 _Color;
				half4 _PulseColor;
				float4 _MainTex_ST;
				float3 _StartPoint;
				float3 _Velocity;
				float3 _Acceleration;
				float _EndT;
				float _Thickness;
				float _TimeSpeed;
				float _SegmentInverse;
				float _Fadeout;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
            	
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
            	
            	float3 inVertex = IN.positionOS.xyz;
            	float t = pow(inVertex.z, 4);
            	t = t * _EndT;			
				float3 beamCenter = _StartPoint + _Velocity * t + _Acceleration * t * t;

            	float3 f = _Velocity + _Acceleration * t;
				float forwardVectorLength = length(f);
				if (forwardVectorLength < .01)
					f = float3(0, -1, 0);
				else
					f = normalize(f);

				float3 bn = cross(f, _Acceleration);
				if (length(bn) < .01)
					bn = float3(1, 0, 0);
				else
					bn = normalize(bn);

				float3 normal = cross(bn, f);

				float3 outVertex = beamCenter + inVertex.x * bn * _Thickness + inVertex.y * normal * _Thickness;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(outVertex);

				OUT.distanceDownLine = forwardVectorLength * t;
                OUT.positionHCS = vertexInput.positionCS;

                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
            	
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
            
                float glow = abs(cos(IN.distanceDownLine * _SegmentInverse - _Time.y * _TimeSpeed));

			    if (glow < .5)
				    glow = 0;
			    else
				    glow = 1;

                half4 color = tex2D(_MainTex, IN.uv);

			    float4 col = lerp(color * _Color, _PulseColor, glow);
			    if (_Fadeout > 0)
				    col.a *= pow(abs((_EndT - IN.distanceDownLine) / _EndT), 2.0*_Fadeout);
			    return col;
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

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
                float distanceDownLine : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _PulseColor;
                float4 _MainTex_ST;
                float3 _StartPoint;
                float3 _Velocity;
                float3 _Acceleration;
                float _EndT;
                float _Thickness;
                float _TimeSpeed;
                float _SegmentInverse;
                float _Fadeout;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 inVertex = IN.positionOS.xyz;
                float t = pow(inVertex.z, 4);
                t = t * _EndT;
                float3 beamCenter = _StartPoint + _Velocity * t + _Acceleration * t * t;

                float3 f = _Velocity + _Acceleration * t;
                float forwardVectorLength = length(f);
                if (forwardVectorLength < .01)
                    f = float3(0, -1, 0);
                else
                    f = normalize(f);

                float3 bn = cross(f, _Acceleration);
                if (length(bn) < .01)
                    bn = float3(1, 0, 0);
                else
                    bn = normalize(bn);

                float3 normal = cross(bn, f);

                float3 outVertex = beamCenter + inVertex.x * bn * _Thickness + inVertex.y * normal * _Thickness;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(outVertex);

                OUT.distanceDownLine = forwardVectorLength * t;
                OUT.positionHCS = vertexInput.positionCS;

                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                return 0;
            }
            ENDHLSL
        }

        Pass //depth
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

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
                float distanceDownLine : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _PulseColor;
                float4 _MainTex_ST;
                float3 _StartPoint;
                float3 _Velocity;
                float3 _Acceleration;
                float _EndT;
                float _Thickness;
                float _TimeSpeed;
                float _SegmentInverse;
                float _Fadeout;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 inVertex = IN.positionOS.xyz;
                float t = pow(inVertex.z, 4);
                t = t * _EndT;
                float3 beamCenter = _StartPoint + _Velocity * t + _Acceleration * t * t;

                float3 f = _Velocity + _Acceleration * t;
                float forwardVectorLength = length(f);
                if (forwardVectorLength < .01)
                    f = float3(0, -1, 0);
                else
                    f = normalize(f);

                float3 bn = cross(f, _Acceleration);
                if (length(bn) < .01)
                    bn = float3(1, 0, 0);
                else
                    bn = normalize(bn);

                float3 normal = cross(bn, f);

                float3 outVertex = beamCenter + inVertex.x * bn * _Thickness + inVertex.y * normal * _Thickness;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(outVertex);

                OUT.distanceDownLine = forwardVectorLength * t;
                OUT.positionHCS = vertexInput.positionCS;

                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                return 0;
            }
            ENDHLSL }

        Pass // depth normals
        {
            Name "DepthNormalsOnly"
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

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
                float distanceDownLine : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _PulseColor;
                float4 _MainTex_ST;
                float3 _StartPoint;
                float3 _Velocity;
                float3 _Acceleration;
                float _EndT;
                float _Thickness;
                float _TimeSpeed;
                float _SegmentInverse;
                float _Fadeout;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 inVertex = IN.positionOS.xyz;
                float t = pow(inVertex.z, 4);
                t = t * _EndT;
                float3 beamCenter = _StartPoint + _Velocity * t + _Acceleration * t * t;

                float3 f = _Velocity + _Acceleration * t;
                float forwardVectorLength = length(f);
                if (forwardVectorLength < .01)
                    f = float3(0, -1, 0);
                else
                    f = normalize(f);

                float3 bn = cross(f, _Acceleration);
                if (length(bn) < .01)
                    bn = float3(1, 0, 0);
                else
                    bn = normalize(bn);

                float3 normal = cross(bn, f);

                float3 outVertex = beamCenter + inVertex.x * bn * _Thickness + inVertex.y * normal * _Thickness;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(outVertex);

                OUT.distanceDownLine = forwardVectorLength * t;
                OUT.positionHCS = vertexInput.positionCS;

                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                return 0;
            }
            ENDHLSL
        }
    }
}