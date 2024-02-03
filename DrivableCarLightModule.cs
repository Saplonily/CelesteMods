using Celeste.Mod.PandorasBox;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.CNY2024Helper;

public static class DrivableCarLightModule
{
    private static Hook drivableCarCtorHook;
    private static Hook drivableCarUpdateHook;

    public static void Load()
    {
        MethodInfo miAdded = typeof(DrivableCar).GetMethod("Added");
        drivableCarCtorHook = new(miAdded, DrivableCar_Added_hook);
        MethodInfo miUpdate = typeof(DrivableCar).GetMethod("Update");
        drivableCarUpdateHook = new(miUpdate, DrivableCar_Update_hook);
    }

    public delegate void DrivableCar_Added_orig(DrivableCar self, Scene scene);
    public delegate void DrivableCar_Update_orig(DrivableCar self);
    public static void DrivableCar_Added_hook(DrivableCar_Added_orig orig, DrivableCar self, Scene scene)
    {
        orig(self, scene);
        if (scene is Level
            {
                Session.MapData.Data.SID:
#if DEBUG
            "Saplonily/TestMap" or
#endif
            "ChineseNewYear2024/1-Maps/ZZ-HeartSide"
            })
        {
            DynamicData dd = DynamicData.For(self);

            Component[] gcs = new Component[10];

            // right
            int lightPosX = 16;
            int lightPosY = -8;
            self.Add(gcs[0] = new BloomPoint(new(lightPosX, lightPosY), 0.5f, 24f));
            self.Add(gcs[1] = new VertexLight(new(lightPosX, lightPosY), Color.White, 1f, 96, 200));
            self.Add(gcs[2] = new LightOcclude(new Rectangle(lightPosX - 2, lightPosY - 3, 1, 7)));
            self.Add(gcs[3] = new LightOcclude(new Rectangle(lightPosX - 3, lightPosY - 3, 7, 1)));
            self.Add(gcs[4] = new LightOcclude(new Rectangle(lightPosX - 3, lightPosY + 3, 7, 1)));

            // left
            lightPosX = -16;
            lightPosY = -8;
            self.Add(gcs[5] = new BloomPoint(new(lightPosX, lightPosY), 0.5f, 24f));
            self.Add(gcs[6] = new VertexLight(new(lightPosX, lightPosY), Color.White, 1f, 96, 200));
            self.Add(gcs[7] = new LightOcclude(new Rectangle(lightPosX + 2, lightPosY - 3, 1, 7)));
            self.Add(gcs[8] = new LightOcclude(new Rectangle(lightPosX - 3, lightPosY - 3, 7, 1)));
            self.Add(gcs[9] = new LightOcclude(new Rectangle(lightPosX - 3, lightPosY + 3, 7, 1)));

            for (int i = 5; i < 10; i++)
                gcs[i].Active = gcs[i].Visible = false;

            dd.Set("cny2024_car_light_components", gcs);
        }
    }

    public static void DrivableCar_Update_hook(DrivableCar_Update_orig orig, DrivableCar self)
    {
        orig(self);
        if (self.SceneAs<Level>() is
            {
                Session.MapData.Data.SID:
#if DEBUG
            "Saplonily/TestMap" or
#endif
            "ChineseNewYear2024/1-Maps/ZZ-HeartSide"
            })
        {
            DynamicData dd = DynamicData.For(self);
            int facing = (int)dd.Get("facing");
            Component[] gcs = (Component[])dd.Get("cny2024_car_light_components");
            if (facing == 1)
            {
                for (int i = 0; i < 5; i++)
                    gcs[i].Active = gcs[i].Visible = true;
                for (int i = 5; i < 10; i++)
                    gcs[i].Active = gcs[i].Visible = false;
            }
            else if (facing == -1)
            {
                for (int i = 0; i < 5; i++)
                    gcs[i].Active = gcs[i].Visible = false;
                for (int i = 5; i < 10; i++)
                    gcs[i].Active = gcs[i].Visible = true;
            }
        }
    }

    public static void Unload()
    {
        drivableCarCtorHook.Dispose();
        drivableCarUpdateHook.Dispose();
    }
}
