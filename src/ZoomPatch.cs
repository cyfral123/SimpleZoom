using HarmonyLib;
using System.Reflection;
using UnityEngine;

[HarmonyPatch]
public static class ZoomPatch
{
    private static float _zoomOffset = 0f;
    private static float _targetZoomOffset = 0f;
    private static float _zoomBase = 100f;

    private static readonly FieldInfo _curFOVField;
    private static readonly FieldInfo _smoothedFOVField;
    private static readonly FieldInfo _slowFOVAdjustField;
    private static float _zoomVelocity = 0f;
    private static float _fovVelocity = 0f;

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
            _zoomBase = Mathf.Clamp(_zoomBase + scroll * ZoomConfig.GetScrollStep(), 10f, ZoomConfig.GetMaxZoom());

        KeyCode zoomKey = ZoomConfig.GetZoomKey();

        _targetZoomOffset = Input.GetKey(zoomKey) ? -_zoomBase : 0f;

        _zoomOffset = ZoomConfig.GetSmoothType() switch
        {
            ZoomConfig.MathFunction.Exponential => _zoomOffset + (_targetZoomOffset - _zoomOffset) * (1f - Mathf.Exp(-ZoomConfig.GetZoomSpeed() * Time.deltaTime)),
            ZoomConfig.MathFunction.SmoothDamp => Mathf.SmoothDamp(_zoomOffset, _targetZoomOffset, ref _zoomVelocity, 0.2f),
            ZoomConfig.MathFunction.Linear => Mathf.Lerp(_zoomOffset, _targetZoomOffset, Time.deltaTime * ZoomConfig.GetZoomSpeed()),
            _ => throw new System.NotImplementedException()
        };
    }

    [HarmonyPatch(typeof(ENT_Player), "FixedUpdate")]
    [HarmonyPrefix]
    public static void FixedUpdatePrefix(ENT_Player __instance)
    {
        if (__instance == null || __instance.cam == null || __instance.IsLocked() || __instance.IsInputLocked())
            return;

        float curFOV = (float)_curFOVField.GetValue(__instance);
        float smoothedFOV = (float)_smoothedFOVField.GetValue(__instance);
        float slowFOVAdjust = (float)_slowFOVAdjustField.GetValue(__instance);
        float targetFOV = curFOV + _zoomOffset + slowFOVAdjust;
        var finalSmoothed = ZoomConfig.GetSmoothType() switch
        {
            ZoomConfig.MathFunction.Exponential => smoothedFOV + (targetFOV - smoothedFOV) * (1f - Mathf.Exp(-ZoomConfig.GetZoomSmoothness() * Time.deltaTime)),
            ZoomConfig.MathFunction.SmoothDamp => Mathf.SmoothDamp(smoothedFOV, targetFOV, ref _fovVelocity, 0.2f),
            ZoomConfig.MathFunction.Linear => Mathf.Lerp(smoothedFOV, targetFOV, Time.deltaTime * ZoomConfig.GetZoomSmoothness()),
             _ => throw new System.NotImplementedException()
        };
        _smoothedFOVField.SetValue(__instance, finalSmoothed);
        __instance.cam.fieldOfView = finalSmoothed;
    }
}
