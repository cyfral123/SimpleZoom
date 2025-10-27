﻿using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

public class ZoomConfig
{
    public static KeyCode ZoomKey { get ; private set; }
    public static float MaxZoom { get; private set; }
    public static float ZoomSpeed { get; private set; }
    public static float ScrollStep { get; private set; }

    public static void Init(BaseUnityPlugin plugin)
    {
        ZoomKey = plugin.Config.Bind("General", "Zoom Key", KeyCode.G, "Key used to activate zoom").Value;
        MaxZoom = plugin.Config.Bind("General", "Max Zoom", 600f, "Maximum zoom amount").Value;
        ZoomSpeed = plugin.Config.Bind("General", "Zoom Speed", 8f, "Speed of FOV change").Value;
        ScrollStep = plugin.Config.Bind("General", "Scroll Step", 50f, "Step size when adjusting zoom with mouse wheel").Value;
    }
}