﻿using Starforge.Core;
using Starforge.Map;
using Starforge.Editor.UI;
using System.Drawing;
using System.Collections.Generic;
using Starforge.Util;
using Starforge.Editor.Actions;
using Microsoft.Xna.Framework;

namespace Starforge.Editor.Tools {

    using Attributes = Dictionary<string, object>;

    public class EntitySelectionTool : Tool {
        Entity SelectedEntity = null;
        EntityRegion HeldRegion = EntityRegion.Outside;
        System.Drawing.Point ClickOffset;
        Attributes InitialAttributes;

        public override string GetName() => "Entity Selection";

        public override void Render() {
            //nothing to render
        }

        public override void Update() {
            UpdateCursor();

            if (Input.Mouse.RightClick) {
                HandleRightClick();
            }

            if (Input.Mouse.LeftUnclick) {
                HandleLeftUnclick();
            }

            if (Input.Mouse.LeftClick) {
                HandleLeftClick();
            }

            if (Input.Mouse.LeftHold) {
                HandleLeftDrag();
            }
        }

        private void HandleLeftClick() {
            if (SelectedEntity == null) {
                SelectedEntity = MapEditor.Instance.State.SelectedRoom.Entities.Find(e => e.ContainsPosition(MapEditor.Instance.State.PixelPointer));
                return;
            }

            InitialAttributes = MiscHelper.CloneDictionary(SelectedEntity.Attributes);
            // start dragging
            HeldRegion = SelectedEntity.GetEntityRegion(MapEditor.Instance.State.PixelPointer);
            if (HeldRegion == EntityRegion.Outside) {
                //let go of currently held entity
                SelectedEntity = null;
                ClickOffset = new System.Drawing.Point();
                return;
            }
            // remember where in the entity we started clicking
            ClickOffset.X = MapEditor.Instance.State.PixelPointer.X - (int)SelectedEntity.Position.X;
            ClickOffset.Y = MapEditor.Instance.State.PixelPointer.Y - (int)SelectedEntity.Position.Y;
        }

        private void HandleLeftDrag() {
            if (HeldRegion == EntityRegion.Outside || SelectedEntity == null) {
                return;
            }

            Vector2 UpdatedPosition;
            UpdatedPosition.X = SelectedEntity.Position.X;
            UpdatedPosition.Y = SelectedEntity.Position.Y;

            int XEnd = (int)SelectedEntity.Position.X + SelectedEntity.Width;
            int YEnd = (int)SelectedEntity.Position.Y + SelectedEntity.Height;

            if ((HeldRegion & EntityRegion.Left) != 0) {
                // Update left side of the entity to mouse position
                UpdatedPosition.X = MiscHelper.GetMousePosition().X;
                SelectedEntity.Width = XEnd - (int)UpdatedPosition.X; 
            }
            if ((HeldRegion & EntityRegion.Right) != 0) {
                // Update left side of the entity to mouse position
                SelectedEntity.Width = MiscHelper.GetMousePositionCeil().X - (int)SelectedEntity.Position.X;
            }
            if ((HeldRegion & EntityRegion.Top) != 0) {
                // Update top side of the entity to mouse position
                UpdatedPosition.Y = MiscHelper.GetMousePosition().Y;
                SelectedEntity.Height = YEnd - (int)UpdatedPosition.Y;
            }
            if ((HeldRegion & EntityRegion.Bottom) != 0) {
                // Update bottom side of the entity to mouse position
                SelectedEntity.Height = MiscHelper.GetMousePositionCeil().Y - (int)SelectedEntity.Position.Y;
            }
            if (HeldRegion == EntityRegion.Middle) {
                // Update location
                UpdatedPosition.X = MiscHelper.GetMousePosition().X - ClickOffset.X;
                UpdatedPosition.Y = MiscHelper.GetMousePosition().Y - ClickOffset.Y;
            }

            int MinSize;
            if (EditorState.PixelPerfect()) {
                MinSize = 1;
            }
            else {
                MinSize = 8;
            }

            SelectedEntity.Height = (int)MathHelper.Max(SelectedEntity.Height, MinSize);
            SelectedEntity.Width = (int)MathHelper.Max(SelectedEntity.Width, MinSize);

            SelectedEntity.Position = UpdatedPosition;
            MapEditor.Instance.Renderer.GetRoom(SelectedEntity.Room).Dirty = true;
        }

        private void HandleLeftUnclick() {
            // make action of movement thus far
            // Only create action if anything actually changed
            if (InitialAttributes != null && HeldRegion != EntityRegion.Outside) {
                MapEditor.Instance.State.Apply(new EntityEditAction(
                    MapEditor.Instance.State.SelectedRoom,
                    SelectedEntity,
                    InitialAttributes,
                    MiscHelper.CloneDictionary(SelectedEntity.Attributes)
                ));
            }

            HeldRegion = EntityRegion.Outside;
        }

        private void HandleRightClick() {
            if (!SelectedEntity.ContainsPosition(MapEditor.Instance.State.PixelPointer)) {
                return;
            }
            HandleSelectedEntity(SelectedEntity);
        }

        public void HandleSelectedEntity(Entity SelectedEntity) {
            Engine.CreateWindow(new WindowEntityEdit(SelectedEntity));
        }

        public void UpdateCursor() {
            if (SelectedEntity == null) {
                return;
            }

            EntityRegion Region;
            if (HeldRegion != EntityRegion.Outside) {
                Region = HeldRegion;
            }
            else {
                Region = SelectedEntity.GetEntityRegion(MapEditor.Instance.State.PixelPointer);
            }
            switch (Region) {
            case EntityRegion.TopLeft:
            case EntityRegion.BottomRight:
                UIHelper.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE);
                break;
            case EntityRegion.TopRight:
            case EntityRegion.BottomLeft:
                UIHelper.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW);
                break;
            case EntityRegion.Left:
            case EntityRegion.Right:
                UIHelper.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE);
                break;
            case EntityRegion.Top:
            case EntityRegion.Bottom:
                UIHelper.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS);
                break;
            case EntityRegion.Middle:
                UIHelper.SetCursor(SDL2.SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
                break;
            default:
                break;
            }
        }

    }

    // Names for the different corners and sides of the entity that can be dragged
    public enum EntityRegion {
        Outside = -1,
        Middle = 0,
        Top = 1 << 0,
        Bottom = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
    }

}
