Shader "Vertigo/UI/UIParticleDisplay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Render Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlackCutoff ("Black Cutoff", Range(0, 0.25)) = 0.015
        _AlphaSoftness ("Alpha Softness", Range(0.001, 0.5)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _BlackCutoff;
            float _AlphaSoftness;
            float4 _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.texcoord);
                float luminanceAlpha = saturate((max(tex.r, max(tex.g, tex.b)) - _BlackCutoff) / _AlphaSoftness);
                tex.a = max(tex.a, luminanceAlpha) * luminanceAlpha;
                tex *= i.color;
                return tex;
            }
            ENDCG
        }
    }
}
