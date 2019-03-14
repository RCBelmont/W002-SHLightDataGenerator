//Generate By Code

using System;
using System.Collections;
using System.Collections.Generic;
using RCBelmont.SHLightGenerator;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

internal class PBRBase_Editor : ShaderGUI
{
    private enum EmbientType
    {
        SkyBox,
        Gradient
    }

    private EmbientType _embientType = EmbientType.SkyBox;

    override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        //Property0: _MainTex
        MaterialProperty _MainTex = FindProperty("_MainTex", props);
        materialEditor.TexturePropertySingleLine(new GUIContent("主纹理"), _MainTex);
        if ((_MainTex.flags & MaterialProperty.PropFlags.NoScaleOffset) == 0)
        {
            materialEditor.TextureScaleOffsetProperty(_MainTex);
        }

        //Property1: _MainColor
        MaterialProperty _MainColor = FindProperty("_MainColor", props);
        materialEditor.ColorProperty(_MainColor, "主纹理颜色Tint");

        //Property2: _NormalTex
        MaterialProperty _NormalTex = FindProperty("_NormalTex", props);
        materialEditor.TexturePropertySingleLine(new GUIContent("法线纹理"), _NormalTex);
        if ((_NormalTex.flags & MaterialProperty.PropFlags.NoScaleOffset) == 0)
        {
            materialEditor.TextureScaleOffsetProperty(_NormalTex);
        }

        //Property3: _FuncTex
        MaterialProperty _FuncTex = FindProperty("_FuncTex", props);
        materialEditor.TexturePropertySingleLine(new GUIContent("功能纹理(金属度, 光滑度, AO)"), _FuncTex);
        if ((_FuncTex.flags & MaterialProperty.PropFlags.NoScaleOffset) == 0)
        {
            materialEditor.TextureScaleOffsetProperty(_FuncTex);
        }

        //Property4: _MetallicStr
        MaterialProperty _MetallicStr = FindProperty("_MetallicStr", props);
        materialEditor.RangeProperty(_MetallicStr, "金属度强度");

        //Property5: _SmoothStr
        MaterialProperty _SmoothStr = FindProperty("_SmoothStr", props);
        materialEditor.RangeProperty(_SmoothStr, "光滑度强度");

        //Property6: _AO
        MaterialProperty _AO = FindProperty("_AO", props);
        materialEditor.RangeProperty(_AO, "环境光遮蔽");

        //Property7: _EmissionTex
        MaterialProperty _EmissionTex = FindProperty("_EmissionTex", props);
        materialEditor.TexturePropertySingleLine(new GUIContent("自发光贴图"), _EmissionTex);
        if ((_EmissionTex.flags & MaterialProperty.PropFlags.NoScaleOffset) == 0)
        {
            materialEditor.TextureScaleOffsetProperty(_EmissionTex);
        }

        //Property8: _EmissionColor
        MaterialProperty _EmissionColor = FindProperty("_EmissionColor", props);
        materialEditor.ColorProperty(_EmissionColor, "自发光颜色");

        //Property9: _EmissionStr
        MaterialProperty _EmissionStr = FindProperty("_EmissionStr", props);
        materialEditor.RangeProperty(_EmissionStr, "自发光强度");

        _embientType = (EmbientType) EditorGUILayout.EnumPopup("环境光类型", _embientType);

        EditorGUI.BeginChangeCheck();
        switch (_embientType)
        {
            case EmbientType.SkyBox:
                //Property10: _SkyBox
                MaterialProperty _SkyBox = FindProperty("_SkyBox", props);
                materialEditor.TextureProperty(_SkyBox,"环境光贴图");
                if ((_SkyBox.flags & MaterialProperty.PropFlags.NoScaleOffset) == 0)
                {
                    materialEditor.TextureScaleOffsetProperty(_SkyBox);
                }

                //Property11: _EnvRotate
                MaterialProperty _EnvRotate = FindProperty("_EnvRotate", props);
                materialEditor.RangeProperty(_EnvRotate, "环境贴图旋转");
                break;
            case EmbientType.Gradient:
                //Property12: _AmbientSky
                MaterialProperty _AmbientSky = FindProperty("_AmbientSky", props);
                materialEditor.ColorProperty(_AmbientSky, "天空颜色");
                //Property13: _AmbientEquator
                MaterialProperty _AmbientEquator = FindProperty("_AmbientEquator", props);
                materialEditor.ColorProperty(_AmbientEquator, "地平线颜色");
                //Property14: _AmbientGround
                MaterialProperty _AmbientGround = FindProperty("_AmbientGround", props);
                materialEditor.ColorProperty(_AmbientGround, "地面颜色");
                break;
        }

        Material m = materialEditor.target as Material;
        if (EditorGUI.EndChangeCheck())
        {
            UpdateSHData(props, m);
        }

        if (GUILayout.Button("UpdateAmbient"))
        {
            UpdateSHData(props, m);
        }


        //Property15: _AmbientStr
        MaterialProperty _AmbientStr = FindProperty("_AmbientStr", props);
        materialEditor.RangeProperty(_AmbientStr, "环境光强度");

        //Property16: _ReflectStrength
        MaterialProperty _ReflectStrength = FindProperty("_ReflectStrength", props);
        materialEditor.RangeProperty(_ReflectStrength, "反射强度");

        //Property17: _FresnelStr
        MaterialProperty _FresnelStr = FindProperty("_FresnelStr", props);
        materialEditor.RangeProperty(_FresnelStr, "菲涅尔强度");

        //Property18: _FresnelColor
        MaterialProperty _FresnelColor = FindProperty("_FresnelColor", props);
        materialEditor.ColorProperty(_FresnelColor, "菲涅尔颜色");
        
        MaterialProperty my_SHAr = FindProperty("my_SHAr", props);
        materialEditor.VectorProperty(my_SHAr, "my_SHAr");
        MaterialProperty my_SHAg = FindProperty("my_SHAg", props);
        materialEditor.VectorProperty(my_SHAg, "my_SHAg");
        MaterialProperty my_SHAb = FindProperty("my_SHAb", props);
        materialEditor.VectorProperty(my_SHAb, "my_SHAb");
        MaterialProperty my_SHBr = FindProperty("my_SHBr", props);
        materialEditor.VectorProperty(my_SHBr, "my_SHBr");
        MaterialProperty my_SHBg = FindProperty("my_SHBg", props);
        materialEditor.VectorProperty(my_SHBg, "my_SHBg");
        MaterialProperty my_SHBb = FindProperty("my_SHBb", props);
        materialEditor.VectorProperty(my_SHBb, "my_SHBb");
        MaterialProperty my_SHC = FindProperty("my_SHC", props);
        materialEditor.VectorProperty(my_SHC, "my_SHC");
    }

    private void UpdateSHData(MaterialProperty[] props, Material m)
    {
        SHData data = null;
        switch (_embientType)
        {
            case EmbientType.SkyBox:
                MaterialProperty _SkyBox = FindProperty("_SkyBox", props);
                MaterialProperty _EnvRotate = FindProperty("_EnvRotate", props);
                data = SHLightGeneratorLib.GenSHBySkyBox(_SkyBox.textureValue, _EnvRotate.floatValue);
                break;
            case EmbientType.Gradient:
                MaterialProperty _AmbientSky = FindProperty("_AmbientSky", props);
                MaterialProperty _AmbientEquator = FindProperty("_AmbientEquator", props);
                MaterialProperty _AmbientGround = FindProperty("_AmbientGround", props);
                data = SHLightGeneratorLib.GenSHByTriLight(_AmbientSky.colorValue, _AmbientEquator.colorValue,
                    _AmbientGround.colorValue);
                break;
        }

        if (data != null)
        {
            m.SetVector("my_SHAr", data.SHAr);
            m.SetVector("my_SHAg", data.SHAg);
            m.SetVector("my_SHAb", data.SHAb);
            m.SetVector("my_SHBr", data.SHBr);
            m.SetVector("my_SHBg", data.SHBg);
            m.SetVector("my_SHBb", data.SHBb);
            m.SetVector("my_SHC", data.SHC);
        }
    }
}