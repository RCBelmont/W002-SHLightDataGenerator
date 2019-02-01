using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SHDataGenerator : EditorWindow
{
    private Cubemap _map;
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

    }
}
