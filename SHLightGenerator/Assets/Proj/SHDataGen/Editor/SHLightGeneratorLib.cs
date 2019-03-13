//============================
//Created By RCBelmont on 2019年3月1日
//Description SH数据生成逻辑
//============================
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RCBelmont.SHLightGenerator
{
    public class SHLightGeneratorLib:Editor
    {
        //需要检查的材质属性列表
        private static string[] _propertyList = new[]
        {
            "_SkyBox",
            "_EnvRotate",
            "_AmbientSky",
            "_AmbientGround",
            "_AmbientEquator",
            "my_SHAr",
            "my_SHAg",
            "my_SHAb",
            "my_SHBr",
            "my_SHBg",
            "my_SHBb",
            "my_SHC"
        };
        /// <summary>
        /// 通过天空盒方式生成球谐光数据
        /// </summary>
        /// <param name="cubeTex"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        public static SHData GenSHBySkyBox(Texture cubeTex, float rotate)
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
        /// <summary>
        /// 使用三色渐变模式生成球谐光数据
        /// </summary>
        /// <param name="sky"></param>
        /// <param name="equator"></param>
        /// <param name="ground"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 检查材质是否满足生成器需求
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        public static bool CheckMatLegal(Material mat)
        {
            foreach (string s in _propertyList)
            {
                if (!mat.HasProperty(s))
                {
                    return false;
                }
            }
            return true;
        }
    }
}