using HarmonyLib;
using System.Reflection;
using UnityEngine;

[HarmonyPatch]
public static class ZoomPatch
{
    private const float MinFov = 10f;
    private const float MaxFov = 140f;
    private const float MinZoomBase = 10f;
    private const float ZoomSpeedScale = 0.4f;
    private const float ScrollStepScale = 0.2f;
    private const float MinZoomSensitivityScale = 0.25f;

    private static float _zoomOffset = 0f;
    private static float _targetZoomOffset = 0f;
    private static float _zoomBase = 60f;
    private static float _focusHoldTime = 0f;
    private static float _focusAmount = 0f;

    private static readonly FieldInfo _smoothedFOVField;
    private static readonly FieldInfo _camSpeedField;

    static ZoomPatch()
    {
        var t = typeof(ENT_Player);
        _smoothedFOVField = t.GetField("smoothedFOV", BindingFlags.NonPublic | BindingFlags.Instance);
        _camSpeedField = t.GetField("camSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [HarmonyPatch(typeof(ENT_Player), "LateUpdate")]
    [HarmonyPrefix]
    public static void LateUpdatePrefix(ENT_Player __instance)
    {
        if (__instance == null || __instance.cam == null)
            return;

        bool zoomHeld = false;
        bool canZoom = !__instance.IsLocked() && !__instance.IsInputLocked();
        if (canZoom)
        {
            float maxZoomBase = GetMaxZoomBase(__instance);
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _zoomBase = Mathf.Clamp(_zoomBase + scroll * ZoomConfig.ScrollStep * ScrollStepScale, MinZoomBase, maxZoomBase);
            }
            else
            {
                _zoomBase = Mathf.Clamp(_zoomBase, MinZoomBase, maxZoomBase);
            }

            KeyCode zoomKey = ZoomConfig.ZoomKey;
            zoomHeld = Input.GetKey(zoomKey);
            _targetZoomOffset = zoomHeld ? -_zoomBase : 0f;
        }
        else
        {
            _targetZoomOffset = 0f;
        }

        float smoothing = 1f - Mathf.Exp(-ZoomConfig.ZoomSpeed * ZoomSpeedScale * Time.deltaTime);
        _zoomOffset = Mathf.Lerp(_zoomOffset, _targetZoomOffset, smoothing);
        _zoomOffset = Mathf.Clamp(_zoomOffset, -GetMaxZoomBase(__instance), 0f);
        UpdateFocusClarity(zoomHeld, __instance);
    }

    [HarmonyPatch(typeof(ENT_Player), "LateUpdate")]
    [HarmonyPostfix]
    public static void LateUpdatePostfix(ENT_Player __instance)
    {
        if (__instance == null || __instance.cam == null)
            return;

        if (__instance.IsLocked() || Mathf.Abs(_zoomOffset) < 0.001f)
            return;

        float baseFov = GetBaseFov(__instance);
        __instance.cam.fieldOfView = Mathf.Clamp(baseFov + _zoomOffset, MinFov, MaxFov);
    }

    [HarmonyPatch(typeof(ENT_Player), "MouseLook")]
    [HarmonyPrefix]
    public static void MouseLookPrefix(ENT_Player __instance, ref float __state)
    {
        __state = float.NaN;
        if (__instance == null || _camSpeedField == null || Mathf.Abs(_zoomOffset) < 0.001f)
            return;

        float camSpeed = (float)_camSpeedField.GetValue(__instance);
        __state = camSpeed;
        _camSpeedField.SetValue(__instance, camSpeed * GetZoomSensitivityScale(__instance));
    }

    [HarmonyPatch(typeof(ENT_Player), "MouseLook")]
    [HarmonyPostfix]
    public static void MouseLookPostfix(ENT_Player __instance, float __state)
    {
        if (__instance == null || _camSpeedField == null || float.IsNaN(__state))
            return;

        _camSpeedField.SetValue(__instance, __state);
    }

    [HarmonyPatch(typeof(FXManager), "CameraFX")]
    [HarmonyPostfix]
    public static void CameraFXPostfix(Camera camera)
    {
        if (!ZoomConfig.FocusClarityEnabled || _focusAmount <= 0.001f || camera == null || !FXManager.showFog || FXManager.fxData == null)
            return;

        ENT_Player player = ENT_Player.GetPlayer();
        if (player == null || player.cam != camera)
            return;

        float reduction = Mathf.Clamp01(ZoomConfig.FocusClarityFogReduction) * _focusAmount;
        float fogMultiplier = Mathf.Max(0f, FXManager.fxData.fogMultiplier * (1f - reduction));
        Vector4 fog = new Vector4(FXManager.fxData.fog.r, FXManager.fxData.fog.g, FXManager.fxData.fog.b, fogMultiplier);

        Shader.SetGlobalVector("_FOG", fog);
        Shader.SetGlobalFloat("_FOGMULT", fogMultiplier);
        Shader.SetGlobalFloat("_FOGDITHERAMOUNT", Mathf.Lerp(FXManager.fxData.fogDitherAmount, 0f, reduction));
    }

    private static void UpdateFocusClarity(bool zoomHeld, ENT_Player player)
    {
        float targetFocus = 0f;
        if (ZoomConfig.FocusClarityEnabled && zoomHeld)
        {
            _focusHoldTime += Time.deltaTime;
            if (_focusHoldTime >= ZoomConfig.FocusClarityDelay)
                targetFocus = GetZoomDepth(player);
        }
        else
        {
            _focusHoldTime = 0f;
        }

        float fadeSpeed = Mathf.Max(0.1f, ZoomConfig.FocusClarityFadeSpeed);
        float smoothing = 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime);
        _focusAmount = Mathf.Lerp(_focusAmount, targetFocus, smoothing);
    }

    private static float GetZoomDepth(ENT_Player player)
    {
        float maxZoomBase = GetMaxZoomBase(player);
        if (maxZoomBase <= 0f)
            return 0f;

        return Mathf.Clamp01(-_zoomOffset / maxZoomBase);
    }

    private static float GetBaseFov(ENT_Player player)
    {
        float baseFov = player.cam.fieldOfView;
        object smoothedFov = _smoothedFOVField?.GetValue(player);
        if (smoothedFov is float value)
            baseFov = value;

        return Mathf.Clamp(baseFov, MinFov, MaxFov);
    }

    private static float GetZoomSensitivityScale(ENT_Player player)
    {
        float baseFov = GetBaseFov(player);
        float zoomedFov = Mathf.Clamp(baseFov + _zoomOffset, MinFov, MaxFov);
        float baseTan = Mathf.Tan(baseFov * Mathf.Deg2Rad * 0.5f);
        float zoomTan = Mathf.Tan(zoomedFov * Mathf.Deg2Rad * 0.5f);

        if (baseTan <= 0f)
            return 1f;

        return Mathf.Clamp(zoomTan / baseTan, MinZoomSensitivityScale, 1f);
    }

    private static float GetMaxZoomBase(ENT_Player player)
    {
        float visibleMax = Mathf.Max(MinZoomBase, GetBaseFov(player) - MinFov);
        return Mathf.Max(MinZoomBase, Mathf.Min(ZoomConfig.MaxZoom, visibleMax));
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
