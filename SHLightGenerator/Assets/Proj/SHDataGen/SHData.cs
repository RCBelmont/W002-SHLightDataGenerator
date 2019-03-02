//============================
//Created By RCBelmont on 2019年2月27日
//Description SH数据类
//============================


using UnityEngine;
using UnityEngine.Rendering;

namespace RCBelmont.SHLightGenerator
{
    public class SHData:ScriptableObject
    {
       
        public Vector4 SHAr;
        public Vector4 SHAg;
        public Vector4 SHAb;
        public Vector4 SHBr;
        public Vector4 SHBg;
        public Vector4 SHBb;
        public Vector4 SHC;

        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="shRaw">原始数据</param>
        public void Init(SphericalHarmonicsL2 shRaw)
        {
            SHAr = new Vector4(shRaw[0, 3], shRaw[0, 1], shRaw[0, 2], shRaw[0, 0]);
            SHAg = new Vector4(shRaw[1, 3], shRaw[1, 1], shRaw[1, 2], shRaw[1, 0]);
            SHAb = new Vector4(shRaw[2, 3], shRaw[2, 1], shRaw[2, 2], shRaw[2, 0]);
            SHBr = new Vector4(shRaw[0, 4], shRaw[0, 5], shRaw[0, 6] * 3, shRaw[0, 7]);
            SHBg = new Vector4(shRaw[1, 4], shRaw[1, 5], shRaw[1, 6] * 3, shRaw[1, 7]);
            SHBb = new Vector4(shRaw[2, 4], shRaw[2, 5], shRaw[2, 6] * 3, shRaw[2, 7]);
            SHC = new Vector4(shRaw[0, 8], shRaw[1, 8], shRaw[2, 8], 1);
        }
        
        /// <summary>
        /// 打印SH数据
        /// </summary>
        public void PrintData()
        {
            Debug.Log("SHAr: " + SHAr);
            Debug.Log("SHAg: " + SHAg);
            Debug.Log("SHAb: " + SHAb);
            Debug.Log("SHBr: " + SHBr);
            Debug.Log("SHBg: " + SHBg);
            Debug.Log("SHBb: " + SHBb);
            Debug.Log("SHC: " + SHC);
        }
    }
}