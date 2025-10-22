﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.Diagnostics;

namespace SimpleZoom
{

    [BepInPlugin("com.cyfral.simplezoom", "Simple Zoom", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {

        private void Awake()
        {
            ZoomConfig.Init(this);
            var harmony = new Harmony("com.cyfral.SimpleZoom");
            harmony.PatchAll();
        }
    }
}