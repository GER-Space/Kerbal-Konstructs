Shader "KK/Ground_Multi_NoUV"
{
    Properties
    {

        [NoScaleOffset] _NearGrassTexture("_NearGrassTexture", 2D) = "grey" {}
		_NearGrassTiling("_NearGrassTiling ", Float) = 1
		[ToggleUI] _NearGrassGrayScale("_NearGrassGrayScale ", Float) = 0
		[NoScaleOffset] _NearGrassNormalTexture("_NearGrassNormalTexture", 2D) = "bump" {}

		[NoScaleOffset] _FarGrassTexture("_FarGrassTexture", 2D) = "grey" {}
		_FarGrassTiling("_FarGrassTiling ", Float) = 1
		[ToggleUI] _FarGrassGrayScale("_FarGrassGrayScale ", Float) = 0
		[NoScaleOffset] _FarGrassNormalTexture("_FarGrassNormalTexture", 2D) = "bump" {}

		_GrassColor("_GrassColor", Color) = (0.2115077,0.9150943,0.2115077,0)
		_FarGrassBlendDistance("_FarGrassBlendDistance ", Range(0, 1000)) = 100

		_BlendMaskTexture("_BlendMaskTexture", 2D) = "black" {}
		_TarmacTexture("The Red areas on the mask", 2D) = "white" {}
		_TarmacColor("_TarmacColor", Color) = (1,1,1,1)
		[ToggleUI] _TarmacTileRandom("Radom tiling for Tarmac Teture", Float) = 0
		[ToggleUI] _TarmacGrayScale("_TarmacGrayScale ", Float) = 0
		[NoScaleOffset] _TarmacNormalTexture("_TarmacNormalTexture", 2D) = "bump" {}

		[NoScaleOffset] _ThirdTexture("Optional Green areas on the mask", 2D)  = "black" {}
		_ThirdTextureTiling("_ThirdTextureTiling", Float) = 1
		_ThirdTextureColor("_ThirdTextureColor", Color) = (0,0,0,0)
		[ToggleUI]_ThirdTextureTileRandom("Radom tiling for third Teture", Float) = 0
		[ToggleUI] _ThirdTextureGrayScale("_ThirdTextureGrayScale ", Float) = 0
		[NoScaleOffset] _ThirdNormalTexture("_ThirdNormalTexture", 2D) = "bump" {}

		[NoScaleOffset] _FourthTexture("Optional Blue areas on the mask", 2D) = "black" {}
		_FourthTextureTiling("_FourthTextureTiling", Float) = 1
		_FourthTextureColor("_FourthTextureColor", Color) = (0,0,0,0)
		[ToggleUI]_FourthTextureTileRandom("Radom tiling for fourth Teture", Float) = 0
		[ToggleUI] _FourthTextureGrayScale("_FourthTextureGrayScale ", Float) = 0
		[NoScaleOffset] _FourthNormalTexture("_FourthNormalTexture", 2D) = "bump" {}


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

		Stencil
		{
			Ref 3
			Comp Always
			Pass Replace
		}  

        CGPROGRAM
        // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
        #pragma exclude_renderers gles
        
        #pragma surface surf BlinnPhongKSP fullforwardshadows nofog

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		//#include "TileRandom-include.cginc"
		#include "TileRandom2-include.cginc"
		#include "LightingKSP.cginc"
		#include "KSP-include.cginc"

    	sampler2D _BlendMaskTexture;

   		float _FarGrassBlendDistance;
		sampler2D _FarGrassTexture;
		sampler2D _FarGrassNormalTexture;
		float _FarGrassTiling;

		float4 _GrassColor;

    	sampler2D _NearGrassTexture;
		sampler2D _NearGrassNormalTexture;
		float _NearGrassTiling;	

		sampler2D _TarmacTexture;
		sampler2D _TarmacNormalTexture;
    	float4 _TarmacColor;
		fixed _TarmacTileRandom;

		sampler2D _ThirdTexture;
		sampler2D _ThirdNormalTexture;
    	float4 _ThirdTextureColor;
		float _ThirdTextureTiling;
		fixed _ThirdTextureTileRandom;

		sampler2D _FourthTexture;
		sampler2D _FourthNormalTexture;
    	float4 _FourthTextureColor;
		float _FourthTextureTiling;
		fixed _FourthTextureTileRandom;

		float _NearGrassGrayScale;
		float _FarGrassGrayScale;
		float _TarmacGrayScale;
		float _ThirdTextureGrayScale;
		float _FourthTextureGrayScale;

        

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
			INTERNAL_DATA
		};


        float3 WorldToTangentNormalVector(Input IN, float3 normal) 
		{
            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));

        }


    	float3 GrayScale(float3 color) 
    	{
	        float a =(0.3*color.r+0.59*color.g+0.11*color.b);
        	float b = (max(max(color.r, color.g),color.b ) + min(min(color.r, color.g),color.b))*0.5;
			float g = (a+b)/2;
        	return float3(g,g,g);
    	}

		void surf(Input IN, inout SurfaceOutput o)
		{

			IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));
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
			float3 worldNormal = IN.worldNormal;
            float3 norm = mul(rot, worldNormal);
 
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


			float4 nearGrassPixel = tex2DStochastic(_NearGrassTexture, (planarUV * _NearGrassTiling));
			float4 farGrassPixel = tex2DStochastic(_FarGrassTexture, (planarUV * _FarGrassTiling));

			if (_NearGrassGrayScale)
			{
				nearGrassPixel.rgb = GrayScale(nearGrassPixel.rgb);
			}
			if (_FarGrassGrayScale)
			{
				farGrassPixel.rgb = GrayScale(farGrassPixel.rgb);
			}


			float4 grassTextureBlended = lerp(nearGrassPixel, farGrassPixel, distanceBlendFactor);

			grassTextureBlended *= _GrassColor;

			// grass normal map: tangent space of the projected plane (planarUV), blended near / far like the color
			float3 nearNormal = UnpackNormal(tex2DStochastic(_NearGrassNormalTexture, (planarUV * _NearGrassTiling)));
			float3 farNormal = UnpackNormal(tex2DStochastic(_FarGrassNormalTexture, (planarUV * _FarGrassTiling)));
			float3 grassTangentNormal = normalize(lerp(nearNormal, farNormal, distanceBlendFactor));


			// float3 blendmask =  tex2D(_BlendMaskTexture, uv+(0.5,0.5));

			// first map
			fixed4 tarmacPixel;
			float3 tarmacNormal = float3(0, 0, 1);
			if (_TarmacTileRandom) 
			{
				tarmacPixel = tex2DStochastic(_TarmacTexture, IN.uv_TarmacTexture);
				tarmacNormal = UnpackNormal(tex2DStochastic(_TarmacNormalTexture, IN.uv_TarmacTexture));
			} 
			else 
			{
				tarmacPixel = tex2D(_TarmacTexture, IN.uv_TarmacTexture);
				tarmacNormal = UnpackNormal(tex2D(_TarmacNormalTexture, IN.uv_TarmacTexture));
			}
			if (_TarmacGrayScale)
			{
				tarmacPixel.rgb = GrayScale(tarmacPixel.rgb);
			}


			float4 tarmacColor = _TarmacColor * tarmacPixel;
		
			// second texture
			fixed4 thirdMapPixel;
			float3 thirdNormal = float3(0, 0, 1);
			if (_ThirdTextureTileRandom) 
			{
				thirdMapPixel = tex2DStochastic(_ThirdTexture, planarUV * _ThirdTextureTiling);
				thirdNormal = UnpackNormal(tex2DStochastic(_ThirdNormalTexture, IN.uv_TarmacTexture));
			} 
			else 
			{
				thirdMapPixel = tex2D(_ThirdTexture, planarUV * _ThirdTextureTiling);
				thirdNormal = UnpackNormal(tex2D(_ThirdNormalTexture, IN.uv_TarmacTexture));
			}
			if (_ThirdTextureGrayScale)
			{
				thirdMapPixel.rgb = GrayScale(thirdMapPixel.rgb);
			}


			float4 thirtColor = thirdMapPixel * _ThirdTextureColor;


			// third texture
			fixed4 fourthMapPixel;
			float3 fourthNormal = float3(0, 0, 1);
			if (_FourthTextureTileRandom) 
			{
				fourthMapPixel = tex2DStochastic(_FourthTexture, planarUV * _FourthTextureTiling);
				fourthNormal = UnpackNormal(tex2DStochastic(_FourthNormalTexture, IN.uv_TarmacTexture));
			} 
			else 
			{
				fourthMapPixel = tex2D(_FourthTexture, planarUV * _FourthTextureTiling);
				fourthNormal = UnpackNormal(tex2D(_FourthNormalTexture, IN.uv_TarmacTexture));
			}

			if (_FourthTextureGrayScale)
			{
				fourthMapPixel.rgb = GrayScale(fourthMapPixel.rgb);
			}

			float4 fourthColor = fourthMapPixel * _FourthTextureColor;

			// Blend everything over
			float3 outputColor = 0;
			outputColor = lerp (grassTextureBlended.rgb, tarmacColor, blendmask.r * tarmacColor.a);
			outputColor = lerp (outputColor, thirtColor.rgb, blendmask.g * thirtColor.a);
			outputColor = lerp (outputColor, fourthColor.rgb, blendmask.b * fourthColor.a );
			

			// grass normal: from the projected plane into the box space of the mapping (whiteout blend with the
			// mesh normal), then into world space and the mesh's tangent space
			float3 boxNormal;
			if (blend.x > max(blend.y, blend.z)) {
				// planarUV = map.yz
				float2 nUV = grassTangentNormal.xy + norm.yz;
				boxNormal = float3(abs(grassTangentNormal.z) * norm.x, nUV.x, nUV.y);
			} else if (blend.z > blend.y) {
				// planarUV = map.xy
				float2 nUV = grassTangentNormal.xy + norm.xy;
				boxNormal = float3(nUV.x, nUV.y, abs(grassTangentNormal.z) * norm.z);
			} else {
				// planarUV = map.xz
				float2 nUV = grassTangentNormal.xy + norm.xz;
				boxNormal = float3(nUV.x, abs(grassTangentNormal.z) * norm.y, nUV.y);
			}
			// rot is a pure rotation (world -> box), its transpose turns it back
			float3 grassWorldNormal = normalize(mul(transpose(rot), boxNormal));
			float3 normal = WorldToTangentNormalVector(IN, grassWorldNormal);

			// tarmac, third and fourth normal maps are in the mesh's tangent space (uv)
			// (the third and fourth used the red mask channel before, now green and blue like their colors)
			normal = lerp(normal, tarmacNormal, blendmask.r * tarmacColor.a);
			normal = lerp(normal, thirdNormal, blendmask.g * thirtColor.a);
			normal = lerp(normal, fourthNormal, blendmask.b * fourthColor.a);
			normal = normalize(normal);

			//emission always sampled and assigned to surface along with stock part-highlighting functionality
			//fixed4 glow = tex2D(_Emissive, (IN.uv_MainTex));
			//o.Emission = glow.rgb * glow.aaa * _EmissiveColor.rgb * _EmissiveColor.aaa + stockEmit(IN.viewDir, normal, _RimColor, _RimFalloff, _TemperatureColor) * _Opacity;
			o.Emission = stockEmit(IN.viewDir, normal, _RimColor, _RimFalloff, _TemperatureColor) * _Opacity;


			//apply the standard shader param multipliers to the sampled/computed values.
			fixed4 fog = UnderwaterFog(IN.worldPos, outputColor);
			o.Normal = normal;
			o.Albedo = fog.rgb;
			o.Emission *= fog.a;
        	o.Specular = (0);
			

#if UNITY_PASS_DEFERRED
			// In deferred rendering do not use the flat ambient because Deferred adds its own ambient as a composite of flat ambient and probe
			unity_SHAr = 0.0.xxxx;
			unity_SHAg = 0.0.xxxx;
			unity_SHAb = 0.0.xxxx;
#endif
		}

        ENDCG

    }
    Fallback "Standard"
}