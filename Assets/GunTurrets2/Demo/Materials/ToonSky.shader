Shader "CIWS/Toon Sky"
{
    Properties
    {
        // The horizon matches the scene fog colour so the ocean melts into the sky.
        _HorizonColor("Horizon", Color) = (0.76, 0.88, 0.94, 1)
        _LowColor("Low Sky", Color) = (0.56, 0.8, 0.95, 1)
        _ZenithColor("Zenith", Color) = (0.27, 0.56, 0.9, 1)
        _SunColor("Sun", Color) = (1, 0.96, 0.86, 1)
        _SunSize("Sun Disc Size", Range(0.99, 0.9999)) = 0.9985
        _SunGlow("Sun Glow", Range(0, 1)) = 0.35
        _BandSoftness("Band Softness", Range(0.005, 0.2)) = 0.06
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+500" "RenderType" = "Opaque" }
        Cull Off
        ZWrite Off

        Pass
        {
            // ForwardBase so Unity provides the main light direction for the sun.
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _HorizonColor, _LowColor, _ZenithColor, _SunColor;
            float _SunSize, _SunGlow, _BandSoftness;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            v2f vert(float4 vertex : POSITION)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                o.viewDir = mul(unity_ObjectToWorld, vertex).xyz - _WorldSpaceCameraPos;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.viewDir);
                float height = saturate(dir.y);
                // Two soft toon bands: haze → low sky → zenith.
                float low = smoothstep(0.02, 0.02 + _BandSoftness * 2, height);
                float high = smoothstep(0.18, 0.18 + _BandSoftness * 6, height);
                float3 color = lerp(_HorizonColor.rgb, _LowColor.rgb, low);
                color = lerp(color, _ZenithColor.rgb, high);

                float3 sunDir = normalize(_WorldSpaceLightPos0.xyz);
                float sunDot = dot(dir, sunDir);
                float glow = pow(saturate(sunDot), 24) * _SunGlow;
                float disc = smoothstep(_SunSize, _SunSize + 0.0004, sunDot);
                color = lerp(color, _SunColor.rgb, saturate(glow + disc));
                return float4(color, 1);
            }
            ENDCG
        }
    }
}
