// ブロック用シェーダー。ライト不要で、マスの境目を少し暗くし、下に行くほど闇に溶ける。
// Built-in / URP のどちらでも動く（ライティングはシェーダー内で計算）。
Shader "CubeOrbit/Block"
{
    Properties
    {
        _Color ("Color", Color) = (0.55, 0.57, 0.65, 1)
        _TopBrightness ("Top Brightness", Range(0.8, 1.3)) = 1.06
        _Emission ("Emission (ignore shading)", Range(0, 1)) = 0
        _EdgeDarken ("Edge Darken", Range(0, 0.6)) = 0.07
        _EdgeWidth ("Edge Width", Range(0.001, 0.25)) = 0.07
        [Toggle] _ObjectSpace ("Edges In Object Space (for moving objects)", Float) = 0
        _FogY ("Fog Bottom Y", Float) = -3.5
        _FogRange ("Fog Range", Float) = 4.2
        _LightDir ("Light Direction", Vector) = (0.35, 1, 0.62, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _TopBrightness, _Emission, _EdgeDarken, _EdgeWidth, _ObjectSpace, _FogY, _FogRange;
            float4 _LightDir;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 wpos : TEXCOORD0;
                float3 wn : TEXCOORD1;
                float3 opos : TEXCOORD2;
                float3 on : TEXCOORD3;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.wn = UnityObjectToWorldNormal(v.normal);
                o.opos = v.vertex.xyz;
                o.on = v.normal;
                return o;
            }

            // 面の上でマスの境目までの距離（マスの中心が整数座標）
            float EdgeDist (float3 p, float3 n)
            {
                float3 an = abs(n);
                float2 uv = an.x > 0.5 ? p.zy : (an.y > 0.5 ? p.xz : p.xy);
                float2 f = frac(uv + 0.5);
                float2 d = min(f, 1.0 - f);
                return min(d.x, d.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 n = normalize(i.wn);
                float l = saturate(dot(n, normalize(_LightDir.xyz)));
                float sh = lerp(0.42 + 0.58 * l, 0.78 + 0.22 * l, _Emission);
                if (n.y > 0.5) sh *= _TopBrightness;
                float e = _ObjectSpace > 0.5 ? EdgeDist(i.opos, i.on) : EdgeDist(i.wpos, n);
                float ed = lerp(1.0 - _EdgeDarken, 1.0, smoothstep(0.0, _EdgeWidth, e));
                float fg = saturate((i.wpos.y - _FogY) / max(0.01, _FogRange));
                fg = fg * fg * (3.0 - 2.0 * fg);
                return fixed4(_Color.rgb * sh * ed * fg, 1);
            }
            ENDCG
        }
    }
    FallBack Off
}
