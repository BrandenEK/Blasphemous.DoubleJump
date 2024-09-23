using BepInEx;

namespace Blasphemous.DoubleJump;

[BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
[BepInDependency("Blasphemous.ModdingAPI", "2.4.1")]
[BepInDependency("Blasphemous.Framework.Items", "0.1.1")]
[BepInDependency("Blasphemous.Framework.Levels", "0.1.4")]
internal class Main : BaseUnityPlugin
{
    public static JumpController JumpController { get; private set; }

    private void Start()
    {
        JumpController = new JumpController();
    }
}
