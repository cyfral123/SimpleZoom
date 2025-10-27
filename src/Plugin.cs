﻿using System.IO;
using BepInEx;
using HarmonyLib;

[BepInPlugin("simplezoom_1.1.1", "Simple Zoom", "1.1.1")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        // deleting configs for old versions
        File.Delete(Path.Combine(Paths.ConfigPath, "com.cyfral.simplezoom.cfg"));
        File.Delete(Path.Combine(Paths.ConfigPath, "simplezoom_1.1.0.cfg"));

        ZoomConfig.Init(this);
        var harmony = new Harmony(Info.Metadata.GUID);
        harmony.PatchAll();
    }
}
