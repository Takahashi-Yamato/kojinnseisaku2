// 半透明用シェーダー。透明ブロック（枠が光る）と影に使う。
Shader "CubeOrbit/Ghost"
{
    Properties
    {
        _Color ("Color", Color) = (0.45, 1, 0.75, 1)
        _Alpha ("Fill Alpha", Range(0, 1)) = 0.08
        _EdgeAlpha ("Edge Alpha", Range(0, 1)) = 0.85
        _EdgeWidth ("Edge Width", Range(0.001, 0.25)) = 0.035
        _Pulse ("Pulse", Range(0, 1)) = 0.25
        [Toggle] _ObjectSpace ("Edges In Object Space", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Alpha, _EdgeAlpha, _EdgeWidth, _Pulse, _ObjectSpace;

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
                float e = _ObjectSpace > 0.5 ? EdgeDist(i.opos, i.on) : EdgeDist(i.wpos, normalize(i.wn));
                float edge = 1.0 - smoothstep(0.0, _EdgeWidth, e);
                float pulse = 1.0 + _Pulse * sin(_Time.y * 3.0);
                float a = lerp(_Alpha, _EdgeAlpha, edge) * pulse * _Color.a;
                return fixed4(_Color.rgb, saturate(a));
            }
            ENDCG
        }
    }
    FallBack Off
}
