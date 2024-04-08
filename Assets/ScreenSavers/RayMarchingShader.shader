Shader "Unlit/RayMarchingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AmbientColor ("Ambient Color", Color) = (1, 1, 1, 1)
        _AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.1
        _LightAzimuth ("Light Azimuth", Range(0, 360)) = 45
        _LightElevation ("Light Elevation", Range(0, 360)) = 45
        _FOV ("Field of View", Range(0, 20)) = .9
    }

SubShader {
    Tags { "RenderType"="Opaque" }
    Pass {
        HLSLPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  
CBUFFER_START(UnityPerMaterial)
        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _AmbientColor;
        float _AmbientStrength;
        float4 _CamPos;
        float4x4 _CamRot;

        float _FOV;
  
        float _LightAzimuth;
        float _LightElevation;
        float3 _LightDir = normalize(float3(-1, -1, -1)); // Light coming from the top left
CBUFFER_END

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

v2f vert(appdata IN)
{
                // Declaring the output object (OUT) with the Varyings struct.
    v2f OUT;
    
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
    //you can view all these helper functions at ShaderLibrary/SpaceTransforms.hlsl
    OUT.posHClip = TransformObjectToHClip(IN.posLocal.xyz);
    OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
    
    half3 worldNormal = TransformObjectToWorldNormal(IN.normal);
    OUT.normal = worldNormal;
    
    OUT.posWSpace = TransformObjectToWorld(IN.posLocal.xyz);
  
                // Returning the output.
    return OUT;
}

void calculateLightDir()
{
    float azimuthRad = _LightAzimuth * 3.14159 / 180.0;
    float elevationRad = _LightElevation * 3.14159 / 180.0;
    _LightDir.x = cos(azimuthRad) * cos(elevationRad);
    _LightDir.y = sin(elevationRad);
    _LightDir.z = sin(azimuthRad) * cos(elevationRad);
    _LightDir = normalize(_LightDir);
}

float3 calculateNormal(float3 pos)
{
    float eps = 0.01; // Small value for finite difference
    float3 epsVec = float3(eps, 0.0, 0.0);
    float dx = length(max(abs(pos + epsVec) - 0.5, 0.0)) - length(max(abs(pos - epsVec) - 0.5, 0.0));
    epsVec = float3(0.0, eps, 0.0);
    float dy = length(max(abs(pos + epsVec) - 0.5, 0.0)) - length(max(abs(pos - epsVec) - 0.5, 0.0));
    epsVec = float3(0.0, 0.0, eps);
    float dz = length(max(abs(pos + epsVec) - 0.5, 0.0)) - length(max(abs(pos - epsVec) - 0.5, 0.0));
    return normalize(float3(dx, dy, dz));
}



float4 frag(v2f i) : SV_Target
{
    calculateLightDir();
    float3 camPos = _CamPos.xyz;
    float aspectRatio = _ScreenParams.y / _ScreenParams.x; // Height / Width
    float2 ndc = float2((i.uv.x * 2.0 - 1.0) * _FOV, (i.uv.y * 2.0 - 1.0) * _FOV * aspectRatio);
    float3 rayDir = mul((float3x3) _CamRot, normalize(float3(ndc, 1.0)));

    // Ray marching parameters
    float maxDistance = 100.0;
    float minDist = 0.01; // Minimum distance to consider a hit
    int maxSteps = 500; // Maximum steps to march
    
    // March the ray
    float distTravelled = 0.0;
    for (int j = 0; j < maxSteps; j++)
    {
        float3 currentPos = camPos + distTravelled * rayDir;
        float distToCube = length(max(abs(currentPos) - 0.5, 0.0)); // Distance to the surface of the cube
        if (distToCube < minDist)
        {
            // Hit detected
            float3 normal = calculateNormal(currentPos);
            float diffuse = max(0.0, dot(normal, _LightDir));

            // Use the ambient color and strength properties
            float3 ambient = _AmbientColor.rgb * _AmbientStrength;

            // Add the ambient color to the diffuse lighting
            float3 finalColor = diffuse + ambient;

            return float4(finalColor, 1); // Use final color
        }
        distTravelled += distToCube;
        if (distTravelled > maxDistance)
            break; // Exit if too far
    }
    // No hit
    return float4(0, 0, 0, 1); // Black color for background
}
            ENDHLSL
        }
    }
}
