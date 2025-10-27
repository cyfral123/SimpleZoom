﻿using BepInEx;
using HarmonyLib;

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
