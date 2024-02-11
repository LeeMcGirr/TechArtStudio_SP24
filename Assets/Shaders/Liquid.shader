Shader "Unlit/Liquid"
{
    Properties
    {
        _Color ("Color", Color ) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _TopCol ("Top Color", Color) = (1,1,1,1)
        _Alpha ("Transparency", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "DisableBatching"="True" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Zwrite On //this allows us to write to the depth buffer
            Cull Off // we want the front and back faces
            AlphaToMask On // transparency

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _TopCol;
            float3 _FillAmount;
            float _WobbleX, _WobbleZ;
            float _Freq, _Amplitude;
            float _Alpha;
            CBUFFER_END

            //this is borrowed from https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Rotate-About-Axis-Node.html
            float3 Unity_RotateAboutAxis_Degrees(float3 In, float3 Axis, float Rotation)
            {
                Rotation = radians(Rotation);
                float s = sin(Rotation);
                float c = cos(Rotation);
                float one_minus_c = 1.0 - c;
 
                Axis = normalize(Axis);
                float3x3 rot_mat =
                {
                    one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
                    one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
                    one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
                };
                float3 Out = mul(rot_mat, In);
                return Out;
            }


            struct appdata //pull pos, UV, normals to start
            {
                float4 posLocal : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : NORMAL;
};

            struct v2f //add a TEXCOORD to save the fog value
            {
    
                half4 posHClip : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD3;
                float3 fillPos : TEXCOORD4;
    
                float3 worldNormal : TEXCOORD5;
                float3 viewDir : COLOR;
                float3 normal : COLOR2;
};

            v2f vert (appdata IN)
            {
                v2f OUT;
    
    
                float3 worldPos = mul(unity_ObjectToWorld, IN.posLocal.xyz);
                float3 worldPosOffset = worldPos - _FillAmount;
    
                //use the unity rotate node to rotate along X/Z
                float3 worldPosX = Unity_RotateAboutAxis_Degrees(worldPosOffset, float3(0, 0, 1), 90);
                float3 worldPosZ = Unity_RotateAboutAxis_Degrees(worldPosOffset, float3(1, 0, 0), 90);
                //adjust based on the wobble vars passed in from Liquid.cs
                float3 worldPosAdjusted = worldPos + (worldPosX * _WobbleX) + (worldPosZ * _WobbleZ);
    
                OUT.fillPos = worldPosAdjusted - _FillAmount;
    
    
                OUT.posHClip = TransformObjectToHClip(IN.posLocal);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.fogCoord = ComputeFogFactor(OUT.posHClip.z);
    
    
                //pass out the fill line for our liquid
                return OUT;
            }

            half4 frag (v2f IN, float facing : VFACE) : SV_Target
            {
    
                float myFillPos = IN.fillPos.y;    
                
    
                // This section covers the base color/texture
                half4 col = tex2D(_MainTex, IN.uv) * _Color;    
                col = float4(MixFog(col, IN.fogCoord), _Alpha*col.w);
    
                //this section defines the color of the top
                half4 topCol = float4(_TopCol.xyz, _Alpha*_TopCol.w);
    
                //set a cutoff value - step returns 1 if .5 is > or equal to fill Pos, otherwise returns zero
                float cutoffTop = step(myFillPos, 0.5);            
                float4 cutoffCol = facing > 0 ? cutoffTop * col : cutoffTop * topCol;
    
                //clip discards values of zero, in this case, anything above the waterline
                clip(cutoffTop);
                return cutoffCol;
            }

            ENDHLSL
        }
    }
}
