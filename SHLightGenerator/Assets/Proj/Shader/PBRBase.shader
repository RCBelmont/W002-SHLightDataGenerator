Shader "Unlit/PBRBase"
{
    Properties
    {
        _MainTex ("主纹理", 2D) = "white" { }
        _MainColor ("主纹理颜色Tint", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_NormalTex ("法线纹理", 2D) = "bump" { }
        [NoScaleOffset] _FuncTex ("功能纹理(金属度, 光滑度, AO)", 2D) = "white" { }
        _MetallicStr ("金属度强度", Range(0, 1)) = 0.5
        _SmoothStr ("光滑度强度", Range(0, 1)) = 0.5
        _AO ("环境光遮蔽", Range(0, 1)) = 1
        [NoScaleOffset]_EmissionTex ("自发光贴图", 2D) = "white" { }
        _EmissionColor ("自发光颜色", Color) = (1, 1, 1, 1)
        _EmissionStr ("自发光强度", Range(0, 1)) = 0
        //For SHDataGenerator https://gitlab.com/RCBelmont-WorksForUnity/w002-shlightdatagenerator
        [Header(Either CubeMap or Gradient)]
        [NoScaleOffset]_SkyBox ("环境光贴图", CUBE) = "" { }
        _EnvRotate ("环境贴图旋转", Range(0, 360)) = 0
        _AmbientSky ("天空颜色", Color) = (0, 0, 0, 0)
        _AmbientEquator ("地平线颜色", Color) = (0, 0, 0, 0)
        _AmbientGround ("地面颜色", Color) = (0, 0, 0, 0)
        _AmbientStr ("环境光强度", Range(0, 2)) = 1
        _ReflectStrength ("反射强度", Range(0, 2)) = 1
        _FresnelStr ("菲涅尔强度", Range(0, 1)) = 1
        _FresnelColor ("菲涅尔颜色", Color) = (1, 1, 1, 1)
        
        [HideInInspector] my_SHAr ("SHAr", Vector) = (0, 0, 0, 0)
        [HideInInspector] my_SHAg ("SHAg", Vector) = (0, 0, 0, 0)
        [HideInInspector] my_SHAb ("SHAb", Vector) = (0, 0, 0, 0)
        [HideInInspector] my_SHBr ("SHBr", Vector) = (0, 0, 0, 0)
        [HideInInspector] my_SHBg ("SHBg", Vector) = (0, 0, 0, 0)
        [HideInInspector] my_SHBb ("SHBb", Vector) = (0, 0, 0, 0)
        [HideInInspector] my_SHC ("SHC", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment fragBase
            #include"PBRFunc.cginc"
            #pragma multi_compile_fwdbase
            ENDCG
            
        }
    }
    Fallback"Diffuse"
    CustomEditor"PBRBase_Editor"
}
