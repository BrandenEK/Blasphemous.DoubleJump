using Blasphemous.ModdingAPI;
using Blasphemous.ModdingAPI.Input;
using Blasphemous.ModdingAPI.Items;
using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Dust;
using UnityEngine;

namespace Blasphemous.DoubleJump;

public class JumpController : BlasMod
{
    public JumpController() : base(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_AUTHOR, ModInfo.MOD_VERSION) { }

    private bool m_AllowDoubleJump;
    public bool AllowDoubleJump
    {
        get => m_AllowDoubleJump;
        set
        {
            m_AllowDoubleJump = value;
            CanDoubleJump = false;
        }
    }

    public ButtonState ButtonStatus { get; private set; }
    public bool CanDoubleJump { get; private set; }
    public bool TriggeredHitImpulse { get; set; }

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

    public void GiveBackDoubleJump()
    {
        if (AllowDoubleJump)
        {
            CanDoubleJump = true;
        }
    }

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

    protected override void OnExitGame()
    {
        AllowDoubleJump = false;
    }

    protected override void OnRegisterServices(ModServiceProvider provider)
    {
        provider.RegisterItem(new PurifiedHand().AddEffect(new DoubleJumpEffect()));
    }

    public enum ButtonState { Waiting, Released, Pressed }
}
