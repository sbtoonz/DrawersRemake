using HarmonyLib;
using JetBrains.Annotations;

namespace ItemDrawers_Remake
{
    public class Patches
    {
        [HarmonyPatch(typeof (Player), nameof(Player.IsOverlappingOtherPiece))]
        private static class OverlapPatch
        {
            [UsedImplicitly]
            [HarmonyPostfix]
            private static void Postfix(string pieceName, ref bool __result)
            {
                if (!(pieceName == "piece_drawer"))
                    return;
                __result = false;
            }
        }

        [HarmonyAfter(new string[] {"org.bepinex.helpers.PieceManager"})]
        [HarmonyPatch(typeof (ZNetScene), nameof(ZNetScene.Awake))]
        private static class awakerpatch
        {
            public static void Postfix(ZNetScene __instance) => ItemDrawersMod.ApplyConfig(__instance.GetPrefab("piece_judeDrawer"));
        }
    }
}