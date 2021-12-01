﻿// <copyright file="GLWindow.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.OpenGL
{
#pragma warning disable IDE0001 // Name can be simplified
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Velaptor.Content;
    using Velaptor.Input;
    using Velaptor.NativeInterop.GLFW;
    using Velaptor.NativeInterop.OpenGL;
    using Velaptor.Observables;
    using Velaptor.Services;
    using Velaptor.UI;

    // TODO: Need to normalize these 2 enums and figure out which one to use if any at all
    using VelaptorMouseButton = Velaptor.Input.MouseButton;
#pragma warning restore IDE0001 // Name can be simplified

    /// <summary>
    /// An OpenGL window implementation to be used inside of the <see cref="Window"/> class.
    /// </summary>
    internal sealed class GLWindow : IWindow
    {
        private const string NullParamExceptionMessage = "The parameter must not be null.";
        private readonly IGLInvoker gl;
        private readonly IGLFWInvoker glfw;
        private readonly ISystemMonitorService systemMonitorService;
        private readonly IGameWindowFacade windowFacade;
        private readonly IPlatform platform;
        private readonly ITaskService taskService;
        private readonly IKeyboardInput<KeyCode, KeyboardState> keyboard;
        private readonly IMouseInput<VelaptorMouseButton, MouseState> mouse;
        private readonly OpenGLInitObservable glObservable;
        private bool isShuttingDown;
        private bool firstRenderInvoked;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GLWindow"/> class.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="glInvoker">Invokes OpenGL functions.</param>
        /// <param name="glfwInvoker">Invokes GLFW functions.</param>
        /// <param name="systemMonitorService">Manages the systems monitors/screens.</param>
        /// <param name="windowFacade">The internal OpenGL window facade.</param>
        /// <param name="platform">Information about the platform that is running the application.</param>
        /// <param name="taskService">Runs asynchronous tasks.</param>
        /// <param name="keyboard">Provides keyboard input.</param>
        /// <param name="mouse">Provides mouse input.</param>
        /// <param name="contentLoader">Loads various kinds of content.</param>
        /// <param name="glObservable">Provides push notifications to OpenGL related events.</param>
        public GLWindow(
            uint width,
            uint height,
            IGLInvoker glInvoker,
            IGLFWInvoker glfwInvoker,
            ISystemMonitorService systemMonitorService,
            IGameWindowFacade windowFacade,
            IPlatform platform,
            ITaskService taskService,
            IKeyboardInput<KeyCode, KeyboardState> keyboard,
            IMouseInput<VelaptorMouseButton, MouseState> mouse,
            IContentLoader contentLoader,
            OpenGLInitObservable glObservable)
        {
            if (glInvoker is null)
            {
                throw new ArgumentNullException(nameof(glInvoker), NullParamExceptionMessage);
            }

            if (glfwInvoker is null)
            {
                throw new ArgumentNullException(nameof(glfwInvoker), NullParamExceptionMessage);
            }

            if (systemMonitorService is null)
            {
                throw new ArgumentNullException(nameof(systemMonitorService), NullParamExceptionMessage);
            }

            if (windowFacade is null)
            {
                throw new ArgumentNullException(nameof(windowFacade), NullParamExceptionMessage);
            }

            if (platform is null)
            {
                throw new ArgumentNullException(nameof(platform), NullParamExceptionMessage);
            }

            if (taskService is null)
            {
                throw new ArgumentNullException(nameof(taskService), NullParamExceptionMessage);
            }

            if (keyboard is null)
            {
                throw new ArgumentNullException(nameof(keyboard), NullParamExceptionMessage);
            }

            if (mouse is null)
            {
                throw new ArgumentNullException(nameof(mouse), NullParamExceptionMessage);
            }

            if (contentLoader is null)
            {
                throw new ArgumentNullException(nameof(contentLoader), NullParamExceptionMessage);
            }

            this.gl = glInvoker;
            this.glfw = glfwInvoker;
            this.systemMonitorService = systemMonitorService;
            this.windowFacade = windowFacade;
            this.platform = platform;
            this.taskService = taskService;
            this.keyboard = keyboard;
            this.mouse = mouse;
            this.glObservable = glObservable;

            ContentLoader = contentLoader;

            SetupWidthHeightPropCaches(width <= 0u ? 1u : width, height <= 0u ? 1u : height);
            SetupOtherPropCaches();
        }

        /// <inheritdoc/>
        public string Title
        {
            get => CachedStringProps[nameof(Title)].GetValue();
            set => CachedStringProps[nameof(Title)].SetValue(value);
        }

        /// <inheritdoc/>
        public Vector2 Position
        {
            get
            {
                if (CachedPosition is null)
                {
                    throw new Exception($"There was an issue getting the '{nameof(Silk.NET.Windowing.IWindow)}.{nameof(Position)}' property value.");
                }

                return CachedPosition.GetValue();
            }
            set
            {
                if (CachedPosition is null)
                {
                    throw new Exception($"There was an issue getting the '{nameof(Silk.NET.Windowing.IWindow)}.{nameof(Position)}' property value.");
                }

                CachedPosition.SetValue(value);
            }
        }

        /// <inheritdoc/>
        public uint Width
        {
            get => CachedUIntProps[nameof(Width)].GetValue();
            set => CachedUIntProps[nameof(Width)].SetValue(value);
        }

        /// <inheritdoc/>
        public uint Height
        {
            get => CachedUIntProps[nameof(Height)].GetValue();
            set => CachedUIntProps[nameof(Height)].SetValue(value);
        }

        /// <inheritdoc/>
        public bool AutoClearBuffer { get; set; } = true;

        /// <inheritdoc/>
        public bool MouseCursorVisible
        {
            get => CachedBoolProps[nameof(MouseCursorVisible)].GetValue();
            set => CachedBoolProps[nameof(MouseCursorVisible)].SetValue(value);
        }

        /// <inheritdoc/>
        public StateOfWindow WindowState
        {
            get
            {
                if (CachedWindowState is null)
                {
                    throw new Exception($"There was an issue getting the '{nameof(Silk.NET.Windowing.IWindow)}.{nameof(WindowState)}' property value.");
                }

                return CachedWindowState.GetValue();
            }
            set
            {
                if (CachedWindowState is null)
                {
                    throw new Exception($"There was an issue setting the '{nameof(Silk.NET.Windowing.IWindow)}.{nameof(WindowState)}' property value.");
                }

                CachedWindowState.SetValue(value);
            }
        }

        /// <inheritdoc/>
        public Action? Initialize { get; set; }

        /// <inheritdoc/>
        public Action<FrameTime>? Update { get; set; }

        /// <inheritdoc/>
        public Action<FrameTime>? Draw { get; set; }

        /// <inheritdoc/>
        public Action? Uninitialize { get; set; }

        /// <inheritdoc/>
        public Action<SizeU>? WinResize { get; set; }

        /// <inheritdoc/>
        public WindowBorder TypeOfBorder
        {
            get
            {
                if (CachedTypeOfBorder is null)
                {
                    throw new Exception($"There was an issue getting the '{nameof(Silk.NET.Windowing.IWindow)}.{nameof(TypeOfBorder)}' property value.");
                }

                return CachedTypeOfBorder.GetValue();
            }
            set
            {
                if (CachedTypeOfBorder is null)
                {
                    throw new Exception($"There was an issue setting the '{nameof(Silk.NET.Windowing.IWindow)}.{nameof(TypeOfBorder)}' property value.");
                }

                CachedTypeOfBorder.SetValue(value);
            }
        }

        /// <inheritdoc/>
        public IContentLoader ContentLoader { get; set; }

        /// <inheritdoc/>
        public int UpdateFrequency
        {
            get => CachedIntProps[nameof(UpdateFrequency)].GetValue();
            set => CachedIntProps[nameof(UpdateFrequency)].SetValue(value);
        }

        /// <inheritdoc/>
        public bool Initialized { get; private set; }

        /// <summary>
        /// Gets the list of caches for <see langword="string"/> properties.
        /// </summary>
        public Dictionary<string, CachedValue<string>> CachedStringProps { get; } = new Dictionary<string, CachedValue<string>>();

        /// <summary>
        /// Gets the list of caches for <see langword="int"/> properties.
        /// </summary>
        public Dictionary<string, CachedValue<int>> CachedIntProps { get; } = new Dictionary<string, CachedValue<int>>();

        /// <summary>
        /// Gets the list of caches for <see langword="uint"/> properties.
        /// </summary>
        public Dictionary<string, CachedValue<uint>> CachedUIntProps { get; } = new Dictionary<string, CachedValue<uint>>();

        /// <summary>
        /// Gets the list of caches for <see langword="bool"/> properties.
        /// </summary>
        public Dictionary<string, CachedValue<bool>> CachedBoolProps { get; } = new Dictionary<string, CachedValue<bool>>();

        /// <summary>
        /// Gets the cache for the <see cref="WindowState"/> property.
        /// </summary>
        public CachedValue<StateOfWindow>? CachedWindowState { get; private set; }

        /// <summary>
        /// Gets the cache for the <see cref="TypeOfBorder"/> property.
        /// </summary>
        public CachedValue<WindowBorder>? CachedTypeOfBorder { get; private set; }

        /// <summary>
        /// Gets the cache for the <see cref="Position"/> property.
        /// </summary>
        public CachedValue<Vector2>? CachedPosition { get; private set; }

        /// <inheritdoc/>
        public void Show()
        {
            this.windowFacade?.PreInit();
            RegisterEvents();
            this.windowFacade?.Show();
        }

        /// <inheritdoc/>
        public async Task ShowAsync()
        {
            this.taskService.SetAction(
                () =>
                {
                    this.windowFacade?.PreInit();
                    RegisterEvents();
                    this.windowFacade?.Show();
                });

            this.taskService.Start();

            await this.taskService.ContinueWith(
                _ => { },
                TaskContinuationOptions.ExecuteSynchronously, // Execute the continuation on the same thread as the show task
                TaskScheduler.Default);
        }

        /// <inheritdoc/>
        public void Close() => this.windowFacade.Close();

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Invokes the <see cref="Initialize"/> action property.
        /// </summary>
        private void GameWindow_Load(object? sender, EventArgs e)
        {
            // OpenGL is ready to take function calls after this Init() call has ran
            // TODO: Verify that this Init() is being called in unit tests
            this.windowFacade.Init(Width, Height);

            this.gl.SetupErrorCallback();
            this.gl.Enable(GLEnableCap.DebugOutput);
            this.gl.Enable(GLEnableCap.DebugOutputSynchronous);

            this.gl.GLError += GL_GLError;

            CachedStringProps.Values.ToList().ForEach(i => i.IsCaching = false);
            CachedBoolProps.Values.ToList().ForEach(i => i.IsCaching = false);
            CachedIntProps.Values.ToList().ForEach(i => i.IsCaching = false);
            CachedUIntProps.Values.ToList().ForEach(i => i.IsCaching = false);

            if (CachedPosition is not null)
            {
                CachedPosition.IsCaching = false;
            }

            if (CachedWindowState is not null)
            {
                CachedWindowState.IsCaching = false;
            }

            if (CachedTypeOfBorder is not null)
            {
                CachedTypeOfBorder.IsCaching = false;
            }

            /* Send a push notification to all subscribers that OpenGL is initialized.
             * The context of initialized here is that the OpenGL context is set
             *and the related GLFW window has been created and is ready to go.
             */
            this.glObservable.OnOpenGLInitialized();

            Initialized = true;

            Initialize?.Invoke();
        }

        /// <summary>
        /// Sets the state of the window as shutting down and starts the uninitialize process.
        /// </summary>
        private void GameWindow_Unload(object? sender, EventArgs e)
        {
            this.isShuttingDown = true;

            ContentLoader.Dispose();
            Uninitialize?.Invoke();
        }

        /// <summary>
        /// Invokes the <see cref="WinResize"/> action property..
        /// </summary>
        /// <param name="e">Resize event args.</param>
        private void GameWindow_Resize(object? sender, WindowSizeEventArgs e)
        {
            var uWidth = (uint)e.Width;
            var uHeight = (uint)e.Height;

            // Update the view port so it is the same size as the window
            this.gl.Viewport(0, 0, uWidth, uHeight);
            WinResize?.Invoke(new SizeU() { Width = uWidth, Height = uHeight });
        }

        /// <summary>
        /// Invokes the <see cref="Update"/> action property.
        /// </summary>
        private void GameWindow_UpdateFrame(object? sender, FrameTimeEventArgs e)
        {
            if (this.isShuttingDown)
            {
                return;
            }

            var frameTime = new FrameTime()
            {
                ElapsedTime = new TimeSpan(0, 0, 0, 0, (int)(e.FrameTime * 1000.0)),
            };

            Update?.Invoke(frameTime);
        }

        /// <summary>
        /// Invokes the <see cref="Draw"/> action property.
        /// </summary>
        private void GameWindow_RenderFrame(object? sender, FrameTimeEventArgs e)
        {
            if (this.firstRenderInvoked is false)
            {
                GameWindow_UpdateFrame(sender, e);
                this.firstRenderInvoked = true;
            }

            if (this.isShuttingDown)
            {
                return;
            }

            var frameTime = new FrameTime()
            {
                ElapsedTime = new TimeSpan(0, 0, 0, 0, (int)(e.FrameTime * 1000.0)),
            };

            if (AutoClearBuffer)
            {
                this.gl.Clear(GLClearBufferMask.ColorBufferBit);
            }

            Draw?.Invoke(frameTime);

            this.windowFacade.SwapBuffers();
        }

        /// <summary>
        /// Sets up the OpenGL window.
        /// </summary>
        private void RegisterEvents()
        {
            this.windowFacade.Load += GameWindow_Load;
            this.windowFacade.Unload += GameWindow_Unload;
            this.windowFacade.UpdateFrame += GameWindow_UpdateFrame;
            this.windowFacade.RenderFrame += GameWindow_RenderFrame;
            this.windowFacade.Resize += GameWindow_Resize;
        }

        /// <summary>
        /// Invoked when an OpenGL error occurs.
        /// </summary>
        private void GL_GLError(object? sender, GLErrorEventArgs e) => throw new Exception(e.ErrorMessage);

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release managed resources.</param>
        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.glObservable.Dispose();

                CachedStringProps.Clear();
                CachedIntProps.Clear();
                CachedBoolProps.Clear();

                // TODO: Unit test for this unsubscription
                this.gl.GLError -= GL_GLError;

                this.windowFacade.Load -= GameWindow_Load;
                this.windowFacade.Unload -= GameWindow_Unload;
                this.windowFacade.UpdateFrame -= GameWindow_UpdateFrame;
                this.windowFacade.RenderFrame -= GameWindow_RenderFrame;
                this.windowFacade.Resize -= GameWindow_Resize;
                this.windowFacade.Dispose();
                this.taskService?.Dispose();

                this.gl.Dispose();
                this.glfw.Dispose();
            }

            this.isDisposed = true;
        }

        /// <summary>
        /// Sets up caching for the <see cref="Width"/> and <see cref="Height"/> properties.
        /// </summary>
        /// <param name="width">The window width.</param>
        /// <param name="height">The window height.</param>
        private void SetupWidthHeightPropCaches(uint width, uint height)
        {
            CachedUIntProps.Add(
                nameof(Width), // key
                new CachedValue<uint>( // value
                    defaultValue: width,
                    getterWhenNotCaching: () =>
                    {
                        return (uint)this.windowFacade.Size.X;
                    },
                    setterWhenNotCaching: (value) =>
                    {
                        this.windowFacade.Size = new (value, this.windowFacade.Size.Y);
                    }));

            CachedUIntProps.Add(
                nameof(Height), // key
                new CachedValue<uint>( // value
                    defaultValue: height,
                    getterWhenNotCaching: () =>
                    {
                        return (uint)this.windowFacade.Size.Y;
                    },
                    setterWhenNotCaching: (value) =>
                    {
                        this.windowFacade.Size = new (this.windowFacade.Size.X, value);
                    }));
        }

        /// <summary>
        /// Setup all of the caching for the properties that need caching.
        /// </summary>
        private void SetupOtherPropCaches()
        {
            CachedStringProps.Add(
                nameof(Title), // key
                new CachedValue<string>( // value
                    defaultValue: "Velaptor Application",
                    getterWhenNotCaching: () =>
                    {
                        return this.windowFacade.Title;
                    },
                    setterWhenNotCaching: (value) =>
                    {
                        this.windowFacade.Title = value;
                    }));

            Vector2 defaultPosition = Vector2.Zero;

            var mainMonitor = this.systemMonitorService.MainMonitor;

            float ToMonitorScale(float value)
            {
                return value * (mainMonitor is null ? 0 : mainMonitor.HorizontalDPI) /
                    (this.platform.CurrentPlatform == OSPlatform.OSX ? 72f : 96f);
            }

            var halfWidth = ToMonitorScale(Width / 2f);
            var halfHeight = ToMonitorScale(Height / 2f);

            if (mainMonitor is not null)
            {
                defaultPosition = new Vector2(mainMonitor.Center.X - halfWidth, mainMonitor.Center.Y - halfHeight);
            }

            CachedPosition = new CachedValue<Vector2>(
                defaultValue: defaultPosition,
                getterWhenNotCaching: () =>
                {
                    return new Vector2(this.windowFacade.Location.X, this.windowFacade.Location.Y);
                },
                setterWhenNotCaching: (value) =>
                {
                    this.windowFacade.Location = value;
                });

            CachedIntProps.Add(
                nameof(UpdateFrequency), // key
                new CachedValue<int>( // value
                    defaultValue: 60,
                    getterWhenNotCaching: () =>
                    {
                        return (int)this.windowFacade.UpdateFrequency;
                    },
                    setterWhenNotCaching: (value) =>
                    {
                        this.windowFacade.UpdateFrequency = value;
                    }));

            CachedBoolProps.Add(
                nameof(MouseCursorVisible), // key
                new CachedValue<bool>( // value
                    defaultValue: true,
                    getterWhenNotCaching: () =>
                    {
                        return this.windowFacade.CursorVisible;
                    },
                    setterWhenNotCaching: (value) =>
                    {
                        this.windowFacade.CursorVisible = value;
                    }));

            CachedWindowState = new CachedValue<StateOfWindow>(
                defaultValue: StateOfWindow.Normal,
                getterWhenNotCaching: () =>
                {
                    return this.windowFacade.WindowState;
                },
                setterWhenNotCaching: (value) =>
                {
                    this.windowFacade.WindowState = value;
                });

            CachedTypeOfBorder = new CachedValue<WindowBorder>(
                defaultValue: Velaptor.WindowBorder.Resizable,
                getterWhenNotCaching: () =>
                {
                    return this.windowFacade.WindowBorder;
                },
                setterWhenNotCaching: (value) =>
                {
                    this.windowFacade.WindowBorder = value;
                });
        }
    }
}
