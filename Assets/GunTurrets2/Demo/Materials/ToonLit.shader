Shader "CIWS/Toon Ship"
{
    Properties
    {
        _Color ("Color", Color) = (0.75,0.78,0.8,1)
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowTint ("Cool Shadows", Color) = (0.55,0.63,0.82,1)
        _SunTint ("Sun", Color) = (1,1,1,1)
        _Tone ("Hull Tone", Range(0.5,1.2)) = 0.88
        _HullRed ("Underside Paint", Color) = (0.62,0.13,0.1,1)
        _BootTop ("Waterline Stripe", Color) = (0.13,0.14,0.16,1)
        _WaterlineY ("Paint Line Height (ship space)", Float) = 0.15
        _StripeWidth ("Stripe Width", Float) = 0.12
        _Facets ("Flat Facets", Range(0,1)) = 0
        _RimStrength ("Edge Light", Range(0,0.3)) = 0.04
        _ShipDetails ("Deck and Bridge Details", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST, _Color, _ShadowTint, _SunTint, _HullRed, _BootTop;
            float _WaterlineY, _StripeWidth;
            float _RimStrength, _ShipDetails, _Facets, _Tone;
            float4x4 _CIWSWorldToShip;
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
                SHADOW_COORDS(3)
                UNITY_FOG_COORDS(4)
            };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float band(float x, float lo, float hi, float aa)
            {
                return smoothstep(lo-aa,lo+aa,x)*(1-smoothstep(hi-aa,hi+aa,x));
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float3 smoothN = normalize(i.normal);
                // Optional per-face normals; off by default because screen derivatives speckle on dense geometry.
                float3 flatN = normalize(cross(ddy(i.worldPos), ddx(i.worldPos)));
                flatN *= sign(dot(flatN, smoothN) + 1e-5);
                float3 n = normalize(lerp(smoothN, flatN, _Facets));
                float3 p = mul(_CIWSWorldToShip,float4(i.worldPos,1)).xyz;
                float3 shipNormal = normalize(mul((float3x3)_CIWSWorldToShip,n));
                float3 albedo = tex2D(_MainTex,i.uv).rgb * _Color.rgb * _Tone;
                // A darker lower hull gives the waterline a clear silhouette.
                albedo *= lerp(1,lerp(0.84,1,smoothstep(0.1,1.8,p.y)),_ShipDetails);
                float aa = max(fwidth(p.y),0.015);
                // Red anti-fouling paint below the line, with a dark boot-top stripe just above it.
                float belowLine = 1 - smoothstep(_WaterlineY - aa, _WaterlineY + aa, p.y);
                float stripe = band(p.y, _WaterlineY, _WaterlineY + _StripeWidth, aa);
                albedo = lerp(albedo, _BootTop.rgb, stripe * _ShipDetails);
                albedo = lerp(albedo, _HullRed.rgb, belowLine * _ShipDetails);
                // Markings follow the existing hull through banking and rocking.
                float top = smoothstep(0.8,0.95,shipNormal.y);
                float deck = band(p.y,0.63,0.79,aa)*band(p.z,-16,-8,0.08)*top;
                float edge = band(abs(p.x+0.36),2.35,2.43,max(fwidth(p.x),0.01));
                float dash = step(0.4,frac(p.z*0.75));
                albedo = lerp(albedo,float3(0.81,0.86,0.85),deck*edge*dash*_ShipDetails);
                float bridge = band(p.y,5.18,5.65,aa)*band(p.z,13.3,16.2,0.06);
                float panes = smoothstep(0.08,0.14,frac((p.x+p.z)*1.9));
                float windows = bridge*(1-top)*panes*_ShipDetails;
                albedo = lerp(albedo,float3(0.075,0.19,0.24),windows*0.82);
                float ndl = dot(n,normalize(_WorldSpaceLightPos0.xyz));
                float shadow = SHADOW_ATTENUATION(i);
                // Three crisp cartoon bands: cool shade, mid tone and full sun.
                float inLight = lerp(0.05,1,shadow);
                float mid = smoothstep(-0.02,0.03,ndl) * inLight;
                float full = smoothstep(0.42,0.47,ndl) * inLight;
                float3 sun = _SunTint.rgb * _LightColor0.rgb;
                float3 shade = _ShadowTint.rgb * 0.42 + max(ShadeSH9(float4(n,1)),0) * 0.08;
                float3 lighting = lerp(lerp(shade, sun * 0.66, mid), sun, full);
                // Faces turned to the sky catch a little extra light.
                lighting += 0.05 * saturate(n.y);
                float rim = pow(1-saturate(dot(n,normalize(_WorldSpaceCameraPos-i.worldPos))),3);
                fixed4 col = fixed4(albedo*lighting + rim*_RimStrength*float3(0.85,0.93,1),1);
                UNITY_APPLY_FOG(i.fogCoord,col);
                return col;
            }
            ENDCG
        }
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            struct shadowV2f { V2F_SHADOW_CASTER; };
            shadowV2f shadowVert(appdata_base v)
            {
                shadowV2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                return o;
            }
            float4 shadowFrag(shadowV2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    Fallback Off
}
