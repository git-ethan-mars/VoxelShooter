Shader "Custom/DepthOnly"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "LightMode"="DepthOnly"}
        Pass {
            ZWrite On
            ColorMask 0
        }
    }
    FallBack "Diffuse"
}