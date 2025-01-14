﻿// <copyright file="WindowSizeEventArgs.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.OpenGL
{
    /// <summary>
    /// Holds information about an event when a window resizes.
    /// </summary>
    internal class WindowSizeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowSizeEventArgs"/> class.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        public WindowSizeEventArgs(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the width of the window.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the window.
        /// </summary>
        public int Height { get; }
    }
}
