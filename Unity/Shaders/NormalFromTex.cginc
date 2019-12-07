    float3 Unity_NormalFromTexture(sampler2D Texture, float2 UV, float Offset, float Strength)
    {
        Offset = pow(Offset, 3) * 0.1;
        float2 offsetU = float2(UV.x + Offset, UV.y);
        float2 offsetV = float2(UV.x, UV.y + Offset);
        float normalSample = tex2D(Texture,UV);
        float uSample =  tex2D(Texture, offsetU);
        float vSample = tex2D(Texture, offsetV);
        float3 va = float3(1, 0, (uSample - normalSample) * Strength);
        float3 vb = float3(0, 1, (vSample - normalSample) * Strength);
        return normalize(cross(va, vb));
    }

    // float3 Unity_NormalFromTexture2(sampler2D Texture, float2 UV, float4 TexelSize, float Strength)
    // {
                
    //     float Offset = TexelSize.x;
    //     float2 offsetU = float2(UV.x + Offset, UV.y);
    //     float2 offsetV = float2(UV.x, UV.y + Offset);
    //     float normalSample = tex2D(Texture,UV);
    //     float uSample =  tex2D(Texture, offsetU);
    //     float vSample = tex2D(Texture, offsetV);
    //     float3 va = float3(1, 0, (uSample - normalSample) * Strength);
    //     float3 vb = float3(0, 1, (vSample - normalSample) * Strength);
    //     return normalize(cross(va, vb));
    // }

    float grayscale(float3 color) 
    {
        float g =(0.3*color.r+0.59*color.g+0.11*color.b);
        float l = (max(max(color.r, color.g),color.b ) + min(min(color.r, color.g),color.b))*0.5;

        return (g+l)/2;

    }

    float3 normal2Color(float3 p) 
    {
        return (p + 1) * 0.5;
    }

    float3 Getnormal(sampler2D Texture, float2 UV, float4 TexelSize, float Offset, float Strength)
    {
        float w = TexelSize.x*Offset;
        float h = TexelSize.y*Offset;

        float topLeft = grayscale(tex2D (Texture, float2(UV.x - w, UV.y + h)));
        float top      = grayscale(tex2D (Texture, float2(UV.x , UV.y +h )));
        float topRight = grayscale(tex2D (Texture, float2(UV.x + w, UV.y + h )));
        float left    = grayscale(tex2D (Texture, float2(UV.x - w, UV.y)));
        float right    = grayscale(tex2D (Texture, float2(UV.x + w, UV.y)));
        float bottomLeft    = grayscale(tex2D (Texture, float2(UV.x - w, UV.y - h )));
        float bottom    = grayscale(tex2D (Texture, float2(UV.x -w , UV.y )));
        float bottomRight    = grayscale(tex2D (Texture, float2(UV.x - w, UV.y + h)));
        

        float dX = (topRight + Strength * right + bottomRight) - (topLeft + Strength * left + bottomLeft);
        float dY = (bottomLeft + Strength * bottom.r + bottomRight) - (bottomLeft + Strength * top + topRight);
        float dZ = 1;

        return float3(dX,dY,dZ);
    }