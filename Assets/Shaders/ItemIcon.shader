// Inventory slot icons: lit by a fixed light in view space, so the scene sun, shadows and camera pitch don't affect them
Shader "VoxelShooter/ItemIcon"
{
	Properties
	{
		[MainTexture] _BaseMap ("Texture", 2D) = "white" {}
		[MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)
		_LightDirection ("Light direction (view space)", Vector) = (-0.4, 0.6, 0.7, 0)
		_Ambient ("Ambient", Range(0, 1)) = 0.55
	}
	SubShader
	{
		Tags
		{
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Opaque"
			"Queue"="Geometry"
		}

		Pass
		{
			Name "ItemIcon"
			Tags { "LightMode"="UniversalForward" }

			HLSLPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			half4 _BaseColor;
			float4 _LightDirection;
			half _Ambient;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normalVS : TEXCOORD1;
			};

			Varyings Vertex(Attributes input)
			{
				Varyings output;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
				output.normalVS = TransformWorldToViewDir(TransformObjectToWorldNormal(input.normalOS));
				return output;
			}

			half4 Fragment(Varyings input) : SV_Target
			{
				half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
				half diffuse = saturate(dot(normalize(input.normalVS), normalize(_LightDirection.xyz)));
				return half4(albedo.rgb * lerp(_Ambient, 1.0, diffuse), 1.0);
			}
			ENDHLSL
		}
	}
}
