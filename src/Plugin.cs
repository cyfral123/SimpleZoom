﻿using System.IO;
using BepInEx;
using HarmonyLib;

[BepInPlugin("simplezoom_1.1.0", "Simple Zoom", "1.1.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        File.Delete(Path.Combine(Paths.ConfigPath, "com.cyfral.simplezoom.cfg"));
        ZoomConfig.Init(this);
        var harmony = new Harmony(Info.Metadata.GUID);
        harmony.PatchAll();
    }
}
