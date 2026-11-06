Shader "KK/Diffuse_Multiply_Random"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "grey" {}
        //_MainTex_ST("_MainTex_ST", Vector) = (1,1,0,0)
        _Color("_Color", Color) = (1,1,1,1)
        _Opacity("_Opacity ", Range(0, 1)) = 1
        _RimFalloff("_RimFalloff ", Range(0.01, 5)) = 0.1
        _RimColor("_RimColor", Color) = (0,0,0,0)
        _TemperatureColor("_TemperatureColor", Color) = (0,0,0,0)
        _UnderwaterFogFactor("_UnderwaterFogFactor ", Range(0, 1)) = 0
		[NoScaleOffset] _BumpMap("Nomral Map", 2D) = "bump" {}
		[ToggleUI] _MakeGrayScale ("Make the texure grayscale", Float) = 0
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
        
		// Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf BlinnPhongKSP fullforwardshadows nofog
 
        #pragma target 3.0

		#include "LightingKSP.cginc"
        #include "KSP-include.cginc"
		#include "TileRandom2-include.cginc"
		//#include "TileRandom-include.cginc"
		// #include "NormalFromTex.cginc"

        sampler2D _MainTex;
		sampler2D _BumpMap;

		float4 _BumpMap_TexelSize;

		//sampler2D _BumpMap;
		//sampler2D _Emissive;
		
		//standard KSP shader property values
		//float4 _EmissiveColor;

		//standard shader params for adjusting color/etc
		float _MakeGrayScale;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 viewDir;
			float3 worldPos;
			float3 WorldSpacePosition;
		};

		float grayscale(float3 color) 
    	{
	        float g =(0.3*color.r+0.59*color.g+0.11*color.b);
        	float l = (max(max(color.r, color.g),color.b ) + min(min(color.r, color.g),color.b))*0.5;
        	return (g+l)/2;
    }

		void surf(Input IN, inout SurfaceOutput o)
		{


			//standard texture samplers used regardless of keywords...
			fixed4 textureColor = tex2DStochastic(_MainTex, (IN.uv_MainTex));
			//fixed4 textureColor = tex2D(_MainTex, (IN.uv_MainTex));

			if (_MakeGrayScale)
			{
				textureColor.rgb = grayscale(textureColor);
			}

			// float3 outputColor = lerp( _Color.rgb, textureColor.rgb, _Color.a);
			float3 outputColor = _Color.rgb * textureColor.rgb;			

			//normal map always sampled and assigned directly to surface

			fixed3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			// _HasNormalMap = normal.r + normal.g + normal.b;
			// float _HasNormalMap = (_BumpMap_TexelSize.z > 3);

			// if (_HasNormalMap) 
			// {
				normal = UnpackNormal (tex2DStochastic(_BumpMap, (IN.uv_MainTex)));

			// }
			// else 
			// {
			// 	normal = Unity_NormalFromTextureRandom(_MainTex,IN.uv_MainTex, 0.4 ,3 );
			// }
			
			// outputColor = normal;


			// fixed3 normal = (0.5);
			// o.Normal = normal;


			//emission always sampled and assigned to surface along with stock part-highlighting functionality
			//fixed4 glow = tex2D(_Emissive, (IN.uv_MainTex));
			//o.Emission = glow.rgb * glow.aaa * _EmissiveColor.rgb * _EmissiveColor.aaa + stockEmit(IN.viewDir, normal, _RimColor, _RimFalloff, _TemperatureColor) * _Opacity;
			o.Emission = stockEmit(IN.viewDir, normal, _RimColor, _RimFalloff, _TemperatureColor) * _Opacity;


			//apply the standard shader param multipliers to the sampled/computed values.
			fixed4 fog = UnderwaterFog(IN.worldPos, outputColor);
			o.Albedo = fog.rgb;
			o.Emission *= fog.a;
        	o.Specular = (0);
			o.Normal = normal;

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
