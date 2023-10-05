Shader "Lit/Chunk new"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Multiplier ("Multiplier", Range(0.0, 1.0)) = 0
    }

    SubShader
    {
        Cull Back
        Pass
        {
            Tags
            {
                "LightMode"="ForwardBase"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma target 3.0

            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
                fixed3 diff : COLOR1;
                fixed3 ambient : COLOR2;
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Multiplier;

            v2f vert(float4 vertex : POSITION, uint vid : SV_VertexID, float3 normal : NORMAL, float4 color : Color)
            {
                v2f o;
                float2 uvs[4];
                uvs[0] = float2(0.0, 0.0);
                uvs[1] = float2(0.0, 1.0);
                uvs[2] = float2(1.0, 0.0);
                uvs[3] = float2(1.0, 1.0);
                const half3 worldNormal = UnityObjectToWorldNormal(normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.pos = UnityObjectToClipPos(vertex);
                o.color = color;
                o.uv = uvs[round(vid % 4)];
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal, 1));
                // compute shadows data
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = lerp(i.color, (1 - tex2D(_MainTex, i.uv)) * i.color, _Multiplier);
                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting;
                return col;
            }
            ENDCG
        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}