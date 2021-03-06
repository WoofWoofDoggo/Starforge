﻿using Microsoft.Xna.Framework;
using Starforge.Map;
using Starforge.Mod.API;
using Starforge.Mod.API.Properties;
using Starforge.Mod.Content;
using Starforge.Util;

namespace Starforge.Vanilla.Entities {
    [EntityDefinition("lightning")]
    public class Lightning : Entity {
        private static readonly Color BgColor = MiscHelper.HexToColor("fcf579") * 0.2f;
        private static readonly Color OutlineColor = MiscHelper.HexToColor("fcf579");

        public override bool StretchableX => true;
        public override bool StretchableY => true;

        public Lightning(EntityData data, Room room) : base(data, room) { }

        public override void Render() {
            Rectangle renderPos = new Rectangle((int)Position.X, (int)Position.Y, GetInt("width", 8), GetInt("height", 8));

            GFX.Pixel.Draw(renderPos, BgColor);
            GFX.Draw.HollowRectangle(renderPos, OutlineColor);
        }

        public static PlacementList Placements = new PlacementList()
        {
            new Placement("Lightning")
        };

        public override PropertyList Properties => new PropertyList() {
            new FloatProperty("moveTime", 1f, "How long the lightning takes for one movement cycle")
        };

    }
}
