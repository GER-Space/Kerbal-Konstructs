//credits for hex tiling goes to Shane (https://www.shadertoy.com/view/Xljczw)
//center, index
float4 GetHexGridInfo(float2 uv)
{
  float2 hexRatio = float2(1.0, sqrt(3.0));
  float4 hexIndex = round(float4(uv, uv - float2(0.5, 1.0)) / hexRatio.xyxy);
  float4 hexCenter = float4(hexIndex.xy * hexRatio, (hexIndex.zw + 0.5) * hexRatio);
  float4 offset = uv.xyxy - hexCenter;
  return dot(offset.xy, offset.xy) < dot(offset.zw, offset.zw) ?  float4(hexCenter.xy, hexIndex.xy) : float4(hexCenter.zw, hexIndex.zw);
  
}

float GetHexSDF(in float2 p)
{
  float2 hexRatio = float2(1.0, sqrt(3.0));
  p = abs(p);
  return 0.5 - max(dot(p, hexRatio * 0.5), p.x);
}

//xy: node pos, z: weight
float3 GetTriangleInterpNode(in float2 pos, in float freq, in int nodeIndex)
{
  float2 hexRatio = float2(1.0, sqrt(3.0));
  float2 nodeOffsets[] = {
    float2(0.0, 0.0),
    float2(1.0, 1.0),
    float2(1.0,-1.0) };

  float2 uv = pos * freq + nodeOffsets[nodeIndex] / hexRatio.xy * 0.5;
  float4 hexInfo = GetHexGridInfo(uv);
  float dist = GetHexSDF(uv - hexInfo.xy) * 2.0;
  return float3(hexInfo.xy / freq, dist);
}

float3 hash33( float3 p )
{
	p = float3( dot(p,float3(127.1,311.7, 74.7)),
			  dot(p,float3(269.5,183.3,246.1)),
			  dot(p,float3(113.5,271.9,124.6)));

	return frac(sin(p)*43758.5453123);
}

float2 hash2D2D(float2 s)
{
	//magic numbers
	return frac(sin(fmod(float2(dot(s, float2(127.1, 311.7)), dot(s, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
}


float4 GetTextureSample(sampler2D Texture, float2 pos, float freq, float2 nodePoint)
{
    float pi = 3.141592;
    float3 hash = hash33(float3(nodePoint.xy, 0));
    float ang = hash.x * 2.0 * pi;
    matrix <float, 2, 2> rotation = { cos(ang), sin(ang), -sin(ang), cos(ang) };
    
    float2 uv = mul( rotation , pos*freq ) + hash.xy;
    return tex2D(Texture, uv);
}


//from Qizhi Yu, et al [2011]. Lagrangian Texture Advection: Preserving Both Spectrum and Velocity Field. 
//IEEE Transactions on Visualization and Computer Graphics 17, 11 (2011), 1612–1623
float3 PreserveVariance(float3 linearColor, float3 meanColor, float moment2)
{
  //return (linearColor / meanColor);
  return (linearColor - meanColor) / sqrt(moment2) + meanColor;
}


float4 tex2DStochastic( sampler2D Texture, in float2 UV)
{
    // Normalized pixel coordinates (from 0 to 1)

    float texFreq = 1.0;


    float chaosLevel = 5.0;

    float4 fragColor = 0 ;
    float moment2 = 0.0;

  for(int j = 0; j < 3; j++)
    {
        float3 interpNode = GetTriangleInterpNode(UV, chaosLevel, j);
        fragColor += GetTextureSample(Texture, UV,  texFreq, interpNode.xy) * interpNode.z;
          
        moment2 += interpNode.z * interpNode.z;
    }
        
        // float4 meanColor = tex2Dlod(Texture, float4(UV,0,10));
       
        // fragColor.rgb = PreserveVariance(fragColor.rgb, meanColor.rgb, moment2);

    return fragColor;

}