Shader"HelloWorldShader"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    // Parameters passed in through Unity
    Properties {
        _Color ("Color", Color ) = (1, 1, 1, 1)
        [MainTexture] _BaseMap ("BaseMap", 2D) = "white" {}
        _Smoothness("Smoothness", Float) = 0
        }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Tags {"LightMode"="RenderObjectsPass"}
            // The HLSL code block. Unity SRP uses the HLSL language.
HLSLPROGRAM

#define _SPECULAR_COLOR
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            #pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            


            // To make the Unity shader SRP Batcher compatible, declare all
            // properties related to a Material in a a single CBUFFER block with
            // the name UnityPerMaterial.
            CBUFFER_START(UnityPerMaterial)

sampler2D _BaseMap;
half4 _Color;
float4 _BaseMap_ST;
float _Smoothness;
            CBUFFER_END

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
struct appdata
{
    // The positionOS variable contains the vertex positions in object space.
    //local space stays static so we're referring to it as "attributes"
    float4 posLocal : POSITION;
    float2 uv : TEXCOORD0;
    half3 normal : NORMAL;
};

struct v2f
{
    // The positions in this struct must have the SV_POSITION semantic.
    //it's clip space so there's no depth
    half4 posHClip : SV_POSITION; 
    float2 uv : TEXCOORD0;
    half3 normal : TEXCOORD1;
    half3 posWSpace : TEXCOORD2;
};

            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
v2f vert(appdata IN)
{
                // Declaring the output object (OUT) with the Varyings struct.
    v2f OUT;
    
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
    //you can view all these helper functions at ShaderLibrary/SpaceTransforms.hlsl
    OUT.posHClip = TransformObjectToHClip(IN.posLocal.xyz);
    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
    
    half3 worldNormal = TransformObjectToWorldNormal(IN.normal);
    OUT.normal = worldNormal;
    
    OUT.posWSpace = TransformObjectToWorld(IN.posLocal.xyz);
  
                // Returning the output.
    return OUT;
}

            // The fragment shader definition.            
half4 frag(v2f IN) : SV_Target
{
    float2 uv = IN.uv;
    half4 color = tex2D(_BaseMap, IN.uv) * _Color;
    //half4 color = _Color;
    //color.rgb = IN.normal * 0.5 + 0.5;
    
    // For lighting, create the InputData struct, which contains position and orientation data
    InputData lightingInput = (InputData) 0; // Found in URP/ShaderLib/Input.hlsl
    lightingInput.positionWS = IN.posWSpace;
    lightingInput.normalWS = normalize(IN.normal);
    lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.posWSpace); // In ShaderVariablesFunctions.hlsl
    lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.posWSpace); // In Shadows.hlsl
	
	// Calculate the surface data struct, which contains data from the material textures
    SurfaceData surfaceInput = (SurfaceData) 0;
    surfaceInput.albedo = color.rgb * _Color.rgb;
    surfaceInput.alpha = color.a * _Color.a;
    surfaceInput.specular = 1;
    surfaceInput.smoothness = _Smoothness;

    return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
}
            ENDHLSL
        }


       // shadow caster rendering pass, implemented manually
        // using macros from UnityCG.cginc
        Pass {
            // The shadow caster pass, which draws to shadow maps
            Name"ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}
            ColorMask 0 // No color output, only depth

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct appData
{
    float4 vertex : POSITION;
    float4 normal : NORMAL;
};

struct v2f
{
    float4 posHClip : SV_POSITION;
 
};

float3 _LightDirection; //Unity sets this based off the light rendering this shadow pass

float4 GetShadowCasterPositionHClip(float3 positionWS, float3 normalWS)
{
    float3 lightDirectionWS = _LightDirection;
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
    
    #if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
    #endif
    return positionCS;
}

v2f vert(appData IN)
{
    v2f OUT;
    
    VertexPositionInputs posnInputs = GetVertexPositionInputs(IN.vertex); // Found in URP/ShaderLib/ShaderVariablesFunctions.hlsl
    VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normal); // Found in URP/ShaderLib/ShaderVariablesFunctions.hlsl

    OUT.posHClip = GetShadowCasterPositionHClip(posnInputs.positionWS, normInputs.normalWS);
    return OUT;
}

float4 frag(v2f IN) : SV_Target
{
    return 0;

}
            ENDHLSL
        }

        Pass{
        Name"Depth"
        Tags {"LightMode"="DepthNormalsOnly"}

        HLSLPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        
float frag : SV_Target
{
        return 1;
}
        ENDHLSL

}
    }
}
