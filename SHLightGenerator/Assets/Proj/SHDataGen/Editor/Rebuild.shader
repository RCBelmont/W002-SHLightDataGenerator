Shader "Unlit/Rebuild"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        my_SHAr ("my_SHAr", Vector) = (0, 0, 0, 0)
        my_SHAg ("my_SHAg", Vector) = (0, 0, 0, 0)
        my_SHAb ("my_SHAb", Vector) = (0, 0, 0, 0)
        my_SHBr ("my_SHBr", Vector) = (0, 0, 0, 0)
        my_SHBg ("my_SHBg", Vector) = (0, 0, 0, 0)
        my_SHBb ("my_SHBb", Vector) = (0, 0, 0, 0)
        my_SHC ("my_SHC", Vector) = (0, 0, 0, 0)
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
                float3 nor: NORMAL;
            };
            
            struct v2f
            {
                float2 uv: TEXCOORD0;
                
                float4 vertex: SV_POSITION;
                float3 nor: NORMAL;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 my_SHAr;
            float4 my_SHAg;
            float4 my_SHAb;
            float4 my_SHBr;
            float4 my_SHBg;
            float4 my_SHBb;
            float4 my_SHC;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.nor = UnityObjectToWorldNormal(v.nor);
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 normal = normalize(i.nor);
                float4 vA = float4(normal, 1);
                float3 l1 = (float3)0;
                l1.r = dot(my_SHAr, vA);
                l1.g = dot(my_SHAg, vA);
                l1.b = dot(my_SHAb, vA);
                
                float3 l2 = (float3)0;
                half4 vB = normal.xyzz * normal.yzzx;
                l2.r = dot(my_SHBr, vB);
                l2.g = dot(my_SHBg, vB);
                l2.b = dot(my_SHBb, vB);
                
                float3 l3 = (float3)0;
                half vC = normal.x * normal.x - normal.y * normal.y;
                l3 = my_SHC.rgb * vC;
                
                float3 res = l1 + l2 + l3;
                #ifdef UNITY_COLORSPACE_GAMMA
                    //res = LinearToGammaSpace(res);
                #endif
                
                return float4(res, 1);
            }
            ENDCG
            
        }
    }
}
