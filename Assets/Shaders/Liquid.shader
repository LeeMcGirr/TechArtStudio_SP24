Shader "Unlit/Liquid"
{
    Properties
    {
        _Color ("Color", Color ) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "DisableBatching"="True" }

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float3 _FillAmount;


            struct appdata //pull pos, UV, normals to start
            {
                float4 posLocal : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f //add a TEXCOORD to save the fog value
            {
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 posHClip : SV_POSITION;
                float3 fillPos : TEXCOORD2;
    
            };

            v2f vert (appdata IN)
            {
                v2f OUT;
    
    
                float3 worldPos = mul(unity_ObjectToWorld, IN.posLocal.xyz);
                float3 worldPosOffset = worldPos - _FillAmount;
    
    
                OUT.posHClip = TransformObjectToHClip(IN.posLocal);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.fogCoord = ComputeFogFactor(OUT.posHClip.z);
    
                //pass out the fill line for our liquid
                OUT.fillPos = worldPos - _FillAmount;
                return OUT;
            }

            half4 frag (v2f IN, float facing : VFACE) : SV_Target
            {
    
                float myFillPos = IN.fillPos.y;    
                // sample the texture
                half4 col = tex2D(_MainTex, myFillPos) * _Color;
    
                // apply fog
                col = float4(MixFog(col, IN.fogCoord), col.w);
    
                //cutoff the color using a step func then the clip() method
                float cutoffTop = step(myFillPos, 0.5);
                float4 cutoffCol = cutoffTop * col;
                clip(cutoffTop);
    
    
                return cutoffCol;
}
            ENDHLSL
        }
    }
}
