//hash for randomness
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
float2 hash2D2D(float2 s)
{
	//magic numbers
	return frac(sin(fmod(float2(dot(s, float2(127.1, 311.7)), dot(s, float2(269.5, 183.3))), 3.14159)) * 43758.5453);
}

//stochastic sampling
float4 tex2DStochastic(sampler2D tex, float2 UV)
{
	//triangle vertices and blend weights
	//BW_vx[0...2].xyz = triangle verts
	//BW_vx[3].xy = blend weights (z is unused)
	float4x3 BW_vx;

	//uv transformed into triangular grid space with UV scaled by approximation of 2*sqrt(3)
	float2 skewUV = mul(float2x2 (1.0, 0.0, -0.57735027, 1.15470054), UV * 3.464);

	//vertex IDs and barycentric coords
	float2 vxID = float2 (floor(skewUV));
	float3 barry = float3 (frac(skewUV), 0);
	barry.z = 1.0 - barry.x - barry.y;

	BW_vx = ((barry.z > 0) ?
		float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
		float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x)));

	//calculate derivatives to avoid triangular grid artifacts
	float2 dx = ddx(UV);
	float2 dy = ddy(UV);

	//blend samples with calculated weights
	return mul(tex2D(tex, UV.xy + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
		mul(tex2D(tex, UV.xy + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
		mul(tex2D(tex, UV.xy + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
}




float sum( float3 v ) { return v.x+v.y+v.z; }

// #define HASHSCALE1 443.8975
#define HASHSCALE1 27.18281828469
float rand(float2 p) {
	float3 p3  = frac(float3(p.xyx) * HASHSCALE1*2);
	p3 += dot(p3, p3.yzx + 19.19);
	return frac((p3.x + p3.y) * p3.z);
}

float noise(float2 p){
	float2 ip = floor(p);
	float2 u = frac(p);
	u = u*u*(3.0-2.0*u);
	
	float res = lerp(
		lerp(rand(ip),rand(ip+float2(1.0,0.0)),u.x),
		lerp(rand(ip+float2(0.0,1.0)),rand(ip+float2(1.0,1.0)),u.x),u.y);
	//return res*res;
    return res;
}


float4 hash4( float2 p ) { return frac(sin(float4( 1.0+dot(p,float2(37.0,17.0)), 
                                              2.0+dot(p,float2(11.0,47.0)),
                                              3.0+dot(p,float2(41.0,29.0)),
                                              4.0+dot(p,float2(23.0,31.0))))*103.0); }

float4 tex2DStochastic2( sampler2D samp, in float2 uv )
{
    float2 iuv = floor( uv );
    float2 fuv = frac( uv );


    // generate per-tile transform
    float4 ofa = hash4( iuv + float2(0.0,0.0) );
    float4 ofb = hash4( iuv + float2(1.0,0.0) );
    float4 ofc = hash4( iuv + float2(0.0,1.0) );
    float4 ofd = hash4( iuv + float2(1.0,1.0) );
    
    float2 ddxx = ddx( uv );
    float2 ddyy = ddy( uv );

    // transform per-tile uvs
    ofa.zw = sign(ofa.zw-0.5);
    ofb.zw = sign(ofb.zw-0.5);
    ofc.zw = sign(ofc.zw-0.5);
    ofd.zw = sign(ofd.zw-0.5);
    
    // uv's, and derivarives (for correct mipmapping)
    float2 uva = uv*ofa.zw + ofa.xy; float2 ddxa = ddxx*ofa.zw; float2 ddya = ddyy*ofa.zw;
    float2 uvb = uv*ofb.zw + ofb.xy; float2 ddxb = ddxx*ofb.zw; float2 ddyb = ddyy*ofb.zw;
    float2 uvc = uv*ofc.zw + ofc.xy; float2 ddxc = ddxx*ofc.zw; float2 ddyc = ddyy*ofc.zw;
    float2 uvd = uv*ofd.zw + ofd.xy; float2 ddxd = ddxx*ofd.zw; float2 ddyd = ddyy*ofd.zw;
        
    // fetch and blend
    float2 b = smoothstep(0.25,0.75,fuv);
    
    return lerp( lerp( tex2D( samp, uva, ddxa, ddya ), 
                     tex2D( samp, uvb, ddxb, ddyb ), b.x ), 
                lerp( tex2D( samp, uvc, ddxc, ddyc ),
                     tex2D( samp, uvd, ddxd, ddyd ), b.x), b.y );
}




float4 tex2DStochastic3(sampler2D Texture, in float2 UV )
{
   	float k = noise(UV);
    
    float2 duvdx = ddx( UV );
    float2 duvdy = ddy( UV );
    
    float l = k*8.0;
    float i = floor( l );
    float f = frac( l );
	
    float ia = floor(l); // my method
    float ib = ia + 1.0;
    
    float2 offa = sin(float2(3.0,7.0)*(ia+0.0)); // can replace with any other hash
    float2 offb = cos(float2(3.0,7.0)*(ib+1.0)); // can replace with any other hash

    float4 cola = tex2D( Texture, UV + offa, duvdx, duvdy );
    float4 colb = tex2D( Texture, UV + offb, duvdx, duvdy );
    
    return lerp( cola, colb, smoothstep(0.2,0.8,f-0.1*sum(cola-colb)) );
}

float3 Unity_NormalFromTextureRandom(sampler2D Texture, float2 UV, float Offset, float Strength)
{
    Offset = pow(Offset, 3) * 0.1;
    float2 offsetU = float2(UV.x + Offset, UV.y);
    float2 offsetV = float2(UV.x, UV.y + Offset);
    float normalSample = tex2DStochastic(Texture,UV);
    float uSample = tex2DStochastic(Texture, offsetU);
    float vSample = tex2DStochastic(Texture, offsetV);
    float3 va = float3(1, 0, (uSample - normalSample) * Strength);
    float3 vb = float3(0, 1, (vSample - normalSample) * Strength);
    return normalize(cross(va, vb));
}