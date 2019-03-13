//============================
//Created By RCBelmont on 2019年3月1日
//Description SH数据生成器面板
//============================

using UnityEditor;
using UnityEngine;

namespace RCBelmont.SHLightGenerator
{
    public class SHDataGenerator : EditorWindow
    {
        private Cubemap _map;
        private Color _skyColor, _equatorColor, _groundColor;
        private SHData _shData;
        private Material _mat;
        private float _rotate;

        private enum AmbientType
        {
            SkyBox,
            TriLight
        }

        private AmbientType _aType = AmbientType.SkyBox;

        [MenuItem("Tools/SHLightDataGenerator")]
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
            _aType = (AmbientType) EditorGUILayout.EnumPopup("AmbientType", _aType);
          
            switch (_aType)
            {
                case AmbientType.SkyBox:
                    _map = (Cubemap) EditorGUILayout.ObjectField("EnvTex: ", _map, typeof(Cubemap), false);
                    _rotate = EditorGUILayout.Slider("EnvTexRotate", _rotate, 0, 360);
                    if (GUILayout.Button("Generate!"))
                    {
                        _shData = SHLightGeneratorLib.GenSHBySkyBox(_map, _rotate);
                    }

                    break;
                case AmbientType.TriLight:
                    _skyColor = EditorGUILayout.ColorField("Sky", _skyColor);
                    _equatorColor = EditorGUILayout.ColorField("Equator", _equatorColor);
                    _groundColor = EditorGUILayout.ColorField("Ground", _groundColor);
                    if (GUILayout.Button("Generate!"))
                    {
                        _shData = SHLightGeneratorLib.GenSHByTriLight(_skyColor, _equatorColor, _groundColor);
                    }

                    break;
            }

            EditorGUILayout.Space();
            if (_shData)
            {
                GUILayout.Label("SHAr: " + _shData.SHAr);
                GUILayout.Label("SHAg: " + _shData.SHAg);
                GUILayout.Label("SHAb: " + _shData.SHAb);
                GUILayout.Label("SHBr: " + _shData.SHBr);
                GUILayout.Label("SHBg: " + _shData.SHBg);
                GUILayout.Label("SHBb: " + _shData.SHBb);
                GUILayout.Label("SHC: " + _shData.SHC);
                if (GUILayout.Button("SaveSHData"))
                {
                    string path = EditorUtility.SaveFilePanel("SaveSHData", Application.dataPath, "SHData", "asset");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (!path.Contains(Application.dataPath))
                        {
                            Debug.LogError("Invalid Path");
                            return;
                        }

                        path = "Assets" + path.Replace(Application.dataPath, "");
                        AssetDatabase.CreateAsset(_shData, path);
                        Selection.activeObject = _shData;
                    }
                }
            }
        }
    }
}