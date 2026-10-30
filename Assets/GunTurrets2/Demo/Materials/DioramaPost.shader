Shader "Hidden/CIWS/Diorama Post"
{
    Properties
    {
        _MainTex("Source", 2D) = "white" {}
    }
    CGINCLUDE
    #include "UnityCG.cginc"
    sampler2D _MainTex, _BlurTex;
    float4 _MainTex_TexelSize;
    float2 _BlurDirection;
    float _FocusCenter, _FocusWidth, _FocusFalloff, _TiltStrength;
    float _Saturation, _Contrast, _Vignette;
    float4 _Lift, _Gain, _VignetteColor;

    // Nine-tap gaussian along one axis; run twice (horizontal, vertical) at half resolution.
    float4 blur(v2f_img i) : SV_Target
    {
        float2 step = _MainTex_TexelSize.xy * _BlurDirection;
        float4 c = tex2D(_MainTex, i.uv) * 0.227;
        c += (tex2D(_MainTex, i.uv + step * 1.385) + tex2D(_MainTex, i.uv - step * 1.385)) * 0.316;
        c += (tex2D(_MainTex, i.uv + step * 3.231) + tex2D(_MainTex, i.uv - step * 3.231)) * 0.070;
        return c;
    }

    float4 composite(v2f_img i) : SV_Target
    {
        float3 sharp = tex2D(_MainTex, i.uv).rgb;
        float3 soft = tex2D(_BlurTex, i.uv).rgb;
        // Tilt-shift: a sharp band through the middle, melting into blur toward the top and bottom.
        float tilt = smoothstep(_FocusWidth, _FocusWidth + _FocusFalloff, abs(i.uv.y - _FocusCenter));
        float3 c = lerp(sharp, soft, tilt * _TiltStrength);

        // Gentle grade: lifted cool shadows, warm highlights, slightly lower saturation and contrast.
        c = c * _Gain.rgb + _Lift.rgb * (1 - c);
        float luma = dot(c, float3(0.2126, 0.7152, 0.0722));
        c = lerp(luma.xxx, c, _Saturation);
        c = (c - 0.5) * _Contrast + 0.5;

        float2 d = (i.uv - 0.5) * float2(1.1, 1);
        float vignette = smoothstep(0.35, 0.85, length(d)) * _Vignette;
        c = lerp(c, c * _VignetteColor.rgb, vignette);
        return float4(saturate(c), 1);
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment blur
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment composite
            ENDCG
        }
    }
    Fallback Off
}
