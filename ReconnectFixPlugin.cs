using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using ExitGames.Client.Photon;

namespace Ayzax.ReconnectFix;

[BepInProcess("PEAK.exe")]
[BepInPlugin("Ayzax.ReconnectFix", "Reconnect Fix", "1.0.3")]
public class ReconnectFixPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Reconnect Fix is loaded! {SystemInfo.deviceUniqueIdentifier}");

        Harmony.CreateAndPatchAll(typeof(ReconnectFixPlugin));
    }

    [HarmonyPatch(typeof(RoomProperties), nameof(RoomProperties.HasReconnected))]
    [HarmonyPrefix]
    static bool HasReconnected(ref bool __result, Hashtable properties)
    {
        string DUI = SystemInfo.deviceUniqueIdentifier + RoomProperties.GetPlayerNumber();
        __result = properties.ContainsKey(DUI) && (bool)properties[DUI];
        return false;
    }
}
