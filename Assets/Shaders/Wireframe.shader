Shader "Custom/Geometry/Wireframe"
{
    Properties
    {
        _WireframeAliasing ("Wireframe aliasing", Range(0., 0.5)) = 0.05
        _Color ("Color", color) = (1., 1., 1., 1.)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Opaque"
        }
        Pass
        {
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			#include "UnityCG.cginc"

            struct v2g
            {
                float4 worldPos : SV_POSITION;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            v2g vert(appdata_base v)
            {
                v2g o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                float3 param = float3(0., 0., 0.);
                float  EdgeA = length(IN[0].worldPos - IN[1].worldPos);
                float  EdgeB = length(IN[1].worldPos - IN[2].worldPos);
                float  EdgeC = length(IN[2].worldPos - IN[0].worldPos);

                if (EdgeA > EdgeB && EdgeA > EdgeC) param.y = 1.;
                else if (EdgeB > EdgeC && EdgeB > EdgeA) param.x = 1.;
                else param.z = 1.;

                g2f o;
                o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
                o.bary = float3(1., 0., 0.) + param;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
                o.bary = float3(0., 0., 1.) + param;
                triStream.Append(o);
                o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
                o.bary = float3(0., 1., 0.) + param;
                triStream.Append(o);
            }

            float _WireframeAliasing;
            fixed4 _Color;

            fixed4 frag(g2f i) : SV_Target
            {
                float3 aliased = smoothstep(float3(0,0,0), _WireframeAliasing, i.bary);
                float alpha = 1 - min(aliased.x, min(aliased.y, aliased.z));
                return float4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}