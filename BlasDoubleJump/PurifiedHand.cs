using ModdingAPI.Items;
using UnityEngine;

namespace BlasDoubleJump
{
    public class PurifiedHand : ModRelic
    {
        protected override string Id => "RE402";

        protected override string Name => Main.JumpController.Localize("itmnam");

        protected override string Description => Main.JumpController.Localize("itmdes");

        protected override string Lore => Main.JumpController.Localize("itmlor");

        protected override bool CarryOnStart => false;

        protected override bool PreserveInNGPlus => true;

        protected override bool AddToPercentCompletion => false;

        protected override bool AddInventorySlot => true;

        protected override void LoadImages(out Sprite picture)
        {
            picture = Main.JumpController.FileUtil.loadDataImages("hand.png", 30, 30, 32, 0, true, out Sprite[] images) ? images[0] : null;
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
}
