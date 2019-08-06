// this shader and related files are based off the unity tutorial
// series by catlikecoding at https://catlikecoding.com/unity/tutorials/rendering/

Shader "Custom/BasicShader" {
    
    Properties {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}

        [NoScaleOffset] _NormalMap ("Normals", 2D) = "bump" {}
        _BumpScale ("Bump Scale", Float) = 1

        [Gamma] _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5

        _DetailMask ("Detail Mask", 2D) = "white" {}
    }

    SubShader {
        Pass {
            Tags {
                "LightMode" = "ForwardBase"
            }

            Blend One Zero
            ZWrite true

            CGPROGRAM

            #pragma target 3.0

            #pragma shader_feature _SPECULAR
            #pragma shader_feature _DIFFUSE

            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram

            #define FORWARD_BASE_PASS

            #include "My Lighting.cginc"

            ENDCG
        }

        Pass {
            Tags {
                "LightMode" = "ForwardAdd"
            }

            // blends based on the alpha value 
            Blend One One
            // prevents writing in zbuffer twice
            ZWrite Off

            CGPROGRAM

            #pragma target 3.0

            #pragma shader_feature _SPECULAR
            #pragma shader_feature _DIFFUSE

            #pragma multi_compile_fwdadd_fullshadows

            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram

            #include "My Lighting.cginc"

            ENDCG
        }
    }
    CustomEditor "MyLightingShaderGUI"
}
