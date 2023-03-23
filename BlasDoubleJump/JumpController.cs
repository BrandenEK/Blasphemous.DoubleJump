using ModdingAPI;
using Framework.Managers;
using Framework.Inventory;
using Framework.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Gameplay.GameControllers.Effects.Player.Dust;
using Gameplay.UI;

namespace BlasDoubleJump
{
    public class JumpController : Mod
    {
        public JumpController(string modId, string modName, string modVersion) : base(modId, modName, modVersion) { }

        public string ItemPersId => "RE402-ITEM";
        public string ItemFlag => "RE402_COLLECTED";

        protected override void Initialize()
        {
            RegisterItem(new PurifiedHand().AddEffect<DoubleJumpEffect>());
            DisableFileLogging = true;
        }

        protected override void LevelLoaded(string oldLevel, string newLevel)
        {
            // When game is first started, load objects
            if (newLevel == "MainMenu" && !LevelModder.LoadedObjects)
            {
                LevelModder.LoadObjects();
            }

            // When loading level MoM, remove wall climb and add the collectible item
            if (newLevel != "D04Z02S01") return;

            // Remove wall climb
            foreach (GameObject obj in SceneManager.GetSceneByName("D04Z02S01_DECO").GetRootGameObjects())
            {
                if (obj.name == "MIDDLEGROUND")
                {
                    Transform holder = obj.transform.Find("AfterPlayer/WallClimb");
                    holder.GetChild(0).gameObject.SetActive(false);
                    holder.GetChild(1).gameObject.SetActive(false);
                }
            }
            foreach (GameObject obj in SceneManager.GetSceneByName("D04Z02S01_LAYOUT").GetRootGameObjects())
            {
                if (obj.name == "NAVIGATION")
                {
                    obj.transform.Find("NAV_Wall Climb (1x3) (2)").gameObject.SetActive(false);
                }
            }

            CreateCollectibleItem(ItemPersId, "RE402", new Vector3(233, 29, 0));
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

        protected override void Update()
        {
            if (Core.Logic.Penitent != null && Core.Logic.Penitent.IsStickedOnWall)
                CanDoubleJump = false;
        }

        public bool CanDoubleJump { get; private set; }

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
