using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.WhatMod;

public sealed class WhatModModule : EverestModule
{
    private static EventInstance soundIns;
    private Hook canPauseHook;

    public static WhatModModule Instance { get; private set; }

    public override Type SettingsType => typeof(WhatModSettings);
    public static WhatModSettings Settings => (WhatModSettings)Instance._Settings;

    public static MTexture WhatTexture { get; private set; }
    public static bool What { get; private set; }

    public override void Load()
    {
        Instance = this;
        On.Celeste.Level.Update += Level_Update;
        On.Celeste.Level.Render += Level_Render;
        On.Celeste.HudRenderer.RenderContent += HudRenderer_RenderContent;
        canPauseHook = new Hook(typeof(Level).GetProperty("CanPause").GetGetMethod(), Level_get_CanPause);
    }

    public override void Unload()
    {
        On.Celeste.Level.Update -= Level_Update;
        On.Celeste.Level.Render -= Level_Render;
        On.Celeste.HudRenderer.RenderContent -= HudRenderer_RenderContent;
        canPauseHook.Dispose();
    }

    public override void LoadContent(bool firstLoad)
    {
        base.LoadContent(firstLoad);
        WhatTexture = GFX.Gui["whatmod_what"];
    }

    private delegate bool orig_get_CanPause(Level self);
    private static bool Level_get_CanPause(orig_get_CanPause orig, Level self)
    {
        return !What && orig(self);
    }

    private static void HudRenderer_RenderContent(On.Celeste.HudRenderer.orig_RenderContent orig, HudRenderer self, Scene scene)
    {
        if (What)
        {
            HiresRenderer.BeginRender();
            WhatTexture.DrawCentered(new Vector2(1920, 1080) / 2f + new Vector2(0, 472));
            HiresRenderer.EndRender();
        }
        else
        {
            orig(self, scene);
        }
    }

    private static void Level_Render(On.Celeste.Level.orig_Render orig, Level self)
    {
        if (!What)
        {
            orig(self);
            return;
        }
        float p = self.Zoom;
        float pt = self.ZoomTarget;
        Vector2 pv = self.ZoomFocusPoint;

        self.Zoom = 0.75f;
        self.ZoomTarget = 0.5f;
        self.ZoomFocusPoint = new Vector2(320f, 180f) / 2f;

        orig(self);

        self.Zoom = p;
        self.ZoomTarget = pt;
        self.ZoomFocusPoint = pv;
    }

    private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        if (Settings.What.Check)
        {
            if (!What && !self.Paused)
            {
                What = true;
                if (soundIns is not null) Audio.Stop(soundIns);
                soundIns = Audio.Play("event:/whatmod/what");
                Audio.MusicVolume = 0f;
                Audio.SfxVolume = 0f;
                if (Settings.PauseWhenWhat)
                    self.Paused = true;
            }
        }
        else
        {
            if (What)
            {
                What = false;
                if (soundIns is not null)
                    Audio.Stop(soundIns);
                global::Celeste.Settings.Instance.ApplyMusicVolume();
                global::Celeste.Settings.Instance.ApplySFXVolume();
                if (Settings.PauseWhenWhat)
                    self.Paused = false;
            }
        }
        orig(self);
    }
}