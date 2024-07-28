namespace Celeste.Mod.ChroniaHelperIndicatorZone;

partial class PlayerIndicatorZone
{
    [Tracked]
    public sealed class IconRenderer : Entity
    {
        private List<MTexture> icons;
        private List<Vector2> iconOffsets;
        private List<Color> iconColors;

        public IconRenderer()
        {
            Tag = Tags.Global;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            var session = ChroniaHelperIndicatorZoneModule.Instance.Session;
            session.ProcessZoneSaves();
            icons = session.Icons;
            iconOffsets = session.IconOffsets;
            iconColors = session.IconColors;
            Depth = session.ZoneDepth;
        }

        public void SwitchToHandle(PlayerIndicatorZone zone)
        {
            var session = ChroniaHelperIndicatorZoneModule.Instance.Session;
            if (zone is not null)
            {
                icons = zone.Icons;
                iconOffsets = zone.IconOffsets;
                iconColors = zone.IconColors;
                Depth = zone.Depth;
                session.RecordZoneSave(zone);
            }
            else
            {
                icons = null;
                iconOffsets = null;
                iconColors = null;
                session.RecordZoneSave(null);
            }
        }

        public override void Render()
        {
            base.Render();
            if (icons is null) return;
            var player = Scene.Tracker.GetEntity<Player>();
            if (player is null) return;
            DrawIcons(player.Position, icons, iconOffsets, iconColors);
        }
    }
}
