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

float4 HardLight (float4 a, float4 b)
            {
                float4 o = b >= .5 ? 1.0 - 2 * (1.0 - b) * (1.0 - a) : 2.0 * a * b;
                o.a = b.a;
                return o;
            }

float GradientNoise(float2 uv)
            {
                uv = floor(uv * _ScreenParams.xy);
                float f = dot(float2(0.06711056f, 0.00583715f), uv);
                return frac(52.9829189f * frac(f));
            }


float4 Overlay (float4 a, float4 b)
            {
                float4 r = a < .5 ? 2.0 * a * b : 1.0 - 2.0 * (1.0 - a) * (1.0 - b);
                r.a = b.a;
                return r;
            }

void LinearBurn(float4 Base, float4 Blend, float Opacity, out float4 Out)
            {
                Out = Base + Blend - 1.0;
                Out = lerp(Base, Out, Opacity);
            }


void Blend_Overlay(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                float4 result1 = 1.0 - 2.0 * (1.0 - Base) * (1.0 - Blend);
                float4 result2 = 2.0 * Base * Blend;
                float4 zeroOrOne = step(Base, 0.5);
                Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
                Out = lerp(Base, Out, Opacity);

                // Out =  1.0 - (1.0 - Blend)/(Base + 0.000000000001);
                // Out = lerp(Base, Out, Opacity);


                // Out = Base * Blend;
                // Out = lerp(Base, Out, Opacity);
            }


         
void Remap(float4 In, float2 InMinMax, float2 OutMinMax, out float4 Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }


void Blend_Softlight(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                float4 result1 = 2.0 * Base * Blend + Base * Base * (1.0 - 2.0 * Blend);
                float4 result2 = sqrt(Base) * (2.0 * Blend - 1.0) + 2.0 * Base * (1.0 - Blend);
                float4 zeroOrOne = step(0.5, Blend);
                Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
                Out = lerp(Base, Out, Opacity);
            }


void Blend_Multiply(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base * Blend;
                Out = lerp(Base, Out, Opacity);
            }
   
            
float3 mod3D289(float3 v)
{
    return v - floor(v / 289.0) * 289.0;
}

float4 mod3D289(float4 v)
{
    return v - floor(v / 289.0) * 289.0;
}

float4 permute(float4 v)
{
    return mod3D289((v * 34.0 + 1.0) * v);
}

float4 taylorInvSqrt(float4 v)
{
    return 1.79284291400159 - v * 0.85373472095314;
}


float PerlinNoise(float3 v)
{
    const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
    float3 i = floor(v + dot(v, C.yyy));
    float3 x0 = v - i + dot(i, C.xxx);
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);
    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy;
    float3 x3 = x0 - 0.5;
    i = mod3D289(i);
    float4 p = permute(
        permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0)) + i.y + float4(0.0, i1.y, i2.y, 1.0)) + i.x + float4(
            0.0, i1.x, i2.x, 1.0));
    float4 j = p - 49.0 * floor(p / 49.0); // mod(p,7*7)
    float4 x_ = floor(j / 7.0);
    float4 y_ = floor(j - 7.0 * x_); // mod(j,N)
    float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
    float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;
    float4 h = 1.0 - abs(x) - abs(y);
    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);
    float4 s0 = floor(b0) * 2.0 + 1.0;
    float4 s1 = floor(b1) * 2.0 + 1.0;
    float4 sh = -step(h, 0.0);
    float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
    float3 g0 = float3(a0.xy, h.x);
    float3 g1 = float3(a0.zw, h.y);
    float3 g2 = float3(a1.xy, h.z);
    float3 g3 = float3(a1.zw, h.w);
    float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
    g0 *= norm.x;
    g1 *= norm.y;
    g2 *= norm.z;
    g3 *= norm.w;
    float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    m = m * m;
    float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
    return 42.0 * dot(m, px);
}

float2 VoronoiHash(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}

float Voronoi(float2 v, float time, inout float2 id, inout float2 mr, float smoothness)
{
    float2 n = floor(v);
    float2 f = frac(v);
    float F1 = 8.0;
    float F2 = 8.0;
    float2 mg = 0;
    for (int j = -1; j <= 1; j++)
    {
        for (int i = -1; i <= 1; i++)
        {
            float2 g = float2(i, j);
            float2 o = VoronoiHash(n + g);
            o = (sin(time + o * 6.2831) * 0.5 + 0.5);
            float2 r = f - g - o;
            float d = 0.5 * dot(r, r);
            float h = smoothstep(0.0, 1.0, 0.5 + 0.5 * (F1 - d) / smoothness);
            F1 = lerp(F1, d, h) - smoothness * h * (1.0 - h);
            mg = g;
            mr = r;
            id = o;
        }
    }
    return F1;
}

inline float4 ComputeGrabScreenPosition(float4 pos)
{
    #if UNITY_UV_STARTS_AT_TOP
    float scale = -1.0;
    #else
    float scale = 1.0;
    #endif
    float4 o = pos;
    o.y = pos.w * 0.5f;
    o.y = (pos.y - o.y) * _ProjectionParams.x * scale + o.y;
    return o;
}