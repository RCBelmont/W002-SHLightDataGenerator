/////////////////////////////////////
//Created By RCBelmont  On 2019/2/10 2:10:38
//
//Description:IDETool核心功能库
///////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace RCBelmont.IDETool
{
    public class IDEToolLib
    {
        #region Const

        private const string INI_FILE_NAME = "IDEToolSetting.ini";
        private const string DATA_VER = "0.1";

        #endregion

        #region Private Variate

        private static IDEToolLib _instance;
        private static string _appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string _cfgSavePath = _appDataPath + "/IDEToolCfg";
        private static string _cfgFilePath = _appDataPath + "/IDEToolCfg/" + INI_FILE_NAME;

        #endregion

        #region Public Variate

        public static string VersionKey = "Version";

        public static IDEToolLib Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new IDEToolLib();
                }

                return _instance;
            }
        }

        public IDEToolLib()
        {
            Init();
        }
        public List<IDEInfo> SettingInfoList = new List<IDEInfo>();
        
        #endregion

        public class IDEInfo
        {
            public string Name = "";
            public string Path = "";

            public IDEInfo(string name, string path)
            {
                Path = path;
                Name = name;
            }
        }

        #region Public Method

        /// <summary>
        /// Initialize
        /// </summary>
        public void Init()
        {
            CheckFile();
            FileStream fs = File.Open(_cfgFilePath, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string line;
            SettingInfoList = new List<IDEInfo>();
            while ((line = sr.ReadLine()) != null)
            {
                string[] sL = line.Split(new char[] {'='}, 2);
               
                if (sL.Length == 2)
                {
                    IDEInfo info = new IDEInfo(sL[0], sL[1]);
                    SettingInfoList.Add(info);
                }
            }

      
            if (TryGetIDEInfo(VersionKey) == null)
            {
                
                SettingInfoList = new List<IDEInfo>();
                SettingInfoList.Add(new IDEInfo(VersionKey, DATA_VER));
            }

            sr.Close();
            fs.Close();
        }

        public void SaveCfg()
        {
            CheckFile();
        
            FileStream fs = File.Open(_cfgFilePath, FileMode.Truncate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            foreach (IDEInfo pair in SettingInfoList)
            {
                if (!string.IsNullOrEmpty(pair.Name) && !string.IsNullOrEmpty(pair.Path))
                {
                  
                    sw.WriteLine(pair.Name.Replace("=", "") + "=" + pair.Path);
                }
            }

            sw.Close();
            fs.Close();
        }
        public IDEInfo TryGetIDEInfo(string key)
        {
            foreach (IDEInfo info in SettingInfoList)
            {
                if (info.Name.Equals(key))
                {
                    return info;
                }
            }

            return null;
        }

        public string GetVsCode()
        {
            foreach (IDEInfo info in SettingInfoList)
            {
                if (info.Path.Contains("Code.exe"))
                {
                    return info.Path;
                }
            }

            return null;
        }
        #endregion

        #region Private Method
        private void CheckFile()
        {
            if (!Directory.Exists(_cfgSavePath))
            {
                Directory.CreateDirectory(_cfgSavePath);
            }

            if (!File.Exists(_cfgFilePath))
            {
                File.Create(_cfgFilePath).Dispose();
            }
        }

        #endregion
    }
}