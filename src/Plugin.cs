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
        public const string PluginVersion = "1.1.0";

        internal static BepInEx.Logging.ManualLogSource Log;
        internal static ConfigEntry<bool> FloorsAreRoofs;
        internal static ConfigEntry<bool> FiresOnFloors;
        internal static ConfigEntry<bool> SmeltersOnFloors;
        internal static ConfigEntry<bool> FloorsDontDecay;

        private void Awake()
        {
            Log = Logger;

            // The first three edit the shared prefabs once, at world load. Turning one off after
            // it has already been applied cannot put the prefab back, so say so here rather
            // than pretend the toggle is live. The fourth is a live patch and really is.
            FloorsAreRoofs = Config.Bind("General", "FloorsAreRoofs", true,
                "Floor tiles count as roof - they shelter the room below from rain. Restart the game after changing.");
            FiresOnFloors = Config.Bind("General", "FiresOnFloors", true,
                "Fires and other fireplace pieces can be placed on floor tiles instead of bare ground. Restart the game after changing.");
            SmeltersOnFloors = Config.Bind("General", "SmeltersOnFloors", true,
                "Smelters, charcoal kilns, blast furnaces and the other Smelter-driven stations can be placed on floor tiles instead of bare ground. Restart the game after changing.");
            FloorsDontDecay = Config.Bind("General", "FloorsDontDecay", true,
                "Floor tiles take no rain damage, roofed or not, so an open deck or a jetty stops rotting. They still darken in the rain, and still fall down without support. Unlike the three above, this one is read every tick, so it takes effect the moment you change it.");

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
    //   Player.UpdatePlacementGhost rejects a ghost on three separate prefab flags:
    //   m_notOnWood (the piece under the cursor is Wood/HardWood WearNTear), m_groundOnly
    //   (the cursor is not on a heightmap) and m_groundPiece (same heightmap test, and it
    //   returns early, so it is the strongest of the three). Fire pieces carry some mix of
    //   them, so they only ever land on bare terrain. All three are plain prefab flags, so
    //   clearing them is enough; no placement code needs patching.
    //
    //   m_groundPiece also picks the ground-clipping branch of the placement snap. Cleared,
    //   the piece takes the ordinary collider snap instead - which is what putting a fire on
    //   a floor wants anyway.
    //
    //   Selecting fire pieces by the Fireplace component alone missed hearths and braziers in
    //   1.0.0, so Piece.m_comfortGroup == ComfortGroup.Fire is accepted too. That is the
    //   game's own grouping for what counts as a fire.
    //
    // Root cause, part 3 - smelters and kilns will not sit on floors:
    //   The same three flags, on a different set of prefabs. The smelter, charcoal kiln, blast
    //   furnace, windmill and spinning wheel are all one component - Smelter - so that component
    //   is the selector, exactly as Fireplace is for part 2. Which of the three flags each
    //   prefab actually carried is named in the log rather than assumed here, since it is the
    //   prefab that decides and a game update can change it.
    //
    //   Freeing the placement flags alone breaks the smelter the moment it is built on wood.
    //   WearNTear.UpdateWear:
    //
    //       if (m_noSupportWear) { UpdateSupport(); if (!HaveSupport()) damage = 100f; }
    //
    //   and 100 is a full health bar in one wear tick, so the piece is destroyed outright rather
    //   than weakened. HaveSupport is m_support >= GetMinSupport(), and GetMinSupport comes from
    //   m_materialType: Stone demands 100, Wood only 10. A smelter is stone. Standing on terrain
    //   it was always fully supported, and m_groundPiece is what guaranteed it stood on terrain.
    //   Put it on a wooden floor and its support has to arrive through wood, which caps at 100
    //   and loses a share at every link, so it never reaches the stone threshold.
    //
    //   m_noSupportWear is cleared on those same prefabs, which switches the whole branch off.
    //   Support is still calculated; it just no longer demolishes the building. Nothing else
    //   reads that flag - rain damage is m_noRoofWear, a separate one, and is left alone.
    //
    // Root cause, part 4 - an uncovered floor rots:
    //   WearNTear.UpdateWear, IL_00cc:
    //
    //       if (m_noRoofWear && !insideShield && GetHealthPercentage() > 0.5f)
    //           if (IsWet()) { every 60s -> damage += 5; }
    //
    //   m_noRoofWear reads "this piece wears when it has no roof". A floor is what you stand
    //   on, so most of them have nothing overhead and fail RoofCheck forever - a deck, a jetty,
    //   a bridge.
    //
    //   Clearing that flag on the prefab is the obvious fix and it is the wrong one, because
    //   the same flag gates the wet look eighteen lines earlier in the same method:
    //
    //       m_rainWet = (!insideShield && !m_haveRoof && m_noRoofWear) && EnvMan.IsWet();
    //       if (m_wet) m_wet.SetActive(m_rainWet);
    //
    //   so a floor that cannot rot would also never darken in the rain. There is no second
    //   flag separating the two, so this one needs a patch - see WearNTear_UpdateWear_Patch.
    //
    //   Nothing else is touched: m_noSupportWear is a different flag, so a floor with nothing
    //   holding it up still falls down. Snow and ash damage are their own branches further
    //   down the same method and are left alone.
    //
    // All are done once on the prefabs, before anything is instantiated from them, so ghosts
    // and already-built pieces in loaded worlds both pick the change up.
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class ZNetScene_Awake_Patch
    {
        private const string LeakyTag = "leaky";

        private static void Postfix(ZNetScene __instance)
        {
            bool roofs = Plugin.FloorsAreRoofs.Value;
            bool fireplaces = Plugin.FiresOnFloors.Value;
            bool stations = Plugin.SmeltersOnFloors.Value;
            bool noDecay = Plugin.FloorsDontDecay.Value;

            var floors = new List<string>();
            var dry = new List<string>();
            var fires = new List<string>();
            var firesAlreadyFree = new List<string>();
            var smelters = new List<string>();
            var smeltersAlreadyFree = new List<string>();
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

                if (noDecay && isFloor && prefab.GetComponent<WearNTear>() != null)
                    dry.Add(prefab.name);

                bool isFire = prefab.GetComponentInChildren<Fireplace>(true) != null
                              || piece.m_comfortGroup == Piece.ComfortGroup.Fire;

                if (fireplaces && isFire)
                {
                    string cleared = FreeForFloors(piece);
                    if (cleared.Length > 0)
                        fires.Add(prefab.name + " [" + cleared + "]");
                    else
                        firesAlreadyFree.Add(prefab.name);
                }

                if (stations && prefab.GetComponentInChildren<Smelter>(true) != null)
                {
                    string cleared = FreeForFloors(piece);

                    WearNTear wear = prefab.GetComponent<WearNTear>();
                    if (wear != null && wear.m_noSupportWear)
                    {
                        wear.m_noSupportWear = false;
                        cleared = (cleared.Length > 0 ? cleared + " " : "") + "noSupportWear";
                    }

                    if (cleared.Length > 0)
                        smelters.Add(prefab.name + " [" + cleared + "]");
                    else
                        smeltersAlreadyFree.Add(prefab.name);
                }

                // A floor piece that does not carry the Floor usage flag would be missed
                // silently. Name it here so one log tells the whole story.
                if (roofs && !isFloor && prefab.name.IndexOf("floor", System.StringComparison.OrdinalIgnoreCase) >= 0 && HasLeakyTag(prefab))
                    missedFloors.Add(prefab.name);
            }

            Plugin.Log.LogInfo($"{Plugin.PluginName} {Plugin.PluginVersion} loaded. Floors now count as roof: {(roofs ? floors.Count + " (" + string.Join(", ", floors.ToArray()) + ")" : "off")}. Fires freed for floor placement: {(fireplaces ? fires.Count + " (" + string.Join(", ", fires.ToArray()) + ")" : "off")}.");

            if (noDecay)
                Plugin.Log.LogInfo($"{Plugin.PluginName}: floors no longer take rain damage: {dry.Count} ({string.Join(", ", dry.ToArray())})");

            if (stations)
                Plugin.Log.LogInfo($"{Plugin.PluginName}: smelters and kilns freed for floor placement: {smelters.Count} ({string.Join(", ", smelters.ToArray())})");

            if (stations && smeltersAlreadyFree.Count > 0)
                Plugin.Log.LogInfo($"{Plugin.PluginName}: smelter-type pieces that were already free to place: {string.Join(", ", smeltersAlreadyFree.ToArray())}");

            if (fireplaces && firesAlreadyFree.Count > 0)
                Plugin.Log.LogInfo($"{Plugin.PluginName}: fire pieces that were already free to place: {string.Join(", ", firesAlreadyFree.ToArray())}");

            if (roofs && missedFloors.Count > 0)
                Plugin.Log.LogWarning($"{Plugin.PluginName}: still leaky, no Floor usage flag: {string.Join(", ", missedFloors.ToArray())}");
        }

        /// Clears the three prefab flags that keep a piece on bare terrain and returns the ones
        /// that were actually set, so the log says what each prefab carried instead of claiming
        /// a fix that did nothing.
        private static string FreeForFloors(Piece piece)
        {
            string cleared = (piece.m_notOnWood ? "notOnWood " : "")
                           + (piece.m_groundOnly ? "groundOnly " : "")
                           + (piece.m_groundPiece ? "groundPiece" : "");

            piece.m_notOnWood = false;
            piece.m_groundOnly = false;
            piece.m_groundPiece = false;

            return cleared.Trim();
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

    /// Rain rot on floors, without taking the wet look with it.
    ///
    /// WearNTear.UpdateWear, IL_00cc, is the whole of the rain branch:
    ///
    ///     if (m_noRoofWear && !insideShield && GetHealthPercentage() > 0.5f) {
    ///         if (IsWet()) {
    ///             if (m_rainTimer == 0f) m_rainTimer = time;
    ///             else if (time - m_rainTimer > 60f) { m_rainTimer = time; damage += 5f; }
    ///         } else m_rainTimer = 0f;
    ///     }
    ///
    /// Holding m_rainTimer at the incoming time means the sixty seconds never elapse, so the
    /// damage line is never reached. Every other line of the method - the wet sheen above it,
    /// snow, ash, lava, support collapse below it - runs exactly as it did.
    ///
    /// A Prefix rather than a Postfix because the damage is applied before the method returns;
    /// a Postfix would be looking at a floor that had already lost the health.
    ///
    /// The other two readers of the rain state are left alone deliberately: m_rainWet still
    /// drives the sheen, and WearNTear.IsWet still reports wet to RPC_Damage, so a rained-on
    /// floor still refuses to catch fire.
    [HarmonyPatch(typeof(WearNTear), "UpdateWear")]
    internal static class WearNTear_UpdateWear_Patch
    {
        private static void Prefix(float time, Piece ___m_piece, ref float ___m_rainTimer)
        {
            if (!Plugin.FloorsDontDecay.Value || ___m_piece == null)
                return;

            if ((___m_piece.m_usage & Piece.UsageTagFlags.Floor) != 0)
                ___m_rainTimer = time;
        }
    }
}
