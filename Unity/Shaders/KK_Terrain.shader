// KK/Terrain: the shader of the KKTerrain module. It draws KK ground meshes like the stock PQS terrain underneath.
// It follows "Terrain/PQS/PQS Main Shader - Triplanar Zoom Rotation" (KSP 1.12, read from its GLSL), the terrain
// shader of the higher terrain qualities; planets with the other PQS shaders get matching parameters from
// KKTerrainShader.cs (no zoom, no specular, brightness 1, their near tiling and texture strength).
//  - color: C' * T. C' is the terrain color at the renderer (the PQS vertex color, from KKTerrainShader.cs, with the
//    material's saturation / contrast / tint applied there, or the color set in the grass editor), T the planet's
//    low/mid/high textures (the two that matter at the renderer's altitude, chosen in C#)
//  - texture coordinates: (world position + KSP's _floatingOriginOffset) * 0.00001 * tiling, like the terrain, so the
//    textures continue the terrain's. Two zoom levels by view depth (scale _factor^n), each turned by n * _factorRotation
//  - triplanar weights from the world normal: |n|^10, normalized; normal maps blended per projection (reoriented normal
//    mapping) like the stock shader
//  - steep texture: lerp(color, steep, saturate((1 - up.n) * _steepPower))
//  - haze: the stock aerial perspective, blended into the color before the brightness
//  - lighting: Unity's Standard Specular with the terrain's _specularColor, smoothness from the textures' alpha and the
//    color times _albedoBrightness, like the stock shader
//  - tarmac (red), third (green) and fourth (blue) texture over it by the blend mask (without the terrain's specular)
// Cost: projections, the second zoom level, the second layer, the steep texture and the mask textures are only sampled
// where they have weight (dynamic branches with explicit texture gradients).
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
		[HideInInspector] _KKTerrainMode("_KKTerrainMode", Float) = 2

		// --- per renderer, set by KKTerrainShader.SetTerrainProperties ---
		_KKTerrainColor("Terrain color (adjusted PQS vertex color or the grass editor's)", Color) = (0.5,0.5,0.5,1)
		// the planet's up direction at the renderer (planet frame)
		[HideInInspector] _KKAnchorUp("_KKAnchorUp", Vector) = (0,1,0,0)
		// how much the textures change the color: 1 = C * T (zoom rotation shader), else C + P * (C * T - C)
		[HideInInspector] _KKTextureStrength("_KKTextureStrength", Float) = 1
		// two of the planet's low / mid / high layers, with their altitude weights and tilings (per meter: x texture,
		// y bump map)
		[NoScaleOffset] _KKLayerATex("_KKLayerATex", 2D) = "white" {}
		[NoScaleOffset] _KKLayerABump("_KKLayerABump", 2D) = "bump" {}
		[HideInInspector] _KKLayerAWeight("_KKLayerAWeight", Float) = 1
		[HideInInspector] _KKLayerATilings("_KKLayerATilings", Vector) = (1,1,0,0)
		[NoScaleOffset] _KKLayerBTex("_KKLayerBTex", 2D) = "white" {}
		[NoScaleOffset] _KKLayerBBump("_KKLayerBBump", 2D) = "bump" {}
		[HideInInspector] _KKLayerBWeight("_KKLayerBWeight", Float) = 0
		[HideInInspector] _KKLayerBTilings("_KKLayerBTilings", Vector) = (1,1,0,0)
		// KK's third and fourth texture: frac(planet frame position of the renderer * tiling)
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

		// Standard Specular like the stock terrain; no Unity fog (KSP doesn't use it), the stock aerial perspective instead
		#pragma surface surf StandardSpecularKSP vertex:vert fullforwardshadows nofog exclude_path:prepass

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
		float _KKTextureStrength;

		KK_TEXTURE(_KKLayerATex);
		KK_SAMPLER(_KKLayerATex)
		KK_TEXTURE(_KKLayerABump);
		float _KKLayerAWeight;
		float4 _KKLayerATilings;

		KK_TEXTURE(_KKLayerBTex);
		KK_TEXTURE(_KKLayerBBump);
		float _KKLayerBWeight;
		float4 _KKLayerBTilings;

		float4 _KKThirdOffset;
		float4 _KKFourthOffset;

		// --- globals of the current planet, set by KK (KKTerrainShader.UpdateGlobals) ---
		float4x4 _KKWorldToPlanet;      // rotation world -> planet frame (the PQS transform)
		float4x4 _KKPlanetToWorld;      // rotation planet frame -> world
		float3 _KKSunDirection;         // world space, from the planet to the sun
		// zoom levels of the zoom rotation shader (_KKZoomFactor 0: one level, scale 1)
		float _KKZoomFactor;
		float _KKZoomBlendWidth;
		float _KKZoomRotation;          // radians
		// lighting
		float _KKAlbedoBrightness;
		float4 _KKSpecularColor;
		float _KKSmoothnessScale;
		// aerial perspective, from the planet's terrain material
		float _KKAerialEnabled;
		float _KKGlobalDensity;
		float _KKFogRampV;
		sampler2D _KKFogColorRamp;
		// steep texture, from the planet's terrain material (tilings per meter)
		float _KKSteepEnabled;
		KK_TEXTURE(_KKSteepTex);
		KK_TEXTURE(_KKSteepBumpMap);
		float _KKSteepPower;
		float _KKSteepTiling;
		float _KKSteepBumpTiling;
		float _KKSteepTexStart;
		float _KKSteepTexEnd;
		// set by KSP
		float _heightDensityAtViewer;
		float4 _floatingOriginOffset;


		struct Input
		{
			float2 uv2_BlendMaskTexture;
			float2 uv_TarmacTexture;
			float3 viewDir;
			float3 worldPos;
			float3 worldNormal;
			// pixel position relative to the renderer's origin, in the planet frame (meters)
			float3 planetDelta;
			// view space depth, like the PQS shaders use it for their zoom levels and the haze
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


		// texture coordinates turned by the zoom level's angle (cs = cos, sn = sin), as the stock shader does it
		float2 Turn(float2 ab, float cs, float sn)
		{
			return float2(cs * ab.x + sn * ab.y, cs * ab.y - sn * ab.x);
		}


		// one zoom level of a triplanar color texture: projections xy (weight w.z), zy (w.x), zx (w.y)
		float4 TriColor(KK_TEXTURE_PARAMS(tex, s), float3 c, float3 dx, float3 dy, float cs, float sn, float3 w)
		{
			float4 result = 0;
			UNITY_BRANCH
			if (w.z > 0.001)
			{
				result += KK_SAMPLE_GRAD(tex, s, Turn(c.xy, cs, sn), Turn(dx.xy, cs, sn), Turn(dy.xy, cs, sn)) * w.z;
			}
			UNITY_BRANCH
			if (w.x > 0.001)
			{
				result += KK_SAMPLE_GRAD(tex, s, Turn(c.zy, cs, sn), Turn(dx.zy, cs, sn), Turn(dy.zy, cs, sn)) * w.x;
			}
			UNITY_BRANCH
			if (w.y > 0.001)
			{
				result += KK_SAMPLE_GRAD(tex, s, Turn(c.zx, cs, sn), Turn(dx.zx, cs, sn), Turn(dy.zx, cs, sn)) * w.y;
			}
			return result;
		}

		// tangent normal of one projection, turned back by the zoom level's angle
		float3 TurnedNormal(float4 packed, float cs, float sn)
		{
			float3 n = UnpackNormal(packed);
			n.xy = Turn(n.xy, cs, -sn);
			return n;
		}

		// one zoom level of a triplanar normal map: the tangent normals of the projections zy (x), xz (y) and xy (z)
		// are added to nX, nY, nZ with the amount; the projections are blended with the surface normal at the end
		void TriBump(KK_TEXTURE_PARAMS(tex, s), float3 c, float3 dx, float3 dy, float cs, float sn, float3 w, float amount,
			inout float3 nX, inout float3 nY, inout float3 nZ)
		{
			UNITY_BRANCH
			if (w.x > 0.001)
			{
				nX += TurnedNormal(KK_SAMPLE_GRAD(tex, s, Turn(c.zy, cs, sn), Turn(dx.zy, cs, sn), Turn(dy.zy, cs, sn)), cs, sn) * amount;
			}
			UNITY_BRANCH
			if (w.y > 0.001)
			{
				nY += TurnedNormal(KK_SAMPLE_GRAD(tex, s, Turn(c.xz, cs, sn), Turn(dx.xz, cs, sn), Turn(dy.xz, cs, sn)), cs, sn) * amount;
			}
			UNITY_BRANCH
			if (w.z > 0.001)
			{
				nZ += TurnedNormal(KK_SAMPLE_GRAD(tex, s, Turn(c.xy, cs, sn), Turn(dx.xy, cs, sn), Turn(dy.xy, cs, sn)), cs, sn) * amount;
			}
		}

		// reoriented normal mapping of one projection
		float3 RNM(float3 n1, float3 n2)
		{
			n1 += float3(0, 0, 1);
			n2 *= float3(-1, -1, 1);
			return n1 * dot(n1, n2) / n1.z - n2;
		}


		// the zoom levels of one pixel
		struct Zoom
		{
			float scale1;
			float scale2;
			float blend;        // weight of level 2
			float cos1;
			float sin1;
			float cos2;
			float sin2;
		};

		Zoom GetZoom(float depth)
		{
			Zoom z;
			z.scale1 = 1;
			z.scale2 = 1;
			z.blend = 0;
			float angle1 = 0;
			float angle2 = 0;
			UNITY_BRANCH
			if (_KKZoomFactor > 1)
			{
				// the largest _factor^n (n <= 15) below the depth
				float scale = 1;
				float next = 1;
				float n = 0;
				UNITY_LOOP
				for (int i = 0; i < 15; i++)
				{
					if (next >= depth)
					{
						break;
					}
					scale = next;
					next *= _KKZoomFactor;
					n += 1;
				}
				angle1 = (n - 1) * _KKZoomRotation;
				angle2 = n * _KKZoomRotation;
				z.scale1 = scale;
				if (depth < 1)
				{
					z.scale2 = scale;
					z.blend = depth;
				}
				else
				{
					float start = _KKZoomFactor * _KKZoomBlendWidth;
					float t = saturate((depth / scale - start) / ((1 - _KKZoomBlendWidth) * _KKZoomFactor - start));
					z.blend = t * t * (3 - 2 * t);
					z.scale2 = scale * _KKZoomFactor;
				}
			}
			z.cos1 = cos(angle1);
			z.sin1 = sin(angle1);
			z.cos2 = cos(angle2);
			z.sin2 = sin(angle2);
			return z;
		}


		// one of the planet's low / mid / high layers, both zoom levels
		void AddLayer(KK_TEXTURE_PARAMS(tex, texSampler), KK_TEXTURE_PARAMS(bump, bumpSampler), float layerWeight, float4 tilings,
			float3 basePos, float3 ddxPos, float3 ddyPos, Zoom z, float3 w,
			inout float4 color, inout float3 nX, inout float3 nY, inout float3 nZ)
		{
			float amount1 = layerWeight * (1 - z.blend);
			float amount2 = layerWeight * z.blend;
			UNITY_BRANCH
			if (amount1 > 0.0001)
			{
				float k = tilings.x / z.scale1;
				color += TriColor(KK_PASS_TEXTURE(tex, texSampler), basePos * k, ddxPos * k, ddyPos * k, z.cos1, z.sin1, w) * amount1;
				float kb = tilings.y / z.scale1;
				TriBump(KK_PASS_TEXTURE(bump, bumpSampler), basePos * kb, ddxPos * kb, ddyPos * kb, z.cos1, z.sin1, w, amount1, nX, nY, nZ);
			}
			UNITY_BRANCH
			if (amount2 > 0.0001)
			{
				float k = tilings.x / z.scale2;
				color += TriColor(KK_PASS_TEXTURE(tex, texSampler), basePos * k, ddxPos * k, ddyPos * k, z.cos2, z.sin2, w) * amount2;
				float kb = tilings.y / z.scale2;
				TriBump(KK_PASS_TEXTURE(bump, bumpSampler), basePos * kb, ddxPos * kb, ddyPos * kb, z.cos2, z.sin2, w, amount2, nX, nY, nZ);
			}
		}


		// planar triplanar sampling for KK's third and fourth texture (planet frame coordinates, no zoom)
		float4 TriPlanet(KK_TEXTURE_PARAMS(tex, s), float3 c, float3 dx, float3 dy, float3 w)
		{
			return TriColor(KK_PASS_TEXTURE(tex, s), c, dx, dy, 1, 0, w);
		}


		void surf(Input IN, inout SurfaceOutputStandardSpecular o)
		{
			float3 worldNormal = WorldNormalVector(IN, float3(0,0,1));
			float3 planetNormal = normalize(mul((float3x3)_KKWorldToPlanet, worldNormal));
			float3 planetUp = normalize(_KKAnchorUp.xyz);
			float3 worldUp = normalize(mul((float3x3)_KKPlanetToWorld, planetUp));

			// the stock triplanar weights: |n|^10 of the world normal, normalized
			float3 w = abs(worldNormal);
			w = w * w;
			float3 w4 = w * w;
			w = max(w * w4 * w4, 0.00001);
			w /= dot(w, float3(1, 1, 1));

			// the terrain's texture coordinates (meters); KSP's floating origin offset makes them absolute
			float3 basePos = IN.worldPos + _floatingOriginOffset.xyz;
			float3 ddxPos = ddx(IN.worldPos);
			float3 ddyPos = ddy(IN.worldPos);
			Zoom z = GetZoom(IN.viewDepth);

			// --- the planet's layers ---
			float4 terrainTexture = 0;
			float3 nX = 0;
			float3 nY = 0;
			float3 nZ = 0;
			AddLayer(KK_TEXTURE_ARGS(_KKLayerATex, _KKLayerATex), KK_TEXTURE_ARGS(_KKLayerABump, _KKLayerATex), _KKLayerAWeight, _KKLayerATilings,
				basePos, ddxPos, ddyPos, z, w, terrainTexture, nX, nY, nZ);
			UNITY_BRANCH
			if (_KKLayerBWeight > 0.0001)
			{
				AddLayer(KK_TEXTURE_ARGS(_KKLayerBTex, _KKLayerATex), KK_TEXTURE_ARGS(_KKLayerBBump, _KKLayerATex), _KKLayerBWeight, _KKLayerBTilings,
					basePos, ddxPos, ddyPos, z, w, terrainTexture, nX, nY, nZ);
			}
			float3 terrainColor = _KKTerrainColor.rgb;
			float3 groundColor = terrainColor + _KKTextureStrength * (terrainColor * terrainTexture.rgb - terrainColor);
			float smoothness = terrainTexture.a * _KKSmoothnessScale;

			// --- the planet's steep texture: it replaces the color on slopes ---
			UNITY_BRANCH
			if (_KKSteepEnabled > 0.5)
			{
				float slope = 1.0 - dot(planetUp, planetNormal);
				float steep = saturate(slope * _KKSteepPower) * (1.0 - smoothstep(_KKSteepTexStart, _KKSteepTexEnd, IN.viewDepth));
				UNITY_BRANCH
				if (steep > 0.0001)
				{
					float4 steepTexture = 0;
					float3 sX = 0;
					float3 sY = 0;
					float3 sZ = 0;
					AddLayer(KK_TEXTURE_ARGS(_KKSteepTex, _KKLayerATex), KK_TEXTURE_ARGS(_KKSteepBumpMap, _KKLayerATex), 1,
						float4(_KKSteepTiling, _KKSteepBumpTiling, 0, 0), basePos, ddxPos, ddyPos, z, w, steepTexture, sX, sY, sZ);
					groundColor = lerp(groundColor, steepTexture.rgb, steep);
					nX = lerp(nX, sX, steep);
					nY = lerp(nY, sY, steep);
					nZ = lerp(nZ, sZ, steep);
				}
			}

			// --- the normal maps' projections blended with the surface normal (reoriented normal mapping) ---
			float3 tX = RNM(float3(worldNormal.zy, abs(worldNormal.x)), nX + float3(0, 0, 0.0001));
			float3 tY = RNM(float3(worldNormal.xz, abs(worldNormal.y)), nY + float3(0, 0, 0.0001));
			float3 tZ = RNM(float3(worldNormal.xy, abs(worldNormal.z)), nZ + float3(0, 0, 0.0001));
			float3 axisSign = float3(worldNormal.x < 0 ? -1 : 1, worldNormal.y < 0 ? -1 : 1, worldNormal.z < 0 ? -1 : 1);
			tX.z *= axisSign.x;
			tY.z *= axisSign.y;
			tZ.z *= axisSign.z;
			float3 bumpedWorldNormal = normalize(tX.zyx * w.x + tY.xzy * w.y + tZ.xyz * w.z);
			float3 normal = WorldToTangentNormalVector(IN, bumpedWorldNormal);

			// the stock shader brightens the (hazy) terrain color by _albedoBrightness
			float3 outputColor = groundColor * _KKAlbedoBrightness;

			// --- the blend mask: tarmac (red, mesh uv), third (green) and fourth (blue, planet frame) ---
			// kkAmount: how much of the pixel is KK's own textures (they get no terrain specular)
			float kkAmount = 0;
			float3 blendmask = tex2D(_BlendMaskTexture, IN.uv2_BlendMaskTexture).rgb;

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
			kkAmount = lerp(kkAmount, 1, tarmacBlend);

			// KK's third and fourth texture: planet frame coordinates and normalized weights from the planet normal
			float3 wPlanet = abs(planetNormal);
			wPlanet /= max(dot(wPlanet, float3(1, 1, 1)), 0.0001);
			float3 ddxDelta = ddx(IN.planetDelta);
			float3 ddyDelta = ddy(IN.planetDelta);

			float thirdMask = blendmask.g * _ThirdTextureColor.a;
			UNITY_BRANCH
			if (thirdMask > 0.0001)
			{
				float3 coord = _KKThirdOffset.xyz + IN.planetDelta * _ThirdTextureTiling;
				float3 dx = ddxDelta * _ThirdTextureTiling;
				float3 dy = ddyDelta * _ThirdTextureTiling;
				float4 thirdPixel = TriPlanet(KK_TEXTURE_ARGS(_ThirdTexture, _ThirdTexture), coord, dx, dy, wPlanet);
				if (_ThirdTextureGrayScale)
				{
					thirdPixel.rgb = GrayScale(thirdPixel.rgb);
				}
				float4 thirdColor = thirdPixel * _ThirdTextureColor;
				float thirdBlend = blendmask.g * thirdColor.a;
				float4 thirdNormalPixel = TriPlanet(KK_TEXTURE_ARGS(_ThirdNormalTexture, _ThirdTexture), coord, dx, dy, wPlanet);
				outputColor = lerp(outputColor, thirdColor.rgb, thirdBlend);
				normal = lerp(normal, UnpackNormal(thirdNormalPixel), thirdBlend);
				kkAmount = lerp(kkAmount, 1, thirdBlend);
			}

			float fourthMask = blendmask.b * _FourthTextureColor.a;
			UNITY_BRANCH
			if (fourthMask > 0.0001)
			{
				float3 coord = _KKFourthOffset.xyz + IN.planetDelta * _FourthTextureTiling;
				float3 dx = ddxDelta * _FourthTextureTiling;
				float3 dy = ddyDelta * _FourthTextureTiling;
				float4 fourthPixel = TriPlanet(KK_TEXTURE_ARGS(_FourthTexture, _ThirdTexture), coord, dx, dy, wPlanet);
				if (_FourthTextureGrayScale)
				{
					fourthPixel.rgb = GrayScale(fourthPixel.rgb);
				}
				float4 fourthColor = fourthPixel * _FourthTextureColor;
				float fourthBlend = blendmask.b * fourthColor.a;
				float4 fourthNormalPixel = TriPlanet(KK_TEXTURE_ARGS(_FourthNormalTexture, _ThirdTexture), coord, dx, dy, wPlanet);
				outputColor = lerp(outputColor, fourthColor.rgb, fourthBlend);
				normal = lerp(normal, UnpackNormal(fourthNormalPixel), fourthBlend);
				kkAmount = lerp(kkAmount, 1, fourthBlend);
			}
			normal = normalize(normal);

			// --- aerial perspective of the PQS shaders: blended into the color before the brightness ---
			// fog = 1 - exp(depth * _globalDensity * _heightDensityAtViewer) (the material's density is negative),
			// fog color from the ramp by the sun's height over the horizon at the pixel
			UNITY_BRANCH
			if (_KKAerialEnabled > 0.5)
			{
				float aerialFog = max(0.0, 1.0 - exp(IN.viewDepth * _KKGlobalDensity * _heightDensityAtViewer));
				float3 fogColor = tex2Dlod(_KKFogColorRamp, float4(dot(_KKSunDirection, worldUp), _KKFogRampV, 0, 0)).rgb;
				outputColor = lerp(outputColor, fogColor * _KKAlbedoBrightness, aerialFog);
			}

			o.Emission = stockEmit(IN.viewDir, normal, _RimColor, _RimFalloff, _TemperatureColor) * _Opacity;

			fixed4 fog = UnderwaterFog(IN.worldPos, outputColor);
			o.Normal = normal;
			o.Albedo = fog.rgb;
			o.Emission *= fog.a;
			o.Specular = _KKSpecularColor.rgb * (1 - kkAmount);
			o.Smoothness = smoothness * (1 - kkAmount);
			o.Occlusion = 1;
			o.Alpha = 1;

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
