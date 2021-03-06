﻿Shader "Hidden/CubeMapProject"
{
    Properties
    {
        _MainTex ("Texture", any) = "white" { }
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv: TEXCOORD0;
                float4 vertex: SV_POSITION;
                half3 vertexLocal: TEXCOORD1;
            };
            
            samplerCUBE _MainTex;
            half4 _MainTex_HDR;
            
            
            v2f vert(appdata v)
            {
                v2f o;
                
                o.uv = v.uv;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertexLocal = half3(v.uv.x, v.uv.y, 0);
                
                return o;
            }
            half4 frag(v2f i): SV_Target
            {
                
                //half4 col = texCUBElod(_MainTex, normalize(float4(sin(i.vertexLocal.x * 3.14), tan(i.vertexLocal.y * 3.14 / 2), cos(i.vertexLocal.x * 3.14), 1)));
                half theta = i.vertexLocal.y * 3.14;
                half phi = 2 * 3.14 * i.vertexLocal.x;
                half3 dir = normalize(float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta)));
                half4 col = texCUBElod(_MainTex, half4(dir, 1));
                //return (half4)vertexLocal.z;
                // half3 c = DecodeHDR(col, _MainTex_HDR);
                //c = c * unity_ColorSpaceDouble.rgb * _ColorTint;
                half3 c = DecodeHDR(col, _MainTex_HDR);
                //return half4(0,1,1,1) ;
                //return half4(i.vertexLocal,1);
                c = GammaToLinearSpace(c);
                return half4(c.rgb, 1);
            }
           
            ENDCG
            
        }
    }
}
