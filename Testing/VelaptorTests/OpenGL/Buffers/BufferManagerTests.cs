﻿// <copyright file="BufferManagerTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace VelaptorTests.OpenGL.Buffers
{
    using System;
    using System.Numerics;
    using Moq;
    using Velaptor;
    using Velaptor.Factories;
    using Velaptor.Graphics;
    using Velaptor.OpenGL;
    using Velaptor.OpenGL.Buffers;
    using VelaptorTests.Helpers;
    using Xunit;

    /// <summary>
    /// Tests the <see cref="BufferManager"/> class.
    /// </summary>
    public class BufferManagerTests
    {
        private readonly Mock<IGPUBufferFactory> mockBufferFactory;
        private readonly Mock<IGPUBuffer<TextureBatchItem>> mockTextureBuffer;
        private readonly Mock<IGPUBuffer<FontGlyphBatchItem>> mockFontGlyphBuffer;
        private readonly Mock<IGPUBuffer<RectShape>> mockRectBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferManagerTests"/> class.
        /// </summary>
        public BufferManagerTests()
        {
            this.mockTextureBuffer = new Mock<IGPUBuffer<TextureBatchItem>>();
            this.mockFontGlyphBuffer = new Mock<IGPUBuffer<FontGlyphBatchItem>>();
            this.mockRectBuffer = new Mock<IGPUBuffer<RectShape>>();

            this.mockBufferFactory = new Mock<IGPUBufferFactory>();
            this.mockBufferFactory.Setup(m => m.CreateTextureGPUBuffer()).Returns(this.mockTextureBuffer.Object);
            this.mockBufferFactory.Setup(m => m.CreateFontGPUBuffer()).Returns(this.mockFontGlyphBuffer.Object);
            this.mockBufferFactory.Setup(m => m.CreateRectGPUBuffer()).Returns(this.mockRectBuffer.Object);
        }

        #region Method Tests
        [Fact]
        public void SetViewPortSize_WithTextureBufferType_ReturnsCorrectResult()
        {
            // Arrange
            var expectedSize = new SizeU(111u, 222u);
            var manager = CreateManager();

            // Act
            manager.SetViewPortSize(VelaptorBufferType.Texture, new SizeU(111u, 222u));

            // Assert
            this.mockTextureBuffer.VerifySetOnce(p => p.ViewPortSize = expectedSize);
        }

        [Fact]
        public void SetViewPortSize_WithFontGlyphBufferType_ReturnsCorrectResult()
        {
            // Arrange
            var expectedSize = new SizeU(111u, 222u);
            var manager = CreateManager();

            // Act
            manager.SetViewPortSize(VelaptorBufferType.Font, new SizeU(111u, 222u));

            // Assert
            this.mockFontGlyphBuffer.VerifySetOnce(p => p.ViewPortSize = expectedSize);
        }

        [Fact]
        public void SetViewPortSize_WithRectangleBufferType_ReturnsCorrectResult()
        {
            // Arrange
            var expectedSize = new SizeU(111u, 222u);
            var manager = CreateManager();

            // Act
            manager.SetViewPortSize(VelaptorBufferType.Rectangle, new SizeU(111u, 222u));

            // Assert
            this.mockRectBuffer.VerifySetOnce(p => p.ViewPortSize = expectedSize);
        }

        [Fact]
        public void SetViewPortSize_WithInvalidBufferType_ThrowsException()
        {
            // Arrange
            var manager = CreateManager();

            // Act & Assert
            AssertExtensions.ThrowsWithMessage<ArgumentOutOfRangeException>(() =>
            {
                manager.SetViewPortSize((VelaptorBufferType)1234, It.IsAny<SizeU>());
            }, $"The enum '{nameof(VelaptorBufferType)}' value is invalid. (Parameter 'bufferType')\r\nActual value was 1234.");
        }

        [Fact]
        public void UploadTextureData_WhenInvoked_UploadsData()
        {
            // Arrange
            var data = default(TextureBatchItem);
            data.Angle = 45;
            data.Effects = RenderEffects.FlipHorizontally;
            data.Size = 1.5f;

            var manager = CreateManager();

            // Act
            manager.UploadTextureData(data, 123u);

            // Assert
            this.mockTextureBuffer.VerifyOnce(m => m.UploadData(data, 123u));
        }

        [Fact]
        public void UploadFontGlyphData_WhenInvoked_UploadsData()
        {
            // Arrange
            var data = default(FontGlyphBatchItem);
            data.Angle = 90;
            data.Effects = RenderEffects.FlipVertically;
            data.Size = 2.5f;

            var manager = CreateManager();

            // Act
            manager.UploadFontGlyphData(data, 456u);

            // Assert
            this.mockFontGlyphBuffer.VerifyOnce(m => m.UploadData(data, 456u));
        }

        [Fact]
        public void UploadRectangleData_WhenInvoked_UploadsData()
        {
            // Arrange
            var data = default(RectShape);
            data.Position = new Vector2(111, 222);
            data.Width = 444;
            data.Height = 555;

            var manager = CreateManager();

            // Act
            manager.UploadRectData(data, 789u);

            // Assert
            this.mockRectBuffer.VerifyOnce(m => m.UploadData(data, 789u));
        }
        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="BufferManager"/> for the purpose of testing.
        /// </summary>
        /// <returns>The instance to test.</returns>
        private BufferManager CreateManager() => new (this.mockBufferFactory.Object);
    }
}
