Shader "Terrain/custom"
{
 Properties
    {
        [Header(Base Controls)]
        [HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "grey" {}
        [Header(Maps)]
        _VerticalTex("Vertical Tex", 2D) = "white" {}
        _VerticalTexScale("Vertical Tex Scale",float) = 1
        _VerticalTexStrength("Vertical Tex Strength",range(0,0.9)) = 0.5
        [Header(Shadow)]
        _ShadowStrength("Shadow Strength",float) = 1
    }
    
    SubShader
    {
        Tags {"RenderPipeline" = "UniversalPipeline"}
        pass
        {
            name "Base"
            Tags { "Queue" = "Geometry-100" "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "False" "TerrainCompatible" = "True"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"      
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 uvMainAndLM              : TEXCOORD0; // xy: control, zw: lightmap
                float4 uvSplat01                : TEXCOORD1; // xy: splat0, zw: splat1
                float4 uvSplat23                : TEXCOORD2; // xy: splat2, zw: splat3
                
                float3 normalWS                   : TEXCOORD3;
                float3 normalOS                   : TEXCOORD8;
                float3 viewDir                  : TEXCOORD4;
                half3 vertexSH                  : TEXCOORD5; // SH

                half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light
                float3 positionWS               : TEXCOORD7;
                float4 positionCS                  : SV_POSITION;
                float3 positionOS                  : TEXCOORD9;
                float4 shadowCoord              : TEXCOORD10;
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
            TEXTURE2D(_VerticalTex);    SAMPLER(sampler_VerticalTex);
            half _VerticalTexScale;
            half _VerticalTexStrength;
            half _ShadowStrength;
            
            Varyings vert (Attributes input)
            {
                Varyings output;
                //Get functions will include WS CS ..... variables
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS);
                output.positionWS = positionInputs.positionWS;
                output.positionCS = positionInputs.positionCS;
                output.positionOS = input.positionOS;

                //get normals
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = vertexNormalInput.normalWS;
                output.normalOS = input.normalOS;

                output.uvMainAndLM.xy = input.texcoord;
                output.uvMainAndLM.zw = input.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
                output.uvSplat01.xy = TRANSFORM_TEX(input.texcoord, _Splat0);
                output.uvSplat01.zw = TRANSFORM_TEX(input.texcoord, _Splat1);
                output.uvSplat23.xy = TRANSFORM_TEX(input.texcoord, _Splat2);
                output.uvSplat23.zw = TRANSFORM_TEX(input.texcoord, _Splat3);
                
                output.shadowCoord = GetShadowCoord(positionInputs);
                return output;
            }
            
            half4 SplitMap(half4 map)
            {
                map.r = step(0.1,map.r - map.g - map.b - map.a);
                map.g = step(0.1,map.g - map.r - map.b - map.a);
                map.b = step(0.1,map.b - map.g - map.r - map.a);
                map.a = step(0.1,map.a - map.g - map.b - map.r);
                return map;
            }

            half4 MixColors(half4 splatControl,Varyings input)
            {
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
                return mixedDiffuse;
            }

            half4 TriplanarColor(Varyings input)
            {
                // Blending factor of triplanar mapping
                float3 bf = normalize(abs(input.normalWS));
                bf /= dot(bf, (float3)1);

                // Triplanar mapping
                float2 tx = input.positionOS.yz * _VerticalTexScale;
                //float2 ty = input.positionOS.zx * _VerticalTexScale;
                float2 tz = input.positionOS.xy * _VerticalTexScale;

                // Base color
                half4 cx = SAMPLE_TEXTURE2D(_VerticalTex,sampler_VerticalTex,tx) * bf.x;
                // half4 cy = SAMPLE_TEXTURE2D(_VerticalTex,sampler_VerticalTex,ty) * bf.y;
                half4 cz = SAMPLE_TEXTURE2D(_VerticalTex,sampler_VerticalTex,tz) * bf.z;
                half4 triplanarColor = cx + cz;
                return triplanarColor;
            }
            
            float4 frag (Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight(input.shadowCoord);
                float2 splatUV = (input.uvMainAndLM.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
                half4 splatControl = SAMPLE_TEXTURE2D(_Control, sampler_Control, splatUV);
                splatControl = SplitMap(splatControl);

                half4 mixedDiffuse = MixColors(splatControl,input);
                half4 triplanarColor = TriplanarColor(input);

                //light
                float3 N = normalize(input.normalWS);
                float3 L = normalize(mainLight.direction);
                half3 lambert = mainLight.color * (dot(N, L) * 0.5 + 0.5);
                
                half ndv = saturate(dot(N,half3(0,1,0)));
                half shadow = step(0.6,mainLight.shadowAttenuation) + _ShadowStrength;
                half3 finalColor = lerp(triplanarColor.rgb,mixedDiffuse.rgb,step(_VerticalTexStrength,ndv)) * lambert * shadow;
                float4 col = float4(finalColor.rgb,1);
               
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
            #ifdef _ALPHATEST_ON
                ClipHoles(IN.texcoord);
            #endif
                return 0;
            }
            ENDHLSL
        }
    }
    Dependency "AddPassShader" = "Hidden/Terrain/custom(add)"
    
}
