using Celeste.Mod.CNY2024Helper.Legacy;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CNY2024Helper.Legacy;

[Tracked]
[CustomEntity("CNY2024Helper/IHPHKDialogEntity")]
public class IHPHKDialogEntity : Entity
{
    private static EntityData entityData;

    public Sprite Sprite;

    public string SpriteName;
    public string SpriteAnim;
    public string DialogId;
    public string Name;
    public bool RandomPlay, SpriteOutline;
    public int SpriteScaleX, SpriteScaleY;

    private int DialogIndex;
    private BirdTutorialGui gui;
    private string[] Dialogs;

    public IHPHKDialogEntity(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        entityData = data;
        this.Depth = data.Int("depth", 100000);
        this.Name = data.Attr("name", "ChineseNewYear2024Helper_OPTIMIZE2_IHPHKDIALOGENTITY_EXAMPLE_NAME");
        this.SpriteName = data.Attr("spriteName", "glider");
        this.SpriteAnim = data.Attr("spriteAnim", "idle");
        this.SpriteScaleX = data.Int("spriteScaleX", 1);
        this.SpriteScaleY = data.Int("spriteScaleY", 1);
        this.SpriteOutline = data.Bool("spriteOutline", true);
        this.DialogId = data.Attr("dialogId", "ChineseNewYear2024Helper_OPTIMIZE2_IHPHKDIALOGENTITY_EXAMPLE_DIALOG");
        this.RandomPlay = data.Bool("randomPlay", false);

        this.Dialogs = Dialog.Get(DialogId).Replace("{break}", "\n").Split('\n');
        base.Add(this.Sprite = GFX.SpriteBank.Create(this.SpriteName));
        this.Sprite.Scale = new Vector2(this.SpriteScaleX, this.SpriteScaleY);
        this.Sprite.Play(this.SpriteAnim);
    }

    public string GetDialog()
    {
        if (this.RandomPlay) return this.Dialogs[Calc.Random.Range(0, this.Dialogs.Length - 1)];
        string result = this.Dialogs[this.DialogIndex].Trim();
        this.DialogIndex++;
        if (this.DialogIndex == this.Dialogs.Length) this.DialogIndex = 0;
        return result;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (scene.Tracker.GetEntity<IHPHKRenderer>() == null)
            scene.Add(new IHPHKRenderer());
    }

    public override void Update()
    {
        if (this == IHPHKRenderer.DialogEntityFaced)
        {
            if (this.gui == null)
            {
                this.gui = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean(this.Name, null), new object[] { BirdTutorialGui.ButtonPrompt.Talk });
                this.gui.Active = true;
                this.gui.Open = true;
                base.SceneAs<Level>().Add(this.gui);
            }
        }
        else
        {
            if (this.gui != null)
            {
                this.gui.RemoveSelf();
                this.gui = null;
            }
        }
        base.Update();
    }

    public override void Render()
    {
        if (this == IHPHKRenderer.DialogEntityFaced)
        {
            this.Sprite.DrawOutline(Color.White, 1);
        }
        else
        {
            if (this.SpriteOutline) this.Sprite.DrawSimpleOutline();
        }
        base.Render();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}