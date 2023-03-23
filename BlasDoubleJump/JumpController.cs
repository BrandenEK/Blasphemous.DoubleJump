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

        public static bool InLoadProcess { get; private set; }

        public string ItemPersId => "RE402-ITEM";
        public string ItemFlag => "RE402_COLLECTED";

        private bool LoadedObjects { get; set; }
        private GameObject itemObject;

        protected override void Initialize()
        {
            RegisterItem(new PurifiedHand().AddEffect<DoubleJumpEffect>());
            DisableFileLogging = true;
        }

        protected override void LevelLoaded(string oldLevel, string newLevel)
        {
            // When game is first started, load objects
            if (newLevel == "MainMenu" && !LoadedObjects)
            {
                UIController.instance.StartCoroutine(LoadCollectibleItem("D02Z02S14_LOGIC"));
                LoadedObjects = true;
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

        private void CreateCollectibleItem(string persistentId, string itemId, Vector3 position)
        {
            if (itemObject == null) return;

            GameObject newItem = Object.Instantiate(itemObject, GameObject.Find("INTERACTABLES").transform);
            newItem.SetActive(true);
            newItem.transform.position = position;
            newItem.GetComponent<UniqueId>().uniqueId = persistentId;

            InteractableInvAdd addComponent = newItem.GetComponent<InteractableInvAdd>();
            addComponent.item = itemId;
            addComponent.itemType = GetItemType(itemId);

            // Hopefully can remove this once the actual pers id is used
            CollectibleItem collectComponent = newItem.GetComponent<CollectibleItem>();
            bool collected = Core.Events.GetFlag(ItemFlag);
            collectComponent.Consumed = collected;
            collectComponent.transform.GetChild(2).gameObject.SetActive(!collected);
        }

        private InventoryManager.ItemType GetItemType(string id)
        {
            if (id != null && id.Length >= 2)
            {
                switch (id.Substring(0, 2))
                {
                    case "RB": return InventoryManager.ItemType.Bead;
                    case "PR": return InventoryManager.ItemType.Prayer;
                    case "RE": return InventoryManager.ItemType.Relic;
                    case "HE": return InventoryManager.ItemType.Sword;
                    case "QI": return InventoryManager.ItemType.Quest;
                    case "CO": return InventoryManager.ItemType.Collectible;
                }
            }
            LogError("Could not determine item type for " + id);
            return InventoryManager.ItemType.Bead;
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

        private IEnumerator LoadCollectibleItem(string sceneName)
        {
            InLoadProcess = true;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            Scene tempScene = SceneManager.GetSceneByName(sceneName);
            foreach (GameObject obj in tempScene.GetRootGameObjects())
            {
                if (obj.name == "LOGIC")
                {
                    // This is the logic object
                    CollectibleItem item = obj.GetComponentInChildren<CollectibleItem>();
                    itemObject = Object.Instantiate(item.gameObject, Main.Transform);
                    itemObject.SetActive(false);
                }

                obj.SetActive(false);
                //Object.DestroyImmediate(obj);
            }

            yield return null;

            asyncLoad = SceneManager.UnloadSceneAsync(tempScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            Camera.main.transform.position = new Vector3(0, 0, -10);
            Camera.main.backgroundColor = new Color(0, 0, 0, 1);

            InLoadProcess = false;
        }
    }
}
