using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System.IO;

public class ZoomConfig
{
    private static ConfigEntry<string> ZoomKey;
    private static ConfigEntry<float> MaxZoom;
    private static ConfigEntry<float> ZoomSpeed;
    private static ConfigEntry<float> ZoomSmoothness;
    private static ConfigEntry<float> ScrollStep;

    public static void Init(BaseUnityPlugin plugin)
    {
        ZoomKey = plugin.Config.Bind("General", "Zoom Key", "G", "Key used to activate zoom");
        MaxZoom = plugin.Config.Bind("General", "Max Zoom", 100f, "Maximum zoom amount");
        ZoomSpeed = plugin.Config.Bind("General", "Zoom Speed", 8f, "Speed of FOV change");
        ZoomSmoothness = plugin.Config.Bind("General", "Zoom Smoothness", 8f, "Smoothness factor for zoom interpolation");
        ScrollStep = plugin.Config.Bind("General", "Scroll Step", 5f, "Step size when adjusting zoom with mouse wheel");
    }

    public static string GetZoomKey() { return ZoomKey.Value; }
    public static float GetMaxZoom() { return MaxZoom.Value; }
    public static float GetZoomSpeed() { return ZoomSpeed.Value; }
    public static float GetZoomSmoothness() { return ZoomSmoothness.Value; }
    public static float GetScrollStep() { return ScrollStep.Value; }
}
