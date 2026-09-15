using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace CarturFlooring
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.jekkle.valheim.carturflooring";
        public const string PluginName = "Cartur's Flooring";
        public const string PluginVersion = "1.0.0";

        internal static BepInEx.Logging.ManualLogSource Log;
        internal static ConfigEntry<bool> FloorsAreRoofs;
        internal static ConfigEntry<bool> FiresOnFloors;

        private void Awake()
        {
            Log = Logger;

            // Both features edit the shared prefabs once, at world load. Turning one off after
            // it has already been applied cannot put the prefab back, so say so here rather
            // than pretend the toggle is live.
            FloorsAreRoofs = Config.Bind("General", "FloorsAreRoofs", true,
                "Floor tiles count as roof - they shelter the room below from rain. Restart the game after changing.");
            FiresOnFloors = Config.Bind("General", "FiresOnFloors", true,
                "Fires and other fireplace pieces can be placed on floor tiles instead of bare ground. Restart the game after changing.");

            try
            {
                Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, PluginGuid);
            }
            catch (System.Exception e)
            {
                Logger.LogWarning($"{PluginName} failed to patch, game continues unmodded: {e}");
            }
        }
    }

    // Root cause, part 1 - floors do not count as roof:
    //   Cover.IsUnderRoof (player shelter, Fireplace.CheckWet) and WearNTear.RoofCheck (rain
    //   damage) both spherecast straight up and accept the first collider that is NOT tagged
    //   "leaky". Wood, ashwood and iron floor prefabs ship with their colliders tagged "leaky",
    //   so a floor overhead is skipped and the room below is treated as open sky. Stone/grausten
    //   floors are not tagged that way, which is why those already work. Clearing the tag on
    //   floor pieces is the whole fix - nothing else in the game reads it.
    //
    // Root cause, part 2 - fires will not sit on floors:
    //   Player.UpdatePlacementGhost rejects a ghost when Piece.m_notOnWood and the piece under
    //   the cursor is Wood/HardWood WearNTear, and when Piece.m_groundOnly and the cursor is not
    //   on a heightmap. Fireplaces carry those flags, so they only ever land on bare terrain.
    //   Both are plain prefab flags, so clearing them on fire pieces is enough; no placement
    //   code needs patching.
    //
    // Both are done once on the prefabs, before anything is instantiated from them, so ghosts
    // and already-built pieces in loaded worlds both pick the change up.
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class ZNetScene_Awake_Patch
    {
        private const string LeakyTag = "leaky";

        private static void Postfix(ZNetScene __instance)
        {
            bool roofs = Plugin.FloorsAreRoofs.Value;
            bool fireplaces = Plugin.FiresOnFloors.Value;

            var floors = new List<string>();
            var fires = new List<string>();
            var missedFloors = new List<string>();

            foreach (GameObject prefab in __instance.m_prefabs)
            {
                if (prefab == null)
                    continue;

                Piece piece = prefab.GetComponent<Piece>();
                if (piece == null)
                    continue;

                bool isFloor = (piece.m_usage & Piece.UsageTagFlags.Floor) != 0;

                if (roofs && isFloor && ClearLeakyTags(prefab))
                    floors.Add(prefab.name);

                if (fireplaces && prefab.GetComponentInChildren<Fireplace>(true) != null && (piece.m_notOnWood || piece.m_groundOnly))
                {
                    piece.m_notOnWood = false;
                    piece.m_groundOnly = false;
                    fires.Add(prefab.name);
                }

                // A floor piece that does not carry the Floor usage flag would be missed
                // silently. Name it here so one log tells the whole story.
                if (roofs && !isFloor && prefab.name.IndexOf("floor", System.StringComparison.OrdinalIgnoreCase) >= 0 && HasLeakyTag(prefab))
                    missedFloors.Add(prefab.name);
            }

            Plugin.Log.LogInfo($"{Plugin.PluginName} {Plugin.PluginVersion} loaded. Floors now count as roof: {(roofs ? floors.Count + " (" + string.Join(", ", floors.ToArray()) + ")" : "off")}. Fires freed for floor placement: {(fireplaces ? fires.Count + " (" + string.Join(", ", fires.ToArray()) + ")" : "off")}.");

            if (roofs && missedFloors.Count > 0)
                Plugin.Log.LogWarning($"{Plugin.PluginName}: still leaky, no Floor usage flag: {string.Join(", ", missedFloors.ToArray())}");
        }

        private static bool ClearLeakyTags(GameObject prefab)
        {
            bool changed = false;
            foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.CompareTag(LeakyTag))
                {
                    t.gameObject.tag = "Untagged";
                    changed = true;
                }
            }
            return changed;
        }

        private static bool HasLeakyTag(GameObject prefab)
        {
            foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.CompareTag(LeakyTag))
                    return true;
            }
            return false;
        }
    }
}
