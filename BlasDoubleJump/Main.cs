using BepInEx;

namespace BlasDoubleJump
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.damocles.blasphemous.modding-api", "1.3.2")]
    [BepInProcess("Blasphemous.exe")]
    public class Main : BaseUnityPlugin
    {
        public static JumpController JumpController { get; private set; }

        private void Start()
        {
            JumpController = new JumpController(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION);
        }
    }
}