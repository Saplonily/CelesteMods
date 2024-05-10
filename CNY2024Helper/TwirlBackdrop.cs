using System.Linq;
using Celeste.Mod.Backdrops;
using static Celeste.TrackSpinner;

namespace Celeste.Mod.CNY2024Helper.Backdrops;

[CustomBackdrop("CNY2024Helper/TwirlBackdrop")]
public class TwirlBackdrop : Backdrop
{
    private float speed;
    private MTexture[] twirlTextures;
    private Vector2[] twirlPositions;
    private float[] twirlSpeeds;
    private Entity entityAttached;

    public TwirlBackdrop(BinaryPacker.Element data)
    {
        Color = Color.White;
        speed = data.AttrFloat("speed", 1f);
        twirlTextures = new MTexture[15];
        for (int i = 0; i < 15; i++)
            twirlTextures[i] = GFX.Misc[$"CNY2024Helper/twirl{(char)(i + 'A')}"];

        twirlPositions = [
            new Vector2(160, 84),
            new Vector2(158, 84),
            new Vector2(149, 84),
            new Vector2(136, 84),
            new Vector2(125, 84),
            new Vector2(110, 84),
            new Vector2(97, 84),
            new Vector2(87, 84),
            new Vector2(81, 84),
            new Vector2(77, 84),
            new Vector2(71, 84),
            new Vector2(69, 84),
            new Vector2(65, 84),
            new Vector2(62, 84),
            new Vector2(58, 84),
        ];

        twirlSpeeds = [900, 840, 780, 720, 660, 600, 540, 480, 420, 360, 300, 240, 180, 120, 60];
        int length = twirlSpeeds.Length;
        for (int i = 0; i < length; i++)
        {
            float v = twirlSpeeds[i];
            v /= 30f;
            v = 1f / v;
            twirlSpeeds[i] = v;
        }

        entityAttached = new Entity();
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        if (!ReferenceEquals(entityAttached.Scene, scene))
        {
            entityAttached.RemoveSelf();
            scene.Add(entityAttached);
        }
    }

    public override void Render(Scene scene)
    {
        base.Render(scene);
        int length = twirlTextures.Length;
        for (int i = 0; i < length; i++)
        {
            twirlTextures[i].DrawCentered(twirlPositions[i], Color, 1f, scene.TimeActive * twirlSpeeds[i] * speed);
        }
    }
}