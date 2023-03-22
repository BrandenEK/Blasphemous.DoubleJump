using ModdingAPI;
using Framework.Managers;
using Framework.Inventory;
using Framework.Util;
using UnityEngine;
using Gameplay.GameControllers.Effects.Player.Dust;

namespace BlasDoubleJump
{
    public class JumpController : Mod
    {
        public JumpController(string modId, string modName, string modVersion) : base(modId, modName, modVersion) { }

        public string ItemPersId => "RE402-ITEM";
        public string ItemFlag => "RE402_COLLECTED";

        private GameObject itemObject;

        protected override void Initialize()
        {
            RegisterItem(new PurifiedHand().AddEffect<DoubleJumpEffect>());
            DisableFileLogging = true;
        }

        protected override void LevelLoaded(string oldLevel, string newLevel)
        {
            if (itemObject == null)
            {
                // Load item object from resources
                InteractableInvAdd[] items = Resources.FindObjectsOfTypeAll<InteractableInvAdd>();
                foreach (InteractableInvAdd item in items)
                {
                    LogWarning(item.name + ": " + item.item);
                    if (item.name == "ACT_OrbCollectible")
                    {
                        //itemObject = item.gameObject;
                        Log("Loaded collectible item object");
                    }
                }
                if (itemObject == null) return;
            }
            
            if (newLevel != "D04Z02S01") return;

            GameObject newItem = Object.Instantiate(itemObject, GameObject.Find("INTERACTABLES").transform);
            newItem.transform.position = new Vector3(233, 29, 0);
            newItem.GetComponent<UniqueId>().uniqueId = ItemPersId;

            InteractableInvAdd addComponent = newItem.GetComponent<InteractableInvAdd>();
            addComponent.item = "RE402";
            addComponent.itemType = InventoryManager.ItemType.Relic;

            CollectibleItem collectComponent = newItem.GetComponent<CollectibleItem>();
            bool collected = Core.Events.GetFlag(ItemFlag);
            collectComponent.Consumed = collected;
            collectComponent.transform.GetChild(2).gameObject.SetActive(!collected);
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

            if (Input.GetKeyDown(KeyCode.P))
                LogError(Core.Logic.Penitent.transform.position.ToString());
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
