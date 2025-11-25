Shader "Unlit/PixelationEffect"
{
    Properties
    {
        _PixelationColor ("Pixelation Color", Color) = (1,1,1,1)
        _Intensity ("Tint Intensity", Range(0,1)) = 0.0
        _PixelSize ("Pixel Size (Screen Pixels)", Range(1,512)) = 8
        _PosterizationLevels ("Posterization Levels", Range(2,1024)) = 512
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            SAMPLER(sampler_BlitTexture);

            float4 _PixelationColor;
            float  _Intensity;
            float  _PixelSize;
            float _PosterizationLevels;
            float4x4 _DitherMatrix;

            float2 QuantizeUV(float2 uv)
            {
                float currentPixelSize = lerp(1.0, _PixelSize, saturate(_Intensity));

                float2 screenSize = _ScreenParams.xy;
                float2 blockCount = max(1.0, screenSize / currentPixelSize);
                return floor(uv * blockCount) / blockCount;
            }

            half4 Frag (Varyings i) : SV_Target
            {
                // ピクセル化
                float2 qUV = QuantizeUV(i.texcoord);
                half4 baseColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, qUV);

                half3 tinted = baseColor.rgb;

                // ポスタリゼーション
                half3 quantizedColor = round(tinted * _PosterizationLevels) / _PosterizationLevels;
                
                // ディザー
                const float dither[64] = {
                     0, 32,  8, 40,  2, 34, 10, 42,
                    48, 16, 56, 24, 50, 18, 58, 26,
                    12, 44,  4, 36, 14, 46,  6, 38,
                    60, 28, 52, 20, 62, 30, 54, 22,
                     3, 35, 11, 43,  1, 33,  9, 41,
                    51, 19, 59, 27, 49, 17, 57, 25,
                    15, 47,  7, 39, 13, 45,  5, 37,
                    63, 31, 55, 23, 61, 29, 53, 21
                };

                float2 screenPos = i.texcoord * _ScreenParams.xy;
                uint ditherIndex = (int(screenPos.x) % 8) + (int(screenPos.y) % 8) * 8;

                float ditherAmount = (dither[ditherIndex] / 64.0 - 0.5) / _PosterizationLevels;

                // ポスタリゼーションとディザーの結果を合成する
                tinted = quantizedColor + ditherAmount;

                return half4(tinted, baseColor.a);
            }
            ENDHLSL
        }
    }
}
