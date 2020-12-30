﻿using Microsoft.Xna.Framework;
using Starforge.Core;
using Starforge.Editor.Actions;
using Starforge.Map;
using Starforge.Mod.Content;

namespace Starforge.Editor.Tools {
    public class TileBrushTool : Tool {
        private Rectangle Hint = new Rectangle(0, 0, 8, 8);
        private TileBrushAction Action = null;

        public override string GetName() => "Tiles (Brush)";

        public override void Render() {
            GFX.Draw.BorderedRectangle(Hint, Settings.ToolColor * 0.5f, Settings.ToolColor);
        }

        public override void Update() {
            Hint.X = MapEditor.Instance.State.TilePointer.X * 8;
            Hint.Y = MapEditor.Instance.State.TilePointer.Y * 8;

            if (Input.Mouse.LeftClick) HandleClick();
            if (Input.Mouse.LeftHold) HandleDrag();
            if (Input.Mouse.LeftUnclick) HandleUnclick();
        }

        private void HandleClick() {
            Room r = MapEditor.Instance.State.SelectedRoom;

            switch (ToolManager.SelectedLayer) {
            case ToolLayer.Background:
                Action = new TileBrushAction(r, ToolLayer.Background, ToolManager.BGTileset, MapEditor.Instance.State.TilePointer);
                break;
            case ToolLayer.Foreground:
                Action = new TileBrushAction(r, ToolLayer.Foreground, ToolManager.FGTileset, MapEditor.Instance.State.TilePointer);
                break;
            }

            MapEditor.Instance.State.Apply(Action);
        }

        private void HandleDrag() {
            Room r = MapEditor.Instance.State.SelectedRoom;

            if (Action != null) Action.AddPoint(MapEditor.Instance.State.TilePointer);
        }

        private void HandleUnclick() {
            Action = null;
        }
    }
}