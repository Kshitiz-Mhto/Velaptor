﻿// <copyright file="TextureLoader.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Raptor.Content
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using Raptor.Graphics;
    using Raptor.OpenGL;
    using Raptor.Services;

    /// <summary>
    /// Loads textures.
    /// </summary>
    public class TextureLoader : ILoader<ITexture>
    {
        private readonly ConcurrentDictionary<string, ITexture> textures = new ConcurrentDictionary<string, ITexture>();
        private readonly IGLInvoker gl;
        private readonly IImageFileService imageFileService;
        private readonly IPathResolver pathResolver;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureLoader"/> class.
        /// </summary>
        /// <param name="imageFileService">Loads an image file.</param>
        /// <param name="texturePathResolver">Resolves paths to texture content.</param>
        [ExcludeFromCodeCoverage]
        public TextureLoader(IImageFileService imageFileService, IPathResolver texturePathResolver)
        {
            this.gl = new GLInvoker();
            this.imageFileService = imageFileService;
            this.pathResolver = texturePathResolver;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureLoader"/> class.
        /// </summary>
        /// <param name="gl">Invokes OpenGL functions.</param>
        /// <param name="imageFileService">Loads an image file.</param>
        /// <param name="texturePathResolver">Resolves paths to texture content.</param>
        internal TextureLoader(IGLInvoker gl, IImageFileService imageFileService, IPathResolver texturePathResolver)
        {
            this.gl = gl;
            this.imageFileService = imageFileService;
            this.pathResolver = texturePathResolver;
        }

        /// <inheritdoc/>
        public ITexture Load(string name)
        {
            var filePath = this.pathResolver.ResolveFilePath(name);

            return this.textures.GetOrAdd(filePath, (key) =>
            {
                var (pixels, width, height) = this.imageFileService.Load(key);

                return new Texture(this.gl, name, pixels, width, height);
            });
        }

        /// <inheritdoc/>
        public void Unload(string name)
        {
            var filePath = this.pathResolver.ResolveFilePath(name);

            if (this.textures.TryRemove(filePath, out var texture))
            {
                texture.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing">True to dispose of managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var texture in this.textures.Values)
                {
                    texture.Dispose();
                }

                this.textures.Clear();
            }

            this.isDisposed = true;
        }
    }
}
