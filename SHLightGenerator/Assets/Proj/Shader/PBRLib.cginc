#ifndef PBR_LIB_INCLUDE
    #define PBR_LIB_INCLUDE
    #include"PBRStruct.cginc"
    half4 my_SHAr;
    half4 my_SHAg;
    half4 my_SHAb;
    half4 my_SHBr;
    half4 my_SHBg;
    half4 my_SHBb;
    half4 my_SHC;
    float4 _SkyBox_HDR;
    float _EnvRotate;
    float _ReflectStrength;
    samplerCUBE _SkyBox;
half3 MY_SHEvalLinearL0L1(half4 normal)
    {
        half3 x;
        
        // Linear (L1) + constant (L0) polynomial terms
        x.r = dot(my_SHAr, normal);
        x.g = dot(my_SHAg, normal);
        x.b = dot(my_SHAb, normal);
        //x.r = my_SHAr.w; 
        return x;
    }
    
    // normal should be normalized, w=1.0
    half3 MY_SHEvalLinearL2(half4 normal)
    {
        half3 x1, x2;
        // 4 of the quadratic (L2) polynomials
        half4 vB = normal.xyzz * normal.yzzx;
        x1.r = dot(my_SHBr, vB);
        x1.g = dot(my_SHBg, vB);
        x1.b = dot(my_SHBb, vB);
        
        // Final (5th) quadratic (L2) polynomial
        half vC = normal.x * normal.x - normal.y * normal.y;
        x2 = my_SHC.rgb * vC;
        
        return x1 + x2;
    }
    
    half3 MY_ShadeSH9(half4 normal)
    {
        // Linear + constant polynomial terms
        half3 res = MY_SHEvalLinearL0L1(normal);
        
        // Quadratic polynomials
        res += MY_SHEvalLinearL2(normal);
        
        #ifdef UNITY_COLORSPACE_GAMMA
            res = LinearToGammaSpace(res);
        #endif
        //return half3(0,res.g,0);
        return res;
    }

    float3 RotateAroundYInDegrees(float3 vertex, float degrees)
    {
        float alpha = degrees * UNITY_PI / 180.0;
        float sina, cosa;
        sincos(alpha, sina, cosa);
        return float3(vertex.x * cosa + vertex.z * sina, -sina * vertex.x + cosa * vertex.z, vertex.y).xzy;
    }
    inline half3 GlossyEnvironment(samplerCUBE tex, half4 hdr, Unity_GlossyEnvironmentData glossIn)
    {
        half perceptualRoughness = glossIn.roughness /* perceptualRoughness */ ;
        half mip = perceptualRoughnessToMipmapLevel(perceptualRoughness);
        half3 R = RotateAroundYInDegrees(glossIn.reflUVW, _EnvRotate);
        half4 rgbm = texCUBElod(tex, half4(R, mip));
        return DecodeHDR(rgbm, _SkyBox_HDR) * _ReflectStrength;
    }
    
    half3 IndirectSpecular(UnityGIInput data, half occlusion, Unity_GlossyEnvironmentData glossIn)
    {
        half3 specular = half3(1,1,1);
        
        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
            // we will tweak reflUVW in glossIn directly (as we pass it to Unity_GlossyEnvironment twice for probe0 and probe1), so keep original to pass into BoxProjectedCubemapDirection
            half3 originalReflUVW = glossIn.reflUVW;
            glossIn.reflUVW = BoxProjectedCubemapDirection(originalReflUVW, data.worldPos, data.probePosition[0], data.boxMin[0], data.boxMax[0]);
        #endif
        
        #ifdef _GLOSSYREFLECTIONS_OFF
            specular = unity_IndirectSpecColor.rgb;
        #else
            half3 env0 = GlossyEnvironment(_SkyBox, data.probeHDR[0], glossIn);
            specular = env0;
        #endif
        
        return specular;
    }
    half3 My_ShadeSHPerPixel(half3 normal, half3 ambient, float3 worldPos)
    {
        half3 ambient_contrib = 0.0;
                                                
        #if UNITY_SAMPLE_FULL_SH_PER_PIXEL
                                                    // Completely per-pixel
            #if UNITY_LIGHT_PROBE_PROXY_VOLUME
                if (unity_ProbeVolumeParams.x == 1.0)
                    ambient_contrib = SHEvalLinearL0L1_SampleProbeVolume(half4(normal, 1.0), worldPos);
                else
                    ambient_contrib = MY_SHEvalLinearL0L1(half4(normal, 1.0));
            #else
                ambient_contrib = MY_SHEvalLinearL0L1(half4(normal, 1.0));
            #endif
                                                    
        ambient_contrib += MY_SHEvalLinearL2(half4(normal, 1.0));
                                                    
        ambient += max(half3(0, 0, 0), ambient_contrib);
                                                    
        #ifdef UNITY_COLORSPACE_GAMMA
            ambient = LinearToGammaSpace(ambient);
        #endif
        #elif(SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
            // Completely per-vertex
            // nothing to do here. Gamma conversion on ambient from SH takes place in the vertex shader, see ShadeSHPerVertex.
        #else
            // L2 per-vertex, L0..L1 & gamma-correction per-pixel
            // Ambient in this case is expected to be always Linear, see ShadeSHPerVertex()
        #if UNITY_LIGHT_PROBE_PROXY_VOLUME
            if (unity_ProbeVolumeParams.x == 1.0)
                ambient_contrib = SHEvalLinearL0L1_SampleProbeVolume(half4(normal, 1.0), worldPos);
            else
                ambient_contrib = MY_SHEvalLinearL0L1(half4(normal, 1.0));
        #else
            ambient_contrib = MY_SHEvalLinearL0L1(half4(normal, 1.0));
        #endif
                                                    
        ambient = max(half3(0, 0, 0), ambient + ambient_contrib);     // include L2 contribution in vertex shader before clamp.
        #ifdef UNITY_COLORSPACE_GAMMA
            ambient = LinearToGammaSpace(ambient);
        #endif
        #endif
                                                
        return ambient;
    }
    inline UnityGI GI_Base(UnityGIInput data, half occlusion, half3 normalWorld)
    {
        UnityGI o_gi;
        ResetUnityGI(o_gi);
        
        // Base pass with Lightmap support is responsible for handling ShadowMask / blending here for performance reason
        #if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
            half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
            float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
            float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
            data.atten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
        #endif
        
        o_gi.light = data.light;
        o_gi.light.color *= data.atten;
        
        #if UNITY_SHOULD_SAMPLE_SH
            o_gi.indirect.diffuse = My_ShadeSHPerPixel(normalWorld, data.ambient, data.worldPos);
        #endif
        
        #if defined(LIGHTMAP_ON)
            // Baked lightmaps
            half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, data.lightmapUV.xy);
            half3 bakedColor = DecodeLightmap(bakedColorTex);
            
            #ifdef DIRLIGHTMAP_COMBINED
                fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);
                o_gi.indirect.diffuse += DecodeDirectionalLightmap(bakedColor, bakedDirTex, normalWorld);
                
                #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
                    ResetUnityLight(o_gi.light);
                    o_gi.indirect.diffuse = SubtractMainLightWithRealtimeAttenuationFromLightmap(o_gi.indirect.diffuse, data.atten, bakedColorTex, normalWorld);
                #endif
                
            #else // not directional lightmap
                o_gi.indirect.diffuse += bakedColor;
                
                #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN)
                    ResetUnityLight(o_gi.light);
                    o_gi.indirect.diffuse = SubtractMainLightWithRealtimeAttenuationFromLightmap(o_gi.indirect.diffuse, data.atten, bakedColorTex, normalWorld);
                #endif
                
            #endif
        #endif
        
        #ifdef DYNAMICLIGHTMAP_ON
            // Dynamic lightmaps
            fixed4 realtimeColorTex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, data.lightmapUV.zw);
            half3 realtimeColor = DecodeRealtimeLightmap(realtimeColorTex);
            
            #ifdef DIRLIGHTMAP_COMBINED
                half4 realtimeDirTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_DynamicDirectionality, unity_DynamicLightmap, data.lightmapUV.zw);
                o_gi.indirect.diffuse += DecodeDirectionalLightmap(realtimeColor, realtimeDirTex, normalWorld);
            #else
                o_gi.indirect.diffuse += realtimeColor;
            #endif
        #endif
        
        o_gi.indirect.diffuse *= occlusion;
        return o_gi;
    }
    
    inline UnityGI GlobalIllumination(UnityGIInput data, half occlusion, half3 normalWorld, Unity_GlossyEnvironmentData glossIn)
    {
        UnityGI o_gi = GI_Base(data, occlusion, normalWorld);
        o_gi.indirect.specular = IndirectSpecular(data, occlusion, glossIn);
        return o_gi;
    }
    
    inline UnityGI GIFunction(GIIData inData)
    {
        UnityGIInput d;
        d.light = inData.light;
        d.worldPos = inData.worldPos;
        d.worldViewDir = inData.viewDir;
        d.atten = inData.atten;
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            d.ambient = 0;
            d.lightmapUV = inData.ambientOrLightMapUV;
        #else
            d.ambient = inData.ambientOrLightMapUV.rgb;
            d.lightmapUV = 0;
        #endif
        
        d.probeHDR[0] = unity_SpecCube0_HDR;
        d.probeHDR[1] = unity_SpecCube1_HDR;
        #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
            d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
        #endif
        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
            d.boxMax[0] = unity_SpecCube0_BoxMax;
            d.probePosition[0] = unity_SpecCube0_ProbePosition;
            d.boxMax[1] = unity_SpecCube1_BoxMax;
            d.boxMin[1] = unity_SpecCube1_BoxMin;
            d.probePosition[1] = unity_SpecCube1_ProbePosition;
        #endif
        
        Unity_GlossyEnvironmentData glossData;
        glossData.roughness = inData.rough;
        glossData.reflUVW = reflect(-inData.viewDir, inData.normalWorld);
        return GlobalIllumination(d, inData.occlusion, inData.normalWorld, glossData);
    }
    
    
    inline half MyOcclusion(float occ, float occStr)
    {
        #if (SHADER_TARGET < 30)
            // SM20: instruction count limitation
            // SM20: simpler occlusion
            return occ;
        #else
            return LerpOneTo(occ, occStr);
        #endif
    }
    float3 NormalizePerPixelNormal(float3 n)
    {
        #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
            return n;
        #else
            return normalize((float3)n); // takes float to avoid overflow
        #endif
    }
    half3 NormalizePerVertexNormal(float3 n) // takes float to avoid overflow
    {
        #if (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
            return normalize(n);
        #else
            return n; // will normalize per-pixel instead
        #endif
    }
    inline float3 MYPerPixelWorldNormal(float2 texcoords, float3 tangentToWorld[3], sampler2D normalT)
    {
        
        half3 tangent = tangentToWorld[0].xyz;
        half3 binormal = tangentToWorld[1].xyz;
        half3 normal = tangentToWorld[2].xyz;
        
        #if UNITY_TANGENT_ORTHONORMALIZE
            normal = NormalizePerPixelNormal(normal);
            // ortho-normalize Tangent
            tangent = normalize(tangent - normal * dot(tangent, normal));
            // recalculate Binormal
            half3 newB = cross(normal, tangent);
            binormal = newB * sign(dot(newB, binormal));
        #endif
        
        half3 normalTangent = UnpackNormal(tex2D(normalT, texcoords));
        float3 normalWorld = NormalizePerPixelNormal(tangent * normalTangent.x + binormal * normalTangent.y + normal * normalTangent.z); // @TODO: see if we can squeeze this normalize on SM2.0 as well
        
        return normalWorld;
    }
    
    
    
    
    
    
    half3 MY_ShadeSHPerVertex(half3 normal, half3 ambient)
    {
        #if UNITY_SAMPLE_FULL_SH_PER_PIXEL
            // Completely per-pixel
            // nothing to do here
        #elif (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
            // Completely per-vertex
            ambient += max(half3(0, 0, 0), MY_ShadeSH9(half4(normal, 1.0)));
        #else
            // L2 per-vertex, L0..L1 & gamma-correction per-pixel
            
            // NOTE: SH data is always in Linear AND calculation is split between vertex & pixel
            // Convert ambient to Linear and do final gamma-correction at the end (per-pixel)
            #ifdef UNITY_COLORSPACE_GAMMA
                ambient = GammaToLinearSpace(ambient);
            #endif
            ambient += MY_SHEvalLinearL2(half4(normal, 1.0));     // no max since this is only L2 contribution
        #endif
        
        return ambient;
    }
    
    inline half4 MY_VertexGIForward(appdata v, float3 posWorld, half3 normalWorld)
    {
        half4 ambientOrLightmapUV = 0;
        // Static lightmaps
        #ifdef LIGHTMAP_ON
            ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            ambientOrLightmapUV.zw = 0;
            // Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
        #elif UNITY_SHOULD_SAMPLE_SH
            #ifdef VERTEXLIGHT_ON
                // Approximated illumination from non-important point lights
                ambientOrLightmapUV.rgb = Shade4PointLights(
                    unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                    unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                    unity_4LightAtten0, posWorld, normalWorld);
                    #endif
                    
                    ambientOrLightmapUV.rgb = MY_ShadeSHPerVertex(normalWorld, ambientOrLightmapUV.rgb);
                    #endif
                    
                    #ifdef DYNAMICLIGHTMAP_ON
                        ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                        #endif
                        
                        return ambientOrLightmapUV;
                        }
                        
                        
                        
                        
                        
                        #endif