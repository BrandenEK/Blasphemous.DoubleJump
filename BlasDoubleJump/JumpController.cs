using ModdingAPI;
using Framework.Managers;
using UnityEngine;
using Gameplay.GameControllers.Effects.Player.Dust;
using CreativeSpore.SmartColliders;
using Rewired;

namespace BlasDoubleJump
{
    public class JumpController : Mod
    {
        public JumpController(string modId, string modName, string modVersion) : base(modId, modName, modVersion) { }

        private const float WALL_CLIMB_DELAY = 0.1f;
        private float timeSinceWallClimbing;

        public PlatformCharacterController PlatformController { get; set; }
        private bool currentlyOnWall;

        private bool waitingForRelease;

        protected override void Initialize()
        {
            RegisterItem(new PurifiedHand().AddEffect<DoubleJumpEffect>());
            DisableFileLogging = true;
        }

        protected override void LateUpdate()
        {
            if (Core.Logic.Penitent == null || PlatformController == null || !AllowDoubleJump)
            {
                WallClimbing = false;
                LogWarning("Null controller");
                return;
            }

            // 3

            if (Core.Logic.Penitent.IsStickedOnWall)
            {
                //LogError("Stuck on wall");
                CanDoubleJump = false;
                waitingForRelease = true;
            }
            else if (waitingForRelease && !ReInput.players.GetPlayer(0).GetButton(6))
            {
                LogWarning("Giving back jump");
                CanDoubleJump = true;
                waitingForRelease = false;
            }

            // 2

            //if (!Core.Logic.Penitent.IsStickedOnWall && currentlyOnWall)
            //{
            //    // Player is no longer on wall, but they were last frame
            //    PlatformController.SetActionState(eControllerActions.Jump, false);
            //    LogWarning("Disabling jump action");
            //}
            //currentlyOnWall = Core.Logic.Penitent.IsStickedOnWall;

            // 1

            //if (Core.Logic.Penitent.IsStickedOnWall)
            //{
            //    WallClimbing = true;
            //    timeSinceWallClimbing = -1;
            //}
            //else
            //{
            //    if (timeSinceWallClimbing == -1f)
            //    {
            //        // Player was climbing wall on last frame
            //        timeSinceWallClimbing = WALL_CLIMB_DELAY;
            //    }

            //    if (timeSinceWallClimbing > 0)
            //    {
            //        timeSinceWallClimbing -= Time.deltaTime;
            //        if (timeSinceWallClimbing <= 0)
            //            WallClimbing = false;
            //    }
            //}
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

        public bool CanDoubleJump { get; private set; }

        public bool WallClimbing { get; private set; }

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
            //LogError("Giving back jump");
            if (AllowDoubleJump)
                CanDoubleJump = true;
        }
    }
}
