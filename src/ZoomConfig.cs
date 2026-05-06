﻿using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

public class ZoomConfig
{
    public static KeyCode ZoomKey { get; private set; }
    public static float MaxZoom { get; private set; }
    public static float ZoomSpeed { get; private set; }
    public static float ScrollStep { get; private set; }
    public static bool FocusClarityEnabled { get; private set; }
    public static float FocusClarityDelay { get; private set; }
    public static float FocusClarityFadeSpeed { get; private set; }
    public static float FocusClarityFogReduction { get; private set; }
    public static int CoolPerkMachine { get; private set; }

    public static void Init(BaseUnityPlugin plugin)
    {
        ZoomKey = plugin.Config.Bind("General", "Zoom Key", KeyCode.G, "Key used to activate zoom").Value;
        MaxZoom = plugin.Config.Bind("General", "Max Zoom", 600f, "Maximum zoom amount").Value;
        ZoomSpeed = plugin.Config.Bind("General", "Zoom Speed", 8f, "Speed of FOV change").Value;
        ScrollStep = plugin.Config.Bind("General", "Scroll Step", 50f, "Step size when adjusting zoom with mouse wheel").Value;
        FocusClarityEnabled = plugin.Config.Bind("Focus Clarity", "Enabled", true, "Reduce fog after holding zoom for a moment, scaled by zoom depth").Value;
        FocusClarityDelay = plugin.Config.Bind("Focus Clarity", "Delay", 0.75f,
            new ConfigDescription("Seconds of held zoom before fog reduction starts following zoom depth",
            new AcceptableValueRange<float>(0f, 5f)
        )).Value;
        FocusClarityFadeSpeed = plugin.Config.Bind("Focus Clarity", "Fade Speed", 2.5f,
            new ConfigDescription("How quickly fog reduction fades in and out",
            new AcceptableValueRange<float>(0.1f, 20f)
        )).Value;
        FocusClarityFogReduction = plugin.Config.Bind("Focus Clarity", "Fog Reduction", 0.65f,
            new ConfigDescription("Fraction of fog removed at maximum zoom focus, from 0 to 1",
            new AcceptableValueRange<float>(0f, 1f)
        )).Value;
        CoolPerkMachine = plugin.Config.Bind("General", "Cool Perk Machine", 70,
            new ConfigDescription("FOV while using Perk Machine or computers (it only for fun and doesn't make sense)",
            new AcceptableValueRange<int>(60, 140)
        )).Value;
    }
}
