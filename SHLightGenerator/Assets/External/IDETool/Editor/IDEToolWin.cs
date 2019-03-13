/////////////////////////////////////
//Created By RCBelmont  On 2019/2/9 23:37:55
//
//Description:IDETool窗口
///////////////////////////////////

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace RCBelmont.IDETool
{
    public class IDEToolWin : EditorWindow
    {
        private Vector2 _scrollPos;

        [MenuItem("IDETool/IDEToolSetting %#i")]
        public static void GenWin()
        {
            GetWindow<IDEToolWin>().Show();
        }

        [MenuItem("Assets/OpenWithVsCode", false, 9999)]
        public static void OpenWithVsCode()
        {
            string appPath = IDEToolLib.Instance.GetVsCode();
            string filePath = AssetDatabase.GetAssetOrScenePath(Selection.activeObject);
            string projPath = Directory.GetParent(Application.dataPath).FullName;
            if (!string.IsNullOrEmpty(appPath))
            {
                Process.Start(appPath, projPath + " " + filePath);
            }
            else
            {
                Debug.LogError("Please Select VsCodePath!!!");
            }
        }

        public void Awake()
        {
            IDEToolLib.Instance.Init();
        }

        void OnDestroy()
        {
            IDEToolLib.Instance.SaveCfg();
        }

        public void OnGUI()
        {
            List<IDEToolLib.IDEInfo> dic = IDEToolLib.Instance.SettingInfoList;
            IDEToolLib.IDEInfo version = IDEToolLib.Instance.TryGetIDEInfo(IDEToolLib.VersionKey);
            GUILayout.Label("CfgVer: " + version.Path);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (IDEToolLib.IDEInfo pair in dic)
            {
                if (pair.Name != IDEToolLib.VersionKey)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal();
                    string tempName = GUILayout.TextField(pair.Name, new GUILayoutOption[] {GUILayout.Width(150)});
                    string tempPath = GUILayout.TextField(pair.Path, new GUILayoutOption[] {GUILayout.Width(400)});
                    if (EditorGUI.EndChangeCheck())
                    {
                        pair.Name = tempName;
                        pair.Path = tempPath;
                    }

                    if (GUILayout.Button("SelectEXE"))
                    {
                        string path = EditorUtility.OpenFilePanel("ChooseExe", "", "exe");
                        if (!string.IsNullOrEmpty(path))
                        {
                            pair.Path = path;
                        }
                    }

                    EditorGUI.BeginDisabledGroup(ScriptEditorUtility.GetExternalScriptEditor() == pair.Path);
                    if (GUILayout.Button("USE"))
                    {
                        ScriptEditorUtility.SetExternalScriptEditor(pair.Path);
                    }

                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Add New IDE"))
            {
                IDEToolLib.IDEInfo newInfo = new IDEToolLib.IDEInfo("NewIDE", "");
                dic.Add(newInfo);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}