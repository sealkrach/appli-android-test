// Shader toon simple pour le pipeline intégré (built-in) : trois paliers d'ombrage
// et un contour par coque inversée. Suffisant pour la première ouverture du
// projet ; à remplacer par un Shader Graph URP quand le pipeline sera basculé.
Shader "PolitiRush/Toon"
{
    Properties
    {
        _Color ("Couleur", Color) = (1,1,1,1)
        _OutlineColor ("Contour", Color) = (0.09,0.07,0.17,1)
        _Outline ("Épaisseur du contour", Range(0, 0.1)) = 0.035
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        // Passe 1 : coque inversée = contour.
        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float _Outline; fixed4 _OutlineColor;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            float4 vert(appdata v) : SV_POSITION
            {
                float3 n = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float4 pos = UnityObjectToClipPos(v.vertex);
                float2 off = TransformViewToProjection(n.xy);
                pos.xy += off * pos.z * _Outline;
                return pos;
            }
            fixed4 frag() : SV_Target { return _OutlineColor; }
            ENDCG
        }
        // Passe 2 : ombrage à paliers.
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            fixed4 _Color;
            struct v2f { float4 pos : SV_POSITION; float3 n : TEXCOORD0; SHADOW_COORDS(1) };
            v2f vert(appdata_base v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.n = UnityObjectToWorldNormal(v.normal);
                TRANSFER_SHADOW(o); return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float ndl = saturate(dot(normalize(i.n), _WorldSpaceLightPos0.xyz)) * SHADOW_ATTENUATION(i);
                float band = ndl > 0.6 ? 1.0 : ndl > 0.25 ? 0.72 : 0.5;
                fixed3 col = _Color.rgb * (band * _LightColor0.rgb + UNITY_LIGHTMODEL_AMBIENT.rgb * 0.8);
                return fixed4(col, 1);
            }
            ENDCG
        }
        // Ombres portées.
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    FallBack "Diffuse"
}
