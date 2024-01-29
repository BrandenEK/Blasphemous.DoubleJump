using BepInEx;

namespace Blasphemous.DoubleJump;

[BepInPlugin(ModInfo.MOD_ID, ModInfo.MOD_NAME, ModInfo.MOD_VERSION)]
[BepInDependency("Blasphemous.ModdingAPI", "2.0.1")]
public class Main : BaseUnityPlugin
{
    public static JumpController JumpController { get; private set; }

    private void Start()
    {
        JumpController = new JumpController();
    }
}
