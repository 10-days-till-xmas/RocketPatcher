using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace RocketPatcher
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        internal void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(GrenadeTranspiler));
            harmony.PatchAll(typeof(ScoreSubmissionPatcher));
            Logger.LogMessage("I HATE ZOOMIES!!! I HATE ZOOMIES!!! I HATE ZOOMIES!!!");
            Logger.LogMessage("ExecuteAction(delegate(){grim.Explode();});");
        }

        private sealed class ScoreSubmissionPatcher
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(GameStateManager), "CanSubmitScores", MethodType.Getter)]
            static void ScoresSubmission(ref bool __result)
            {
                // prevent scores from being submitted so making a universal clip accidentally doesn't cause havoc
                __result = false;
            }
        }
    }
}
