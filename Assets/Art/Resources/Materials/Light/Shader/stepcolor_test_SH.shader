Shader "Template"
{
 Properties
    {
        [Header(Base Colors)]
        _BaseColorA("Base color A", color) = (1,0,0,1)
        _BaseColorB("Base color A", color) = (0,0,1,1)
        _BaseColorC("Base color A", color) = (0,1,0,1)
        _BaseColorD("Base color A", color) = (1,1,0.5,1)
        
        [Header(Noise map)]
        _NoiseMap ("Noise Map", 2D) = "white" {}
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

            float4 _BaseColorA;
            float4 _BaseColorB;
            float4 _BaseColorC;
            float4 _BaseColorD;

            sampler2D _NoiseMap;
            float4 _NoiseMap_ST;
            
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

                output.uv = TRANSFORM_TEX(input.texcoord, _NoiseMap);
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                half colorSteps = 0.1;
                half3 col = floor(lerp(_BaseColorA,_BaseColorB,input.uv.x) / colorSteps) * colorSteps;
                return half4(col.rgb,1);
            }
            ENDHLSL
        }
    }
}
