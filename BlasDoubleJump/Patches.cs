using HarmonyLib;
using Framework.Managers;
using CreativeSpore.SmartColliders;
using Tools.Level.Layout;

namespace BlasDoubleJump
{
    // Use the double jump when pressing jump and unable to normally
    [HarmonyPatch(typeof(PlatformCharacterController), "CanGhostJump", MethodType.Getter)]
    public class PlatformJump_Patch
    {
        public static void Postfix(ref bool __result, PlatformCharacterController __instance, PlatformCharacterPhysics ___m_platformPhysics)
        {
            if (__instance.name == "Penitent(Clone)" && !__result && Main.JumpController.CanDoubleJump)
            {
                Main.JumpController.UseDoubleJump(___m_platformPhysics.HSpeed != 0);
                __result = true;
            }
        }
    }

    // Allow double jump again when standing on ground
    [HarmonyPatch(typeof(PlatformCharacterController), "Update")]
    public class PlatformStanding_Patch
    {
        public static void Postfix(PlatformCharacterController __instance)
        {
            if (__instance.name == "Penitent(Clone)" && (__instance.IsGrounded || __instance.IsClimbing || Core.Logic.Penitent.IsGrabbingCliffLede))
                Main.JumpController.GiveBackDoubleJump();
        }
    }

    // Set flag when collecting the item
    [HarmonyPatch(typeof(CollectibleItem), "InteractionEnd")]
    public class ItemUse_Patch
    {
        public static void Postfix(CollectibleItem __instance)
        {
            if (__instance.GetPersistenID() == Main.JumpController.ItemPersId)
                Core.Events.SetFlag(Main.JumpController.ItemFlag, true, false);
        }
    }
    
    // Prevent certain things when loading temp level
    [HarmonyPatch(typeof(LevelInitializer), "Awake")]
    public class LevelInit_Patch
    {
        public static bool Prefix()
        {
            return !LevelModder.InLoadProcess;
        }
    }
}
