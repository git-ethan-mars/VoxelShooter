Shader "Custom/NewUnlitUniversalRenderPipelineShader"
{
    Properties
    {
        _AmbientLighting ("Ambient lighting", Color) = (0.2, 0.2, 0.2, 1)
        _EdgeDetection ("Edge detection", Range(0, 1)) = 0.95
        _EdgeColorLightness ("Edge lightness", Range(0, 1)) = 0.015
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOW_SOFT

            #define _MAIN_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct vertexInput
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float  uv1 : TEXCOORD1;
            };

            struct fragmentInput
            {
                float4 positionCS : SV_POSITION;
                float3 color : COLOR;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                uint   neighbours : TEXCOORD2;
                float  ao : TEXCOORD3;
                float4 shadowCoords : TEXCOORD4;
            };

            float GetAmbientOcclusion(float alpha)
            {
                int t = clamp(alpha * 255, 0.0, 3.0);

                if (t == 0)
                {
                    return 0.1;
                }
                if (t == 1)
                {
                    return 0.6;
                }
                if (t == 2)
                {
                    return 0.8;
                }

                return 1;
            }

            float maxcomp(float4 v)
            {
                return max(v.x, max(v.y, max(v.z, v.w)));
            }

            float _EdgeDetection;

            float EdgeDetection(float2 uv0, uint neighbours)
            {
                float4 va = float4(neighbours >> 0 & 1, neighbours >> 1 & 1, neighbours >> 2 & 1, neighbours >> 3 & 1);
                float4 vb = float4(neighbours >> 4 & 1, neighbours >> 5 & 1, neighbours >> 6 & 1, neighbours >> 7 & 1);
                float4 vc = float4(neighbours >> 8 & 1, neighbours >> 9 & 1, neighbours >> 10 & 1, neighbours >> 11 & 1);
                float4 vd = float4(neighbours >> 12 & 1, neighbours >> 13 & 1, neighbours >> 14 & 1, neighbours >> 15 & 1);

                float2 st = 1.0 - uv0;
                // sides    
                float4 wb = smoothstep(_EdgeDetection, 1, float4(uv0.x, st.x, uv0.y, st.y)) * (1.0 - va + va * vc);
                // corners
                float4 wc = smoothstep(_EdgeDetection, 1, float4(uv0.x * uv0.y, st.x * uv0.y, st.x * st.y, uv0.x * st.y)) * (1.0 - vb + vd * vb);
                return maxcomp(max(wb, wc));
            }

            float3 RGBToLinear(float3 color)
            {
                float3 linearRGBLo = color / 12.92;;
                float3 linearRGBHi = pow(max(abs((color + 0.055) / 1.055), 1.192092896e-07), float3(2.4, 2.4, 2.4));
                return float3(color <= 0.04045) ? linearRGBLo : linearRGBHi;
            }

            fragmentInput vert(vertexInput IN)
            {
                fragmentInput OUT;
                float3        positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.color = IN.color.xyz;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.neighbours = asuint(IN.uv1);
                OUT.ao = GetAmbientOcclusion(IN.color.a);
                OUT.shadowCoords = TransformWorldToShadowCoord(positionWS);
                return OUT;
            }

            float4 _AmbientLighting;
            float _EdgeColorLightness;

            float4 frag(fragmentInput IN) : SV_Target
            {
                float4 color = float4(RGBToLinear(IN.color), 1);
                
                Light  mainLight = GetMainLight(IN.shadowCoords);
                float3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
                float3 lightingColor = LightingLambert(attenuatedLightColor, mainLight.direction, IN.normalWS);
                lightingColor = saturate(lightingColor + _AmbientLighting);
                color.rgb *= lightingColor;

                float4 edge = EdgeDetection(IN.uv, IN.neighbours);
                float3 hsv = RgbToHsv(color.rgb);
                float4 edgeColor = float4(HsvToRgb(float3(hsv.rg, saturate(hsv.b + _EdgeColorLightness))), 1);

                return (color * (1 - edge) + edgeColor * edge) * IN.ao;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode"="ShadowCaster"
            }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x gles
            //#pragma target 4.5

            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}