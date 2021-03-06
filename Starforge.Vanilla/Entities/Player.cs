﻿using Microsoft.Xna.Framework;
using Starforge.Map;
using Starforge.Mod.API;
using Starforge.Mod.Content;
using Starforge.Util;
using System;

namespace Starforge.Vanilla.Entities {
    [EntityDefinition("player")]
    public class Player : Entity {
        private static Lazy<DrawableTexture> Texture = new Lazy<DrawableTexture>(() => GFX.Gameplay["characters/player/sitDown00"]);

        public Player(EntityData data, Room room) : base(data, room) { }

        public override void Render() {
            Vector2 pos = Position;
            pos.Y -= 16f;
            Texture.Value.DrawCentered(pos);
        }

        public override Rectangle Hitbox => MiscHelper.RectangleCentered(Position.X, Position.Y - 16f, Texture.Value.Width, Texture.Value.Height);

        public static PlacementList Placements = new PlacementList()
        {
            new Placement("Player (Spawn Point)")
        };

    }
}
