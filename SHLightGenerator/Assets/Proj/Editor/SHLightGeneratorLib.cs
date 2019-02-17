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
            Material mSky = new Material(Shader.Find("Skybox/Cubemap"));
            mSky.SetTexture("_Tex", cubeTex);
            mSky.SetFloat("_Rotation", rotate);
            Material oldSky = RenderSettings.skybox;
            RenderSettings.skybox = mSky;
            SphericalHarmonicsL2 sh = new SphericalHarmonicsL2();
            LightProbes.GetInterpolatedProbe(Vector3.zero, new Renderer(),  out sh);
            SHData retData = new SHData(sh);
            RenderSettings.skybox = oldSky;
            retData.PrintData();
            DestroyImmediate(mSky);
            return retData;
        }
    }
}