using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Dust;
using ModdingAPI;
using UnityEngine;

namespace BlasDoubleJump
{
    public class JumpController : Mod
    {
        public JumpController(string modId, string modName, string modVersion) : base(modId, modName, modVersion) { }
        public enum ButtonState { Waiting, Released, Pressed }

        protected override void Initialize()
        {
            RegisterItem(new PurifiedHand().AddEffect<DoubleJumpEffect>());
            DisableFileLogging = true;
        }

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

        protected override void LevelLoaded(string oldLevel, string newLevel)
        {
            if (newLevel == "MainMenu")
                AllowDoubleJump = false;
        }

        protected override void Update()
        {
            if (Core.Logic.Penitent == null) return;

            if (Core.Logic.Penitent.IsStickedOnWall && Core.Input.HasBlocker("PLAYER_LOGIC"))
            {
                // If stuck on wall, wait until you let go of jump to be able to use dbl jump
                ButtonStatus = ButtonState.Waiting;
            }
            else if (ButtonStatus == ButtonState.Waiting && !Input.GetButton(InputHandler.ButtonCode.Jump))
            {
                // If not on wall anymore, once jump button is not held allow the dbl jump
                ButtonStatus = ButtonState.Released;
            }
            else if (ButtonStatus == ButtonState.Released && Input.GetButton(InputHandler.ButtonCode.Jump))
            {
                ButtonStatus = ButtonState.Pressed;
            }
        }

        public bool CanDoubleJump { get; private set; }
        public ButtonState ButtonStatus { get; private set; }
        public bool SpecialStatus { get; private set; }
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
            SpecialStatus = true;
            Core.Logic.Penitent.PenitentAttack.GetExecutionBonus();
            SpecialStatus = false;
        }

        public void GiveBackDoubleJump()
        {
            if (AllowDoubleJump)
            {
                CanDoubleJump = true;
            }
        }
    }
}
