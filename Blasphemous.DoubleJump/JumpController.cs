using Blasphemous.Framework.Items;
using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Input;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Dust;
using UnityEngine;

namespace Blasphemous.DoubleJump;

/// <summary>
/// Handles double jump mechanics
/// </summary>
public class JumpController : BlasMod
{
    internal JumpController() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION) { }

    private bool m_AllowDoubleJump;
    internal bool AllowDoubleJump
    {
        get => m_AllowDoubleJump;
        set
        {
            m_AllowDoubleJump = value;
            CanDoubleJump = false;
        }
    }

    internal ButtonState ButtonStatus { get; private set; }
    internal bool CanDoubleJump { get; private set; }
    internal bool TriggeredHitImpulse { get; set; }

    /// <summary>
    /// Forces the jump animation and removes the dj flag
    /// </summary>
    public void UseDoubleJump(bool movingHorizontally)
    {
        Log("Using double jump!");
        CanDoubleJump = false;

        // Handle animations
        Animator anim = Core.Logic.Penitent.Animator;
        anim.SetBool("IS_JUMPING_OFF", false);
        anim.SetBool("FALLING", false);
        anim.Play(movingHorizontally ? "Jump Forward" : "Jump", 0, 0);

        // Sound & dust effects
        Core.Logic.Penitent.Audio.PlayJumpSound();
        StepDustSpawner dust = Core.Logic.Penitent.StepDustSpawner;
        dust.CurrentStepDustSpawn = StepDust.StepDustType.Landing;
        dust.GetStepDust(Core.Logic.Penitent.transform.position);

        // Give back an air impulse
        AttackSpecial_Patch.ImpulseFlag = true;
        Core.Logic.Penitent.PenitentAttack.GetExecutionBonus();
        AttackSpecial_Patch.ImpulseFlag = false;
    }

    /// <summary>
    /// Sets the dj flag
    /// </summary>
    public void GiveBackDoubleJump()
    {
        if (AllowDoubleJump)
        {
            CanDoubleJump = true;
        }
    }

    /// <summary>
    /// Recalculate the button status
    /// </summary>
    protected override void OnUpdate()
    {
        if (Core.Logic.Penitent == null)
            return;

        if (Core.Logic.Penitent.IsStickedOnWall && Core.Input.HasBlocker("PLAYER_LOGIC"))
        {
            // If stuck on wall, wait until you let go of jump to be able to use dbl jump
            ButtonStatus = ButtonState.Waiting;
        }
        else if (ButtonStatus == ButtonState.Waiting && !InputHandler.GetButton(ButtonCode.Jump))
        {
            // If not on wall anymore, once jump button is not held allow the dbl jump
            ButtonStatus = ButtonState.Released;
        }
        else if (ButtonStatus == ButtonState.Released && InputHandler.GetButton(ButtonCode.Jump))
        {
            ButtonStatus = ButtonState.Pressed;
        }
    }

    /// <summary>
    /// When exiting game, remove the dj flag
    /// </summary>
    protected override void OnExitGame()
    {
        AllowDoubleJump = false;
    }

    /// <summary>
    /// Register handlers
    /// </summary>
    protected override void OnInitialize()
    {
        LocalizationHandler.RegisterDefaultLanguage("en");
    }

    /// <summary>
    /// Register dj relic
    /// </summary>
    protected override void OnRegisterServices(ModServiceProvider provider)
    {
        provider.RegisterItem(new PurifiedHand().AddEffect(new DoubleJumpEffect()));
    }

    internal enum ButtonState { Waiting, Released, Pressed }
}
