#ifndef PBR_STRUCT_INCLUDE
    #define PBR_STRUCT_INCLUDE
    #include"AutoLight.cginc"
    #include"Lighting.cginc"
    struct appdata
    {
        float4 vertex: POSITION;
        float2 uv0: TEXCOORD0;
        float2 uv1: TEXCOORD1;
        #if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
            float2 uv2: TEXCOORD2;
        #endif
        float3 normal: NORMAL;
        float4 tangent: TANGENT;
    };
    
    struct v2f
    {
        float2 uv: TEXCOORD0;
        float4 pos: SV_POSITION;
        float3 normal: NORMAL;
        float4 worldPos: TEXCOORD1;
        float3 tangentDir: TEXCOORD2;
        float3 biTangentDir: TEXCOORD3;
        UNITY_LIGHTING_COORDS(4, 5)
        half4 ambientOrLightmapUV: TEXCOORD6;    // SH or Lightmap UV
        float3 tanToWorld[3]: TEXCOORD7;
        float3 viewDir:TEXCOORD10;
    };
    //UnityGI输入结构体
    struct GIIData
    {
        UnityLight light;
        float3 worldPos;
        float atten;
        half occlusion;
        half4 ambientOrLightMapUV;
        half3 normalWorld;
        half rough;
        half3 viewDir;
    };
    
#endif