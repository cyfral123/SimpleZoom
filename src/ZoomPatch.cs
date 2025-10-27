using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

[HarmonyPatch]
public static class ZoomPatch
{
    private static float _zoomOffset = -1f;
    private static float _targetZoomOffset = -1f;
    private static float _zoomBase = 60f;

    private static readonly FieldInfo _curFOVField;
    private static readonly FieldInfo _smoothedFOVField;
    private static readonly FieldInfo _slowFOVAdjustField;

    static ZoomPatch()
    {
        var t = typeof(ENT_Player);
        _curFOVField = t.GetField("curFOV", BindingFlags.NonPublic | BindingFlags.Instance);
        _smoothedFOVField = t.GetField("smoothedFOV", BindingFlags.NonPublic | BindingFlags.Instance);
        _slowFOVAdjustField = t.GetField("slowFOVAdjust", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [HarmonyPatch(typeof(ENT_Player), "LateUpdate")]
    [HarmonyPostfix]
    public static void LateUpdatePostfix(ENT_Player __instance)
    {
        if (__instance == null || __instance.cam == null || __instance.IsLocked() || __instance.IsInputLocked())
            return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            _zoomBase = Mathf.Clamp(_zoomBase + scroll * ZoomConfig.ScrollStep, 10f, ZoomConfig.MaxZoom);
        }


        KeyCode zoomKey = ZoomConfig.ZoomKey;

        _targetZoomOffset = Input.GetKey(zoomKey) ? -_zoomBase : 0f;

        _zoomOffset = Mathf.Lerp(_zoomOffset, _targetZoomOffset, Time.deltaTime * ZoomConfig.ZoomSpeed);
    }

    [HarmonyPatch(typeof(ENT_Player), "FixedUpdate")]
    [HarmonyPrefix]
    public static void FixedUpdatePrefix(ENT_Player __instance)
    {
        if (__instance == null || __instance.cam == null || __instance.IsLocked())
            return;

        float curFOV = (float)_curFOVField.GetValue(__instance);
        _curFOVField.SetValue(__instance, curFOV + _zoomOffset);
    }


    [HarmonyPatch(typeof(ENT_Player), "FixedUpdate")]
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(ENT_Player __instance)
    {
        if (__instance == null || __instance.cam == null || __instance.IsLocked())
            return;

        float curFOV = (float)_curFOVField.GetValue(__instance);
        float smoothedFOV = (float)_smoothedFOVField.GetValue(__instance);
        float slowFOVAdjust = (float)_slowFOVAdjustField.GetValue(__instance);
        _smoothedFOVField.SetValue(__instance, Mathf.Lerp(smoothedFOV, curFOV + slowFOVAdjust + _zoomOffset, Time.deltaTime));
    }


    [HarmonyPatch(typeof(UT_CameraTakeover), "Start")]
    public static class CameraTakeoverPatch
    {
        [HarmonyPrefix]
        public static bool CoolPerkMachine(UT_CameraTakeover __instance)
        {
            __instance.fov = ZoomConfig.CoolPerkMachine;
            return true;
        }
    }

}