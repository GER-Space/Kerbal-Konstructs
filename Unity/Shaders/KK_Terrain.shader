// KK/Terrain: the shader of the KKTerrain module. It draws KK ground meshes with the stock PQS terrain formula, so they
// look like the terrain underneath (reconstructed from the GLSL of "Terrain/PQS/PQS Main - Extras", KSP 1.12):
//  - albedo = C + P * (C * T - C): C the terrain color at the renderer (PQS vertex color, from KKTerrainShader.cs, or the
//    color set in the grass editor), T the planet's low/mid/high textures, near and far tiling, weighted by altitude,
//    distance and the near/far power P of the planet's terrain material
//  - textures triplanar in the planet frame, with the stock projections and (unnormalized) weights from the direction
//    to the planet's center. The coordinates are anchored per renderer: offsets computed in double precision in C#,
//    plus the small distance of the pixel to the renderer's origin.
//  - the planet's steep texture on slopes, the planet's bump maps
//  - lighting: Lambert, like the stock terrain
//  - atmospheric haze: the stock PQS aerial perspective, blended into the albedo before the lighting
//  - tarmac (red), third (green) and fourth (blue) texture over it by the blend mask
// Samplers: DX11 style textures with shared samplers (a pass may use at most 16).
// Cost: KKTerrainShader passes at most two of low/mid/high per renderer, and projections, near or far tiling, steep
// texture and mask layers with no weight are skipped (dynamic branches with explicit texture gradients).
Shader "KK/Terrain"
{
	Properties
	{
		_BlendMaskTexture("_BlendMaskTexture", 2D) = "black" {}
		_TarmacTexture("The Red areas on the mask", 2D) = "white" {}
		_TarmacColor("_TarmacColor", Color) = (1,1,1,1)
		[ToggleUI] _TarmacTileRandom("Radom tiling for Tarmac Teture", Float) = 0
		[ToggleUI] _TarmacGrayScale("_TarmacGrayScale ", Float) = 0
		[NoScaleOffset] _TarmacNormalTexture("_TarmacNormalTexture", 2D) = "bump" {}

		[NoScaleOffset] _ThirdTexture("Optional Green areas on the mask", 2D)  = "black" {}
		_ThirdTextureTiling("_ThirdTextureTiling (per meter)", Float) = 1
		_ThirdTextureColor("_ThirdTextureColor", Color) = (0,0,0,0)
		[ToggleUI] _ThirdTextureGrayScale("_ThirdTextureGrayScale ", Float) = 0
		[NoScaleOffset] _ThirdNormalTexture("_ThirdNormalTexture", 2D) = "bump" {}

		[NoScaleOffset] _FourthTexture("Optional Blue areas on the mask", 2D) = "black" {}
		_FourthTextureTiling("_FourthTextureTiling (per meter)", Float) = 1
		_FourthTextureColor("_FourthTextureColor", Color) = (0,0,0,0)
		[ToggleUI] _FourthTextureGrayScale("_FourthTextureGrayScale ", Float) = 0
		[NoScaleOffset] _FourthNormalTexture("_FourthNormalTexture", 2D) = "bump" {}

		// marks the terrain shader for the C# code
		[HideInInspector] _KKTerrainMode("_KKTerrainMode", Float) = 1

		// --- per renderer, set by KKTerrainShader.SetTerrainProperties ---
		_KKTerrainColor("Terrain color (PQS vertex color or the grass editor's)", Color) = (0.5,0.5,0.5,1)
		[HideInInspector] _KKAnchorUp("_KKAnchorUp", Vector) = (0,1,0,0)
		// stock triplanar weights: abs(direction from the planet's center), not normalized
		[HideInInspector] _KKTriWeights("_KKTriWeights", Vector) = (0,1,0,0)
		[HideInInspector] _KKPowerNear("_KKPowerNear", Float) = 0.5
		[HideInInspector] _KKPowerFar("_KKPowerFar", Float) = 0.5
		// two of the planet's low / mid / high layers, with their altitude weights
		[NoScaleOffset] _KKLayerATex("_KKLayerATex", 2D) = "white" {}
		[NoScaleOffset] _KKLayerABump("_KKLayerABump", 2D) = "bump" {}
		[HideInInspector] _KKLayerAWeight("_KKLayerAWeight", Float) = 1
		// tilings per meter: x near texture, y far texture, z near bump, w far bump
		[HideInInspector] _KKLayerATilings("_KKLayerATilings", Vector) = (0,0,0,0)
		[HideInInspector] _KKLayerANearOffset("_KKLayerANearOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKLayerAFarOffset("_KKLayerAFarOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKLayerABumpNearOffset("_KKLayerABumpNearOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKLayerABumpFarOffset("_KKLayerABumpFarOffset", Vector) = (0,0,0,0)
		[NoScaleOffset] _KKLayerBTex("_KKLayerBTex", 2D) = "white" {}
		[NoScaleOffset] _KKLayerBBump("_KKLayerBBump", 2D) = "bump" {}
		[HideInInspector] _KKLayerBWeight("_KKLayerBWeight", Float) = 0
		[HideInInspector] _KKLayerBTilings("_KKLayerBTilings", Vector) = (0,0,0,0)
		[HideInInspector] _KKLayerBNearOffset("_KKLayerBNearOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKLayerBFarOffset("_KKLayerBFarOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKLayerBBumpNearOffset("_KKLayerBBumpNearOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKLayerBBumpFarOffset("_KKLayerBBumpFarOffset", Vector) = (0,0,0,0)
		// the planet's steep texture: offsets and tilings per meter
		[HideInInspector] _KKSteepNearOffset("_KKSteepNearOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKSteepFarOffset("_KKSteepFarOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKSteepNearTiling("_KKSteepNearTiling", Float) = 0
		[HideInInspector] _KKSteepFarTiling("_KKSteepFarTiling", Float) = 0
		// KK's third and fourth texture
		[HideInInspector] _KKThirdOffset("_KKThirdOffset", Vector) = (0,0,0,0)
		[HideInInspector] _KKFourthOffset("_KKFourthOffset", Vector) = (0,0,0,0)

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

		// Separate textures and samplers (a pass may use at most 16 samplers), like Unity's UNITY_DECLARE_TEX2D macros:
		// the surface shader analysis pass doesn't support the DX11 syntax, so it gets plain sampler2D
		#if defined(UNITY_SEPARATE_TEXTURE_SAMPLER) && !defined(SHADER_TARGET_SURFACE_ANALYSIS)
			#define KK_TEXTURE(t) Texture2D t
			// with its own semicolon: in the analysis pass it is empty, and a lone ";" is an error there
			#define KK_SAMPLER(t) SamplerState sampler##t;
			#define KK_TEXTURE_PARAMS(t, s) Texture2D t, SamplerState s
			// a declared texture with the sampler of the declared texture s
			#define KK_TEXTURE_ARGS(t, s) t, sampler##s
			// a texture and a sampler that are function parameters
			#define KK_PASS_TEXTURE(t, s) t, s
			#define KK_SAMPLE_GRAD(t, s, uv, dx, dy) t.SampleGrad(s, uv, dx, dy)
		#else
			#define KK_TEXTURE(t) sampler2D t
			#define KK_SAMPLER(t)
			#define KK_TEXTURE_PARAMS(t, s) sampler2D t
			#define KK_TEXTURE_ARGS(t, s) t
			#define KK_PASS_TEXTURE(t, s) t
			#define KK_SAMPLE_GRAD(t, s, uv, dx, dy) tex2Dgrad(t, uv, dx, dy)
		#endif

		sampler2D _BlendMaskTexture;

		sampler2D _TarmacTexture;
		sampler2D _TarmacNormalTexture;
		float4 _TarmacColor;
		fixed _TarmacTileRandom;
		float _TarmacGrayScale;

		// shared samplers: the third and fourth texture and their normal maps use the sampler of the third texture, the
		// planet's textures that of layer A
		KK_TEXTURE(_ThirdTexture);
		KK_SAMPLER(_ThirdTexture)
		KK_TEXTURE(_ThirdNormalTexture);
		float4 _ThirdTextureColor;
		float _ThirdTextureTiling;
		float _ThirdTextureGrayScale;

		KK_TEXTURE(_FourthTexture);
		KK_TEXTURE(_FourthNormalTexture);
		float4 _FourthTextureColor;
		float _FourthTextureTiling;
		float _FourthTextureGrayScale;

		// per renderer
		float4 _KKTerrainColor;
		float4 _KKAnchorUp;
		float4 _KKTriWeights;
		float _KKPowerNear;
		float _KKPowerFar;

		KK_TEXTURE(_KKLayerATex);
		KK_SAMPLER(_KKLayerATex)
		KK_TEXTURE(_KKLayerABump);
		float _KKLayerAWeight;
		float4 _KKLayerATilings;
		float4 _KKLayerANearOffset;
		float4 _KKLayerAFarOffset;
		float4 _KKLayerABumpNearOffset;
		float4 _KKLayerABumpFarOffset;

		KK_TEXTURE(_KKLayerBTex);
		KK_TEXTURE(_KKLayerBBump);
		float _KKLayerBWeight;
		float4 _KKLayerBTilings;
		float4 _KKLayerBNearOffset;
		float4 _KKLayerBFarOffset;
		float4 _KKLayerBBumpNearOffset;
		float4 _KKLayerBBumpFarOffset;

		float4 _KKSteepNearOffset;
		float4 _KKSteepFarOffset;
		float _KKSteepNearTiling;
		float _KKSteepFarTiling;

		float4 _KKThirdOffset;
		float4 _KKFourthOffset;

		// --- globals of the current planet, set every frame by KK (KKTerrainShader.UpdateGlobals) ---
		float4x4 _KKWorldToPlanet;      // rotation world -> planet frame (the PQS transform)
		float4x4 _KKPlanetToWorld;      // rotation planet frame -> world
		float3 _KKSunDirection;         // world space, from the planet to the sun
		// aerial perspective, from the planet's terrain material
		float _KKAerialEnabled;
		float _KKGlobalDensity;
		sampler2D _KKFogColorRamp;
		// steep texture and distance blends, from the planet's terrain material
		float _KKSteepEnabled;
		KK_TEXTURE(_KKSteepTex);
		KK_TEXTURE(_KKSteepBumpMap);
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


		float3 GrayScale(float3 color)
		{
			float a =(0.3*color.r+0.59*color.g+0.11*color.b);
			float b = (max(max(color.r, color.g),color.b ) + min(min(color.r, color.g),color.b))*0.5;
			float g = (a+b)/2;
			return float3(g,g,g);
		}


		// --- triplanar sampling with the projections of the PQS shaders: xy by w.z, zy by w.x, zx by w.y ---
		// The weights are per renderer: a projection without weight is skipped. The gradients come from the caller
		// (derivatives of the coordinates), so these may be used inside branches.

		float4 TriSample(KK_TEXTURE_PARAMS(tex, s), float3 coord, float3 dx, float3 dy, float3 w)
		{
			float4 result = 0;
			UNITY_BRANCH
			if (w.z > 0.001)
			{
				result += KK_SAMPLE_GRAD(tex, s, coord.xy, dx.xy, dy.xy) * w.z;
			}
			UNITY_BRANCH
			if (w.x > 0.001)
			{
				result += KK_SAMPLE_GRAD(tex, s, coord.zy, dx.zy, dy.zy) * w.x;
			}
			UNITY_BRANCH
			if (w.y > 0.001)
			{
				result += KK_SAMPLE_GRAD(tex, s, coord.zx, dx.zx, dy.zx) * w.y;
			}
			return result;
		}

		// the PQS shaders add their unpacked triplanar normals with the same weights and use the sum as tangent normal
		float3 TriNormal(KK_TEXTURE_PARAMS(normalMap, s), float3 coord, float3 dx, float3 dy, float3 w)
		{
			float3 result = 0;
			UNITY_BRANCH
			if (w.z > 0.001)
			{
				result += UnpackNormal(KK_SAMPLE_GRAD(normalMap, s, coord.xy, dx.xy, dy.xy)) * w.z;
			}
			UNITY_BRANCH
			if (w.x > 0.001)
			{
				result += UnpackNormal(KK_SAMPLE_GRAD(normalMap, s, coord.zy, dx.zy, dy.zy)) * w.x;
			}
			UNITY_BRANCH
			if (w.y > 0.001)
			{
				result += UnpackNormal(KK_SAMPLE_GRAD(normalMap, s, coord.zx, dx.zx, dy.zx)) * w.y;
			}
			return result;
		}


		// one of the planet's low / mid / high layers, added with the stock weights
		void AddLayer(KK_TEXTURE_PARAMS(tex, texSampler), KK_TEXTURE_PARAMS(bump, bumpSampler), float layerWeight, float4 tilings, float4 nearOffset, float4 farOffset,
			float4 bumpNearOffset, float4 bumpFarOffset, float nearWeight, float farWeight,
			float3 delta, float3 ddxDelta, float3 ddyDelta, float3 w, inout float3 color, inout float3 normal)
		{
			float nearAmount = layerWeight * nearWeight;
			float farAmount = layerWeight * farWeight;
			UNITY_BRANCH
			if (nearAmount > 0.0001)
			{
				color += TriSample(KK_PASS_TEXTURE(tex, texSampler), nearOffset.xyz + delta * tilings.x, ddxDelta * tilings.x, ddyDelta * tilings.x, w).rgb * nearAmount;
				normal += TriNormal(KK_PASS_TEXTURE(bump, bumpSampler), bumpNearOffset.xyz + delta * tilings.z, ddxDelta * tilings.z, ddyDelta * tilings.z, w) * nearAmount;
			}
			UNITY_BRANCH
			if (farAmount > 0.0001)
			{
				color += TriSample(KK_PASS_TEXTURE(tex, texSampler), farOffset.xyz + delta * tilings.y, ddxDelta * tilings.y, ddyDelta * tilings.y, w).rgb * farAmount;
				normal += TriNormal(KK_PASS_TEXTURE(bump, bumpSampler), bumpFarOffset.xyz + delta * tilings.w, ddxDelta * tilings.w, ddyDelta * tilings.w, w) * farAmount;
			}
		}


		void surf(Input IN, inout SurfaceOutput o)
		{
			float3 delta = IN.planetDelta;
			float3 ddxDelta = ddx(delta);
			float3 ddyDelta = ddy(delta);
			float3 w = _KKTriWeights.xyz;
			float3 wNormalized = w / max(dot(w, float3(1, 1, 1)), 0.0001);

			float3 worldNormal = WorldNormalVector(IN, float3(0,0,1));
			float3 planetNormal = normalize(mul((float3x3)_KKWorldToPlanet, worldNormal));
			float3 planetUp = normalize(_KKAnchorUp.xyz);
			float3 worldUp = normalize(mul((float3x3)_KKPlanetToWorld, planetUp));

			// --- the stock terrain: C + P * (C * T - C) ---
			float nearFar = smoothstep(_KKGroundTexStart, _KKGroundTexEnd, IN.viewDepth);
			float nearWeight = (1.0 - nearFar) * _KKPowerNear;
			float farWeight = nearFar * _KKPowerFar;
			float power = nearWeight + farWeight;

			float3 terrainTexture = 0;
			float3 terrainNormal = 0;
			AddLayer(KK_TEXTURE_ARGS(_KKLayerATex, _KKLayerATex), KK_TEXTURE_ARGS(_KKLayerABump, _KKLayerATex), _KKLayerAWeight, _KKLayerATilings, _KKLayerANearOffset, _KKLayerAFarOffset,
				_KKLayerABumpNearOffset, _KKLayerABumpFarOffset, nearWeight, farWeight, delta, ddxDelta, ddyDelta, w,
				terrainTexture, terrainNormal);
			UNITY_BRANCH
			if (_KKLayerBWeight > 0.0001)
			{
				AddLayer(KK_TEXTURE_ARGS(_KKLayerBTex, _KKLayerATex), KK_TEXTURE_ARGS(_KKLayerBBump, _KKLayerATex), _KKLayerBWeight, _KKLayerBTilings, _KKLayerBNearOffset, _KKLayerBFarOffset,
					_KKLayerBBumpNearOffset, _KKLayerBBumpFarOffset, nearWeight, farWeight, delta, ddxDelta, ddyDelta, w,
					terrainTexture, terrainNormal);
			}
			float3 terrainColor = _KKTerrainColor.rgb;
			float3 groundColor = terrainColor + power * (terrainColor * terrainTexture - terrainColor);

			// --- the planet's steep texture: it replaces the color on slopes ---
			UNITY_BRANCH
			if (_KKSteepEnabled > 0.5)
			{
				float slope = 1.0 - dot(planetUp, planetNormal);
				float steep = saturate(slope * _KKSteepPower) * (1.0 - smoothstep(_KKSteepTexStart, _KKSteepTexEnd, IN.viewDepth));
				float steepNear = steep * (1.0 - nearFar);
				float steepFar = steep * nearFar;
				UNITY_BRANCH
				if (steepNear > 0.0001)
				{
					float3 coord = _KKSteepNearOffset.xyz + delta * _KKSteepNearTiling;
					float3 dx = ddxDelta * _KKSteepNearTiling;
					float3 dy = ddyDelta * _KKSteepNearTiling;
					groundColor = lerp(groundColor, TriSample(KK_TEXTURE_ARGS(_KKSteepTex, _KKLayerATex), coord, dx, dy, w).rgb, steepNear);
					terrainNormal = lerp(terrainNormal, TriNormal(KK_TEXTURE_ARGS(_KKSteepBumpMap, _KKLayerATex), coord, dx, dy, w), steepNear);
				}
				UNITY_BRANCH
				if (steepFar > 0.0001)
				{
					float3 coord = _KKSteepFarOffset.xyz + delta * _KKSteepFarTiling;
					float3 dx = ddxDelta * _KKSteepFarTiling;
					float3 dy = ddyDelta * _KKSteepFarTiling;
					groundColor = lerp(groundColor, TriSample(KK_TEXTURE_ARGS(_KKSteepTex, _KKLayerATex), coord, dx, dy, w).rgb, steepFar);
					terrainNormal = lerp(terrainNormal, TriNormal(KK_TEXTURE_ARGS(_KKSteepBumpMap, _KKLayerATex), coord, dx, dy, w), steepFar);
				}
			}
			float3 normal = normalize(terrainNormal + float3(0, 0, 0.0001));

			// --- the blend mask: tarmac (red, mesh uv), third (green) and fourth (blue, planet frame) ---
			float3 blendmask = tex2D(_BlendMaskTexture, IN.uv2_BlendMaskTexture).rgb;
			float3 outputColor = groundColor;

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
			float tarmacBlend = blendmask.r * tarmacColor.a;
			outputColor = lerp(outputColor, tarmacColor.rgb, tarmacBlend);
			normal = lerp(normal, tarmacNormal, tarmacBlend);

			float thirdMask = blendmask.g * _ThirdTextureColor.a;
			UNITY_BRANCH
			if (thirdMask > 0.0001)
			{
				float3 coord = _KKThirdOffset.xyz + delta * _ThirdTextureTiling;
				float3 dx = ddxDelta * _ThirdTextureTiling;
				float3 dy = ddyDelta * _ThirdTextureTiling;
				float4 thirdPixel = TriSample(KK_TEXTURE_ARGS(_ThirdTexture, _ThirdTexture), coord, dx, dy, wNormalized);
				if (_ThirdTextureGrayScale)
				{
					thirdPixel.rgb = GrayScale(thirdPixel.rgb);
				}
				float4 thirdColor = thirdPixel * _ThirdTextureColor;
				float thirdBlend = blendmask.g * thirdColor.a;
				outputColor = lerp(outputColor, thirdColor.rgb, thirdBlend);
				normal = lerp(normal, normalize(TriNormal(KK_TEXTURE_ARGS(_ThirdNormalTexture, _ThirdTexture), coord, dx, dy, wNormalized)), thirdBlend);
			}

			float fourthMask = blendmask.b * _FourthTextureColor.a;
			UNITY_BRANCH
			if (fourthMask > 0.0001)
			{
				float3 coord = _KKFourthOffset.xyz + delta * _FourthTextureTiling;
				float3 dx = ddxDelta * _FourthTextureTiling;
				float3 dy = ddyDelta * _FourthTextureTiling;
				float4 fourthPixel = TriSample(KK_TEXTURE_ARGS(_FourthTexture, _ThirdTexture), coord, dx, dy, wNormalized);
				if (_FourthTextureGrayScale)
				{
					fourthPixel.rgb = GrayScale(fourthPixel.rgb);
				}
				float4 fourthColor = fourthPixel * _FourthTextureColor;
				float fourthBlend = blendmask.b * fourthColor.a;
				outputColor = lerp(outputColor, fourthColor.rgb, fourthBlend);
				normal = lerp(normal, normalize(TriNormal(KK_TEXTURE_ARGS(_FourthNormalTexture, _ThirdTexture), coord, dx, dy, wNormalized)), fourthBlend);
			}
			normal = normalize(normal);

			// --- aerial perspective of the PQS shaders: the albedo is blended to the fog color before the lighting ---
			// fog = 1 - exp(depth * _globalDensity * _heightDensityAtViewer) (the material's density is negative),
			// fog color from the ramp by the sun's height over the horizon at the pixel, row 0.75
			UNITY_BRANCH
			if (_KKAerialEnabled > 0.5)
			{
				float aerialFog = max(0.0, 1.0 - exp(IN.viewDepth * _KKGlobalDensity * _heightDensityAtViewer));
				float3 fogColor = tex2Dlod(_KKFogColorRamp, float4(dot(_KKSunDirection, worldUp), 0.75, 0, 0)).rgb;
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
