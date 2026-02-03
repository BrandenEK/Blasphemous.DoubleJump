using CreativeSpore.SmartColliders;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.Attack;
using HarmonyLib;

namespace Blasphemous.DoubleJump;

/// <summary>
/// Use the double jump when pressing jump and unable to normally
/// </summary>
[HarmonyPatch(typeof(PlatformCharacterController), nameof(PlatformCharacterController.CanGhostJump), MethodType.Getter)]
class PlatformJump_Patch
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

/// <summary>
/// Allow double jump again when standing on ground
/// </summary>
[HarmonyPatch(typeof(PlatformCharacterController), nameof(PlatformCharacterController.Update))]
class PlatformStanding_Patch
{
    public static void Postfix(PlatformCharacterController __instance)
    {
        if (__instance.name == "Penitent(Clone)" && (__instance.IsGrounded || __instance.IsClimbing || Core.Logic.Penitent.IsGrabbingCliffLede || Core.Logic.Penitent.IsStickedOnWall))
            Main.JumpController.GiveBackDoubleJump();
    }
}

/// <summary>
/// Give jump back after successful air impulse
/// </summary>
[HarmonyPatch(typeof(PenitentAttack), nameof(PenitentAttack.HitImpulse))]
class AttackImpulse_Patch
{
    public static void Postfix(PenitentAttack __instance, int ____currentImpulses)
    {
        if (!Core.Logic.Penitent.PlatformCharacterController.IsGrounded && ____currentImpulses < Core.Logic.Penitent.Stats.AirImpulses.Final && Main.JumpController.TriggeredHitImpulse)
            Main.JumpController.GiveBackDoubleJump();
    }
}

/// <summary>
/// Calling this function with special status will give back an air impulse
/// </summary>
[HarmonyPatch(typeof(PenitentAttack), nameof(PenitentAttack.GetExecutionBonus))]
class AttackSpecial_Patch
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

/// <summary>
/// Store the hit impulse trigger in main class
/// </summary>
[HarmonyPatch(typeof(PenitentAttack), nameof(PenitentAttack.HitImpulseTriggered), MethodType.Setter)]
class AttackTrigger_Patch
{
    public static void Postfix(bool value)
    {
        Main.JumpController.TriggeredHitImpulse = value;
    }
}
