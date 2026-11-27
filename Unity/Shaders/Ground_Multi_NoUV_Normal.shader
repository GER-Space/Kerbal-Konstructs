// KK ground shader (GrassColor2), drawn like the stock PQS terrain it continues:
//  - textures are mapped in the planet's frame (triplanar, like the PQS shaders), so they don't turn with the static and
//    continue seamlessly between statics. The coordinates are anchored per renderer by GrassColor2 (KKTerrainShader.cs):
//    exact offsets in double precision, plus the small distance of the pixel to the renderer's origin.
//  - steep surfaces get the planet's steep texture, like the terrain
//  - lighting: Lambert, as the stock "Terrain/PQS/PQS Main - Extras" and "KSP/Scenery/Diffuse Multiply KSC Terrain"
//  - atmospheric haze: the stock PQS aerial perspective (applied to the albedo, before the lighting)
//  - tarmac (red), third (green) and fourth (blue) texture over the grass by the blend mask
Shader "KK/Ground_Multi_NoUV"
{
	Properties
	{
		[NoScaleOffset] _NearGrassTexture("_NearGrassTexture", 2D) = "grey" {}
		_NearGrassTiling("_NearGrassTiling (per meter)", Float) = 1
		[ToggleUI] _NearGrassGrayScale("_NearGrassGrayScale ", Float) = 0
		[NoScaleOffset] _NearGrassNormalTexture("_NearGrassNormalTexture", 2D) = "bump" {}

		[NoScaleOffset] _FarGrassTexture("_FarGrassTexture", 2D) = "grey" {}
		_FarGrassTiling("_FarGrassTiling (per meter)", Float) = 1
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
		_ThirdTextureTiling("_ThirdTextureTiling (per meter)", Float) = 1
		_ThirdTextureColor("_ThirdTextureColor", Color) = (0,0,0,0)
		[ToggleUI]_ThirdTextureTileRandom("Radom tiling for third Teture", Float) = 0
		[ToggleUI] _ThirdTextureGrayScale("_ThirdTextureGrayScale ", Float) = 0
		[NoScaleOffset] _ThirdNormalTexture("_ThirdNormalTexture", 2D) = "bump" {}

		[NoScaleOffset] _FourthTexture("Optional Blue areas on the mask", 2D) = "black" {}
		_FourthTextureTiling("_FourthTextureTiling (per meter)", Float) = 1
		_FourthTextureColor("_FourthTextureColor", Color) = (0,0,0,0)
		[ToggleUI]_FourthTextureTileRandom("Radom tiling for fourth Teture", Float) = 0
		[ToggleUI] _FourthTextureGrayScale("_FourthTextureGrayScale ", Float) = 0
		[NoScaleOffset] _FourthNormalTexture("_FourthNormalTexture", 2D) = "bump" {}

		// marks the shader version with the fixed normals: GrassColor2 then stops setting its flat-normal workaround textures
		[HideInInspector] _KKNormalFix("_KKNormalFix", Float) = 1
		// marks the terrain shader: GrassColor2 sets the planet anchored coordinates below
		[HideInInspector] _KKTerrainMode("_KKTerrainMode", Float) = 1

		// per renderer, set by GrassColor2 (KKTerrainShader.SetRendererProperties): the planet's up direction at the
		// renderer (planet frame), and frac(planet frame position of the renderer's origin * tiling) per texture
		[HideInInspector] _KKAnchorUp("_KKAnchorUp", Vector) = (0,1,0,0)
		[HideInInspector] _KKNearOffset("_KKNearOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKFarOffset("_KKFarOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKThirdOffset("_KKThirdOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKFourthOffset("_KKFourthOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKSteepNearOffset("_KKSteepNearOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKSteepFarOffset("_KKSteepFarOffset", Vector) = (0,0,0,0)
		// the planet's steep tilings converted to repeats per meter at the renderer
		[HideInInspector] _KKSteepNearTiling("_KKSteepNearTiling", Float) = 0
		[HideInInspector] _KKSteepFarTiling("_KKSteepFarTiling", Float) = 0

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

		// Deferred mod stencil values: 1 parts, 2 terrain, 3 local scenery. The KK ground continues the terrain
		// underneath, so it is marked as terrain (like Parallax's replacement shader for it).
		Stencil
		{
			Ref 2
			Comp Always
			Pass Replace
		}

		CGPROGRAM
		#pragma exclude_renderers gles

		// Lambert like the stock terrain; no Unity fog (KSP doesn't use it), the stock aerial perspective instead
		#pragma surface surf LambertKSP vertex:vert fullforwardshadows nofog exclude_path:prepass

		#pragma target 3.0

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

		// per renderer (GrassColor2)
		float4 _KKAnchorUp;
		float4 _KKNearOffset;
		float4 _KKFarOffset;
		float4 _KKThirdOffset;
		float4 _KKFourthOffset;
		float4 _KKSteepNearOffset;
		float4 _KKSteepFarOffset;
		float _KKSteepNearTiling;
		float _KKSteepFarTiling;

		// --- globals of the current planet, set every frame by KK (KKTerrainShader.UpdateGlobals) ---
		float4x4 _KKWorldToPlanet;      // rotation world -> planet frame (the PQS transform)
		float4x4 _KKPlanetToWorld;      // rotation planet frame -> world
		float3 _KKSunDirection;         // world space, from the planet to the sun
		// aerial perspective, from the planet's terrain material
		float _KKAerialEnabled;
		float _KKGlobalDensity;
		sampler2D _KKFogColorRamp;
		// steep texture, from the planet's terrain material
		float _KKSteepEnabled;
		sampler2D _KKSteepTex;
		sampler2D _KKSteepBumpMap;
		float _KKSteepPower;
		float _KKSteepTexStart;
		float _KKSteepTexEnd;
		float _KKGroundTexStart;
		float _KKGroundTexEnd;
		// set by KSP (PQSMod_AerialPerspectiveMaterial)
		float _heightDensityAtViewer;


		struct Input
		{
			float2 uv2_BlendMaskTexture;
			float2 uv_TarmacTexture;
			float3 viewDir;
			float3 worldPos;
			float3 worldNormal;
			// pixel position relative to the renderer's origin, in the planet frame (meters)
			float3 planetDelta;
			// view space depth, like the PQS shaders use it for their distance blends and the haze
			float viewDepth;
			INTERNAL_DATA
		};


		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 rendererOrigin = unity_ObjectToWorld._m03_m13_m23;
			o.planetDelta = mul((float3x3)_KKWorldToPlanet, worldPos - rendererOrigin);
			o.viewDepth = -UnityObjectToViewPos(v.vertex).z;
		}


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


		// --- triplanar mapping in the planet frame, the projections and weights of the PQS shaders ---
		// projections: xy weighted by |n.z|, zy by |n.x|, zx by |n.y|. The stock shaders don't normalize these weights,
		// which brightens their textures by up to 1.73 depending on the place on the planet: the planet's steep texture
		// is drawn like that (it continues the terrain's), KK's own textures get normalized weights so their colors stay
		// the ones set in the editor.

		float4 Triplanar(sampler2D tex, float3 coord, float3 weights, float stochastic)
		{
			if (stochastic > 0.5)
			{
				return tex2DStochastic(tex, coord.xy) * weights.z + tex2DStochastic(tex, coord.zy) * weights.x
					+ tex2DStochastic(tex, coord.zx) * weights.y;
			}
			return tex2D(tex, coord.xy) * weights.z + tex2D(tex, coord.zy) * weights.x + tex2D(tex, coord.zx) * weights.y;
		}

		// normal map of a triplanar texture, as a planet frame normal (whiteout blend with the surface normal n)
		float3 TriplanarNormal(sampler2D normalMap, float3 coord, float3 n, float3 weights)
		{
			// projection on z: uv = (x, y)
			float3 tz = UnpackNormal(tex2D(normalMap, coord.xy));
			tz = float3(tz.xy + n.xy, abs(tz.z) * n.z);
			// projection on x: uv = (z, y)
			float3 tx = UnpackNormal(tex2D(normalMap, coord.zy));
			tx = float3(tx.xy + n.zy, abs(tx.z) * n.x);
			// projection on y: uv = (z, x)
			float3 ty = UnpackNormal(tex2D(normalMap, coord.zx));
			ty = float3(ty.xy + n.zx, abs(ty.z) * n.y);
			// back into the planet frame: x: (u, v, n) = (z, y, x), y: (z, x, y), z: (x, y, z)
			return normalize(tz.xyz * weights.z + tx.zyx * weights.x + ty.yzx * weights.y);
		}


		void surf(Input IN, inout SurfaceOutput o)
		{
			float3 worldNormal = WorldNormalVector(IN, float3(0,0,1));
			float3 planetNormal = normalize(mul((float3x3)_KKWorldToPlanet, worldNormal));
			float3 stockWeights = abs(planetNormal);
			float3 weights = stockWeights / dot(stockWeights, float3(1, 1, 1));
			float3 planetUp = normalize(_KKAnchorUp.xyz);
			float3 worldUp = normalize(mul((float3x3)_KKPlanetToWorld, planetUp));

			float cameraDistance = distance(IN.worldPos, _WorldSpaceCameraPos);
			float distanceBlendFactor = saturate(cameraDistance / max(_FarGrassBlendDistance, 0.001));

			float3 blendmask = tex2D(_BlendMaskTexture, IN.uv2_BlendMaskTexture).rgb;

			// --- grass ---
			float3 nearCoord = _KKNearOffset.xyz + IN.planetDelta * _NearGrassTiling;
			float3 farCoord = _KKFarOffset.xyz + IN.planetDelta * _FarGrassTiling;

			float4 nearGrassPixel = Triplanar(_NearGrassTexture, nearCoord, weights, 1);
			float4 farGrassPixel = Triplanar(_FarGrassTexture, farCoord, weights, 1);
			if (_NearGrassGrayScale)
			{
				nearGrassPixel.rgb = GrayScale(nearGrassPixel.rgb);
			}
			if (_FarGrassGrayScale)
			{
				farGrassPixel.rgb = GrayScale(farGrassPixel.rgb);
			}
			float3 grassColor = lerp(nearGrassPixel.rgb, farGrassPixel.rgb, distanceBlendFactor) * _GrassColor.rgb;

			float3 nearNormal = TriplanarNormal(_NearGrassNormalTexture, nearCoord, planetNormal, weights);
			float3 farNormal = TriplanarNormal(_FarGrassNormalTexture, farCoord, planetNormal, weights);
			float3 grassPlanetNormal = normalize(lerp(nearNormal, farNormal, distanceBlendFactor));

			// --- steep texture of the planet, blended in like the PQS terrain does it (it replaces the color) ---
			if (_KKSteepEnabled > 0.5)
			{
				float slope = 1.0 - dot(planetUp, planetNormal);
				float steep = saturate(slope * _KKSteepPower) * (1.0 - smoothstep(_KKSteepTexStart, _KKSteepTexEnd, IN.viewDepth));
				float nearFar = smoothstep(_KKGroundTexStart, _KKGroundTexEnd, IN.viewDepth);

				float3 steepNearCoord = _KKSteepNearOffset.xyz + IN.planetDelta * _KKSteepNearTiling;
				float3 steepFarCoord = _KKSteepFarOffset.xyz + IN.planetDelta * _KKSteepFarTiling;

				// no branches around the samples: their texture gradients must stay valid
				float steepNear = steep * (1.0 - nearFar);
				float steepFar = steep * nearFar;
				grassColor = lerp(grassColor, Triplanar(_KKSteepTex, steepNearCoord, stockWeights, 0).rgb, steepNear);
				grassColor = lerp(grassColor, Triplanar(_KKSteepTex, steepFarCoord, stockWeights, 0).rgb, steepFar);
				grassPlanetNormal = normalize(lerp(grassPlanetNormal,
					TriplanarNormal(_KKSteepBumpMap, steepNearCoord, planetNormal, weights), steepNear));
				grassPlanetNormal = normalize(lerp(grassPlanetNormal,
					TriplanarNormal(_KKSteepBumpMap, steepFarCoord, planetNormal, weights), steepFar));
			}

			// --- tarmac (red), mapped by the mesh's uv ---
			fixed4 tarmacPixel;
			float3 tarmacNormal;
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

			// --- third (green) and fourth (blue) texture, mapped in the planet frame like the grass ---
			float3 thirdCoord = _KKThirdOffset.xyz + IN.planetDelta * _ThirdTextureTiling;
			float4 thirdPixel = Triplanar(_ThirdTexture, thirdCoord, weights, _ThirdTextureTileRandom);
			if (_ThirdTextureGrayScale)
			{
				thirdPixel.rgb = GrayScale(thirdPixel.rgb);
			}
			float4 thirdColor = thirdPixel * _ThirdTextureColor;
			float3 thirdPlanetNormal = TriplanarNormal(_ThirdNormalTexture, thirdCoord, planetNormal, weights);

			float3 fourthCoord = _KKFourthOffset.xyz + IN.planetDelta * _FourthTextureTiling;
			float4 fourthPixel = Triplanar(_FourthTexture, fourthCoord, weights, _FourthTextureTileRandom);
			if (_FourthTextureGrayScale)
			{
				fourthPixel.rgb = GrayScale(fourthPixel.rgb);
			}
			float4 fourthColor = fourthPixel * _FourthTextureColor;
			float3 fourthPlanetNormal = TriplanarNormal(_FourthNormalTexture, fourthCoord, planetNormal, weights);

			// --- blend everything over ---
			float tarmacBlend = blendmask.r * tarmacColor.a;
			float thirdBlend = blendmask.g * thirdColor.a;
			float fourthBlend = blendmask.b * fourthColor.a;

			float3 outputColor = lerp(grassColor, tarmacColor.rgb, tarmacBlend);
			outputColor = lerp(outputColor, thirdColor.rgb, thirdBlend);
			outputColor = lerp(outputColor, fourthColor.rgb, fourthBlend);

			// normals: planet frame -> world -> the mesh's tangent space; the tarmac normal map is in tangent space (uv)
			float3 normal = WorldToTangentNormalVector(IN, mul((float3x3)_KKPlanetToWorld, grassPlanetNormal));
			normal = lerp(normal, tarmacNormal, tarmacBlend);
			normal = lerp(normal, WorldToTangentNormalVector(IN, mul((float3x3)_KKPlanetToWorld, thirdPlanetNormal)), thirdBlend);
			normal = lerp(normal, WorldToTangentNormalVector(IN, mul((float3x3)_KKPlanetToWorld, fourthPlanetNormal)), fourthBlend);
			normal = normalize(normal);

			// --- aerial perspective of the PQS shaders: the albedo is blended to the fog color before the lighting ---
			// fog = 1 - exp(depth * _globalDensity * _heightDensityAtViewer) (the material's density is negative),
			// fog color from the ramp by the sun's height over the horizon at the pixel, row 0.75
			if (_KKAerialEnabled > 0.5)
			{
				float aerialFog = max(0.0, 1.0 - exp(IN.viewDepth * _KKGlobalDensity * _heightDensityAtViewer));
				float3 fogColor = tex2D(_KKFogColorRamp, float2(dot(_KKSunDirection, worldUp), 0.75)).rgb;
				outputColor = lerp(outputColor, fogColor, aerialFog);
			}

			o.Emission = stockEmit(IN.viewDir, normal, _RimColor, _RimFalloff, _TemperatureColor) * _Opacity;

			fixed4 fog = UnderwaterFog(IN.worldPos, outputColor);
			o.Normal = normal;
			o.Albedo = fog.rgb;
			o.Emission *= fog.a;
			o.Specular = 0;
			o.Gloss = 0;

#if UNITY_PASS_DEFERRED
			// In deferred rendering do not use the flat ambient because Deferred adds its own ambient as a composite of flat ambient and probe
			unity_SHAr = 0.0.xxxx;
			unity_SHAg = 0.0.xxxx;
			unity_SHAb = 0.0.xxxx;
#endif
		}

		ENDCG

	}
	Fallback "Diffuse"
}
