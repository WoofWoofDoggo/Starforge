﻿using Starforge.Editor;
using Starforge.Editor.Render;
using Starforge.Map;
using Starforge.Mod.API;

namespace Starforge.Vanilla.Entities {
    [EntityDefinition("dashBlock")]
    class DashBlock : Entity {
        public DashBlock(EntityData data, Room room) : base(data, room) { }

        public override bool StretchableX => true;
        public override bool StretchableY => true;

        public override void Render() {
            TextureMap map = MapEditor.Instance.FGAutotiler.GenerateFakeTileMap(Room, Position, Width / 8, Height / 8, (short)GetChar("tiletype", '3')); ;
            for (int i = 0; i < map.Textures.Length; i++) map.Textures[i].Position += Position;
            map.Draw();
        }

        public static PlacementList Placements = new PlacementList()
        {
            new Placement("Dash Block (Dirt)") {
                ["tiletype"] = '1'
            },
            new Placement("Dash Block (Ice)") {
                ["tiletype"] = '3'
            },
            new Placement("Dash Block (Stone)") {
                ["tiletype"] = '6'
            },
            new Placement("Dash Block (Wood)") {
                ["tiletype"] = '9'
            }
        };

        public override PropertyList Properties => new PropertyList() {
            new Property("tiletype", PropertyType.Char, "The tile the block is. 1 is dirt, 3 is ice, 6 is stone and 9 is wood"),
            new Property("blendin", PropertyType.Bool, ""),
            new Property("permanent", PropertyType.Bool, ""),
            new Property("canDash", PropertyType.Bool, "")
        };

    }
}
