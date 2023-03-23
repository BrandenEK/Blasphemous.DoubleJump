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

        private GameObject itemObject;

        protected override void Initialize()
        {
            RegisterItem(new PurifiedHand().AddEffect<DoubleJumpEffect>());
            DisableFileLogging = true;
        }

        protected override void LevelLoaded(string oldLevel, string newLevel)
        {
            LogWarning("Loaded level");
            if (newLevel == "MainMenu" && itemObject == null)
            {
                UIController.instance.StartCoroutine(LoadCollectibleItem("D02Z02S14_LOGIC"));
                //LoadCollectibleItem("D02Z02S14_LOGIC");
            }

            if (itemObject == null || newLevel != "D04Z02S01") return;

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

            GameObject newItem = Object.Instantiate(itemObject, GameObject.Find("INTERACTABLES").transform);
            newItem.SetActive(true);
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

        private IEnumerator LoadCollectibleItem(string sceneName)
        {
            InLoadProcess = true;
            LogWarning("Starting corroutine");

            LogError(Camera.main.transform.position.ToString());
            LogError(Camera.main.backgroundColor.ToString());

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            //SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

            Scene tempScene = SceneManager.GetSceneByName(sceneName);
            LogWarning("Loaded temp scene");

            foreach (GameObject obj in tempScene.GetRootGameObjects())
            {
                if (obj.name == "LOGIC")
                {
                    LogWarning("Duplicating item object");
                    // This is the logic object
                    CollectibleItem item = obj.GetComponentInChildren<CollectibleItem>();
                    itemObject = Object.Instantiate(item.gameObject, Main.Transform);
                    itemObject.SetActive(false);
                }

                if (obj.name == "CAMERAS")
                {
                    foreach (Component c in obj.transform.GetChild(0).GetComponents<Component>())
                        LogWarning(c.ToString());
                }

                obj.SetActive(false);
                Object.DestroyImmediate(obj);
            }
            LogWarning("Disabled all");

            yield return null;

            asyncLoad = SceneManager.UnloadSceneAsync(tempScene);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            LogWarning("unloaded temp scene");

            Camera.main.transform.position = new Vector3(0, 0, -10);
            Camera.main.backgroundColor = new Color(0, 0, 0, 1);
            LogError(Camera.main.transform.position.ToString());
            LogError(Camera.main.backgroundColor.ToString());
            LogWarning("Scenes: " + SceneManager.sceneCount);
            InLoadProcess = false;
        }

        //private IEnumerator LoadCollectibleItem(string sceneName)
        //{
        //    InLoadProcess = true;
        //    LogWarning("Starting corroutine");
        //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        //    while (!asyncLoad.isDone)
        //    {
        //        yield return null;
        //    }

        //    LogWarning("Loaded temp scene");

        //    Scene tempScene = SceneManager.GetSceneByName(sceneName);
        //    foreach (GameObject obj in tempScene.GetRootGameObjects())
        //    {
        //        if (obj.name == "LOGIC")
        //        {
        //            LogWarning("Duplicating item object");
        //            // This is the logic object
        //            CollectibleItem item = obj.GetComponentInChildren<CollectibleItem>();
        //            itemObject = Object.Instantiate(item.gameObject, Main.Transform);
        //            itemObject.SetActive(false);
        //        }
        //        obj.SetActive(false);
        //    }
        //    LogWarning("Disabled all");

        //    asyncLoad = SceneManager.UnloadSceneAsync(tempScene);
        //    while (!asyncLoad.isDone)
        //    {
        //        yield return null;
        //    }
        //    LogWarning("unloaded temp scene");
        //    InLoadProcess = false;
        //}
    }
}
