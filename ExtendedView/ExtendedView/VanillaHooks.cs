using System.Collections;
using MonoMod.RuntimeDetour;
using static Celeste.Mod.ExtendedView.ExtendedViewModule;

namespace Celeste.Mod.ExtendedView;

public static class VanillaHooks
{
    private static ILHook cameraTargetILhook;
    private static ILHook ParallaxOrigRenderILhook;

    public static void Load()
    {
        On.Celeste.GameplayBuffers.Create_int_int += GameplayBuffers_Create_int_int;
        IL.Celeste.GameplayRenderer.ctor += GameplayRenderer_ctor;
        On.Celeste.Level.Update += Level_Update;
        IL.Celeste.Level.Render += Level_Render;
        IL.Celeste.BlackholeBG.BeforeRender += Replace320x180;
        IL.Celeste.BlackholeBG.ctor += Replace320x180;
        IL.Celeste.BlackholeBG.Update += Replace320x180;
        IL.Celeste.LightningRenderer.OnRenderBloom += Replace320x180;
        IL.Celeste.BloomRenderer.Apply += BloomRenderer_Apply;
        IL.Celeste.LightingRenderer.BeforeRender += Replace320x180;
        IL.Celeste.Parallax.Render += Replace320x180;
        IL.Celeste.Audio.Position += Replace320x180;
        IL.Celeste.Godrays.Update += Godrays_Update;
        IL.Celeste.CrystalStaticSpinner.InView += Replace320x180;
        On.Celeste.Audio.Position += Audio_Position;
        IL.Celeste.WindSnowFG.ctor += WindSnowFG_ctor;
        IL.Celeste.StardustFG.Render += Replace320x180;
        IL.Celeste.StardustFG.Reset += Replace320x180;
        On.Celeste.LightningRenderer.Bolt.ctor += Bolt_ctor;
        IL.Celeste.Lightning.InView += Replace320x180;
        IL.Celeste.Level.EnforceBounds += Replace320x180;
        On.Celeste.Level.ZoomTo += Level_ZoomTo;
        On.Celeste.Level.ZoomBack += Level_ZoomBack;
        On.Celeste.MapData.ParseBackdrop += MapData_ParseBackdrop;
        On.Celeste.Torch.Added += Torch_Added;
        IL.Celeste.StarsBG.Render += Replace320x180;
        IL.Celeste.StarsBG.ctor += Replace320x180;
        IL.Celeste.CoreStarsFG.Render += Replace320x180;
        IL.Celeste.CoreStarsFG.Reset += Replace320x180;
        IL.Celeste.TalkComponent.TalkComponentUI.Render += Clear;
        IL.Celeste.Starfield.Render += Starfield_Render;
        IL.Celeste.Starfield.ctor += Replace320x180;
        var method = typeof(Player).GetProperty("CameraTarget").GetGetMethod();
        cameraTargetILhook = new(method, Replace320x180);
        method = typeof(Parallax).GetMethod("orig_Render");
        ParallaxOrigRenderILhook = new(method, Replace320x180);
    }

    private static void Starfield_Render(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(448f) || ins.MatchLdcR4(212f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
        cur.Index = 0;
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(-64f) || ins.MatchLdcR4(-16f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
    }

    public static void Unload()
    {
        On.Celeste.GameplayBuffers.Create_int_int -= GameplayBuffers_Create_int_int;
        IL.Celeste.GameplayRenderer.ctor -= GameplayRenderer_ctor;
        On.Celeste.Level.Update -= Level_Update;
        IL.Celeste.Level.Render -= Level_Render;
        IL.Celeste.BlackholeBG.BeforeRender -= Replace320x180;
        IL.Celeste.BlackholeBG.ctor -= Replace320x180;
        IL.Celeste.BlackholeBG.Update -= Replace320x180;
        IL.Celeste.LightningRenderer.OnRenderBloom -= Replace320x180;
        IL.Celeste.BloomRenderer.Apply -= BloomRenderer_Apply;
        IL.Celeste.LightingRenderer.BeforeRender -= Replace320x180;
        IL.Celeste.Parallax.Render -= Replace320x180;
        IL.Celeste.Audio.Position -= Replace320x180;
        IL.Celeste.CrystalStaticSpinner.InView -= Replace320x180;
        On.Celeste.Audio.Position -= Audio_Position;
        IL.Celeste.WindSnowFG.ctor -= WindSnowFG_ctor;
        IL.Celeste.StardustFG.Render -= Replace320x180;
        IL.Celeste.StardustFG.Reset -= Replace320x180;
        On.Celeste.LightningRenderer.Bolt.ctor -= Bolt_ctor;
        IL.Celeste.Lightning.InView -= Replace320x180;
        IL.Celeste.Level.EnforceBounds -= Replace320x180;
        On.Celeste.Level.ZoomTo -= Level_ZoomTo;
        On.Celeste.Level.ZoomBack -= Level_ZoomBack;
        On.Celeste.MapData.ParseBackdrop -= MapData_ParseBackdrop;
        On.Celeste.Torch.Added -= Torch_Added;
        IL.Celeste.StarsBG.Render -= Replace320x180;
        IL.Celeste.StarsBG.ctor -= Replace320x180;
        IL.Celeste.CoreStarsFG.Render -= Replace320x180;
        IL.Celeste.CoreStarsFG.Reset -= Replace320x180;
        IL.Celeste.TalkComponent.TalkComponentUI.Render -= Clear;
        IL.Celeste.Starfield.Render -= Starfield_Render;
        IL.Celeste.Starfield.ctor -= Replace320x180;
        cameraTargetILhook?.Dispose();
        ParallaxOrigRenderILhook?.Dispose();
    }

    private static void Torch_Added(On.Celeste.Torch.orig_Added orig, Torch self, Scene scene)
    {
        orig(self, scene);
        self.OnPlayer(null);
    }

    private static Backdrop MapData_ParseBackdrop(On.Celeste.MapData.orig_ParseBackdrop orig, MapData self, BinaryPacker.Element child, BinaryPacker.Element above)
    {
        var drop = orig(self, child, above);
        drop.LoopY = true;
        //drop.LoopX = true;
        return drop;
    }

    private static IEnumerator Level_ZoomBack(On.Celeste.Level.orig_ZoomBack orig, Level self, float duration)
    {
        yield return null;
    }

    private static IEnumerator Level_ZoomTo(On.Celeste.Level.orig_ZoomTo orig, Level self, Vector2 screenSpaceFocusPoint, float zoom, float duration)
    {
        yield return null;
    }

    private static void Level_Render(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(320f) || ins.MatchLdcR4(160f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
        cur.Index = 0;
        if (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdfld<Level>("Zoom")))
        {
            cur.EmitLdcR4(1f / Mul);
            cur.EmitMul();
        }
    }

    private static void Bolt_ctor(On.Celeste.LightningRenderer.Bolt.orig_ctor orig, object self, Color color, float scale, int width, int height)
    {
        orig(self, color, scale, (int)(width * Mul), (int)(height * Mul));
    }

    private static void WindSnowFG_ctor(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(640f) || ins.MatchLdcR4(360f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
    }

    private static void Audio_Position(On.Celeste.Audio.orig_Position orig, FMOD.Studio.EventInstance instance, Vector2 position)
    {
        orig(instance, position);
        if (instance is null) return;
        Audio.attributes3d.position.x /= Mul;
        Audio.attributes3d.position.y /= Mul;
        Audio.attributes3d.position.z /= Mul;
        instance.set3DAttributes(Audio.attributes3d);
        Audio.attributes3d.position.x *= Mul;
        Audio.attributes3d.position.y *= Mul;
        Audio.attributes3d.position.z *= Mul;
    }

    private static void Godrays_Update(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(384f) || ins.MatchLdcR4(244f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
    }

    private static void BloomRenderer_Apply(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(340f) || ins.MatchLdcR4(200f)))
        {
            cur.EmitLdcR4(Mul);
            cur.EmitMul();
        }
    }

    private static void Clear(ILContext il)
    {
        ILCursor cur = new(il);
        cur.Instrs.Clear();
        cur.EmitRet();
    }

    private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        if (Input.CrouchDash.Pressed)
        {
            Input.CrouchDash.ConsumePress();
        }
    }

    private static void GameplayRenderer_ctor(ILContext il)
    {
        ILCursor cur = new(il);
        while (cur.TryGotoNext(MoveType.After, ins => ins.MatchLdcI4(320) || ins.MatchLdcI4(180)))
        {
            cur.EmitLdcI4((int)Mul);
            cur.EmitMul();
        }
    }

    private static VirtualRenderTarget GameplayBuffers_Create_int_int(On.Celeste.GameplayBuffers.orig_Create_int_int orig, int width, int height)
    {
        return orig((int)(width * Mul), (int)(height * Mul));
    }
}
