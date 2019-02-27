using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RCBelmont.SHLightGenerator
{
    public class SHLightGeneratorLib:Editor
    {
        public static SHData GenSHBySkyBox(Cubemap cubeTex, float rotate)
        {
            AmbientMode oMode = RenderSettings.ambientMode;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            Material mSky = new Material(Shader.Find("Skybox/Cubemap"));
            mSky.SetTexture("_Tex", cubeTex);
            mSky.SetFloat("_Rotation", rotate);
            Material oldSky = RenderSettings.skybox;
            RenderSettings.skybox = mSky;
            DynamicGI.UpdateEnvironment();
            SphericalHarmonicsL2 sh = new SphericalHarmonicsL2();
            LightProbes.GetInterpolatedProbe(Vector3.zero, new Renderer(),  out sh);
            SHData retData = CreateInstance<SHData>();
            retData.Init(sh);
            RenderSettings.skybox = oldSky;
            RenderSettings.ambientMode = oMode;
            DynamicGI.UpdateEnvironment();
           
            DestroyImmediate(mSky);
            return retData;
        }

        public static SHData GenSHByTriLight(Color sky, Color equator, Color ground)
        {
            Color oSkyColor, oEquatorColor, oGroundColor;
            AmbientMode oMode = RenderSettings.ambientMode;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            oSkyColor = RenderSettings.ambientSkyColor;
            oEquatorColor = RenderSettings.ambientEquatorColor;
            oGroundColor = RenderSettings.ambientGroundColor;
            RenderSettings.ambientSkyColor = sky;
            RenderSettings.ambientEquatorColor = equator;
            RenderSettings.ambientGroundColor = ground;
            DynamicGI.UpdateEnvironment();
            SphericalHarmonicsL2 sh = new SphericalHarmonicsL2();
            LightProbes.GetInterpolatedProbe(Vector3.zero, new Renderer(),  out sh);
            SHData retData = CreateInstance<SHData>();
            retData.Init(sh);
            RenderSettings.ambientMode = oMode;
            RenderSettings.ambientSkyColor = oSkyColor;
            RenderSettings.ambientEquatorColor = oEquatorColor;
            RenderSettings.ambientGroundColor = oGroundColor;
            DynamicGI.UpdateEnvironment();
           
            return retData;


        }
    }
}