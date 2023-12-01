#define DECLARE_LAYER(l)      TEXTURE2D(_Layer_##l##_AlbedoMap);   float _Layer_##l##_Scale;   float4 _Layer_##l##_Color; float4 _Layer_##l##_MetallicOcclusionSmoothness;
#define DECLARE_NORMAL(l)     TEXTURE2D(_Layer_##l##_NormalMap); float _Layer_##l##_NormalStrength;
#define DECLARE_MASK(l)       TEXTURE2D(_Layer_##l##_Mask);      float4 _Layer_##l##_Min; float4 _Layer_##l##_Max;


TEXTURE2D(_Splatmap);
SAMPLER(sampler_Splatmap);
SAMPLER(sampler_Layer_0_AlbedoMap);

DECLARE_LAYER(0)
DECLARE_NORMAL(0)
DECLARE_MASK(0)

DECLARE_LAYER(1)
DECLARE_NORMAL(1)
DECLARE_MASK(1)

DECLARE_LAYER(2)
DECLARE_NORMAL(2)
DECLARE_MASK(2)

DECLARE_LAYER(3)
DECLARE_NORMAL(3)
DECLARE_MASK(3)

TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);

float4 MaskRemap(float4 value, float4 min, float4 max)
{
    return min + value * (max - min);
}


#define UNPACK_ALBEDO(index,uv, sum,splat)	 float4 paintColor##index = SAMPLE_TEXTURE2D(_Layer_##index##_AlbedoMap, sampler_Layer_0_AlbedoMap, uv * _Layer_##index##_Scale);	sum += paintColor##index * _Layer_##index##_Color * splat;
#define UNPACK_NORMAL(index,uv,sum,splat)     sum += UnpackNormalRGB(SAMPLE_TEXTURE2D(_Layer_##index##_NormalMap, sampler_Layer_0_AlbedoMap, uv * _Layer_##index##_Scale), _Layer_##index##_NormalStrength * splat);;
#define UNPACK_MASK(index,uv,sum,splat)       sum += MaskRemap(SAMPLE_TEXTURE2D(_Layer_##index##_Mask, sampler_Layer_0_AlbedoMap, uv * _Layer_##index##_Scale), _Layer_##index##_Min, _Layer_##index##_Max) * splat;

void CalculateLayersBlend(float2 uv, out float3 albedo, out float3 normal, out float metallic, out float smoothness, out float ao)
{
    float4 splatmap = SAMPLE_TEXTURE2D(_Splatmap, sampler_Splatmap, uv);
    float4 diffuse = 0;
    float4 mask = 0;
    normal = float3(0, 0, 0);

    UNPACK_ALBEDO(0, uv, diffuse, splatmap.r);
    UNPACK_ALBEDO(1, uv, diffuse, splatmap.g);
    UNPACK_ALBEDO(2, uv, diffuse, splatmap.b);
    UNPACK_ALBEDO(3, uv, diffuse, splatmap.a);

    UNPACK_NORMAL(0, uv, normal, splatmap.r);
    UNPACK_NORMAL(1, uv, normal, splatmap.g);
    UNPACK_NORMAL(2, uv, normal, splatmap.b);
    UNPACK_NORMAL(3, uv, normal, splatmap.a);

    UNPACK_MASK(0, uv, mask, splatmap.r);
    UNPACK_MASK(1, uv, mask, splatmap.g);
    UNPACK_MASK(2, uv, mask, splatmap.b);
    UNPACK_MASK(3, uv, mask, splatmap.a);

    albedo = diffuse.rgb;
    mask = saturate(mask);
    metallic = mask.r;
    smoothness = mask.a;
    ao = mask.g;
}
