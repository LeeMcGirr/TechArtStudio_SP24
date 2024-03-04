Shader "Unlit/depthTex"
// This Unity shader reconstructs the world space positions for pixels using a depth
// texture and screen space UV coordinates. The shader draws a checkerboard pattern
// on a mesh to visualize the positions.
{
    Properties
    { 
        _Scale ("Scale", Int) = 10

    }

    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

CBUFFER_START(UnityPerMaterial)
int _Scale;
CBUFFER_END


struct appdata
{
    float4 pos : POSITION;
};

struct v2f
{
    float4 posHClip : SV_POSITION;
};

v2f vert(appdata IN)
{
    v2f OUT;
   OUT.posHClip = TransformObjectToHClip(IN.pos.xyz);
    return OUT;
}


half4 frag(v2f IN) : SV_Target
{
                // To calculate the UV coordinates for sampling the depth buffer,
                // divide the pixel location by the render target resolution
                // _ScaledScreenParams.
                float2 UV = IN.posHClip.xy / _ScaledScreenParams.xy;

                    //the following is useful boilerplate for different target platforms
                    //normals and depth values vary between OpenGL/D3D/etc
                    // Sample the depth from the Camera depth texture.
                    #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(UV);
                    #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                    #endif

                // Reconstruct the world space positions.
                float3 worldPos = ComputeWorldSpacePosition(UV, depth, UNITY_MATRIX_I_VP);

                // The following part creates the checkerboard effect.
                // Scale is the inverse size of the squares.
                //uint scale = 10;
                // Scale, mirror and snap the coordinates.
                uint3 worldIntPos = uint3(abs(worldPos.xyz * _Scale));
                // Divide the surface into squares. Calculate the color ID value.
                bool white = ((worldIntPos.x) & 1) ^ (worldIntPos.y & 1) ^ (worldIntPos.z & 1);
                // Color the square based on the ID value (black or white).
                half4 color = white ? half4(1, 1, 1, 1) : half4(0, 0, 0, 1);

                // Set the color to black in the proximity to the far clipping
                // plane.
                    #if UNITY_REVERSED_Z
                    // Case for platforms with REVERSED_Z, such as D3D 
                    if(depth < 0.0001)
                        return half4(0,0,0,1);
                    #else
                    // Case for platforms without REVERSED_Z, such as OpenGL
                    if (depth > 0.9999)
                    return half4(0, 0, 0, 1);
                    #endif

    return color;
}
            ENDHLSL
        }
    }
}
