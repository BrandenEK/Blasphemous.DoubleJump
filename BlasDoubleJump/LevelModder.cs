using Framework.Managers;
using Framework.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace BlasDoubleJump
{
    public static class LevelModder
    {
        private const string ITEM_SCENE = "D02Z02S14_LOGIC";

        public static bool InLoadProcess { get; private set; }
        public static bool LoadedObjects { get; private set; }

        private static GameObject itemObject;

        public static void LoadObjects()
        {
            itemObject = null;
            Main.Instance.StartCoroutine(LoadCollectibleItem(ITEM_SCENE));

            LoadedObjects = true;
            if (itemObject == null)
                Main.JumpController.LogError("Failed to load item object!");
        }

        public static void CreateCollectibleItem(string persistentId, string itemId, Vector3 position)
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

        private static InventoryManager.ItemType GetItemType(string id)
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

        private static IEnumerator LoadCollectibleItem(string sceneName)
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
                    itemObject = Object.Instantiate(item.gameObject, Main.Instance.transform);
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
