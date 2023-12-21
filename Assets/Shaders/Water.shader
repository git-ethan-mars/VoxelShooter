Shader "Lit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseVisibility ("NoiseVisibility", Range(0.0, 1.0)) = 0
        _WireframeAliasing("Wireframe aliasing", float) = 1.5
        _EdgeMultiplier("Edge multiplier", Range(0.0, 5.0)) = 1.05
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
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #pragma target 3.5
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_fog

            // We add our barycentric variables to the geometry struct.
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
                float4 worldPos : TEXCOORD0;
                float2 uv : TEXTCOORD1;
                fixed3 diff : COLOR1;
                fixed3 ambient : COLOR2;
                SHADOW_COORDS(2)
                UNITY_FOG_COORDS(3)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _NoiseVisibility;
            float _WireframeAliasing;
            float _EdgeMultiplier;

            v2f vert(float4 vertex : POSITION, uint vid : SV_VertexID, float3 normal : NORMAL, float4 color : Color)
            {
                v2f o;
                const half3 worldNormal = UnityObjectToWorldNormal(normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.pos = UnityObjectToClipPos(vertex);
                o.color = color;
                o.worldPos = mul(unity_ObjectToWorld, vertex);
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal, 1));
                float2 uvs[4];
                uvs[0] = float2(0.0, 0.0);
                uvs[1] = float2(0.0, 1.0);
                uvs[2] = float2(1.0, 0.0);
                uvs[3] = float2(1.0, 1.0);
                o.uv = uvs[round(vid % 4)];
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = lerp(i.color,(1 - tex2D(_MainTex,frac(i.worldPos.xz))) * i.color, _NoiseVisibility);
                const fixed shadow = SHADOW_ATTENUATION(i);
                const fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    Fallback "Diffuse"
}