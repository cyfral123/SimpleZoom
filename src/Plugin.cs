using BepInEx;
using HarmonyLib;

[BepInPlugin("simplezoom_1.2.0", "Simple Zoom", "1.2.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        ZoomConfig.Init(this);
        var harmony = new Harmony(Info.Metadata.GUID);
        harmony.PatchAll();
    }
}
