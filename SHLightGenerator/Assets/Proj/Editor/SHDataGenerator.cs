using System;
using System.Collections;
using System.Collections.Generic;
using RCBelmont.SHLightGenerator;
using UnityEditor;
using UnityEngine;

public class SHDataGenerator : EditorWindow
{
    private Cubemap _map;
    private enum AmbientType
    {
        SkyBox,
        Trilight
    }

    private AmbientType _aType = AmbientType.SkyBox;
    [MenuItem("Toos/SHLightDataGenerator")]
    public static void OpenWindow()
    {
        EditorWindow win = GetWindow<SHDataGenerator>();
        win.minSize = new Vector2(300, 500);
        win.minSize = new Vector2(300, 500);
        win.name = "SHLightGenerator";
        win.Show();
    }

    
    void OnGUI()
    {
        _aType = (AmbientType)EditorGUILayout.EnumPopup("AmbientType", _aType);
        switch (_aType)
        {
            
        }
        
        _map = (Cubemap) EditorGUILayout.ObjectField("EnvTex: ", _map, typeof(Cubemap), false);
        if (GUILayout.Button("Generate!"))
        {
            SHLightGeneratorLib.GenSHBySkyBox(_map, 0);
        }
    }
}