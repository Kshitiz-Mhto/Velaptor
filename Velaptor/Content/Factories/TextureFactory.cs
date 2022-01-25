﻿// <copyright file="TextureFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.Content.Factories
{
    // ReSharper disable RedundantNameQualifier
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Velaptor;
    using Velaptor.Graphics;
    using Velaptor.NativeInterop.OpenGL;
    using Velaptor.Observables;
    using VelObservable = Velaptor.Observables.Core.IObservable<uint>;

    // ReSharper restore RedundantNameQualifier

    /// <summary>
    /// Creates <see cref="ITexture"/> objects for rendering.
    /// </summary>
    internal class TextureFactory : ITextureFactory
    {
        private readonly IGLInvoker gl;
        private readonly IGLInvokerExtensions glExtensions;
        private readonly VelObservable disposeTexturesObservable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureFactory"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TextureFactory()
        {
            this.gl = IoC.Container.GetInstance<IGLInvoker>();
            this.glExtensions = IoC.Container.GetInstance<IGLInvokerExtensions>();
            this.disposeTexturesObservable = IoC.Container.GetInstance<DisposeTexturesObservable>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureFactory"/> class.
        /// </summary>
        /// <param name="gl">Provides access to OpenGL functions.</param>
        /// <param name="glExtensions">Provides extensions/helper methods for OpenGL related operations.</param>
        /// <param name="disposeTexturesObservable">Sends push notifications to dispose of textures.</param>
        internal TextureFactory(IGLInvoker gl, IGLInvokerExtensions glExtensions, VelObservable disposeTexturesObservable)
        {
            this.gl = gl ?? throw new ArgumentNullException(nameof(gl), "The parameter must not be null.");
            this.glExtensions = glExtensions ?? throw new ArgumentNullException(nameof(glExtensions), "The parameter must not be null.");
            this.disposeTexturesObservable = disposeTexturesObservable ?? throw new ArgumentNullException(nameof(disposeTexturesObservable), "The parameter must not be null.");;
        }

        /// <inheritdoc/>
        public ITexture Create(string name, string filePath, ImageData imageData)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), "The parameter must not be null or empty.");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath), "The parameter must not be null or empty.");
            }

            return new Texture(this.gl, this.glExtensions, this.disposeTexturesObservable, name, filePath, imageData);
        }
    }
}
