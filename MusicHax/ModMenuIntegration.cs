﻿using System.Collections.Generic;
using UnityEngine;
using LLModMenu;
using System.IO;
using System;

namespace MusicHax
{
    public class ModMenuIntegration : MonoBehaviour
    {
        private ModMenu mm;
        private bool mmAdded = false;

        public Dictionary<string, string> configKeys = new Dictionary<string, string>();
        public Dictionary<string, string> configBools = new Dictionary<string, string>();
        public Dictionary<string, string> configInts = new Dictionary<string, string>();
        public Dictionary<string, string> configSliders = new Dictionary<string, string>();
        public Dictionary<string, string> configHeaders = new Dictionary<string, string>();
        public Dictionary<string, string> configGaps = new Dictionary<string, string>();
        public Dictionary<string, string> configText = new Dictionary<string, string>();
        public List<string> writeQueue = new List<string>();

        private void Start()
        {
            InitConfig();
            ReadIni();
        }

        private void Update()
        {
            mm = FindObjectOfType<ModMenu>();
            if (mm != null)
            {
                if (mmAdded == false)
                {
                    mm.mods.Add(base.gameObject.name);
                    mmAdded = true;
                }
            }
        }

        public void InitConfig()
        {
            /*
             * Mod menu now uses a single function to add options etc. (AddToWriteQueue)
             * your specified options should be added to this function in the same format as stated under
             * 
            Keybindings:
            AddToWriteQueue("(key)keyName", "LeftShift");                                       value can be: Any KeyCode as a string e.g. "LeftShift"
            Options:
            AddToWriteQueue("(bool)boolName", "true");                                          value can be: ["true" | "false"]
            AddToWriteQueue("(int)intName", "27313");                                           value can be: any number as a string. For instance "123334"
            AddToWriteQueue("(slider)sliderName", "50|0|100");                                  value must be: "Default value|Min Value|MaxValue"
            AddToWriteQueue("(header)headerName", "Header Text");                               value can be: Any string
            AddToWriteQueue("(gap)gapName", "identifier");                                      value does not matter, just make name and value unique from other gaps
            ModInformation:
            AddToWriteQueue("(text)text1", "Descriptive text");                                  value can be: Any string
            */


            // Insert your options here \/
            AddToWriteQueue("(bool)allowVanillaMusicToRandomlyPlay", "false");
            AddToWriteQueue("(bool)enablePreloading", "true");

            AddToWriteQueue("(text)text1", "Set the above option to True if you'd like vanilla music to be in your song list");
            AddToWriteQueue("(text)text2", "Preloading can be very long if you have lots of musics, disabling it also allows for hot-swapping");
            ModMenu.Instance.WriteIni(
                gameObject.name,
                writeQueue,
                configKeys,
                configBools,
                configInts,
                configSliders,
                configHeaders,
                configGaps,
                configText
            );
            writeQueue.Clear();
        }

        public void ReadIni()
        {
            string[] lines = File.ReadAllLines(Directory.GetParent(Application.dataPath).FullName + @"\ModSettings\" + gameObject.name + ".ini");
            configBools.Clear();
            configKeys.Clear();
            configInts.Clear();
            configSliders.Clear();
            configHeaders.Clear();
            configGaps.Clear();
            configText.Clear();
            foreach (string line in lines)
            {
                if (line.StartsWith("(key)"))
                {
                    string[] split = line.Split('=');
                    configKeys.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(bool)"))
                {
                    string[] split = line.Split('=');
                    configBools.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(int)"))
                {
                    string[] split = line.Split('=');
                    configInts.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(slider)"))
                {
                    string[] split = line.Split('=');
                    configSliders.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(header)"))
                {
                    string[] split = line.Split('=');
                    configHeaders.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(gap)"))
                {
                    string[] split = line.Split('=');
                    configGaps.Add(split[0], split[1]);
                }
                else if (line.StartsWith("(text)"))
                {
                    string[] split = line.Split('=');
                    configText.Add(split[0], split[1]);
                }
            }
        }

        public void AddToWriteQueue(string key, string value)
        {
            if (key.StartsWith("(key)"))
            {
                configKeys.Add(key, value);
                writeQueue.Add(key);
            }
            else if (key.StartsWith("(bool)"))
            {
                configBools.Add(key, value);
                writeQueue.Add(key);
            }
            else if (key.StartsWith("(int)"))
            {
                configInts.Add(key, value);
                writeQueue.Add(key);
            }
            else if (key.StartsWith("(slider)"))
            {
                configSliders.Add(key, value);
                writeQueue.Add(key);
            }
            else if (key.StartsWith("(header)"))
            {
                configHeaders.Add(key, value);
                writeQueue.Add(key);
            }
            else if (key.StartsWith("(gap)"))
            {
                configGaps.Add(key, value);
                writeQueue.Add(key);
            }
            else if (key.StartsWith("(text)"))
            {
                configText.Add(key, value);
                writeQueue.Add(key);
            }
        }

        public KeyCode GetKeyCode(string keyCode)
        {
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (vKey.ToString() == keyCode)
                {
                    return vKey;
                }
            }
            return KeyCode.A;
        }

        public bool GetTrueFalse(string boolName)
        {
            if (boolName == "true") return true;
            else return false;
        }

        public int GetSliderValue(string sliderName)
        {
            string[] vals = configSliders[sliderName].Split('|');
            return Convert.ToInt32(vals[0]);
        }

        public int GetInt(string intName)
        {
            return Convert.ToInt32(configInts[intName]);
        }
    }
}