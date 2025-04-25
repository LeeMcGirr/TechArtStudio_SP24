Shader"HelloWorld2"
{
	//the public properties block of the shader in Unity
	//parameters here are passed out into the inspector
	Properties
	{
		//you can include information in () to name and categorize the variable for public users
		_Color("Color", Color) = (1,1,1,1)
		[MainTexture] _BaseMap("BaseMap", 2D) = "white" {}
	}

	//a subShader block contains information for a single rendering pass
	SubShader
	{
		//tags tell Unity when to render this pass and in what pipelines
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

		Pass
		{
			//this KEYWORD declares that we are starting a shader / this info all goes to the GPU
			HLSLPROGRAM

			// This line declares our vertex shader
			#pragma vertex vert
			// This line declares our fragment shader
			#pragma fragment frag //test
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			//this KEYWORD enables SRP batching for the material/shader
			CBUFFER_START(UnityPerMaterial)
			
			//This is the private color variable for the shader
			//half refers to precision (number of decimal points)
			//4 means this is a vector4 (color value)
			half4 _Color;
			sampler2D _BaseMap;
			float4 _BaseMap_ST;
			CBUFFER_END

			//appData is a struct that stores relevant info from UNITY
			//this is a declaration of a type of data that exists, it is NOT an instance of the data existing
			struct appdata
			{
				//in appdata we declare variables and SEMANTICS that help up store data on the GPU
				//variableType variableName : SEMANTIC;
				//for this shader we just need position data and maybe a single UV coordinate texture for later
				float4 posLocal : POSITION;
				float2 uv : TEXCOORD0;
				half3 normal : NORMAL;

			};

			//v2f is a struct that stores data relevant to the vertex positions/shader
			//this is a declaration of a type of data that exists, it is NOT an instance of the data existing
			struct vertex2fragment
			{
				//hClip refers to camera space, this semantic is for saving the screenspace translation of the local coord
				//MUST USE the SV_POSITION semantic - otherwise Unity will not know where the hell your screenspace pos data is
				half4 posHClip : SV_POSITION;
				float2 uv : TEXCOORD0;
				half3 normal : TEXCOORD1; //here we're just transforming the texture image from 2D UV space to the normal space of the object
			
			};

			//this IS a concrete instance of a data struct that exists ON THE GPU
			// our vertex to fragment instance will take an instance of appdata as an input
			//we'll name this appdata instance IN 
			vertex2fragment vert(appdata IN)
			{
				vertex2fragment OUT; //it will send out this data to whatever variables or functions request it
				//currently the OUT is EMPTY
				//test
				//let's fill that empty OUT with information from the appdata IN that we used as input for this instance
				OUT.posHClip = TransformObjectToHClip(IN.posLocal.xyz);
				OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

				half3 worldNormal = TransformObjectToWorldNormal(IN.normal);
				OUT.normal = worldNormal;

				return OUT;

			}

			//a color variable constructed or calculated from information grabbed off the GPU
			//this MUST send data to the semantic SV_Target
			half4 frag(vertex2fragment IN) : SV_Target
			{
				float2 uv = IN.uv;
				half4 color = tex2D(_BaseMap, IN.uv) * _Color;

				return color * _Color;
			}

			ENDHLSL

			
		}


	}



}

