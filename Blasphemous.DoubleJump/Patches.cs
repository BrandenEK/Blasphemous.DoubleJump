using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.Attack;
using HarmonyLib;

namespace Blasphemous.DoubleJump;

// Use the double jump when pressing jump and unable to normally
[HarmonyPatch(typeof(PlatformCharacterController), "CanGhostJump", MethodType.Getter)]
public class PlatformJump_Patch
{
    public static void Postfix(ref bool __result, PlatformCharacterController __instance, PlatformCharacterPhysics ___m_platformPhysics)
    {
        if (__instance.name == "Penitent(Clone)" && !__result && Main.JumpController.CanDoubleJump && Main.JumpController.ButtonStatus == JumpController.ButtonState.Pressed)
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
        if (__instance.name == "Penitent(Clone)" && (__instance.IsGrounded || __instance.IsClimbing || Core.Logic.Penitent.IsGrabbingCliffLede || Core.Logic.Penitent.IsStickedOnWall))
            Main.JumpController.GiveBackDoubleJump();
    }
}

// Give jump back after successful air impulse
[HarmonyPatch(typeof(PenitentAttack), "HitImpulse")]
public class AttackImpulse_Patch
{
    public static void Postfix(PenitentAttack __instance, int ____currentImpulses)
    {
        if (!Core.Logic.Penitent.PlatformCharacterController.IsGrounded && ____currentImpulses < Core.Logic.Penitent.Stats.AirImpulses.Final && Main.JumpController.TriggeredHitImpulse)
            Main.JumpController.GiveBackDoubleJump();
    }
}

// Calling this function with special status will give back an air impulse
[HarmonyPatch(typeof(PenitentAttack), "GetExecutionBonus")]
public class AttackSpecial_Patch
{
    public static bool Prefix(ref int ____currentImpulses)
    {
        if (ImpulseFlag)
        {
            if (____currentImpulses > 0)
                ____currentImpulses--;
            return false;
        }
        return true;
    }

    public static bool ImpulseFlag { get; set; }
}

// Store the hit impulse trigger in main class
[HarmonyPatch(typeof(PenitentAttack), "HitImpulseTriggered", MethodType.Setter)]
public class AttackTrigger_Patch
{
    public static void Postfix(bool value)
    {
        Main.JumpController.TriggeredHitImpulse = value;
    }
}
