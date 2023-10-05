Shader "Lit/Chunk new"
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
            #pragma geometry geom
            #pragma fragment frag
            #include "Lighting.cginc"
            // shadow helper functions and macros
            #include "AutoLight.cginc"
            #pragma target 3.0
            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_fog

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
                fixed3 diff : COLOR1;
                fixed3 ambient : COLOR2;
                float2 uv : TEXCOORD0;
            };

            // We add our barycentric variables to the geometry struct.
            struct g2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
                fixed3 diff : COLOR1;
                fixed3 ambient : COLOR2;
                float2 uv : TEXCOORD0;
                float3 barycentric : TEXCOORD1;
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
                float2 uvs[4];
                uvs[0] = float2(0.0, 0.0);
                uvs[1] = float2(0.0, 1.0);
                uvs[2] = float2(1.0, 0.0);
                uvs[3] = float2(1.0, 1.0);
                const half3 worldNormal = UnityObjectToWorldNormal(normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.pos = vertex;
                o.color = color;
                o.uv = uvs[round(vid % 4)];
                o.diff = nl * _LightColor0.rgb;
                o.ambient = ShadeSH9(half4(worldNormal, 1));
                // compute shadows data
                return o;
            }

            // This applies the barycentric coordinates to each vertex in a triangle.

            [maxvertexcount(3)]
            void geom(triangle v2f IN[3], inout TriangleStream<g2f> triStream)
            {
                float edgeLengthX = length(IN[1].pos - IN[2].pos);
                float edgeLengthY = length(IN[0].pos - IN[2].pos);
                float edgeLengthZ = length(IN[0].pos - IN[1].pos);
                float3 modifier = float3(0.0, 0.0, 0.0);
                // We're fine using if statments it's a trivial function.
                if ((edgeLengthX > edgeLengthY) && (edgeLengthX > edgeLengthZ))
                {
                    modifier = float3(1.0, 0.0, 0.0);
                }
                else if ((edgeLengthY > edgeLengthX) && (edgeLengthY > edgeLengthZ))
                {
                    modifier = float3(0.0, 1.0, 0.0);
                }
                else if ((edgeLengthZ > edgeLengthX) && (edgeLengthZ > edgeLengthY))
                {
                    modifier = float3(0.0, 0.0, 1.0);
                }

                g2f o;
                o.pos = UnityObjectToClipPos(IN[0].pos);
                o.barycentric = float3(1.0, 0.0, 0.0) + modifier;
                o.color = IN[0].color;
                o.diff = IN[0].diff;
                o.ambient = IN[0].ambient;
                o.uv = IN[0].uv;
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);
                triStream.Append(o);
                o.pos = UnityObjectToClipPos(IN[1].pos);
                o.barycentric = float3(0.0, 1.0, 0.0) + modifier;
                o.color = IN[1].color;
                o.diff = IN[1].diff;
                o.ambient = IN[1].ambient;
                o.uv = IN[1].uv;
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);
                triStream.Append(o);
                o.pos = UnityObjectToClipPos(IN[2].pos);
                o.barycentric = float3(0.0, 0.0, 1.0) + modifier;
                o.color = IN[2].color;
                o.diff = IN[2].diff;
                o.ambient = IN[2].ambient;
                o.uv = IN[2].uv;
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);
                triStream.Append(o);
            }

            fixed4 frag(g2f i) : SV_Target
            {
                // Calculate the unit width based on triangle size.
                const float3 unitWidth = fwidth(i.barycentric);
                // Alias the line a bit.
                float3 aliased = smoothstep(float3(0.0, 0.0, 0.0), unitWidth * _WireframeAliasing, i.barycentric);
                // Use the coordinate closest to the edge.
                float alpha = 1 - min(aliased.x, min(aliased.y, aliased.z));
                // Set to our forwards facing wireframe colour.
                fixed4 col = lerp(i.color, (1 - tex2D(_MainTex, i.uv)) * i.color, _NoiseVisibility);
                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                // darken light's illumination with shadow, keep ambient intact
                const fixed shadow = SHADOW_ATTENUATION(i);
                const fixed3 lighting = i.diff * shadow + i.ambient;
                col.rgb *= lighting;
                UNITY_APPLY_FOG(i.fogCoord, col);
                if (alpha != 0)
                {
                    return float4(col.rgb * _EdgeMultiplier, alpha);
                }

                return col;
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}