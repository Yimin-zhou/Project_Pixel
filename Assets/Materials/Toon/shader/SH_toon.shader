Shader "Toon"
{
 Properties
    {
        [Header(Main map)]
        _MainTex ("Main Map", 2D) = "white" {}
        _BaseTint("Base Tint", color) = (1,1,1,1)
        
        [Header(Shadow)]
        _ShadowRange("Shadow Range",float) = 1
        _ShadowColor("Shadow Color",color) = (1,1,1,1)
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True"}
        pass
        {
            name "Base"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"      

            half _ShadowRange;
            half4 _ShadowColor;

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            half4 _MainTex_ST;
            half4 _BaseTint;
            
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
                float4 shadowCoord : TEXCOORD6;
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

                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.shadowCoord = GetShadowCoord(positionInputs);
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                #if _MAIN_LIGHT_SHADOWS || _MAIN_LIGHT_SHADOWS_CASCADE
                    Light mainLight = GetMainLight(input.shadowCoord);
                #else
                    Light mainLight = GetMainLight();
                #endif
                
                float3 N = normalize(input.normalWS);
                float3 L = normalize(mainLight.direction);

                half3 baseColor = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv) * _BaseTint.rgb;
                half shadow = step(0.9,mainLight.shadowAttenuation) * 0.5 + 0.5;
                half halfLambert = (dot(N, L) * 0.5 + 0.5) * shadow;
                half3 diffuse = halfLambert > _ShadowRange ? baseColor.rgb : _ShadowColor.rgb * baseColor.rgb;
                float4 col = float4(diffuse.rgb,1);
               
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            // -------------------------------------
            // Universal Pipeline keywords

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            float3 _LightDirection;
            float3 _LightPosition;

            struct AttributesLean
            {
                float4 position     : POSITION;
                float3 normalOS       : NORMAL;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsLean
            {
                float4 clipPos      : SV_POSITION;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VaryingsLean ShadowPassVertex(AttributesLean v)
            {
                VaryingsLean o = (VaryingsLean)0;
                float3 positionWS = TransformObjectToWorld(v.position.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);

            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

                float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

            #if UNITY_REVERSED_Z
                clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
            #else
                clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
            #endif

                o.clipPos = clipPos;

                o.texcoord = v.texcoord;

                return o;
            }

            half4 ShadowPassFragment(VaryingsLean IN) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
