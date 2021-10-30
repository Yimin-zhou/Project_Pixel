Shader "Hidden/Terrain/custom(add)"
{
 Properties
    {
        [Header(Base Controls)]
        [HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "grey" {}
    }
    
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"}
        pass
        {
            name "AddBase"
            Tags { "Queue" = "Geometry-99" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define TERRAIN_SPLAT_ADDPASS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"      
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 uvMainAndLM              : TEXCOORD0; // xy: control, zw: lightmap
                float4 uvSplat01                : TEXCOORD1; // xy: splat0, zw: splat1
                float4 uvSplat23                : TEXCOORD2; // xy: splat2, zw: splat3
                
                float3 normalWS                   : TEXCOORD3;
                float3 viewDir                  : TEXCOORD4;
                half3 vertexSH                  : TEXCOORD5; // SH

                half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light
                float3 positionWS               : TEXCOORD7;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord              : TEXCOORD8;
            #endif
                float4 positionCS                  : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            //terrain textures
            float4 _Control_ST;
            float4 _Control_TexelSize;
            half4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
            TEXTURE2D(_Control);    SAMPLER(sampler_Control);
            TEXTURE2D(_Splat0);     SAMPLER(sampler_Splat0);
            TEXTURE2D(_Splat1);
            TEXTURE2D(_Splat2);
            TEXTURE2D(_Splat3);

            half4 SplitMap(half4 map)
            {
                map.r = step(0.1,map.r - map.g - map.b - map.a);
                map.g = step(0.1,map.g - map.r - map.b - map.a);
                map.b = step(0.1,map.b - map.g - map.r - map.a);
                map.a = step(0.1,map.a - map.g - map.b - map.r);
                return map;
            }

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

                output.uvMainAndLM.xy = input.texcoord;
                output.uvMainAndLM.zw = input.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
                output.uvSplat01.xy = TRANSFORM_TEX(input.texcoord, _Splat0);
                output.uvSplat01.zw = TRANSFORM_TEX(input.texcoord, _Splat1);
                output.uvSplat23.xy = TRANSFORM_TEX(input.texcoord, _Splat2);
                output.uvSplat23.zw = TRANSFORM_TEX(input.texcoord, _Splat3);
                
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float2 splatUV = (input.uvMainAndLM.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
                half4 splatControl = SAMPLE_TEXTURE2D(_Control, sampler_Control, splatUV);
                splatControl = SplitMap(splatControl);

                //only for addpass
                float weight = dot(splatControl, 1.0h);
                #ifdef TERRAIN_SPLAT_ADDPASS
                    clip(weight <= 0.005h ? -1.0h : 1.0h);
                #endif
                
                half4 diffAlbedo[4];
                diffAlbedo[0] = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, input.uvSplat01.xy);
                diffAlbedo[1] = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat0, input.uvSplat01.zw);
                diffAlbedo[2] = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat0, input.uvSplat23.xy);
                diffAlbedo[3] = SAMPLE_TEXTURE2D(_Splat3, sampler_Splat0, input.uvSplat23.zw);

                half4 mixedDiffuse = 0.0f;
                mixedDiffuse += diffAlbedo[0] * half4(splatControl.rrr, 1.0f);
                mixedDiffuse += diffAlbedo[1] * half4(splatControl.ggg, 1.0f);
                mixedDiffuse += diffAlbedo[2] * half4(splatControl.bbb, 1.0f);
                mixedDiffuse += diffAlbedo[3] * half4(splatControl.aaa, 1.0f);
                
                float4 col = float4(mixedDiffuse.rgb,1);
               
                return col;
            }
            ENDHLSL
        }
    }
}
