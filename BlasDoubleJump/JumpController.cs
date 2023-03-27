using ModdingAPI;
using Framework.Managers;
using UnityEngine;
using Gameplay.GameControllers.Effects.Player.Dust;
using Rewired;

namespace BlasDoubleJump
{
    public class JumpController : Mod
    {
        public JumpController(string modId, string modName, string modVersion) : base(modId, modName, modVersion) { }
        public enum ButtonState { Waiting, Released, Pressed }

        private Player input;
        private Player Input
        {
            get
            {
                if (input == null)
                {
                    input = ReInput.players.GetPlayer(0);
                }
                return input;
            }
        }

        protected override void Initialize()
        {
            RegisterItem(new PurifiedHand().AddEffect<DoubleJumpEffect>());
            DisableFileLogging = true;
        }

        private bool m_AllowDoubleJump;
        public bool AllowDoubleJump
        {
            get { return m_AllowDoubleJump; }
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
                LogWarning("Grabbed wall");
            }
            else if (ButtonStatus == ButtonState.Waiting && !Input.GetButton(6))
            {
                // If not on wall anymore, once jump button is not held allow the dbl jump
                ButtonStatus = ButtonState.Released;
                LogWarning("Released jump");
            }
            else if (ButtonStatus == ButtonState.Released && Input.GetButton(6))
            {
                ButtonStatus = ButtonState.Pressed;
                LogWarning("Pressed jump");
            }
        }

        public bool CanDoubleJump { get; private set; }
        public ButtonState ButtonStatus { get; private set; }

        public void UseDoubleJump(bool movingHorizontally)
        {
            Log("Using double jump!");
            CanDoubleJump = false;

            // Handle animations
            Animator anim = Core.Logic.Penitent.Animator;
            anim.SetBool("IS_JUMPING_OFF", false);
            anim.SetBool("FALLING", false);
            anim.SetTrigger(movingHorizontally ? "FORWARD_JUMP" : "JUMP");

            // Sound & dust effects
            Core.Logic.Penitent.Audio.PlayJumpSound();
            StepDustSpawner dust = Core.Logic.Penitent.StepDustSpawner;
            dust.CurrentStepDustSpawn = StepDust.StepDustType.Landing;
            dust.GetStepDust(Core.Logic.Penitent.transform.position);
        }

        public void GiveBackDoubleJump()
        {
            if (AllowDoubleJump)
                CanDoubleJump = true;
        }
    }
}
