using Blasphemous.ModdingAPI.Items;
using UnityEngine;

namespace Blasphemous.DoubleJump;

public class PurifiedHand : ModRelic
{
    protected override string Id => "RE402";

    protected override string Name => Main.JumpController.LocalizationHandler.Localize("djname");

    protected override string Description => Main.JumpController.LocalizationHandler.Localize("djdesc");

    protected override string Lore => Main.JumpController.LocalizationHandler.Localize("djlore");

    protected override bool CarryOnStart => false;

    protected override bool PreserveInNGPlus => true;

    protected override bool AddToPercentCompletion => false;

    protected override bool AddInventorySlot => true;

    protected override void LoadImages(out Sprite picture)
    {
        Main.JumpController.FileHandler.LoadDataAsSprite("hand.png", out picture);
    }
}

public class DoubleJumpEffect : ModItemEffectOnEquip
{
    protected override void ApplyEffect()
    {
        Main.JumpController.AllowDoubleJump = true;
    }

    protected override void RemoveEffect()
    {
        Main.JumpController.AllowDoubleJump = false;
    }
}
