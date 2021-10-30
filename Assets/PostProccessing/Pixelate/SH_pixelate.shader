Shader "pixelate"
{
 Properties
    {
        [Header(map)]
        _RenderTex ("Render Tex", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"}
        pass
        {
            name "Base"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"      

            half4 _RenderTex_ST;
            TEXTURE2D(_RenderTex);    SAMPLER(sampler_RenderTex);
            
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float3 positionWS : TEXCOORD5;
                float4 positionCS : SV_POSITION;
            };

            Varyings vert (Attributes input)
            {
                Varyings output;
                //Get functions will include WS CS ..... variables
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS);
                output.positionWS = positionInputs.positionWS;
                output.positionCS = positionInputs.positionCS;

                //get normals
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = vertexNormalInput.normalWS;
                output.tangentWS = vertexNormalInput.tangentWS;
                output.bitangentWS = vertexNormalInput.bitangentWS;

                output.uv = TRANSFORM_TEX(input.texcoord, _RenderTex);
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_RenderTex, sampler_RenderTex, input.uv);
               
                return col;
            }
            ENDHLSL
        }
    }
}
