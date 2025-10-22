using HarmonyLib;
using SimpleZoom;
using UnityEngine;

[HarmonyPatch(typeof(ENT_Player), "LateUpdate")]
public static class ZoomPatch
{
    private static float _currentFOV = -1f;
    private static float _targetFOV = -1f;
    private static float _baseFOV = -1f;
    private static float _zoomBase = 60f;

    public static void Postfix(ENT_Player __instance)
    {
        if (__instance == null || __instance.cam == null) return;

        float settingsFOV = SettingsManager.settings.playerFOV;

        if (Mathf.Abs(settingsFOV - _baseFOV) > 0.01f)
        {
            _baseFOV = settingsFOV;

            _currentFOV = Mathf.Lerp(_currentFOV, _baseFOV, 0.5f);
            _targetFOV = _baseFOV;
        }

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            _zoomBase = Mathf.Clamp(_zoomBase + scroll * ZoomConfig.GetScrollStep(), 5f, ZoomConfig.GetMaxZoom());
        }

        KeyCode zoomKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), ZoomConfig.GetZoomKey(), true);

        if (Input.GetKey(zoomKey))
        {
            _targetFOV = _baseFOV - _zoomBase;
        }
        else
        {
            _targetFOV = _baseFOV;
        }

        _currentFOV = Mathf.Lerp(_currentFOV, _targetFOV, Time.deltaTime * ZoomConfig.GetZoomSpeed());
        __instance.cam.fieldOfView = _currentFOV;
    }
}