using BepInEx;
using BepInEx.Logging;

namespace Ayzax.ReconnectFix;

[BepInProcess("PEAK.exe")]
[BepInPlugin("Ayzax.ReconnectFix", "Reconnect Fix", "1.0.0")]
public class ReconnectFixPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Reconnect Fix is loaded!");

        ReconnectFixer reconnectFixer = gameObject.AddComponent<ReconnectFixer>();
        reconnectFixer.InitLogger(Logger);
    }
}
