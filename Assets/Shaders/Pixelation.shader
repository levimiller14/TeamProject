Shader "Custom/Pixelation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "Pixelation"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _PixelSize;
            float4 _SourceSize; // xy = 1/width, 1/height, zw = width, height

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // calculate pixel grid
                float2 pixelCount = _SourceSize.zw / _PixelSize;

                // snap uv to pixel grid
                float2 pixelatedUV = floor(uv * pixelCount) / pixelCount;

                // offset to center of pixel
                pixelatedUV += 0.5 / pixelCount;

                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, pixelatedUV);
            }
            ENDHLSL
        }
    }
}
