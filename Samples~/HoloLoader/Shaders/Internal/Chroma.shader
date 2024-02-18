Shader "Cavrnus/Internal/Chroma"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_ChromaKeyColor("ChromaColor", Color) = (0.0, 1.0, 0.0, 0.0)
		_ChromaKeyHueAllowance("Hue Allowance", Range(0,1)) = .1
		_ChromaKeySaturationAllowance("Saturation Allowance", Range(0,1)) = .5
		_ChromaKeyValueAllowance("Brightness Allowance", Range(0,1)) = 1 //.5
		_BackfaceAlphaMult("Backface Alpha Multiplier", Range(0,1)) = .4
    	_Transparency("Transparency", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent +1" "RenderType" = "Transparent" "IGNOREPROJECTOR"="true" "PreviewType"="Plane"}
    	Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
    	Zwrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

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
            	UNITY_FOG_COORDS(2)
	            UNITY_VERTEX_OUTPUT_STEREO  
            };

			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ChromaKeyColor;
			float _ChromaKeyHueAllowance;
			float _ChromaKeySaturationAllowance;
			float _ChromaKeyValueAllowance;
			float _BackfaceAlphaMult;
            float _Transparency;

            
			inline float3 ChromaKeyRGB2HSV(float3 rgb)
			{
				float4 k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(rgb.bg, k.wz), float4(rgb.gb, k.xy), step(rgb.b, rgb.g));
				float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));
				float d = q.x - min(q.w, q.y);
				float e = 1e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			inline float3 ChromaKeyCalcDiffrence(float4 col)
			{
				float3 hsv = ChromaKeyRGB2HSV(col.rgb);
				float3 key = ChromaKeyRGB2HSV(_ChromaKeyColor.rgb);
				return abs(hsv - key);
			}

			inline float3 ChromaKeyGetRange()
			{
				return float3(_ChromaKeyHueAllowance, _ChromaKeySaturationAllowance, _ChromaKeyValueAllowance);
			}

			inline void ChromaKeyCutout(float4 col)
			{
				float3 d = ChromaKeyCalcDiffrence(col);
				clip(all(step(0.0, ChromaKeyGetRange() - d)) ? -1 : 1);
			}

			inline float ChromaKeyAlpha(float4 col)
			{
				float3 d = ChromaKeyCalcDiffrence(col);
				clip(all(step(0.0, ChromaKeyGetRange() - d)) ? -1 : 1);
				return saturate(length(d / ChromaKeyGetRange()) - 1.0);
			}

            
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
		        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
            	i.uv.x = 1.0 - i.uv.x;
				fixed4 col = tex2D(_MainTex, i.uv);
				float alpha = ChromaKeyAlpha(col);
				if (facing < 0)
					alpha = alpha * _BackfaceAlphaMult;

				//col.rgba *= _Color.rgba;
				col.a *= alpha;
            	col.a *= 1- _Transparency;
            	UNITY_APPLY_FOG(i.fogCoord, col);
            	return col;

            }
            ENDCG
        }
	    Pass
		{
			Name "CastShadow"
			Tags { "LightMode" = "ShadowCaster" }
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"
			
			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _ChromaKeyColor;
			float _ChromaKeyHueAllowance;
			float _ChromaKeySaturationAllowance;
			float _ChromaKeyValueAllowance;


			// Courtesy of https://github.com/hecomi/uChromaKey/blob/master/Assets/uChromaKey/Shaders/ChromaKey.cginc (MIT licensed)
			inline float3 ChromaKeyRGB2HSV(float3 rgb)
			{
				float4 k = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(rgb.bg, k.wz), float4(rgb.gb, k.xy), step(rgb.b, rgb.g));
				float4 q = lerp(float4(p.xyw, rgb.r), float4(rgb.r, p.yzx), step(p.x, rgb.r));
				float d = q.x - min(q.w, q.y);
				float e = 1e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			inline float3 ChromaKeyCalcDiffrence(float4 col)
			{
				float3 hsv = ChromaKeyRGB2HSV(col.rgb);
				float3 key = ChromaKeyRGB2HSV(_ChromaKeyColor.rgb);
				return abs(hsv - key);
			}

			inline float3 ChromaKeyGetRange()
			{
				return float3(_ChromaKeyHueAllowance, _ChromaKeySaturationAllowance, _ChromaKeyValueAllowance);
			}

			inline void ChromaKeyCutout(float4 col)
			{
				float3 d = ChromaKeyCalcDiffrence(col);
				clip(all(step(0.0, ChromaKeyGetRange() - d)) ? -1 : 1);
			}

			inline float ChromaKeyAlpha(float4 col)
			{
				float3 d = ChromaKeyCalcDiffrence(col);
				clip(all(step(0.0, ChromaKeyGetRange() - d)) ? -1 : 1);
				return saturate(length(d / ChromaKeyGetRange()) - 1.0);
			}


			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 uv : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o);
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
		        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				// Chroma-ize
				float alpha = ChromaKeyAlpha(col);
				clip(alpha - .05);

				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
    }
}
}
