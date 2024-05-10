namespace Celeste.Mod.StaminaDisplay;

public class StaminaDisplayModule : EverestModule
{
    public override void Load()
    {
        On.Celeste.Player.Added += Player_Added;
    }

    private void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
    {
        orig(self, scene);
        scene.Add(new StaminaDisplay());
        Console.WriteLine("hi");
    }

    public override void Unload()
    {
        On.Celeste.Player.Added -= Player_Added;
    }
}


// Token: 0x02000322 RID: 802
public class StaminaDisplay : Entity
{
    // Token: 0x04001609 RID: 5641
    private Player player;

    // Token: 0x0400160A RID: 5642
    private float drawStamina;

    // Token: 0x0400160B RID: 5643
    private float displayTimer;

    // Token: 0x0400160C RID: 5644
    private Level level;

    // Token: 0x0600194E RID: 6478 RVA: 0x00029DAC File Offset: 0x00027FAC
    public StaminaDisplay()
    {
    }

    // Token: 0x0600194F RID: 6479 RVA: 0x000A2809 File Offset: 0x000A0A09
    public override void Added(Scene scene)
    {
        base.Added(scene);
        this.level = base.SceneAs<Level>();
        this.player = scene.Tracker.GetEntity<Player>();
        this.drawStamina = this.player.Stamina;
        this.Tag |= Tags.HUD;
    }

    // Token: 0x06001950 RID: 6480 RVA: 0x000A283C File Offset: 0x000A0A3C
    public override void Update()
    {
        base.Update();
        this.drawStamina = Calc.Approach(this.drawStamina, this.player.Stamina, 300f * Engine.DeltaTime);
        if (this.drawStamina < 110f && this.drawStamina > 0f)
        {
            this.displayTimer = 0.75f;
            return;
        }
        if (this.displayTimer > 0f)
        {
            this.displayTimer -= Engine.DeltaTime;
        }
    }

    // Token: 0x06001951 RID: 6481 RVA: 0x000A28BC File Offset: 0x000A0ABC
    public override void Render()
    {
        if (this.displayTimer > 0f)
        {
            Vector2 vector = this.level.Camera.CameraToScreen(this.player.Position + new Vector2(0f, -18f)) * 6f;
            Color color;
            if (this.drawStamina < 20f)
            {
                color = Color.Red;
            }
            else
            {
                color = Color.Lime;
            }
            Draw.Rect(vector.X - 48f - 1f, vector.Y - 6f - 1f, 98f, 14f, Color.Black);
            Draw.Rect(vector.X - 48f, vector.Y - 6f, 96f * (this.drawStamina / 110f), 12f, color);
        }
    }
}