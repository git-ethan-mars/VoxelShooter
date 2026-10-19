// Example Shader for Universal RP
// Written by @Cyanilux
// https://www.cyanilux.com/tutorials/urp-shader-code

/*
Roughly equivalent to the URP/SimpleLit.shader (but Forward path only)
https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Shaders/SimpleLit.shader
*/

Shader "Cyanilux/URPTemplates/SimpleLitShaderExample" {
	Properties
    {
        _EdgeDetection ("Edge detection", Range(0, 1)) = 0.95
        _EdgeColorLightness ("Edge lightness", Range(0, 1)) = 0.015
        _AmbientFloor ("Ambient floor", Color) = (0.6, 0.6, 0.6, 1)
        _AmbientStrength ("Ambient probe strength", Range(0, 2)) = 1
        _LightWrap ("Light wrap", Range(0, 1)) = 0.3
        _AOStrength ("AO strength", Range(0, 1)) = 0.7
        _DrawDistanceFade ("Draw distance fade", Range(0, 64)) = 16
    }
	SubShader {
		Tags {
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		ENDHLSL

		Pass {
			Name "ForwardLit"
			Tags { "LightMode"="UniversalForward" }

			HLSLPROGRAM
			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment

			// Material Keywords
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local_fragment _EMISSION
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			//#pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
			//#pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
			#pragma shader_feature_local_fragment _ _SPECGLOSSMAP
			#define _SPECULAR_COLOR // always on
			#pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

			// URP Keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			// Note, v11 changes this to :
			// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING // v10+ only, renamed from "_MIXED_LIGHTING_SUBTRACTIVE"
			#pragma multi_compile _ SHADOWS_SHADOWMASK // v10+ only

			// Unity Keywords
			#pragma multi_compile_fog

			// GPU Instancing (not supported)
			//#pragma multi_compile_instancing

			// Includes
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

			// Structs
			struct Attributes {
				float4 positionOS	: POSITION;
				float4 normalOS		: NORMAL;
				float4 color		: COLOR;
				float2 uv		    : TEXCOORD0;
				float uv1	        : TEXCOORD1;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings {
				float4 positionCS 					: SV_POSITION;
				float2 uv		    				: TEXCOORD0;
				uint   neighbours                   : TEXCOORD1;
                float  ao                           : TEXCOORD2;
				float3 positionWS					: TEXCOORD3;
				half3 normalWS					    : TEXCOORD4;
				
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					half4 fogFactorAndVertexLight	: TEXCOORD5; // x: fogFactor, yzw: vertex light
				#else
					half  fogFactor					: TEXCOORD5;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					float4 shadowCoord 				: TEXCOORD6;
				#endif

				float4 color						: COLOR;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float _EdgeDetection;
			float _EdgeColorLightness;
			half4 _AmbientFloor;
			half _AmbientStrength;
			half _LightWrap;
			half _AOStrength;
			float _DrawDistanceFade;
			CBUFFER_END

			float DitherNoise(float2 pixel)
			{
				return frac(52.9829189 * frac(dot(pixel, float2(0.06711056, 0.00583715))));
			}

			// The camera far plane is flat, so cut chunks by radial distance instead to get a circular draw distance
			void ClipByDrawDistance(float distanceToCamera, float2 pixel)
			{
				float drawDistance = _ProjectionParams.z;
				float fade = saturate((drawDistance - distanceToCamera) / max(_DrawDistanceFade, 0.001));
				clip(fade - DitherNoise(pixel));
			}

			float GetAmbientOcclusion(float alpha)
            {
                int    t = clamp(alpha * 255, 0.0, 3.0);
                float4 aoValues = float4(0.1, 0.6, 0.8, 1);
                return lerp(1.0, aoValues[t], _AOStrength);
            }

            float maxcomp(float4 v)
            {
                return max(v.x, max(v.y, max(v.z, v.w)));
            }

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

			//  SurfaceData & InputData
			void InitalizeSurfaceData(Varyings IN, out SurfaceData surfaceData){
				surfaceData = (SurfaceData)0; // avoids "not completely initalized" errors
				surfaceData.albedo = IN.color.rgb;
				surfaceData.occlusion = 1; // unused
			}

			void InitializeInputData(Varyings input, out InputData inputData) {
				inputData = (InputData)0; // avoids "not completely initalized" errors

				inputData.positionWS = input.positionWS;
				half3 viewDirWS = GetWorldSpaceNormalizeViewDir(inputData.positionWS);
				inputData.normalWS = input.normalWS;
				
				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);

				viewDirWS = SafeNormalize(viewDirWS);
				inputData.viewDirectionWS = viewDirWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					inputData.shadowCoord = input.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif

				// Fog
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.fogCoord = input.fogFactorAndVertexLight.x;
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#else
					inputData.fogCoord = input.fogFactor;
					inputData.vertexLighting = half3(0, 0, 0);
				#endif

				inputData.bakedGI = EvaluateAmbientProbeSRGB(inputData.normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
			}

			// Vertex Shader
			Varyings LitPassVertex(Attributes IN) {
				Varyings OUT;

				//UNITY_SETUP_INSTANCE_ID(IN);
				//UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
				VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);

				OUT.positionCS = positionInputs.positionCS;
				OUT.positionWS = positionInputs.positionWS;

				half fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
				OUT.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);

				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					OUT.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				#else
					OUT.fogFactor = fogFactor;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					OUT.shadowCoord = GetShadowCoord(positionInputs);
				#endif

				OUT.uv = IN.uv;
				OUT.neighbours = asuint(IN.uv1);
				OUT.ao = GetAmbientOcclusion(IN.color.a);
				OUT.color = SRGBToLinear(IN.color);
				return OUT;
			}

			// Fragment Shader
			half4 LitPassFragment(Varyings IN) : SV_Target {
				float distanceToCamera = distance(IN.positionWS, _WorldSpaceCameraPos);
				ClipByDrawDistance(distanceToCamera, IN.positionCS.xy);

				float3 hsv = RgbToHsv(IN.color.rgb);
                float4 edgeFactor = EdgeDetection(IN.uv, IN.neighbours);
                float4 edgeColor = float4(HsvToRgb(float3(hsv.rg, saturate(hsv.b + _EdgeColorLightness))), 1);
                IN.color = lerp(IN.color, edgeColor, edgeFactor);
				// Setup SurfaceData
				SurfaceData surfaceData;
				InitalizeSurfaceData(IN, surfaceData);
				// Setup InputData
				InputData inputData;
				InitializeInputData(IN, inputData);
				half4 shadowMask = CalculateShadowMask(inputData);
				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);

				// Wrapped Lambert: faces turned away from the sun fade out gradually instead of dropping to zero
				half NdotL = dot(inputData.normalWS, mainLight.direction);
				half diffuse = saturate((NdotL + _LightWrap) / (1.0 + _LightWrap));
				half sunAmount = diffuse * mainLight.shadowAttenuation * mainLight.distanceAttenuation;

				// Ambient never goes below the floor, so back faces stay readable even with a dark skybox probe
				half3 ambient = max(inputData.bakedGI * _AmbientStrength, _AmbientFloor.rgb);

				// Map colors are authored for unlit rendering, so a fully sunlit face keeps its original color
				// and faces turned away from the sun or in shadow fade down to the ambient level
				half3 lighting = lerp(ambient, mainLight.color, sunAmount);
				half3 lit = surfaceData.albedo * lighting * IN.ao;
				// Radial fog, so it matches the circular draw distance instead of the flat view depth
				return half4(MixFog(lit, ComputeFogFactorZ0ToFar(distanceToCamera)), 1);
			}
			ENDHLSL
		}

		// ShadowCaster, for casting shadows
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			// GPU Instancing
			#pragma multi_compile_instancing
			//#pragma multi_compile _ DOTS_INSTANCING_ON

			// Universal Pipeline Keywords
			// (v11+) This is used during shadow map generation to differentiate between directional and punctual (point/spot) light shadows, as they use different formulas to apply Normal Bias
			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
			ENDHLSL
		}
	}
}