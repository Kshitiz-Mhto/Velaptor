﻿// <copyright file="IOpenGLService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace Velaptor.NativeInterop.OpenGL
{
    using System.Drawing;
    using System.Numerics;
    using Velaptor.OpenGL;

    /// <summary>
    /// Provides OpenGL helper methods to improve OpenGL related operations.
    /// </summary>
    internal interface IOpenGLService
    {
        /// <summary>
        /// Gets the size of the viewport.
        /// </summary>
        /// <returns>The size of the viewport.</returns>
        Size GetViewPortSize();

        /// <summary>
        /// Sets the size of the view port.
        /// </summary>
        /// <param name="size">The vector representing the width and height of the viewport.</param>
        void SetViewPortSize(Size size);

        /// <summary>
        /// Gets the position of the viewport.
        /// </summary>
        /// <returns>The position of the viewport.</returns>
        Vector2 GetViewPortPosition();

        /// <summary>
        /// Binds a vertex buffer object for updating buffer data in OpenGL.
        /// </summary>
        /// <param name="vbo">The vertex buffer object ID.</param>
        void BindVBO(uint vbo);

        /// <summary>
        /// Unbinds the current vertex buffer object if one is currently bound.
        /// </summary>
        void UnbindVBO();

        /// <summary>
        /// Binds an element buffer object for updating element data in OpenGL.
        /// </summary>
        /// <param name="ebo">The element buffer object ID.</param>
        /// <remarks>
        ///     This is also called an IBO (Index Buffer Object).
        /// </remarks>
        void BindEBO(uint ebo);

        /// <summary>
        /// Unbinds the element array buffer object if one is currently bound.
        /// </summary>
        /// <remarks>
        /// NOTE: Make sure to unbind AFTER you unbind the VAO.  This is because the EBO is stored
        /// inside of the VAO.  Unbinding the EBO before unbinding, (or without unbinding the VAO),
        /// you are telling OpenGL that you don't want your VAO to use the EBO.
        /// </remarks>
        void UnbindEBO();

        /// <summary>
        /// Binds the element array object for updating vertex buffer data in OpenGL.
        /// </summary>
        /// <param name="vao">The vertex array buffer object ID.</param>
        void BindVAO(uint vao);

        /// <summary>
        /// Unbinds the current element array object that is currently bound if one is currently bound.
        /// </summary>
        void UnbindVAO();

        /// <summary>
        /// Binds a two dimensional texture using the given <paramref name="textureId"/>.
        /// </summary>
        /// <param name="textureId">The ID of the texture to bind.</param>
        void BindTexture2D(uint textureId);

        /// <summary>
        /// Binds the currently bound two dimensional texture.
        /// </summary>
        void UnbindTexture2D();

        /// <summary>
        /// Returns a value indicating if the program linking process was successful.
        /// </summary>
        /// <param name="programId">The ID of the program to check.</param>
        /// <returns><see langword="true"/> if the linking was successful.</returns>
        bool ProgramLinkedSuccessfully(uint programId);

        /// <summary>
        /// Returns a value indicating if the shader was compiled successfully.
        /// </summary>
        /// <param name="shaderId">The ID of the shader to check.</param>
        /// <returns><see langword="true"/> if the shader compiled successfully.</returns>
        bool ShaderCompiledSuccessfully(uint shaderId);

        /// <summary>
        /// Creates a debug group into the command stream.
        /// </summary>
        /// <param name="label">The label for the debug group.</param>
        void BeginGroup(string label);

        /// <summary>
        /// Ends the most recent debug group.
        /// </summary>
        void EndGroup();

        /// <summary>
        /// Labels a shader with the given <paramref name="shaderId"/> with the given <paramref name="label"/>.
        /// </summary>
        /// <param name="shaderId">The ID of the shader.</param>
        /// <param name="label">The label to give the shader.</param>
        void LabelShader(uint shaderId, string label);

        /// <summary>
        /// Labels a shader program with the given <paramref name="shaderId"/> with the given <paramref name="label"/>.
        /// </summary>
        /// <param name="shaderId">The ID of the shader program.</param>
        /// <param name="label">The label to give the shader program.</param>
        void LabelShaderProgram(uint shaderId, string label);

        /// <summary>
        /// Labels a vertex array object with the given <paramref name="vertexArrayId"/> with the given <paramref name="label"/>.
        /// </summary>
        /// <param name="vertexArrayId">The ID of the shader.</param>
        /// <param name="label">The label to give the vertex array object.</param>
        void LabelVertexArray(uint vertexArrayId, string label);

        /// <summary>
        /// Labels a buffer object with the given <paramref name="bufferId"/> with the given <paramref name="label"/>.
        /// </summary>
        /// <param name="bufferId">The ID of the buffer object.</param>
        /// <param name="label">The label to give the buffer object.</param>
        /// <param name="bufferType">The type of buffer.</param>
        void LabelBuffer(uint bufferId, string label, BufferType bufferType);

        /// <summary>
        /// Labels a texture with the given <paramref name="textureId"/> with the given <paramref name="label"/>.
        /// </summary>
        /// <param name="textureId">The ID of the texture.</param>
        /// <param name="label">The label to give the texture.</param>
        void LabelTexture(uint textureId, string label);
    }
}