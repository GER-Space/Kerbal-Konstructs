// WHAT IS THIS FILE?
// this file provides a replacement for the LightingKSP.cginc file that ships with part tools for writing custom shaders.
// This version enables support for the Deferred mod
//
// HOW DO I USE IT?
// Step 1)
// replace LightingKSP.cginc in your shader folder with this file, if present. If you aren't using LightingKSP.cginc
// in your shader, add the following below `CGPROGRAM`:
// `#include "../LightingKSP.cginc"`
//
// Step 2)
// add the following above `CGPROGRAM`, respect the stencil values described in
// https://github.com/LGhassen/Deferred?tab=readme-ov-file#stencil-buffer-usage
// ```
// Stencil
// {
//     Ref 1
//     Comp Always
//     Pass Replace
// }
// ```
//
// Step 3)
// there should be a line in your shader that looks like this:
// `#pragma surface surf BlinnPhongSmooth keepalpha`
// Remove the `keepalpha` if it's there. the part after `surf` is the name of the lighting function your shader uses now.
// If the lighting function is `BlinnPhong` or `BlinnPhongSmooth`, change it to `BlinnPhongKSP`
// If the lighting function is `Standard`, change it to `StandardKSP`
// If the lighting function is `StandardSpecular`, change it to `StandardSpecularKSP`

#ifndef LIGHTING_KSP_INCLUDED
#define LIGHTING_KSP_INCLUDED

#include "UnityPBSLighting.cginc"

#define blinnPhongShininessPower 0.215

// An exact conversion from blinn-phong to PBR is impossible, but the look can be approximated perceptually
// and by observing how blinn-phong looks and feels at various settings, although it can never be perfect
// 1) The specularColor can be used as is in the PBR specular flow, just needs to be divided by PI so it sums up to 1 over the hemisphere
// 2) Blinn-phong shininess doesn't stop feeling shiny unless at very low values, like below 0.04
// while the PBR smoothness feels more linear -> map shininess to smoothness accordingly using a function
// that increases very quickly at first then slows down, I went with something like x^(1/4) or x^(1/6) then made the power configurable
// I tried various mappings from the literature but nothing really worked as well as this
// 3) Finally I noticed that some parts still looked very shiny like the AV-R8 winglet while in stock they looked rough thanks a low
// specularColor but high shininess and specularMap, so I multiplied the smoothness by the sqrt of the specularColor and that caps
// the smoothness when specularColor is low
void GetStandardSpecularPropertiesFromLegacy(float legacyShininess, float specularMap, out float3 specular,
                                             out float smoothness)
{
    float3 legacySpecularColor = saturate(_SpecColor);

    smoothness = pow(legacyShininess, blinnPhongShininessPower) * specularMap;
    smoothness *= sqrt(length(legacySpecularColor));

    specular = legacySpecularColor * UNITY_INV_PI;
}

float4 _Color;

// LEGACY BLINN-PHONG LIGHTING FUNCTION FOR KSP WITH PBR CONVERSION FOR DEFERRED

inline float4 LightingBlinnPhongKSP(SurfaceOutput s, half3 viewDir, UnityGI gi)
{
    return LightingBlinnPhong(s,viewDir, gi);
}

inline float4 LightingBlinnPhongKSP_Deferred(SurfaceOutput s, float3 worldViewDir, UnityGI gi,
                                             out float4 outDiffuseOcclusion, out float4 outSpecSmoothness,
                                             out float4 outNormal)
{
    SurfaceOutputStandardSpecular ss;
    ss.Albedo = s.Albedo;
    ss.Normal = s.Normal;
    ss.Emission = s.Emission;
    ss.Occlusion = 1;
    ss.Alpha = saturate(s.Alpha);
    GetStandardSpecularPropertiesFromLegacy(s.Specular, s.Gloss, ss.Specular, ss.Smoothness);

    return LightingStandardSpecular_Deferred(ss, worldViewDir, gi, outDiffuseOcclusion, outSpecSmoothness, outNormal);
}

inline void LightingBlinnPhongKSP_GI(inout SurfaceOutput s, UnityGIInput gi_input, inout UnityGI gi)
{
    #ifndef UNITY_PASS_DEFERRED
    gi = UnityGlobalIllumination(gi_input, 1.0, s.Normal);
    #endif
}

// STANDARD UNITY LIGHTING FUNCTION FOR KSP

inline float4 LightingStandardKSP(SurfaceOutputStandard s, float3 worldViewDir, UnityGI gi)
{
    return LightingStandard(s, worldViewDir, gi); // no change
}

inline float4 LightingStandardKSP_Deferred(SurfaceOutputStandard s, float3 worldViewDir, UnityGI gi,
                                           out float4 outDiffuseOcclusion,
                                           out float4 outSpecSmoothness, out float4 outNormal)
{
    return LightingStandard_Deferred(s, worldViewDir, gi, outDiffuseOcclusion, outSpecSmoothness, outNormal);
}

inline void LightingStandardKSP_GI(inout SurfaceOutputStandard s, UnityGIInput gi_input, inout UnityGI gi)
{
    #ifndef UNITY_PASS_DEFERRED
    LightingStandard_GI(s, gi_input, gi);
    #endif
}

// STANDARD SPECULAR UNITY LIGHTING FUNCTION FOR KSP

inline float4 LightingStandardSpecularKSP(SurfaceOutputStandardSpecular s, float3 worldViewDir, UnityGI gi)
{
    return LightingStandardSpecular(s, worldViewDir, gi); // no change
}

inline float4 LightingStandardSpecularKSP_Deferred(SurfaceOutputStandardSpecular s, float3 worldViewDir, UnityGI gi,
                                                   out float4 outDiffuseOcclusion,
                                                   out float4 outSpecSmoothness, out float4 outNormal)
{
    return LightingStandardSpecular_Deferred(s, worldViewDir, gi, outDiffuseOcclusion, outSpecSmoothness, outNormal);
}

inline void LightingStandardSpecularKSP_GI(inout SurfaceOutputStandardSpecular s, UnityGIInput gi_input,
                                           inout UnityGI gi)
{
    #ifndef UNITY_PASS_DEFERRED
    LightingStandardSpecular_GI(s, gi_input, gi);
    #endif
}

float4 _LocalCameraPos;
float4 _LocalCameraDir;
float4 _UnderwaterFogColor;
float _UnderwaterMinAlphaFogDistance;
float _UnderwaterMaxAlbedoFog;
float _UnderwaterMaxAlphaFog;
float _UnderwaterAlbedoDistanceScalar;
float _UnderwaterAlphaDistanceScalar;
float _UnderwaterFogFactor;

float4 UnderwaterFog(float3 worldPos, float3 color)
{
    // skip fog in deferred mode
    #ifdef UNITY_PASS_DEFERRED
    return float4(color, 1);
    #endif

    float3 toPixel = worldPos - _LocalCameraPos.xyz;
    float toPixelLength = length(toPixel); ///< Comment out the math--looks better without it.

    float underwaterDetection = _UnderwaterFogFactor * _LocalCameraDir.w; ///< sign(1 - sign(_LocalCameraPos.w));
    float albedoLerpValue = underwaterDetection * (_UnderwaterMaxAlbedoFog * saturate(
        toPixelLength * _UnderwaterAlbedoDistanceScalar));
    float alphaFactor = 1 - underwaterDetection * (_UnderwaterMaxAlphaFog * saturate(
        (toPixelLength - _UnderwaterMinAlphaFogDistance) * _UnderwaterAlphaDistanceScalar));

    return float4(lerp(color, _UnderwaterFogColor.rgb, albedoLerpValue), alphaFactor);
}

#endif
