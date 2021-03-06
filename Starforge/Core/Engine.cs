﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Starforge.Core.Boot;
using Starforge.Core.Interop;
using Starforge.Editor.UI;
using Starforge.Mod.Content;
using Starforge.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Starforge.Core {
    /// <summary>
    /// The core engine responsible for running the program.
    /// </summary>
    public class Engine : Game {
        /// <summary>
        /// The globally used SpriteBatch.
        /// </summary>
        public static SpriteBatch Batch { get; private set; }

        /// <summary>
        /// The graphics device manager for the Starforge window.
        /// </summary>
        public static GraphicsDeviceManager GDM { get; private set; }

        /// <summary>
        /// The ImGui renderer.
        /// </summary>
        public static ImGuiRenderer GUIRenderer { get; private set; }

        /// <summary>
        /// The instance of Engine which is running the program.
        /// </summary>
        public static Engine Instance { get; private set; }

        /// <summary>
        /// The platform helper for the host operating system.
        /// </summary>
        public static PlatformBase Platform { get; private set; }

        /// <summary>
        /// The root directory in which the application is running.
        /// </summary>
        public static readonly string RootDirectory = Environment.CurrentDirectory;

        /// <summary>
        /// Whether or not the startup process has completed.
        /// </summary>
        public static bool StartupComplete = false;

        /// <summary>
        /// Whether or not a map is currently loaded.
        /// </summary>
        public static bool MapLoaded = false;

        /// <summary>
        /// The list of currently loaded textures.
        /// </summary>
        public static List<VirtualTexture> VirtualContent;

        /// <summary>
        /// A list of open ImGui windows.
        /// </summary>
        public static List<Window> Windows;

        /// <summary>
        /// Event which is fired when the window size changes.
        /// </summary>
        public static ViewportUpdate OnViewportUpdate;
        public delegate void ViewportUpdate();

        /// <summary>
        /// The current scene.
        /// </summary>
        private static Scene Scene;

        #region Boot

        private Engine() {
            GDM = new GraphicsDeviceManager(this);

            // Default settings
            GDM.IsFullScreen = false;
            GDM.PreferredBackBufferWidth = 1280;
            GDM.PreferredBackBufferHeight = 720;
            GDM.PreferMultiSampling = false;
            GDM.SynchronizeWithVerticalRetrace = Settings.VerticalSync;
            IsFixedTimeStep = Settings.VerticalSync;
            IsMouseVisible = true;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (object sender, EventArgs e) => OnViewportUpdate?.Invoke();

            Windows = new List<Window>();
        }

        private static void Main(string[] args) {
            // Set up platform helper
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Platform = new PlatformWindows();
            } else {
                Logger.CreateErrorLog($"Unsupported platform detected.");
            }

            Thread.CurrentThread.Name = "Starforge";

            Settings.ConfigDirectory = Platform.GetAppDataFolder();

            // Create folder for Starforge configs if it doesn't already exist
            if (!Directory.Exists(Settings.ConfigDirectory)) {
                try {
                    Directory.CreateDirectory(Settings.ConfigDirectory);
                } catch (Exception e) {
                    Logger.CreateErrorLog(e.ToString());
                }
            }

            string logOld = Path.Combine(Settings.ConfigDirectory, "log_old.txt");
            string log = Path.Combine(Settings.ConfigDirectory, "log.txt");

            if (File.Exists(logOld)) File.Delete(logOld);
            if (File.Exists(log)) File.Move(log, logOld);

            Logger.SetOutputStream(new StreamWriter(File.OpenWrite(log)));

            Logger.Log("Launching Starforge.");
            if (Type.GetType("Mono.Runtime") != null) Logger.Log("Detected Mono runtime");

            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

            Settings.LoadConfig(Path.Combine(Settings.ConfigDirectory, "settings.cfg"));

            using (Engine e = new Engine()) {
                Instance = e;
                e.Run();
            }

            Logger.Log("FNA window closed.");
            Logger.Log("Writing configuration changes.");

            Settings.WriteConfig(Path.Combine(Settings.ConfigDirectory, "settings.cfg"));
        }

        private static void GlobalExceptionHandler(object sender, UnhandledExceptionEventArgs e) {
            Logger.Log(LogLevel.Critical, $"Encountered a{(e.IsTerminating ? " fatal" : "n")} unhandled exception.");
            Logger.LogException((Exception)e.ExceptionObject);

            if (e.IsTerminating) Logger.CreateErrorLog(((Exception)e.ExceptionObject).ToString());
        }

        #endregion

        #region Game

        protected override void Draw(GameTime gt) {
            base.Draw(gt);
            GUIRenderer.BeforeLayout(gt);
            Scene.Render(gt);

            // Set render target back to the window so we don't accidentally render UI content on top of a room.
            GraphicsDevice.SetRenderTarget(null);
            for (int i = 0; i < Windows.Count; i++) {
                if (Windows[i].Visible) {
                    Windows[i].Render();
                }
                else {
                    DeleteWindow(Windows[i]);
                }
            }

            Menubar.Render();
            GUIRenderer.AfterLayout();
        }

        protected override void Update(GameTime gt) {
            base.Update(gt);

            if (!IsActive) {
                SuppressDraw();
            }

            Input.Update();

            Scene.Update(gt);
        }

        protected override void Initialize() {
            base.Initialize();

            GUIRenderer = new ImGuiRenderer(this);
            GUIRenderer.BuildFontAtlas();
        }

        protected override void LoadContent() {
            Batch = new SpriteBatch(GraphicsDevice);
            VirtualContent = new List<VirtualTexture>();

            GFX.LoadContent();
            SetScene(new StartupScene());
        }
        protected override void UnloadContent() {
            base.UnloadContent();
        }

        #endregion

        /// <summary>
        /// Sets the current scene.
        /// </summary>
        /// <param name="scene">The scene to switch to.</param>
        /// <returns>Whether or not the scene was successfully switched.</returns>
        public static bool SetScene(Scene scene) {
            if (Scene != null) {
                if (Scene.End()) {
                    Scene = scene;
                    Scene.Begin();
                    return true;
                } else {
                    return false;
                }
            } else {
                Scene = scene;
                Scene.Begin();
                return true;
            }
        }

        /// <summary>
        /// Adds a window to the list of currently open ImGui windows.
        /// </summary>
        /// <param name="win">The window to add.</param>
        public static void CreateWindow(Window win) {
            Windows.Add(win);
        }

        /// <summary>
        /// Removes a window from the list of currently open ImGui windows.
        /// </summary>
        /// <param name="win">The window to remove.</param>
        public static void DeleteWindow(Window win) {
            Windows.Remove(win);
            win.End();
        }
    }
}
