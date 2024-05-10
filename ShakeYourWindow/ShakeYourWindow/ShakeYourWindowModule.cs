using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.ShakeYourWindow;

public class ShakeYourWindowModule : EverestModule
{
    private Hook hook;
    private static Vector2 winPos;
    private static Vector2 shake;

    public override void Load()
    {
        On.Celeste.OuiMainMenu.Enter += OuiMainMenu_Enter;
        On.Celeste.Level.Update += Level_Update;
        Everest.Events.Player.OnSpawn += Player_OnSpawn;
        var method = typeof(Celeste).Assembly.GetType("Celeste.Mod.EverestModuleAssemblyContext")
            .GetMethod("LoadUnmanagedDll", BindingFlags.Instance | BindingFlags.NonPublic);
        hook = new(method, EverestModuleAssemblyContext_LoadUnmanagedDll);
    }

    public delegate IntPtr orig_LoadUnmanagedDll(EverestModuleAssemblyContext self, string name);
    private IntPtr EverestModuleAssemblyContext_LoadUnmanagedDll(orig_LoadUnmanagedDll orig, EverestModuleAssemblyContext self, string name)
    {
        if (name == "user32.dll")
            return IntPtr.Zero;
        else
            return orig(self, name);
    }

    private IEnumerator OuiMainMenu_Enter(On.Celeste.OuiMainMenu.orig_Enter orig, OuiMainMenu self, Oui from)
    {
        yield return new SwapImmediately(orig(self, from));
        var win = FindWindowW(null, "Celeste");
        RECT r = new();
        GetWindowRect(win, ref r);
        winPos = new Vector2(r.left, r.top);
    }

    private void Player_OnSpawn(Player player)
    {
        var shaker = new Shaker(false);
        shaker.OnShake = v =>
        {
            var win = FindWindowW(null, "Celeste");
            shake = shaker.Value * 30f;
            var pos = winPos + shake;
            SetWindowPos(win, 0, (int)pos.X, (int)pos.Y, 0, 0, 0x0001);
        };
        shaker.Interval = 0.02f;
        player.Add(shaker);
    }

    private void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        orig(self);
        var p = self.Tracker.GetEntity<Player>();
        var shaker = p?.Get<Shaker>();
        if (Input.Grab.Pressed)
        {
            shaker?.ShakeFor(1, false);
        }
    }

    public override void Unload()
    {
        On.Celeste.Level.Update -= Level_Update;
        Everest.Events.Player.OnSpawn -= Player_OnSpawn;
        On.Celeste.OuiMainMenu.Enter -= OuiMainMenu_Enter;
        hook.Dispose();
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowPos(nint hwnd, nint hwndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    public static extern int GetWindowRect(nint hWnd, ref RECT lpRect);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern nint FindWindowW(string lpClassName, string lpWindowName);

    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    };
}