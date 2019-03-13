﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class TestGen : EditorWindow
{
    [MenuItem("Tools/TestGen")]
    public static void OpenWin()
    {
        EditorWindow win = EditorWindow.GetWindow<TestGen>();
        win.Show();
    }

    private float[] coefficientL = new[]
    {
        1 / (2 * Mathf.Sqrt(Mathf.PI)), //0 0
        -Mathf.Sqrt(3 / (4 * Mathf.PI)), //1 -1
        Mathf.Sqrt(3 / (4 * Mathf.PI)), //1 0
        -Mathf.Sqrt(3 / (4 * Mathf.PI)), //1 1
        Mathf.Sqrt(15 / (4 * Mathf.PI)), //2 -2
        -Mathf.Sqrt(15 / (4 * Mathf.PI)), //2 -1
        Mathf.Sqrt(15 / (16 * Mathf.PI)), //2 0
        -Mathf.Sqrt(15 / (4 * Mathf.PI)), //2 1
        Mathf.Sqrt(15 / (16 * Mathf.PI)) //2 2
    };

    private Cubemap _cubeTex;
    private Texture2D _projTex;
    private Material _mat;

    private void OnGUI()
    {
        _cubeTex = (Cubemap) EditorGUILayout.ObjectField("C", _cubeTex, typeof(Cubemap), false);
        EditorGUILayout.ObjectField("C", _projTex, typeof(Texture2D), false);
        _mat = (Material) EditorGUILayout.ObjectField("Mat", _mat, typeof(Material), false);
        if (GUILayout.Button("test"))
        {
            float[] a = new float[9];
            for (int i = 0; i <= 1000; i++)
            {
                for (int j = 0; j <= 1000; j++)
                {
                    float dtheta = Mathf.PI / 1000;
                    float dphi = 2 * Mathf.PI / 1000;
                    float theta = dtheta * i;
                    float phi = dphi * j;
                    float dv = sin(theta) * dtheta * dphi;

                    a[0] += coefficientL[0] * coefficientL[0] * dv;
                    a[1] += coefficientL[1] * coefficientL[1] * (sin(theta) * sin(phi)) * dv;
                    a[2] += coefficientL[2] * coefficientL[2] * (sin(theta) * cos(theta)) * dv;
                    a[3] += coefficientL[3] * coefficientL[3] * cos(theta) * dv;
                }
            }

            //TODO:DELETE ADDBYZJ
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = (float) Math.Round(a[i], 3);
            }

            Debug.Log("=========>>" + (a[3]));
            Debug.Log("=========>>" + G2L(a[1]));
            Debug.Log("=========>>" + G2L(a[2]));
            Debug.Log("=========>>" + G2L(a[0]));
        }

        if (GUILayout.Button("test1"))
        {
            float[,] a = new float[3, 9];
            if (_projTex)
            {
                DestroyImmediate(_projTex);
            }

            _projTex = GetProjText(_cubeTex);
            int w = _projTex.width;
            int h = _projTex.height;

            Color[] cl = _projTex.GetPixels();
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    //Color c = cl[i * w + j];
                    Color c = _projTex.GetPixel(j, i);
                    float dtheta = Mathf.PI / h;
                    float dphi = 2 * Mathf.PI / w;
                    float theta = Mathf.PI - Mathf.PI * (i * 1.0f / h);
                    float phi = dphi * j;
                    float dv = sin(theta) * dtheta * dphi;
                    float x = sin(theta) * cos(phi);
                    float y = cos(theta);
                    float z = sin(theta) * sin(phi);
                    
                    a[0, 0] += c.r * (coefficientL[0] * coefficientL[0]) * dv; //0 0

                    a[0, 1] += c.r * coefficientL[1] * coefficientL[1] * y * dv; //1 -1
                    a[0, 2] += c.r * coefficientL[2] * coefficientL[2] * z * dv; //1 0
                    a[0, 3] += c.r * coefficientL[3] * coefficientL[3] * x * dv; //1 1

                    a[0, 4] += c.r * coefficientL[4] * coefficientL[4] * y * x * dv; //2 -2
                    a[0, 5] += c.r * coefficientL[5] * coefficientL[5] * y * z * dv; //2 -1
                    a[0, 6] += c.r * coefficientL[6] * coefficientL[6] * (3 * z * z - 1) * dv; //2 0
                    a[0, 7] += c.r * coefficientL[7] * coefficientL[7] * x * z * dv; //2 1
                    a[0, 8] += c.r * coefficientL[8] * coefficientL[8] * (x * x - y * y) * dv; //2 2
                    //-------------------------------------------------------------------------------------------------------------

                    a[1, 0] += c.g * (coefficientL[0] * coefficientL[0]) * dv; //0 0
                    a[1, 1] += c.g * coefficientL[1] * coefficientL[1] * y * dv; //1 -1
                    a[1, 2] += c.g * coefficientL[2] * coefficientL[2] * z * dv; //1 0
                    a[1, 3] += c.g * coefficientL[3] * coefficientL[3] * x * dv; //1 1

                    a[1, 4] += c.g * coefficientL[4] * coefficientL[4] * y * x * dv; //2 -2
                    a[1, 5] += c.g * coefficientL[5] * coefficientL[5] * y * z * dv; //2 -1
                    a[1, 6] += c.g * coefficientL[6] * coefficientL[6] * (3 * z * z - 1) * dv; //2 0
                    a[1, 7] += c.g * coefficientL[7] * coefficientL[7] * x * z * dv; //2 1
                    a[1, 8] += c.g * coefficientL[8] * coefficientL[8] * (x * x - y * y) * dv; //2 2
                    //-------------------------------------------------------------------------------------------

                    a[2, 0] += c.b * (coefficientL[0] * coefficientL[0]) * dv; //0 0
                    a[2, 1] += c.b * coefficientL[1] * coefficientL[1] * y * dv; //1 -1
                    a[2, 2] += c.b * coefficientL[2] * coefficientL[2] * z * dv; //1 0
                    a[2, 3] += c.b * coefficientL[3] * coefficientL[3] * x * dv; //1 1

                    a[2, 4] += c.b * coefficientL[4] * coefficientL[4] * y * x * dv; //2 -2
                    a[2, 5] += c.b * coefficientL[5] * coefficientL[5] * y * z * dv; //2 -1
                    a[2, 6] += c.b * coefficientL[6] * coefficientL[6] * (3 * z * z - 1) * dv; //2 0
                    a[2, 7] += c.b * coefficientL[7] * coefficientL[7] * x * z * dv; //2 1
                    a[2, 8] += c.b * coefficientL[8] * coefficientL[8] * (x * x - y * y) * dv; //2 2
                }
            }

            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    //a[i, j] = G2L(a[i, j]);
                }
            }

            if (_mat)
            {
                _mat.SetVector("my_SHAr", new Vector4(a[0, 3], a[0, 1], a[0, 2], a[0, 0] - a[0, 6]));
                _mat.SetVector("my_SHAg", new Vector4(a[1, 3], a[1, 1], a[1, 2], a[1, 0] - a[1, 6]));
                _mat.SetVector("my_SHAb", new Vector4(a[2, 3], a[2, 1], a[2, 2], a[2, 0] - a[2, 6]));
                _mat.SetVector("my_SHBr", new Vector4(a[0, 4], a[0, 5], a[0, 6] * 3, a[0, 7]));
                _mat.SetVector("my_SHBg", new Vector4(a[1, 4], a[1, 5], a[1, 6] * 3, a[1, 7]));
                _mat.SetVector("my_SHBb", new Vector4(a[2, 4], a[2, 5], a[2, 6] * 3, a[2, 7]));
                _mat.SetVector("my_SHC", new Vector4(a[0, 8], a[0, 8], a[0, 8], 1));
            }
        }
        
        
    }

    private Texture2D GetProjText(Cubemap cube)
    {
        Material m = new Material(Shader.Find("Hidden/CubeMapProject"));
        RenderTexture rt = RenderTexture.GetTemporary(512, 256, 0);
        Graphics.Blit(cube, rt, m);
        rt.hideFlags = HideFlags.HideAndDontSave;
        rt.name = "AAAAA";
        DestroyImmediate(m);
        Texture2D t = CopyRTToTexture2d(rt);
        rt.Release();
        return t;
    }

    private Texture2D CopyRTToTexture2d(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D ret = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        ret.ReadPixels(new Rect(0, 0, ret.width, ret.height), 0, 0);
        ret.Apply();
        RenderTexture.active = null;
        return ret;
    }

    private Vector3 GetDir(CubemapFace face, float u, float v)
    {
        Vector3 dir = Vector3.zero;
        switch (face)
        {
            case CubemapFace.PositiveX: //+X
                dir.x = 1;
                dir.y = v * -2.0f + 1.0f;
                dir.z = u * -2.0f + 1.0f;
                break;

            case CubemapFace.NegativeX: //-X
                dir.x = -1;
                dir.y = v * -2.0f + 1.0f;
                dir.z = u * 2.0f - 1.0f;
                break;

            case CubemapFace.PositiveY: //+Y
                dir.x = u * 2.0f - 1.0f;
                dir.y = 1.0f;
                dir.z = v * 2.0f - 1.0f;
                break;

            case CubemapFace.NegativeY: //-Y
                dir.x = u * 2.0f - 1.0f;
                dir.y = -1.0f;
                dir.z = v * -2.0f + 1.0f;
                break;

            case CubemapFace.PositiveZ: //+Z
                dir.x = u * 2.0f - 1.0f;
                dir.y = v * -2.0f + 1.0f;
                dir.z = 1;
                break;

            case CubemapFace.NegativeZ: //-Z
                dir.x = u * -2.0f + 1.0f;
                dir.y = v * -2.0f + 1.0f;
                dir.z = -1;
                break;
        }

        return dir.normalized;
    }

    private float sin(float rad)
    {
        return Mathf.Sin(rad);
    }

    private float cos(float rad)
    {
        return Mathf.Cos(rad);
    }

    //Gamma To Linear 
    private float G2L(float c)
    {
        return c * (c * (c * 0.305306011f + 0.682171111f) + 0.012522878f);
        //return Mathf.Pow((c + 0.055f) / 1.055f, -0.416666667f);
    }
}