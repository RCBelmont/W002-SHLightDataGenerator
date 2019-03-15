#ifndef PBR_FUNC_INCLUDE
    #define PBR_FUNC_INCLUDE
    #include"PBRStruct.cginc"
    #include"PBRLib.cginc"
    #include"UnityCG.cginc"
    #include"UnityStandardUtils.cginc"
    
    sampler2D _MainTex;
    float4 _MainTex_ST;
    float4 _MainColor;
    sampler2D _NormalTex;
    sampler2D _FuncTex; //metallic, smooth, AO1
    float _MetallicStr;
    float _SmoothStr;
    float _AO;
    sampler2D _EmissionTex;
    float3 _EmissionColor;
    float _EmissionStr;
    float _AmbientStr;
    float _FresnelStr;
    half3 _FresnelColor;
    
    v2f vert(appdata v)
    {
        v2f o = (v2f)0;
        
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv0, _MainTex);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0))).xyz;
        float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
        o.normal = worldNormal;
        o.biTangentDir = (cross(o.normal, o.tangentDir) * v.tangent.w);
        //We need this for shadow receving
        UNITY_TRANSFER_LIGHTING(o, v.uv1);
        o.ambientOrLightmapUV = MY_VertexGIForward(v, o.worldPos, o.normal);
        o.tanToWorld[0] = o.tangentDir;
        o.tanToWorld[1] = o.biTangentDir;
        o.tanToWorld[2] = o.normal;
        o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
        return o;
    }
    
    fixed4 fragBase(v2f i): COLOR
    {
        //i.viewDir = -normalize(i.worldPos.xyz - _WorldSpaceCameraPos);
        float3 col = tex2D(_MainTex, i.uv) * _MainColor;
        UnityLight light;
        i.tanToWorld[2] = normalize(i.tanToWorld[2]);
        light.color = _LightColor0.rgb * _LightColor0.a;
        light.dir = normalize(_WorldSpaceLightPos0.xyz);
        float3 normal = MYPerPixelWorldNormal(i.uv, i.tanToWorld, _NormalTex);
        //组织PBR功能参数
        half4 funcCol = tex2D(_FuncTex, i.uv);
        //metallic, smooth, AO1
        float metallic = _MetallicStr * funcCol.r;
        float smooth = _SmoothStr * funcCol.g;
        float AO = MyOcclusion(funcCol.b, _AO);
        half3 halfDir = Unity_SafeNormalize(i.viewDir + light.dir);
        half perceptualRoughness = SmoothnessToPerceptualRoughness(smooth);
        half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
        //Emission
        float3 emissionCol = tex2D(_EmissionTex, i.uv) * _EmissionColor * _EmissionStr;
        
        half ndl = saturate(dot(normal, light.dir));
        half ldh = saturate(dot(light.dir, halfDir));
        half ndh = saturate(dot(normal, halfDir));
        half ndv = saturate(dot(normal, i.viewDir));
        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz);
        GIIData data = (GIIData)0;
        data.light = light;
        data.worldPos = i.worldPos;
        data.atten = atten;
        data.occlusion = AO;
        data.ambientOrLightMapUV = i.ambientOrLightmapUV;
        data.normalWorld = normal;
        data.rough = perceptualRoughness;
        data.viewDir = i.viewDir;
        UnityGI gi = GIFunction(data);
        half diffuseTerm = ndl;
        roughness = max(roughness, 0.002);
        half V = SmithJointGGXVisibilityTerm(ndl, ndv, roughness);
        float D = GGXTerm(ndh, roughness);
        half specularTerm = V * D * UNITY_PI;
       
        #if defined(UNITY_COLORSPACE_GAMMA)
            specularTerm = sqrt(max(1e-4h, specularTerm));
        #endif
        specularTerm = max(0, specularTerm * ndl);
        
        half surfaceReduction;
        #if defined(UNITY_COLORSPACE_GAMMA)
            surfaceReduction = 1.0 - 0.28 * roughness * perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
        #else
            surfaceReduction = 1.0 / (roughness * roughness + 1.0);           // fade \in [0.5;1]
        #endif
        float3 specColor;
        float oneMinusReflectivity;
        float3 diffColor = DiffuseAndSpecularFromMetallic(col, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);
        specularTerm *= any(specColor)?1.0: 0;
        
        half grazingTerm = saturate(smooth + (1 - oneMinusReflectivity));
        
        half3 directDiffuse = diffColor * light.color * diffuseTerm * atten;
        half3 indirectDiffuse = diffColor * gi.indirect.diffuse * _AmbientStr;
        half3 directSpecular = specularTerm * light.color * FresnelTerm(specColor, ldh) * atten;
        half3 indirectSpecular = surfaceReduction * gi.indirect.specular * (FresnelLerp(specColor, grazingTerm * _FresnelStr * _FresnelColor, ndv)) + emissionCol;
        half3 color = directDiffuse + indirectDiffuse ;// indirectSpecular + directSpecular;
        //return half4(i.viewDir, 1); 
        return half4(MY_ShadeSH9(half4(i.normal, 1)), 1); 
    }
    
    
#endif