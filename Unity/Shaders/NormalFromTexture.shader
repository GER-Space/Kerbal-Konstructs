Shader "KK/Calc/NormalFromTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strengh ("Nomral Strength", float) = 6
        _Offset ("Nomral Offset", float) = 1
        [ToggleUI]_UsenewCode("_UsenewCode", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Stencil
        {
            Ref 3
            Comp Always
            Pass Replace
        }  

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "NormalFromTex.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _Strengh;
            float _Offset;
            float _UsenewCode;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col;

                // if (_UsenewCode) 
                // {
                   col = float4(Getnormal(_MainTex, i.uv, _MainTex_TexelSize, _Offset ,_Strengh ),1);
                // } else 
                // {
                //     col = float4(Unity_NormalFromTexture(_MainTex, i.uv, _Offset , _Strengh ),0);
                // }
                // apply fog
                col = col/2 +0.5 ;
                
                return col;
            }
            ENDCG
        }
    }
}
