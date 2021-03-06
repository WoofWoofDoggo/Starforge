﻿using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using Starforge.Core;
using Starforge.Core.Interop;
using Starforge.Editor.Render;
using System;
using System.IO;

namespace Starforge.Editor.UI {
    /// <summary>
    /// The menubar of the window.
    /// </summary>
    public static class Menubar {
        public static float MenubarHeight { get; private set; }

        public static RenderFlags RerenderFlags { get; private set; } = RenderFlags.All;
        public static bool View_BGDecals = true;
        public static bool View_BGTiles = true;
        public static bool View_FGDecals = true;
        public static bool View_FGTiles = true;
        public static bool View_OBTiles = true;
        public static bool View_Entities = true;
        public static bool View_Triggers = true;

        public static bool DemoWindow = false;
        public static bool MetricsWindow = false;

        /// <summary>
        /// Renders the window menubar.
        /// </summary>
        /// <param name="hasEditor">Whether or not the map editor is currently loaded.</param>
        public static void Render() {
            if (DemoWindow) ImGui.ShowDemoWindow();
            if (MetricsWindow) ImGui.ShowMetricsWindow();

            if (!ImGui.BeginMainMenuBar()) return;
            MenubarHeight = ImGui.GetWindowHeight();

            if (ImGui.BeginMenu("File")) {
                if (ImGui.MenuItem("New")) New();
                if (ImGui.MenuItem("Open", "CTRL+O")) Open();
                if (ImGui.MenuItem("Save", "CTRL+S", false, Engine.MapLoaded && MapEditor.Instance.State.Unsaved)) Save();
                if (ImGui.MenuItem("Save As", "", false, Engine.MapLoaded)) SaveAs();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit")) {
                if (ImGui.MenuItem("Undo", "CTRL+Z", false, Engine.MapLoaded && MapEditor.Instance.State.CanUndo())) MapEditor.Instance.State.Undo();
                if (ImGui.MenuItem("Redo", "CTRL+SHIFT+Z", false, Engine.MapLoaded && MapEditor.Instance.State.CanRedo())) MapEditor.Instance.State.Redo();
                ImGui.Separator();
                if (ImGui.MenuItem("Add Room", "", false, Engine.MapLoaded)) Engine.CreateWindow(new WindowRoomConfig());

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View")) {
                if (ImGui.MenuItem("Background Decals", "", ref View_BGDecals, Engine.MapLoaded)) ChangeView();
                if (ImGui.MenuItem("Background Tiles", "", ref View_BGTiles, Engine.MapLoaded)) ChangeView();
                if (ImGui.MenuItem("Foreground Decals", "", ref View_FGDecals, Engine.MapLoaded)) ChangeView();
                if (ImGui.MenuItem("Foreground Tiles", "", ref View_FGTiles, Engine.MapLoaded)) ChangeView();
                if (ImGui.MenuItem("Object Tiles", "", ref View_OBTiles, Engine.MapLoaded)) ChangeView();
                if (ImGui.MenuItem("Entities", "", ref View_Entities, Engine.MapLoaded)) ChangeView();
                if (ImGui.MenuItem("Triggers", "", ref View_Triggers, Engine.MapLoaded)) ChangeView();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Tools")) {
                if (ImGui.MenuItem("Settings")) Engine.CreateWindow(new WindowSettings());
                if (ImGui.MenuItem("Save Room As Image", Engine.MapLoaded && MapEditor.Instance.State.SelectedRoom != null)) SaveRoomImage();

                if (Settings.DebugMode) {
                    ImGui.Separator();
                    if (ImGui.BeginMenu("Debug Options")) {
                        if (ImGui.MenuItem("Force GC")) GC.Collect(2, GCCollectionMode.Forced, true, true);
                        if (ImGui.MenuItem("Clear Render Cache")) ChangeView();
                        if (ImGui.MenuItem("Reset Input")) Input.Reset();
                        ImGui.MenuItem("Show Demo Window", "", ref DemoWindow);
                        ImGui.MenuItem("Show Metrics Window", "", ref MetricsWindow);

                        ImGui.EndMenu();
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help")) {
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }

        #region File

        public static void New() {
            if (Engine.MapLoaded && MapEditor.Instance.State.Unsaved) {
                // Unsaved changes - notify user
                Engine.CreateWindow(new WindowUnsavedChanges(New));
            } else {
                Engine.CreateWindow(new WindowNewMap());
            }
        }

        public static void Open() {
            if (Engine.MapLoaded && MapEditor.Instance.State.Unsaved) {
                Engine.CreateWindow(new WindowUnsavedChanges(Open));
                return;
            } else if (NfdResult.OKAY == NFD.OpenDialog("bin", Settings.CelesteDirectory, out string mapPath)) {
                MapEditor editor = new MapEditor();
                editor.LoadLevel(mapPath + (mapPath.EndsWith(".bin") ? "" : ".bin"));
                Engine.SetScene(editor);
            }
            Input.Reset();
        }

        public static bool Save() {
            if (Engine.MapLoaded && MapEditor.Instance.State.Unsaved) {
                if (string.IsNullOrEmpty(MapEditor.Instance.State.LoadedPath)) {
                    if (NfdResult.OKAY == NFD.SaveDialog("bin", Settings.CelesteDirectory, out string mapPath)) {
                        MapEditor.Instance.State.LoadedPath = mapPath + (mapPath.EndsWith(".bin") ? "" : ".bin");
                    }
                    Input.Reset();
                }

                if (string.IsNullOrEmpty(MapEditor.Instance.State.LoadedPath)) return false;

                MapEditor.Instance.State.Save();
                return true;
            }

            return true;
        }

        public static void SaveAs() {
            if (Engine.MapLoaded) {
                if (NfdResult.OKAY == NFD.SaveDialog("bin", Settings.CelesteDirectory, out string mapPath)) {
                    MapEditor.Instance.State.LoadedPath = mapPath + (mapPath.EndsWith(".bin") ? "" : ".bin");
                    MapEditor.Instance.State.Save();
                }
                Input.Reset();
            }
        }

        #endregion

        #region Edit



        #endregion

        #region View

        public static void ChangeView() {
            if (!Engine.MapLoaded) return;

            RerenderFlags = RenderFlags.None;
            if (View_BGDecals) RerenderFlags |= RenderFlags.BGDecals;
            if (View_BGTiles) RerenderFlags |= RenderFlags.BGTiles;
            if (View_FGDecals) RerenderFlags |= RenderFlags.FGDecals;
            if (View_FGTiles) RerenderFlags |= RenderFlags.FGTiles;
            if (View_OBTiles) RerenderFlags |= RenderFlags.OBTiles;
            if (View_Entities) RerenderFlags |= RenderFlags.Entities;
            if (View_Triggers) RerenderFlags |= RenderFlags.Triggers;

            MapEditor.Instance.RerenderAll(RerenderFlags);
        }

        #endregion

        #region Tools

        public static void SaveRoomImage() {
            if (!Engine.MapLoaded || MapEditor.Instance == null) return;

            if (NfdResult.OKAY == NFD.SaveDialog("png", Settings.CelesteDirectory, out string pngPath)) {
                if (!File.Exists(pngPath)) {
                    using (FileStream stream = File.Create(pngPath + ".png")) {
                        stream.Close();
                    }
                }

                using (FileStream stream = File.Open(pngPath, FileMode.Truncate)) {
                    DrawableRoom dr = MapEditor.Instance.Renderer.SelectedRoom;

                    dr.Target.SaveAsPng(stream, dr.Room.Width, dr.Room.Height);
                }
            }
        }

        public static void LogInputState() {
            string res = "Keyboard Current:";
            foreach (Keys key in Input.Keyboard.GetPressedKeys()) {
                res += $" {key.ToString()}";
            }
            Logger.Log(res);
        }

        #endregion

        #region Help



        #endregion
    }
}
