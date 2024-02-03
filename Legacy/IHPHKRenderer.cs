namespace Celeste.Mod.CNY2024Helper.Legacy;

[Tracked]
public class IHPHKRenderer : Entity
{
    public static IHPHKDialogEntity DialogEntityFaced;

    private float displayTime;
    private string displayDialog = "";

    private Level level;
    private Player player;

    public static string Text, TextPosition, TextColor;

    public IHPHKRenderer() : base(Vector2.Zero)
    {
        Tag |= TagsExt.SubHUD;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void Update()
    {
        DialogEntityFaced = null;
        displayTime += Engine.DeltaTime;
        base.Update();
        DialogEntityFaced = null;
        level = base.SceneAs<Level>();
        if (level == null || level.Paused) return;
        player = level.Tracker.GetEntity<Player>();
        if (player != null && !player.Dead)
        {
            float minDistance = 10f;
            level.Tracker.GetEntities<IHPHKDialogEntity>().ForEach(e =>
            {
                float curDistance = (e.Position - player.Position).Length();
                if (curDistance <= minDistance)
                {
                    minDistance = curDistance;
                    DialogEntityFaced = (IHPHKDialogEntity)e;
                }
            });
            if (this.displayDialog != "")
            {
                int len = this.displayDialog.Length;
                if (this.displayTime > len * 0.06f + 2f)
                {
                    this.displayTime = 0;
                    this.displayDialog = "";
                }
            }
            if (Input.Talk.Pressed && DialogEntityFaced != null)
            {
                this.displayTime = 0;
                this.displayDialog = DialogEntityFaced.GetDialog();
            }
        }
    }

    private static string InsertFormat(string input, int interval, string value)
    {
        for (int i = interval; i < input.Length; i += interval + 1)
            input = input.Insert(i, value);
        return input;
    }

    public override void Render()
    {
        if (level == null || level.Paused) return;
        player = level.Tracker.GetEntity<Player>();
        if (player == null || player.Dead) return;
        if (this.displayDialog != "")
        {
            int len = this.displayDialog.Length;
            string curDisplay = this.displayDialog.Substring(0, (int)(len * Calc.Min(1, this.displayTime / (len * 0.06f))));
            string formated = InsertFormat(curDisplay, 10, "\n");

            ActiveFont.Draw(
                formated,
                (player.Position - Vector2.UnitY * 16f - SceneAs<Level>().Camera.Position.Floor()) * 6f,
                new Vector2(0.5f, 1f),
                Vector2.One,
                Color.White * (1f - Calc.Min(1f, Calc.Max(0f, (displayTime - len * 0.06f - 1.5f) / 0.5f)))
            );
        }
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}