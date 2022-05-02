﻿// <copyright file="GLWindowTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace VelaptorTests.OpenGL
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Moq;
    using Silk.NET.Core.Contexts;
    using Silk.NET.Input;
    using Silk.NET.Maths;
    using Silk.NET.Windowing;
    using Velaptor;
    using Velaptor.Content;
    using Velaptor.Exceptions;
    using Velaptor.Factories;
    using Velaptor.Graphics;
    using Velaptor.Hardware;
    using Velaptor.Input;
    using Velaptor.Input.Exceptions;
    using Velaptor.NativeInterop.GLFW;
    using Velaptor.NativeInterop.OpenGL;
    using Velaptor.OpenGL;
    using Velaptor.Reactables.Core;
    using Velaptor.Reactables.ReactableData;
    using Velaptor.Services;
    using VelaptorTests.Helpers;
    using Xunit;
    using SilkMouseButton = Silk.NET.Input.MouseButton;
    using SilkWindowBorder = Silk.NET.Windowing.WindowBorder;
    using SysVector2 = System.Numerics.Vector2;
    using VelaptorKeyboardState = Velaptor.Input.KeyboardState;
    using VelaptorMouseButton = Velaptor.Input.MouseButton;
    using VelaptorMouseState = Velaptor.Input.MouseState;
    using VelaptorWindowBorder = Velaptor.WindowBorder;

    /// <summary>
    /// Tests the <see cref="GLWindow"/> class.
    /// </summary>
    public class GLWindowTests
    {
        private readonly Mock<IGLInvoker> mockGL;
        private readonly Mock<IGLFWInvoker> mockGLFW;
        private readonly Mock<IGLContext> mockGLContext;
        private readonly Mock<ISystemMonitorService> mockMonitorService;
        private readonly Mock<IPlatform> mockPlatform;
        private readonly Mock<IContentLoader> mockContentLoader;
        private readonly Mock<IRenderer> mockRenderer;
        private readonly Mock<IKeyboardInput<KeyCode, KeyboardState>> mockKeyInput;
        private readonly Mock<IMouseInput<VelaptorMouseButton, MouseState>> mockMouseInput;
        private readonly Mock<ITaskService> mockTaskService;
        private readonly Mock<IReactable<GLContextData>> mockContextReactable;
        private readonly Mock<IReactable<GLInitData>> mockGLInitReactable;
        private readonly Mock<IReactable<ShutDownData>> mockShutDownReactable;
        private readonly Mock<IWindow> mockSilkWindow;
        private readonly Mock<IWindowFactory> mockWindowFactory;
        private Mock<IInputFactory>? mockInputFactory;
        private Mock<IInputContext>? mockSilkInputContext;
        private Mock<IKeyboard>? mockSilkKeyboard;
        private Mock<IMouse>? mockSilkMouse;
        private Mock<ICursor>? mockMouseCursor;

        /// <summary>
        /// Initializes a new instance of the <see cref="GLWindowTests"/> class.
        /// </summary>
        public GLWindowTests()
        {
            this.mockGLContext = new Mock<IGLContext>();
            this.mockSilkWindow = new Mock<IWindow>();
            this.mockSilkWindow.SetupGet(p => p.GLContext).Returns(this.mockGLContext.Object);
            this.mockSilkWindow.Setup(m => m.Run(It.IsAny<Action>()))
                .Callback<Action>(_ =>
                {
                    // Mock the behavior of the load event being invoked when the app is ran
                    this.mockSilkWindow.Raise(e => e.Load += null);
                });

            MockSystemSilkInput();

            this.mockWindowFactory = new Mock<IWindowFactory>();
            this.mockWindowFactory.Setup(m => m.CreateSilkWindow()).Returns(this.mockSilkWindow.Object);

            this.mockGL = new Mock<IGLInvoker>();
            this.mockGLFW = new Mock<IGLFWInvoker>();
            this.mockMonitorService = new Mock<ISystemMonitorService>();
            this.mockPlatform = new Mock<IPlatform>();
            this.mockContentLoader = new Mock<IContentLoader>();
            this.mockRenderer = new Mock<IRenderer>();
            this.mockKeyInput = new Mock<IKeyboardInput<KeyCode, VelaptorKeyboardState>>();
            this.mockMouseInput = new Mock<IMouseInput<VelaptorMouseButton, MouseState>>();
            this.mockTaskService = new Mock<ITaskService>();
            this.mockContextReactable = new Mock<IReactable<GLContextData>>();
            this.mockGLInitReactable = new Mock<IReactable<GLInitData>>();
            this.mockShutDownReactable = new Mock<IReactable<ShutDownData>>();
        }

        #region Contructor Tests
        [Fact]
        public void Ctor_WithNullWindowFactoryParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    null,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'windowFactory')");
        }

        [Fact]
        public void Ctor_WithNullInputFactoryParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    null,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'inputFactory')");
        }

        [Fact]
        public void Ctor_WithNullGLInvokerParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    null,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'glInvoker')");
        }

        [Fact]
        public void Ctor_WithNullGLFWInvokerParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    null,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'glfwInvoker')");
        }

        [Fact]
        public void Ctor_WithNullSystemMonitorServiceParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    null,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'systemMonitorService')");
        }

        [Fact]
        public void Ctor_WithNullPlatformParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    null,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'platform')");
        }

        [Fact]
        public void Ctor_WithNullTaskServiceParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    null,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'taskService')");
        }

        [Fact]
        public void Ctor_WithNullContentLoaderParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    null,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'contentLoader')");
        }

        [Fact]
        public void Ctor_WithNullRendererParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    null,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'renderer')");
        }

        [Fact]
        public void Ctor_WithNullKeyboardInputParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    null,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'keyboardInput')");
        }

        [Fact]
        public void Ctor_WithNullMouseInputParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    null,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'mouseInput')");
        }

        [Fact]
        public void Ctor_WithNullGLReactableParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    null,
                    this.mockGLInitReactable.Object,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'glReactable')");
        }

        [Fact]
        public void Ctor_WithNullGLInitReactableParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    null,
                    this.mockShutDownReactable.Object);
            }, "The parameter must not be null. (Parameter 'glInitReactable')");
        }

        [Fact]
        public void Ctor_WithNullShutDownReactableParam_ThrowsException()
        {
            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentNullException>(() =>
            {
                _ = new GLWindow(
                    It.IsAny<uint>(),
                    It.IsAny<uint>(),
                    this.mockWindowFactory.Object,
                    this.mockInputFactory.Object,
                    this.mockGL.Object,
                    this.mockGLFW.Object,
                    this.mockMonitorService.Object,
                    this.mockPlatform.Object,
                    this.mockTaskService.Object,
                    this.mockContentLoader.Object,
                    this.mockRenderer.Object,
                    this.mockKeyInput.Object,
                    this.mockMouseInput.Object,
                    this.mockContextReactable.Object,
                    this.mockGLInitReactable.Object,
                    null);
            }, "The parameter must not be null. (Parameter 'shutDownReactable')");
        }
        #endregion

        #region Prop Tests
        [Fact]
        public void Width_WhenCachingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow(100, 200);

            // Act
            var actual = window.Width;

            // Assert
            Assert.Equal(100u, actual);
        }

        [Fact]
        public void Width_WhenSettingValueAndNotCaching_ReturnsCorrectResult()
        {
            // Arrange
            this.mockSilkWindow.SetupProperty(p => p.Size);
            var window = CreateWindow();
            window.Show();

            window.CachedUIntProps[nameof(window.Width)].IsCaching = false;

            // Act
            window.Width = 111;
            var actual = window.Width;

            // Assert
            Assert.Equal(111u, actual);
        }

        [Fact]
        public void Height_WhenCachingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow(100, 200);

            // Act
            var actual = window.Height;

            // Assert
            Assert.Equal(200u, actual);
        }

        [Fact]
        public void Height_WhenSettingValueAndNotCaching_ReturnsCorrectResult()
        {
            // Arrange
            this.mockSilkWindow.SetupProperty(p => p.Size);
            var window = CreateWindow();
            window.Show();

            window.CachedUIntProps[nameof(window.Height)].IsCaching = false;

            // Act
            window.Height = 111;
            var actual = window.Height;

            // Assert
            Assert.Equal(111u, actual);
        }

        [Fact]
        public void Title_WhenCachingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow();

            // Act
            var actual = window.Title;

            // Assert
            Assert.Equal("Velaptor Application", actual);
        }

        [Fact]
        public void Title_WhenSettingValueAndNotCaching_ReturnsCorrectResult()
        {
            // Arrange
            this.mockSilkWindow.SetupProperty(p => p.Title);
            var window = CreateWindow();
            window.Show();

            window.CachedStringProps[nameof(window.Title)].IsCaching = false;

            // Act
            window.Title = "test-title";
            var actual = window.Title;

            // Assert
            Assert.Equal("test-title", actual);
        }

        [Fact]
        public void Position_WhenCachingValueOnOSXPlatform_ReturnsCorrectResult()
        {
            // Arrange
            this.mockPlatform.SetupGet(p => p.CurrentPlatform).Returns(OSPlatform.OSX);

            this.mockMonitorService.SetupGet(p => p.MainMonitor)
                .Returns(() => new SystemMonitor(this.mockPlatform.Object)
                {
                    HorizontalScale = 1f,
                    VerticalScale = 1f,
                    Width = 2000,
                    Height = 1000,
                });

            var window = CreateWindow(100, 200);

            // Act
            var actual = window.Position;

            // Assert
            Assert.Equal(new SysVector2(950, 400), actual);
        }

        [Fact]
        public void Position_WhenCachingValueOnWindowsPlatform_ReturnsCorrectResult()
        {
            // Arrange
            this.mockPlatform.SetupGet(p => p.CurrentPlatform).Returns(OSPlatform.Windows);

            this.mockMonitorService.SetupGet(p => p.MainMonitor)
                .Returns(() => new SystemMonitor(this.mockPlatform.Object)
                {
                    HorizontalScale = 1f,
                    VerticalScale = 1f,
                    Width = 2000,
                    Height = 1000,
                });

            var window = CreateWindow(100, 200);

            // Act
            var actual = window.Position;

            // Assert
            Assert.Equal(new SysVector2(950, 400), actual);
        }

        [Fact]
        public void Position_WhenSettingValueAndNotCaching_ReturnsCorrectResult()
        {
            // Arrange
            this.mockSilkWindow.SetupProperty(p => p.Position);
            var window = CreateWindow();
            window.Show();

            window.CachedPosition.IsCaching = false;

            // Act
            window.Position = new SysVector2(123, 456);
            var actual = window.Position;

            // Assert
            Assert.Equal(new SysVector2(123, 456), actual);
        }

        [Fact]
        public void UpdateFrequency_WhenCachingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow();

            // Act
            var actual = window.UpdateFrequency;

            // Assert
            Assert.Equal(60, actual);
        }

        [Fact]
        public void UpdateFrequency_WhenSettingValueAndNotCaching_ReturnsCorrectResult()
        {
            // Arrange
            this.mockSilkWindow.SetupProperty(p => p.UpdatesPerSecond);
            var window = CreateWindow();
            window.Show();

            window.CachedIntProps[nameof(window.UpdateFrequency)].IsCaching = false;

            // Act
            window.UpdateFrequency = 30;
            var actual = window.UpdateFrequency;

            // Assert
            Assert.Equal(30, actual);
        }

        [Fact]
        public void ContentLoader_WhenSettingValue_ReturnsCorrectResult()
        {
            // Arrange
            var mockOtherContentLoader = new Mock<IContentLoader>();
            var window = CreateWindow();

            // Act
            window.ContentLoader = mockOtherContentLoader.Object;
            var actual = window.ContentLoader;

            // Assert
            Assert.Same(mockOtherContentLoader.Object, actual);
        }

        [Fact]
        public void MouseCursorVisible_WhenCachingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow();

            // Act
            var actual = window.MouseCursorVisible;

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void MouseCursorVisible_WhenSettingValueAndNotCaching_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            window.CachedBoolProps[nameof(window.MouseCursorVisible)].IsCaching = false;

            // Act
            window.MouseCursorVisible = false;
            var actual = window.MouseCursorVisible;

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void WindowState_WhenGettingInvalidValue_ThrowsException()
        {
            // Arrange
            this.mockSilkWindow.SetupGet(p => p.WindowState).Returns((WindowState)1234);
            var window = CreateWindow();
            window.Show();

            // Act & Assert
            AssertExtensions.ThrowsWithMessage<EnumOutOfRangeException>(() =>
            {
                _ = window.WindowState;
            }, "The enum 'Silk.NET.Windowing.WindowState' is invalid because it is out of range.");
        }

        [Fact]
        public void WindowState_WhenSettingInvalidValue_ThrowsException()
        {
            // Arrange
            // this.mockSilkWindow.SetupSet(p => p.WindowState).Returns((SilkWindowBorder)1234);
            var window = CreateWindow();
            window.Show();

            // Act & Assert
            AssertExtensions.ThrowsWithMessage<EnumOutOfRangeException>(() =>
            {
                window.WindowState = (StateOfWindow)1234;
            }, "The enum 'Velaptor.StateOfWindow' is invalid because it is out of range.");
        }

        [Fact]
        public void WindowState_WhenCachingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow();

            // Act
            var actual = window.WindowState;

            // Assert
            Assert.Equal(StateOfWindow.Normal, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void WindowState_WhenSettingValueAndNotCaching_ReturnsCorrectResult(int windowStateValue)
        {
            // Arrange
            var windowState = (StateOfWindow)windowStateValue;
            this.mockSilkWindow.SetupProperty(p => p.WindowState);
            var window = CreateWindow();
            window.Show();

            window.CachedWindowState.IsCaching = false;

            // Act
            window.WindowState = windowState;
            var actual = window.WindowState;

            // Assert
            Assert.Equal(windowState, actual);
        }

        [Fact]
        public void Initialize_WhenSettingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow();
            var initAction = new Action(() => { });

            // Act
            window.Initialize = initAction;

            // Assert
            Assert.Equal(initAction, window.Initialize);
        }

        [Fact]
        public void Uninitialize_WhenSettingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow();
            var unInitAction = new Action(() => { });

            // Act
            window.Uninitialize = unInitAction;

            // Assert
            Assert.Equal(unInitAction, window.Uninitialize);
        }

        [Fact]
        public void Initialized_WhenWindowIsInitialized_ReturnsTrue()
        {
            // Arrange
            var window = CreateWindow();

            // Act
            window.Show();

            // Assert
            Assert.True(window.Initialized);
        }

        [Fact]
        public void TypeOfBorder_WhenGettingInvalidValue_ThrowsException()
        {
            // Arrange
            this.mockSilkWindow.SetupGet(p => p.WindowBorder).Returns((SilkWindowBorder)1234);
            var window = CreateWindow();
            window.Show();

            // Act & Assert
            AssertExtensions.ThrowsWithMessage<EnumOutOfRangeException>(() =>
            {
                _ = window.TypeOfBorder;
            }, "The enum 'Silk.NET.Windowing.WindowBorder' is invalid because it is out of range.");
        }

        [Fact]
        public void TypeOfBorder_WhenSettingInvalidValue_ThrowsException()
        {
            // Arrange
            this.mockSilkWindow.SetupGet(p => p.WindowBorder).Returns((SilkWindowBorder)1234);
            var window = CreateWindow();
            window.Show();

            // Act & Assert
            AssertExtensions.ThrowsWithMessage<EnumOutOfRangeException>(() =>
            {
                window.TypeOfBorder = (VelaptorWindowBorder)1234;
            }, "The enum 'Velaptor.WindowBorder' is invalid because it is out of range.");
        }

        [Fact]
        public void TypeOfBorder_WhenCachingValue_ReturnsCorrectResult()
        {
            // Arrange
            var window = CreateWindow();

            // Act
            var actual = window.TypeOfBorder;

            // Assert
            Assert.Equal(Velaptor.WindowBorder.Resizable, actual);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void TypeOfBorder_WhenSettingValueAndNotCaching_ReturnsCorrectResult(int windowBorderValue)
        {
            // Arrange
            var windowBorder = (VelaptorWindowBorder)windowBorderValue;
            this.mockSilkWindow.SetupProperty(p => p.WindowBorder);

            var window = CreateWindow();
            window.Show();

            window.CachedTypeOfBorder.IsCaching = false;

            // Act
            window.TypeOfBorder = windowBorder;
            var actual = window.TypeOfBorder;

            // Assert
            Assert.Equal(windowBorder, actual);
        }
        #endregion

        #region Method Tests
        [Fact]
        public void Show_WithNullGLWindow_ThrowsException()
        {
            // Arrange
            this.mockWindowFactory.Setup(m => m.CreateSilkWindow())
                .Returns(() => null!);
            var window = CreateWindow();

            // Act & Assert
            AssertExtensions.ThrowsWithMessage<InvalidOperationException>(() =>
            {
                window.Show();
            }, "The constructed native window cannot be null.");
        }

        [Fact]
        public void Show_WithNoSystemKeyboards_ThrowsException()
        {
            // Arrange
            this.mockSilkInputContext.Setup(p => p.Keyboards)
                .Returns(Array.Empty<IKeyboard>().ToReadOnlyCollection());
            var window = CreateWindow();

            // Act & Assert
            AssertExtensions.ThrowsWithMessage<NoKeyboardException>(() =>
            {
                window.Show();
            }, "Input Exception: No connected keyboards available.");
        }

        [Fact]
        public void Show_WithNoSystemMice_ThrowsException()
        {
            // Arrange
            this.mockSilkInputContext.Setup(p => p.Mice)
                .Returns(Array.Empty<IMouse>().ToReadOnlyCollection());
            var window = CreateWindow();

            // Act & Assert
            AssertExtensions.ThrowsWithMessage<NoMouseException>(() =>
            {
                window.Show();
            }, "Input Exception: No connected mice available.");
        }

        [Fact]
        public void Show_WhenInvoked_InitializesWindowFacade()
        {
            // Arrange
            var window = CreateWindow(123, 456);

            // Act
            window.Show();
            // this.mockWindowFacade.Raise(w => w.Load += null, EventArgs.Empty);

            // Assert
            // this.mockWindowFacade.Verify(m => m.Init(123, 456), Times.Once());
        }

        [Fact]
        public void Show_WhenInvoked_RunsWindowFacade()
        {
            // Arrange
            var window = CreateWindow(123, 456);

            // Act
            window.Show();

            // Assert
            // this.mockWindowFacade.Verify(m => m.Show(), Times.Once());
        }

        [Fact]
        public void Show_WhenInvoked_SetsUpOpenGLErrorCallback()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // this.mockWindowFacade.Raise(i => i.Load += null, EventArgs.Empty);

            // Act
            AssertExtensions.ThrowsWithMessage<Exception>(() =>
            {
                this.mockGL.Raise(i => i.GLError += null, new GLErrorEventArgs("gl-error"));
            }, "gl-error");

            // Assert
            this.mockGL.VerifyAdd(i => i.GLError += It.IsAny<EventHandler<GLErrorEventArgs>>(), Times.Once());
        }

        [Fact]
        public void Show_WhenUpdatingFrameWhileShuttingDown_DoesNotInvokeUpdateEvent()
        {
            // Arrange
            var updateInvoked = false;
            var window = CreateWindow();

            void TestHandler(FrameTime e) => updateInvoked = true;

            window.Update += TestHandler;

            // Act
            window.Show();
            // this.mockWindowFacade.Raise(m => m.Unload += null, EventArgs.Empty);
            // this.mockWindowFacade.Raise(m => m.UpdateFrame += null, new FrameTimeEventArgs(123));
            window.Update -= TestHandler;

            // Assert
            Assert.False(updateInvoked);
        }

        [Fact]
        public void Show_WhenRenderingFrameWhileShuttingDown_DoesNotInvokeRenderEvent()
        {
            // Arrange
            var renderInvoked = false;
            var window = CreateWindow();

            void TestHandler(FrameTime e) => renderInvoked = true;

            window.Draw += TestHandler;

            // Act
            window.Show();
            // this.mockWindowFacade.Raise(m => m.Unload += null, EventArgs.Empty);
            // this.mockWindowFacade.Raise(m => m.RenderFrame += null, new FrameTimeEventArgs(234));
            window.Draw -= TestHandler;

            // Assert
            Assert.False(renderInvoked);
        }

        [Fact]
        public async void ShowAsync_WhenInvoked_SubscribesToWindowEvents()
        {
            // Arrange
            this.mockTaskService.Setup(m => m.SetAction(It.IsAny<Action>()))
                .Callback<Action>(action =>
                {
                    action();
                });

            var window = CreateWindow();

            // Act
            await window.ShowAsync();

            // Assert
            // this.mockWindowFacade.VerifyAdd(s => s.Load += It.IsAny<EventHandler<EventArgs>>(), Times.Once(), $"Subscription of the '{nameof(IGameWindowFacade.Load)}' event did not occur.");
            // this.mockWindowFacade.VerifyAdd(s => s.Unload += It.IsAny<EventHandler<EventArgs>>(), Times.Once(), $"Subscription of the '{nameof(IGameWindowFacade.Unload)}' event did not occur.");
            // this.mockWindowFacade.VerifyAdd(s => s.UpdateFrame += It.IsAny<EventHandler<FrameTimeEventArgs>>(), Times.Once(), $"Subscription of the '{nameof(IGameWindowFacade.UpdateFrame)}' event did not occur.");
            // this.mockWindowFacade.VerifyAdd(s => s.RenderFrame += It.IsAny<EventHandler<FrameTimeEventArgs>>(), Times.Once(), $"Subscription of the '{nameof(IGameWindowFacade.RenderFrame)}' event did not occur.");
            // this.mockWindowFacade.VerifyAdd(s => s.Resize += It.IsAny<EventHandler<WindowSizeEventArgs>>(), Times.Once(), $"Subscription of the '{nameof(IGameWindowFacade.Resize)}' event did not occur.");
        }

        [Fact]
        public async void ShowAsync_WhenInvoked_InitWindowFacade()
        {
            // Arrange
            this.mockTaskService.Setup(m => m.SetAction(It.IsAny<Action>()))
                .Callback<Action>(action =>
                {
                    action();
                });

            var window = CreateWindow();

            // Act
            await window.ShowAsync();
            // this.mockWindowFacade.Raise(w => w.Load += null, EventArgs.Empty);

            // Assert
            // this.mockWindowFacade.Verify(m => m.Init(It.IsAny<uint>(), It.IsAny<uint>()), Times.Once());
        }

        [Fact]
        public async void ShowAsync_WhenInvoked_ExecutesWindowShow()
        {
            // Arrange
            this.mockTaskService.Setup(m => m.SetAction(It.IsAny<Action>()))
                .Callback<Action>(action =>
                {
                    action();
                });

            var window = CreateWindow();

            // Act
            await window.ShowAsync();

            // Assert
            // this.mockWindowFacade.Verify(m => m.Show(), Times.Once());
        }

        [Fact]
        public async void ShowAsync_WhenInvoked_StartsInternalShowTask()
        {
            // Arrange
            this.mockTaskService.Setup(m => m.SetAction(It.IsAny<Action>()))
                .Callback<Action>(action =>
                {
                    action();
                });

            var window = CreateWindow();

            // Act
            await window.ShowAsync();

            // Assert
            this.mockTaskService.Verify(m => m.Start(), Times.Once());
        }

        [Fact]
        public async void ShowAsync_WhenAfterStartParamIsNotNull_ExecutesAtCorrectTime()
        {
            // Arrange
            var taskServiceSetActionInvoked = false;
            var taskServiceStartInvoked = false;
            this.mockTaskService.Setup(m => m.SetAction(It.IsAny<Action>()))
                .Callback<Action>(_ => taskServiceSetActionInvoked = true);
            this.mockTaskService.Setup(m => m.Start())
                .Callback(() => taskServiceStartInvoked = true);

            var window = CreateWindow();

            // Act
            await window.ShowAsync(() => { });

            // Assert
            Assert.True(taskServiceSetActionInvoked,
                $"The {nameof(ITaskService)}.{nameof(ITaskService.SetAction)}() method must be executed before the 'afterStart` parameter");
            Assert.True(taskServiceStartInvoked,
                $"The {nameof(ITaskService)}.{nameof(ITaskService.Start)}() method must be executed before the 'afterStart` parameter");
        }

        [Fact]
        public async void ShowAsync_WhenAfterUnloadParamIsNotNull_ExecutesActionParamAfterWindowUnloads()
        {
            // Arrange
            this.mockSilkWindow.Setup(m => m.Close())
                .Callback(() =>
                {
                    this.mockSilkWindow.Raise(e => e.Closing += null);
                });

            var afterUnloadExecuted = false;
            var window = CreateWindow();
            window.Show();

            // Act
            await window.ShowAsync(null, () => afterUnloadExecuted = true);
            window.Close();

            // Assert
            Assert.True(afterUnloadExecuted,
                $"The 'afterUnload` parameter must be executed after the window unloads.");
        }

        [Fact]
        public void Dispose_WhenInvoked_DisposesOfWindowFacade()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // Act
            window.Dispose();
            window.Dispose();

            // Assert
            this.mockGL.VerifyRemoveOnce(e => e.GLError -= It.IsAny<EventHandler<GLErrorEventArgs>>(), $"Unsubscription of the '{nameof(IGLInvoker.GLError)}' event did not occur.");
            this.mockSilkWindow.VerifyRemoveOnce(e => e.Load -= It.IsAny<Action>(), $"Unsubscription of the '{nameof(IWindow.Load)}' event did not occur.");
            this.mockSilkWindow.VerifyRemoveOnce(s => s.Update -= It.IsAny<Action<double>>(), $"Unsubscription of the '{nameof(IGameWindowFacade.UpdateFrame)}' event did not occur.");
            this.mockSilkWindow.VerifyRemoveOnce(s => s.Render -= It.IsAny<Action<double>>(), $"Unsubscription of the '{nameof(IGameWindowFacade.RenderFrame)}' event did not occur.");
            this.mockSilkWindow.VerifyRemoveOnce(s => s.Resize -= It.IsAny<Action<Vector2D<int>>>(), $"Unsubscription of the '{nameof(IGameWindowFacade.Resize)}' event did not occur.");
            this.mockSilkWindow.VerifyRemoveOnce(s => s.Closing -= It.IsAny<Action>(), $"Unsubscription of the '{nameof(IGameWindowFacade.Unload)}' event did not occur.");
            this.mockTaskService.Verify(m => m.Dispose(), Times.Once());
            this.mockGL.Verify(m => m.Dispose(), Times.Once());
            this.mockGLFW.Verify(m => m.Dispose(), Times.Once());
        }

        [Fact]
        public void GLWindow_WhenWindowResizes_SetsGLViewportAndTriggersResizeEvent()
        {
            // Arrange
            var window = CreateWindow();
            var actualSize = default(SizeU);
            window.WinResize = u => actualSize = u;
            window.Show();

            // Act
            this.mockSilkWindow.Raise(e => e.Resize += It.IsAny<Action<Vector2D<int>>>(), new Vector2D<int>(11, 22));

            // Assert
            this.mockGL.Verify(m => m.Viewport(0, 0, 11, 22));
            this.mockRenderer.Verify(m => m.OnResize(new SizeU(11u, 22u)), Times.Once);
            Assert.Equal(11u, actualSize.Width);
            Assert.Equal(22u, actualSize.Height);
        }

        [Fact]
        public void GLWindow_WhenUpdatingWhileShuttingDown_DoesNotUpdateAnything()
        {
            // Arrange
            var windowUpdateInvoked = false;
            var window = CreateWindow();
            window.Show();
            window.Update = _ => windowUpdateInvoked = true;

            // Act
            this.mockSilkWindow.Raise(e => e.Closing += null);
            this.mockSilkWindow.Raise(e => e.Update += It.IsAny<Action<double>>(), 0.016);

            // Assert
            Assert.False(windowUpdateInvoked, $"{nameof(GLWindow.Update)} should not of been invoked during window shutdown.");
            this.mockMouseInput.VerifyNever(m => m.SetScrollWheelSpeed(It.IsAny<int>()));
            this.mockMouseInput.VerifyNever(m => m.SetScrollWheelDirection(It.IsAny<MouseScrollDirection>()));
        }

        [Fact]
        public void GLWindow_WhenUpdatingWhenNotShuttingDown_PerformsUpdate()
        {
            // Arrange
            var windowUpdateInvoked = false;
            var window = CreateWindow();
            window.Show();
            window.Update = time =>
            {
                windowUpdateInvoked = true;

                AssertExtensions.EqualWithMessage(0, time.ElapsedTime.Days, "The days value must be 0.");
                AssertExtensions.EqualWithMessage(0, time.ElapsedTime.Hours, "The hours value must be 0.");
                AssertExtensions.EqualWithMessage(0, time.ElapsedTime.Minutes, "The minutes value must be 0.");
                AssertExtensions.EqualWithMessage(0, time.ElapsedTime.Seconds, "The seconds value must be 0.");
                AssertExtensions.EqualWithMessage(16, time.ElapsedTime.Milliseconds, "The milliseconds value must be 16.");
            };

            // Act
            this.mockSilkWindow.Raise(e => e.Update += It.IsAny<Action<double>>(), 0.016);

            // Assert
            Assert.True(windowUpdateInvoked, $"{nameof(GLWindow.Update)} was not invoked.");
            this.mockMouseInput.VerifyOnce(m => m.SetScrollWheelSpeed(It.IsAny<int>()));
            this.mockMouseInput.VerifyOnce(m => m.SetScrollWheelDirection(MouseScrollDirection.None));
        }

        [Fact]
        public void GLWindow_WhenRenderingFrameWithAutoClearEnabled_ClearsGLBuffer()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // Act
            this.mockSilkWindow.Raise(e => e.Render += It.IsAny<Action<double>>(), 0.016);

            // Assert
            this.mockGL.Verify(m => m.Clear(GLClearBufferMask.ColorBufferBit), Times.Once);
        }

        [Fact]
        public void GLWindow_WhenRenderingFrameWithAutoClearDisabled_ClearsGLBuffer()
        {
            // Arrange
            var window = CreateWindow();
            window.AutoClearBuffer = false;
            window.Show();

            // Act
            // this.mockWindowFacade.Raise(e => e.RenderFrame += null, null, new FrameTimeEventArgs(16));

            // Assert
            this.mockGL.Verify(m => m.Clear(It.IsAny<GLClearBufferMask>()), Times.Never);
        }

        [Fact]
        public void GLWindow_WhenRenderingFrame_InvokesDrawAndSwapsBuffer()
        {
            // Arrange
            var drawInvoked = false;
            var window = CreateWindow();
            window.Draw = time =>
            {
                drawInvoked = true;
                Assert.Equal(time.ElapsedTime, new TimeSpan(0, 0, 0, 0, 16));
            };
            window.AutoClearBuffer = false;
            window.Show();

            // Act
            this.mockSilkWindow.Raise(e => e.Render += It.IsAny<Action<double>>(), 0.016);

            // Assert
            Assert.True(drawInvoked, $"The '{nameof(GLWindow.Draw)}()' method should of been invoked.");
            this.mockGLContext.VerifyOnce(m => m.SwapBuffers());
        }

        [Fact]
        public void GLWindow_WhenRenderingFrameDuringShutdown_DoesNotPerformRenderProcess()
        {
            // Arrange
            var drawInvoked = false;
            var window = CreateWindow();
            window.Draw = _ =>
            {
                drawInvoked = true;
            };
            window.AutoClearBuffer = true;
            window.Show();

            // Act
            this.mockSilkWindow.Raise(e => e.Closing += null);
            this.mockSilkWindow.Raise(e => e.Render += It.IsAny<Action<double>>(), 0.016);

            // Assert
            this.mockGL.VerifyNever(m => m.Clear(It.IsAny<GLClearBufferMask>()));
            Assert.False(drawInvoked, $"The '{nameof(GLWindow.Draw)}()' method should not of been invoked.");
            this.mockGLContext.VerifyNever(m => m.SwapBuffers());
        }

        [Fact]
        public void GLWindow_WhenUnloadingWindow_ShutsDownWindow()
        {
            // Arrange
            var uninitializeInvoked = false;
            var window = CreateWindow();
            window.Uninitialize += () => uninitializeInvoked = true;
            window.Show();

            // Act
            this.mockSilkWindow.Raise(e => e.Closing += null);

            // Assert
            Assert.True(uninitializeInvoked);
            this.mockShutDownReactable.Verify(m => m.PushNotification(default, true), Times.Once);
            this.mockShutDownReactable.Verify(m => m.Dispose(), Times.Once);
        }

        [Fact]
        public void GLWindow_WhenKeyboardKeyIsPressedDown_UpdatesKeyboardInputState()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // Act
            this.mockSilkKeyboard.Raise(e => e.KeyDown += It.IsAny<Action<IKeyboard, Key, int>>(),
                null,
                Key.Space,
                0);

            // Assert
            this.mockKeyInput.VerifyOnce(m => m.SetState(KeyCode.Space, true));
        }

        [Fact]
        public void GLWindow_WhenKeyboardKeyIsReleased_UpdatesKeyboardInputState()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // Act
            this.mockSilkKeyboard.Raise(e => e.KeyUp += It.IsAny<Action<IKeyboard, Key, int>>(),
                null,
                Key.Z,
                0);

            // Assert
            this.mockKeyInput.VerifyOnce(m => m.SetState(KeyCode.Z, false));
        }

        [Fact]
        public void GLWindow_WhenMouseButtonIsPressedDown_UpdatesMouseInputState()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // Act
            this.mockSilkMouse.Raise(e => e.MouseDown += It.IsAny<Action<IMouse, SilkMouseButton>>(),
                null,
                SilkMouseButton.Left);

            // Assert
            this.mockMouseInput.VerifyOnce(m => m.SetState(VelaptorMouseButton.LeftButton, true));
        }

        [Fact]
        public void GLWindow_WhenMouseButtonIsReleasedDown_UpdatesMouseInputState()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // Act
            this.mockSilkMouse.Raise(e => e.MouseUp += It.IsAny<Action<IMouse, SilkMouseButton>>(),
                null,
                SilkMouseButton.Right);

            // Assert
            this.mockMouseInput.VerifyOnce(m => m.SetState(VelaptorMouseButton.RightButton, false));
        }

        [Theory]
        [InlineData(123, MouseScrollDirection.ScrollUp)]
        [InlineData(-123, MouseScrollDirection.ScrollDown)]
        [InlineData(0, MouseScrollDirection.None)]
        public void GLWindow_WhenMouseIsScrolled_UpdatesMouseInputState(int wheelValue, MouseScrollDirection expected)
        {
            // Arrange
            var wheelData = new ScrollWheel(0, wheelValue);

            var window = CreateWindow();
            window.Show();

            // Act
            this.mockSilkMouse.Raise(e => e.Scroll += It.IsAny<Action<IMouse, ScrollWheel>>(),
                null,
                wheelData);

            // Assert
            this.mockMouseInput.VerifyOnce(m => m.SetScrollWheelSpeed(wheelValue));
            this.mockMouseInput.VerifyOnce(m => m.SetScrollWheelDirection(expected));
        }

        [Fact]
        public void GLWindow_WhenMouseMoves_UpdatesMouseInputState()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // Act
            this.mockSilkMouse.Raise(e => e.MouseMove += It.IsAny<Action<IMouse, SysVector2>>(),
                null,
                new SysVector2(11f, 22f));

            // Assert
            this.mockMouseInput.VerifyOnce(m => m.SetXPos(11));
            this.mockMouseInput.VerifyOnce(m => m.SetYPos(22));
        }

        [Fact]
        public void Close_WhenInvoked_ClosesInternalWindowFacade()
        {
            // Arrange
            var window = CreateWindow();
            window.Show();

            // Act
            window.Close();

            // Assert
            this.mockSilkWindow.VerifyOnce(m => m.Close());
        }
        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="GLWindow"/> for the purpose of testing.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <returns>The instance to test.</returns>
        private GLWindow CreateWindow(uint width = 10, uint height = 20)
            => new (
                width,
                height,
                this.mockWindowFactory.Object,
                this.mockInputFactory.Object,
                this.mockGL.Object,
                this.mockGLFW.Object,
                this.mockMonitorService.Object,
                this.mockPlatform.Object,
                this.mockTaskService.Object,
                this.mockContentLoader.Object,
                this.mockRenderer.Object,
                this.mockKeyInput.Object,
                this.mockMouseInput.Object,
                this.mockContextReactable.Object,
                this.mockGLInitReactable.Object,
                this.mockShutDownReactable.Object);

        /// <summary>
        /// Mocks the following:
        /// <list type="bullet">
        ///     <item><see cref="Silk"/>.<see cref="IInputContext"/></item>
        ///     <item><see cref="Velaptor"/>.<see cref="IInputFactory"/></item>
        ///     <item><see cref="Silk"/>.<see cref="IKeyboard"/></item>
        ///     <item><see cref="Silk"/>.<see cref="IMouse"/></item>
        /// </list>
        /// </summary>
        private void MockSystemSilkInput()
        {
            this.mockSilkInputContext = new Mock<IInputContext>();
            this.mockInputFactory = new Mock<IInputFactory>();
            this.mockInputFactory.Setup(m => m.CreateInput()).Returns(this.mockSilkInputContext.Object);

            MockSilkKeyboard();
            MockSilkMouse();
        }

        /// <summary>
        /// Mocks the <see cref="Silk"/> keyboard input.
        /// </summary>
        private void MockSilkKeyboard()
        {
            this.mockSilkKeyboard = new Mock<IKeyboard>();
            var keyboards = new List<IKeyboard> { this.mockSilkKeyboard.Object };
            this.mockSilkInputContext.Setup(p => p.Keyboards)
                .Returns(keyboards.ToReadOnlyCollection());
        }

        /// <summary>
        /// Mocks the <see cref="Silk"/> mouse input.
        /// </summary>
        private void MockSilkMouse()
        {
            this.mockMouseCursor = new Mock<ICursor>();
            this.mockMouseCursor.SetupProperty(p => p.CursorMode);

            this.mockSilkMouse = new Mock<IMouse>();
            this.mockSilkMouse.SetupGet(p => p.Cursor).Returns(this.mockMouseCursor.Object);

            var mice = new List<IMouse> { this.mockSilkMouse.Object };

            this.mockSilkInputContext.Setup(p => p.Mice)
                .Returns(mice.ToReadOnlyCollection());
        }
    }
}
