Shader "KK/Ground_Multi_NoUVOld"
{
    Properties
    {

        [NoScaleOffset] _NearGrassTexture("_NearGrassTexture", 2D) = "grey" {}
		_NearGrassTiling("_NearGrassTiling ", Float) = 1
		[NoScaleOffset] _FarGrassTexture("_FarGrassTexture", 2D) = "grey" {}
		_FarGrassTiling("_FarGrassTiling ", Float) = 1
		_FarGrassBlendDistance("_FarGrassBlendDistance ", Range(0, 1000)) = 100
		_GrassColor("_GrassColor", Color) = (0.2115077,0.9150943,0.2115077,0)

		_BlendMaskTexture("_BlendMaskTexture", 2D) = "black" {}
		_TarmacTexture("The Red areas on the mask", 2D) = "white" {}
		_TarmacColor("_TarmacColor", Color) = (1,1,1,0)
		[ToggleUI]_TarmacTileRandom("Radom tiling for Tarmac Teture", Float) = 0

		[NoScaleOffset] _ThirdTexture("Optional Green areas on the mask", 2D)  = "black" {}
		_ThirdTextureTiling("_ThirdTextureTiling", Float) = 1
		_ThirdTextureColor("_ThirdTextureColor", Color) = (0,0,0,0)
		[ToggleUI]_ThirdTextureTileRandom("Radom tiling for third Teture", Float) = 0

		[NoScaleOffset] _FourthTexture("Optional Blue areas on the mask", 2D) = "black" {}
		_FourthTextureTiling("_FourthTextureTiling", Float) = 1
		_FourthTextureColor("_FourthTextureColor", Color) = (0,0,0,0)
		[ToggleUI]_FourthTextureTileRandom("Radom tiling for fourth Teture", Float) = 0

        _Opacity("_Opacity ", Range(0, 1)) = 1
        _RimFalloff("_RimFalloff ", Range(0.01, 5)) = 0.1
        _RimColor("_RimColor", Color) = (0,0,0,0)
        _UnderwaterFogFactor("_UnderwaterFogFactor ", Range(0, 1)) = 0
        _TemperatureColor("_TemperatureColor", Color) = (0,0,0,0)
    }


    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
		ZWrite On
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
        #pragma exclude_renderers gles
        //
        #pragma surface surf Lambert fullforwardshadows nofog


        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		//#include "TileRandom-include.cginc"
		#include "TileRandom2-include.cginc"

    	sampler2D _BlendMaskTexture;

   		float _FarGrassBlendDistance;
		sampler2D _FarGrassTexture;
		float _FarGrassTiling;

		float4 _GrassColor;

    	sampler2D _NearGrassTexture;
		float _NearGrassTiling;	

		sampler2D _TarmacTexture;
    	float4 _TarmacColor;
		fixed _TarmacTileRandom;

		sampler2D _ThirdTexture;
    	float4 _ThirdTextureColor;
		float _ThirdTextureTiling;
		fixed _ThirdTextureTileRandom;

		sampler2D _FourthTexture;
    	float4 _FourthTextureColor;
		float _FourthTextureTiling;
		fixed _FourthTextureTileRandom;


        #include "KSP-include.cginc"

		//sampler2D _BumpMap;
		//sampler2D _Emissive;
		
		//standard KSP shader property values
		//float4 _EmissiveColor;

		//standard shader params for adjusting color/etc
		// float4 _Color;


		struct Input
		{
			float2 uv2_BlendMaskTexture;
			float2 uv_TarmacTexture;
			float3 viewDir;
			float3 worldPos;
			float3 worldNormal;
		};


		void surf(Input IN, inout SurfaceOutput o)
		{

        
           // get scale from matrix
            float3 scale = float3(
                length(unity_WorldToObject._m00_m01_m02),
                length(unity_WorldToObject._m10_m11_m12),
                length(unity_WorldToObject._m20_m21_m22)
                );
 
            // get translation from matrix
            float3 pos = unity_WorldToObject._m03_m13_m23 / scale;
 
            // get unscaled rotation from matrix
            float3x3 rot = float3x3(
                normalize(unity_WorldToObject._m00_m01_m02),
                normalize(unity_WorldToObject._m10_m11_m12),
                normalize(unity_WorldToObject._m20_m21_m22)
                );
            // make box mapping with rotation preserved
            float3 map = mul(rot, IN.worldPos) + pos;
            float3 norm = mul(rot, IN.worldNormal);
 
            float3 blend = abs(norm) / dot(abs(norm), float3(1,1,1));
            float2 planarUV;
            if (blend.x > max(blend.y, blend.z)) {
                planarUV = map.yz;
            } else if (blend.z > blend.y) {
                planarUV = map.xy;
            } else {
                planarUV = map.xz;
            }
			planarUV *= scale;


			float distanceBlendFactor = clamp(pow(distance(IN.worldPos, _WorldSpaceCameraPos)/_FarGrassBlendDistance, 1),0,1);
		   
			float3 blendmask =  tex2D(_BlendMaskTexture, IN.uv2_BlendMaskTexture);


			fixed4 nearGrassPixel = tex2DStochastic(_NearGrassTexture, (planarUV * _NearGrassTiling));
			fixed4 farGrassPixel = tex2DStochastic(_FarGrassTexture, (planarUV * _FarGrassTiling));

			float3 nearGrassColor; 
			float3 farGrassColor;


			 nearGrassColor =  _GrassColor.rgb*  nearGrassPixel.rgb;
			 farGrassColor = _GrassColor.rgb * farGrassPixel.rgb;
	
			float3 grassTextureBlended = lerp(nearGrassColor, farGrassColor, distanceBlendFactor);

			// float3 blendmask =  tex2D(_BlendMaskTexture, uv+(0.5,0.5));

			// first map
			fixed4 tarmacPixel;
			if (_TarmacTileRandom) 
			{
				tarmacPixel = tex2DStochastic(_TarmacTexture, IN.uv_TarmacTexture);
			} 
			else 
			{
				tarmacPixel = tex2D(_TarmacTexture, IN.uv_TarmacTexture);
			}
			float3 tarmacColor = _TarmacColor.rgb * tarmacPixel.rgb;
		
			// second texture
			fixed4 thirdMapPixel;
			if (_ThirdTextureTileRandom) 
			{
				thirdMapPixel = tex2DStochastic(_ThirdTexture, planarUV * _ThirdTextureTiling);
			} 
			else 
			{
				thirdMapPixel = tex2D(_ThirdTexture, planarUV * _ThirdTextureTiling);
			}

			float4 thirtColor = thirdMapPixel * _ThirdTextureColor;


			// third texture
			fixed4 fourthdMapPixel;
			if (_FourthTextureTileRandom) 
			{
				fourthdMapPixel = tex2DStochastic(_FourthTexture, planarUV * _FourthTextureTiling);
			} 
			else 
			{
				fourthdMapPixel = tex2D(_FourthTexture, planarUV * _FourthTextureTiling);
			}

			float4 fourthColor = fourthdMapPixel * _FourthTextureColor;

			// Blend everything over
			float3 outputColor = 0;
			outputColor = lerp (grassTextureBlended.rgb, tarmacColor, blendmask.r );
			outputColor = lerp (outputColor, thirtColor.rgb, blendmask.g * thirtColor.a);
			outputColor = lerp (outputColor, fourthColor.rgb, blendmask.b * fourthColor.a );
			
			// outputColor = lerp (outputColor, grassTextureBlended.rgb, 1-blendmask.g);
			// outputColor = lerp (outputColor, grassTextureBlended.rgb, 1-blendmask.b);



			//normal map always sampled and assigned directly to surface
			// fixed3 normal = UnpackNormal(tex2DStochastic(_BumpMap, IN.uv_MainTex));
			fixed3 normal = UnpackNormal(0.5);
			// o.Normal = normal;


			//emission always sampled and assigned to surface along with stock part-highlighting functionality
			//fixed4 glow = tex2D(_Emissive, (IN.uv_MainTex));
			//o.Emission = glow.rgb * glow.aaa * _EmissiveColor.rgb * _EmissiveColor.aaa + stockEmit(IN.viewDir, normal, _RimColor, _RimFalloff, _TemperatureColor) * _Opacity;
			o.Emission = stockEmit(IN.viewDir, normal, _RimColor, _RimFalloff, _TemperatureColor) * _Opacity;

			//controlled directly by shader property
			o.Alpha = _Opacity;


			//apply the standard shader param multipliers to the sampled/computed values.
			fixed4 fog = UnderwaterFog(IN.worldPos, outputColor);
			o.Albedo = fog.rgb;
			o.Emission *= fog.a;
        	o.Specular = (0);
			// o.SpecularColor = (0,0,0,0);
		}

        ENDCG

    }
    Fallback "Standard"
}