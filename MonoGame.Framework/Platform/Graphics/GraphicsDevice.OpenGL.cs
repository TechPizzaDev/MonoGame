// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Globalization;
using System.Diagnostics;

#if ANGLE
using OpenTK.Graphics;
#else
using MonoGame.OpenGL;
#endif

namespace MonoGame.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        private static List<IntPtr> ContextDisposeQueue { get; } = new List<IntPtr>();
        private static object DisposeContextsLock { get; } = new object();

        private static BufferBindingInfo[]? _bufferBindingInfos;
        private static bool[]? _newEnabledVertexAttributes;

#if DESKTOPGL || ANGLE
        internal IGraphicsContext Context { get; private set; }
#endif

        private DrawBuffersElementType[] _drawBuffers;

        private List<GLHandle> _disposeThisFrame = new List<GLHandle>();
        private List<GLHandle> _resourceFreeQueue = new List<GLHandle>();
        private object ResourceFreeingLock { get; } = new object();

        private ShaderProgramCache _programCache;
        private ShaderProgram? _shaderProgram;

        internal static HashSet<int> EnabledVertexAttributes { get; } = new HashSet<int>();
        internal static bool _attribsDirty;

        internal FramebufferHelper _framebufferHelper;

        internal int _glMajorVersion;
        internal int _glMinorVersion;
        internal int _glFramebuffer;
        internal int MaxVertexAttributes;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal bool _lastBlendEnable;
        internal BlendState _lastBlendState = new BlendState();
        internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        internal RasterizerState _lastRasterizerState = new RasterizerState();
        private DepthStencilState _clearDepthStencilState = new DepthStencilState { StencilEnable = true };
        private Vector4 _lastClearColor = Vector4.Zero;
        private float _lastClearDepth = 1f;
        private int _lastClearStencil;

        /// <summary>
        /// Get a hashed value based on the currently bound shaders.
        /// </summary>
        /// <exception cref="InvalidOperationException">No shaders are bound.</exception>
        private int ShaderProgramHash
        {
            get
            {
                if (_vertexShader == null && _pixelShader == null)
                    throw new InvalidOperationException("There is no shader bound.");

                if (_vertexShader == null)
                    return _pixelShader.HashKey;
                if (_pixelShader == null)
                    return _vertexShader.HashKey;

                return HashCode.Combine(_pixelShader.HashKey, _vertexShader.HashKey);
            }
        }

        internal static void SetVertexAttributeArray(bool[] attrs)
        {
            for (int i = 0; i < attrs.Length; i++)
            {
                bool contains = EnabledVertexAttributes.Contains(i);
                if (attrs[i] && !contains)
                {
                    EnabledVertexAttributes.Add(i);
                    GL.EnableVertexAttribArray(i);
                    GL.CheckError();
                }
                else if (!attrs[i] && contains)
                {
                    EnabledVertexAttributes.Remove(i);
                    GL.DisableVertexAttribArray(i);
                    GL.CheckError();
                }
            }
        }

        private void ApplyAttribs(Shader shader, int baseVertex)
        {
            Debug.Assert(_bufferBindingInfos != null);
            Debug.Assert(_newEnabledVertexAttributes != null);

            int programHash = ShaderProgramHash;
            bool bindingsChanged = false;

            int vertexBufferCount = _vertexBuffers.Count;
            for (int slot = 0; slot < vertexBufferCount; slot++)
            {
                var vertexBufferBinding = _vertexBuffers.Get(slot);
                var vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                var attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);

                int vertexStride = vertexDeclaration.VertexStride;
                var offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                if (!_attribsDirty)
                {
                    var info = _bufferBindingInfos[slot];
                    if (info.VertexOffset == offset &&
                        info.InstanceFrequency == vertexBufferBinding.InstanceFrequency &&
                        info.Vbo == vertexBufferBinding.VertexBuffer._glBuffer &&
                        ReferenceEquals(info.AttributeInfo, attrInfo))
                        continue;
                }

                bindingsChanged = true;

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferBinding.VertexBuffer._glBuffer);
                GL.CheckError();

                // If InstanceFrequency of the buffer is not zero
                // and instancing is not supported, throw an exception.
                if (vertexBufferBinding.InstanceFrequency > 0)
                    AssertSupportsInstancing();

                foreach (var element in attrInfo.Elements.Span)
                {
                    GL.VertexAttribPointer(
                        element.AttributeLocation,
                        element.NumberOfElements,
                        element.VertexAttribPointerType,
                        element.Normalized,
                        vertexStride,
                        offset + element.Offset);

                    // only set the divisor if instancing is supported
                    if (Capabilities.SupportsInstancing)
                        GL.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);

                    GL.CheckError();
                }

                _bufferBindingInfos[slot].VertexOffset = offset;
                _bufferBindingInfos[slot].AttributeInfo = attrInfo;
                _bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
                _bufferBindingInfos[slot].Vbo = vertexBufferBinding.VertexBuffer._glBuffer;
            }

            _attribsDirty = false;

            if (bindingsChanged)
            {
                for (int i = 0; i < _newEnabledVertexAttributes.Length; i++)
                    _newEnabledVertexAttributes[i] = false;

                for (int slot = 0; slot < vertexBufferCount; slot++)
                {
                    foreach (var element in _bufferBindingInfos[slot].AttributeInfo.Elements.Span)
                        _newEnabledVertexAttributes[element.AttributeLocation] = true;
                }
            }
            SetVertexAttributeArray(_newEnabledVertexAttributes);
        }

        private void PlatformSetup()
        {
            _programCache = new ShaderProgramCache(this);

#if DESKTOPGL || ANGLE
            var window = SDLGameWindow.Instance;

            if (Context == null || Context.IsDisposed)
                Context = GL.CreateContext(window);

            Context.MakeCurrent(window);
            Context.SwapInterval = PresentationParameters.PresentationInterval.GetSwapInterval();
#endif

            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out MaxTextureSlots);
            GL.CheckError();

            GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
            GL.CheckError();

            _maxVertexBufferSlots = MaxVertexAttributes;
            _newEnabledVertexAttributes = new bool[MaxVertexAttributes];

            // try getting the context version
            // GL_MAJOR_VERSION and GL_MINOR_VERSION are GL 3.0+ only, so we need to rely on the GL_VERSION string
            // for non GLES this string always starts with the version number in the "major.minor" format,
            // but can be followed by multiple vendor specific characters.
            // For GLES this string is formatted as: OpenGL ES <version number> <vendor-specific information>

            try
            {
                string? version = GL.GetString(StringName.Version);
                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version.");

                if (GL.IsES)
                {
                    string[] versionSplit = version.Split(' ');
                    if (versionSplit.Length > 2 && versionSplit[0].Equals("OpenGL") && versionSplit[1].Equals("ES"))
                    {
                        _glMajorVersion = Convert.ToInt32(versionSplit[2].Substring(0, 1));
                        _glMinorVersion = Convert.ToInt32(versionSplit[2].Substring(2, 1));
                    }
                    else
                    {
                        _glMajorVersion = 1;
                        _glMinorVersion = 1;
                    }
                }
                else
                {
                    _glMajorVersion = Convert.ToInt32(version.Substring(0, 1), CultureInfo.InvariantCulture);
                    _glMinorVersion = Convert.ToInt32(version.Substring(2, 1), CultureInfo.InvariantCulture);
                }
            }
            catch (FormatException)
            {
                //if it fails we default to 1.1 context
                _glMajorVersion = 1;
                _glMinorVersion = 1;
            }


            if (!GL.IsES)
            {
                // Initialize draw buffer attachment array
                GL.GetInteger(GetPName.MaxDrawBuffers, out int maxDrawBuffers);
                GL.CheckError();

                _drawBuffers = new DrawBuffersElementType[maxDrawBuffers];
                for (int i = 0; i < maxDrawBuffers; i++)
                    _drawBuffers[i] = (DrawBuffersElementType)(FramebufferAttachment.ColorAttachment0Ext + i);
            }
        }

        private void PlatformInitialize()
        {
            _viewport = new Viewport(
                0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            EnabledVertexAttributes.Clear();

            // Free all the cached shader programs. 
            _programCache.Clear();
            _shaderProgram = null;

            _framebufferHelper = FramebufferHelper.Create(this);

            // Force resetting states
            PlatformApplyBlend(true);
            DepthStencilState.PlatformApplyState(this, true);
            RasterizerState.PlatformApplyState(this, true);

            _bufferBindingInfos = new BufferBindingInfo[_maxVertexBufferSlots];
            for (int i = 0; i < _bufferBindingInfos.Length; i++)
                _bufferBindingInfos[i] = new BufferBindingInfo { Vbo = default };
        }

        private void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // TODO: We need to figure out how to detect if we have a
            // depth stencil buffer or not, and clear options relating
            // to them if not attached.

            // Unlike with XNA and DirectX...  GL.Clear() obeys several
            // different render states:
            //
            //  - The color write flags.
            //  - The scissor rectangle.
            //  - The depth/stencil state.
            //
            // So overwrite these states with what is needed to perform
            // the clear correctly and restore it afterwards.
            //
            var prevScissorRect = ScissorRectangle;
            var prevDepthStencilState = DepthStencilState;
            var prevBlendState = BlendState;
            ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            DepthStencilState = _clearDepthStencilState;
            BlendState = BlendState.Opaque;
            ApplyState(false);

            ClearBufferMask bufferMask = 0;
            if ((options & ClearOptions.Color) == ClearOptions.Color)
            {
                if (color != _lastClearColor)
                {
                    GL.ClearColor(color.X, color.Y, color.Z, color.W);
                    GL.CheckError();
                    _lastClearColor = color;
                }
                bufferMask |= ClearBufferMask.ColorBufferBit;
            }
            if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
            {
                if (stencil != _lastClearStencil)
                {
                    GL.ClearStencil(stencil);
                    GL.CheckError();
                    _lastClearStencil = stencil;
                }
                bufferMask |= ClearBufferMask.StencilBufferBit;
            }

            if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
            {
                if (depth != _lastClearDepth)
                {
                    if (GL.ClearDepthF != null)
                        GL.ClearDepthF(depth);
                    else
                        GL.ClearDepth(depth);

                    GL.CheckError();
                    _lastClearDepth = depth;
                }
                bufferMask |= ClearBufferMask.DepthBufferBit;
            }

#if MONOMAC || IOS
            if (GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt) == 
                FramebufferErrorCode.FramebufferComplete)
#endif
            {
                GL.Clear(bufferMask);
                GL.CheckError();
            }

            // Restore the previous render state.
            ScissorRectangle = prevScissorRect;
            DepthStencilState = prevDepthStencilState;
            BlendState = prevBlendState;
        }

        private void PlatformFlush()
        {
            GL.Flush();
            GL.CheckError();
        }

        private void PlatformReset()
        {
        }

        private void PlatformDispose()
        {
            // Free all the cached shader programs.
            _programCache.Dispose();

#if DESKTOPGL || ANGLE
            Context?.Dispose();
#endif

            _lastBlendState?.Dispose();

            _lastDepthStencilState?.Dispose();

            _lastRasterizerState?.Dispose();

            _clearDepthStencilState?.Dispose();
        }

        internal void DisposeResource(GLHandle handle)
        {
            if (IsDisposed || handle.IsNull)
                return;

            lock (ResourceFreeingLock)
                _resourceFreeQueue.Add(handle);
        }

#if DESKTOPGL || ANGLE
        internal static void DisposeContext(IntPtr resource)
        {
            lock (DisposeContextsLock)
                ContextDisposeQueue.Add(resource);
        }

        internal static void DisposeContexts()
        {
            lock (DisposeContextsLock)
            {
                int count = ContextDisposeQueue.Count;
                for (int i = 0; i < count; i++)
                    SDL.GL.DeleteContext(ContextDisposeQueue[i]);
                ContextDisposeQueue.Clear();
            }
        }
#endif

        private void PlatformPresent()
        {
#if DESKTOPGL || ANGLE
            Context.SwapBuffers();
#endif
            GL.CheckError();

            // Dispose of any GL resources that were disposed in another thread
            int count = _disposeThisFrame.Count;
            for (int i = 0; i < count; i++)
                _disposeThisFrame[i].Free();
            _disposeThisFrame.Clear();

            lock (ResourceFreeingLock)
            {
                // Swap lists so resources added during this draw will be released after the next draw
                var tmp = _disposeThisFrame;
                _disposeThisFrame = _resourceFreeQueue;
                _resourceFreeQueue = tmp;
            }
        }

        private void PlatformSetViewport(in Viewport value)
        {
            if (IsRenderTargetBound)
            {
                GL.Viewport(value.X, value.Y, value.Width, value.Height);
            }
            else
            {
                var pp = PresentationParameters;
                GL.Viewport(value.X, pp.BackBufferHeight - value.Y - value.Height, value.Width, value.Height);
            }
            GL.LogError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(value.MinDepth, value.MaxDepth);
            GL.LogError("GraphicsDevice.Viewport_set() GL.DepthRange");

            // In OpenGL we have to re-apply the special "posFixup"
            // vertex shader uniform if the viewport changes.
            VertexShaderDirty = true;
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            _framebufferHelper.BindFramebuffer(_glFramebuffer);

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.MarkDirty();
        }

        /// <summary>
        /// FBO cache, we create 1 FBO per RenderTargetBinding combination.
        /// </summary>
        private Dictionary<RenderTargetBinding[], int> _glFramebuffers =
            new Dictionary<RenderTargetBinding[], int>(RenderTargetBindingArrayComparer.Instance);

        /// <summary>
        /// FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        /// </summary>
        private Dictionary<RenderTargetBinding[], int> _glResolveFramebuffers =
            new Dictionary<RenderTargetBinding[], int>(RenderTargetBindingArrayComparer.Instance);

        internal void PlatformCreateRenderTarget(
            IRenderTarget renderTarget, int width, int height, bool mipMap,
            SurfaceFormat preferredFormat,
            DepthFormat preferredDepthFormat,
            int preferredMultiSampleCount,
            RenderTargetUsage usage)
        {
            void Construct()
            {
                int color = 0;
                int depth = 0;
                int stencil = 0;

                if (preferredMultiSampleCount > 0 && _framebufferHelper.SupportsBlitFramebuffer)
                {
                    color = _framebufferHelper.GenRenderbuffer();
                    _framebufferHelper.BindRenderbuffer(color);
                    _framebufferHelper.RenderbufferStorageMultisample(
                        preferredMultiSampleCount, (int)RenderbufferStorage.Rgba8, width, height);
                }

                if (preferredDepthFormat != DepthFormat.None)
                {
                    var depthInternalFormat = RenderbufferStorage.DepthComponent16;
                    var stencilInternalFormat = (RenderbufferStorage)0;
                    switch (preferredDepthFormat)
                    {
                        case DepthFormat.Depth16:
                            depthInternalFormat = RenderbufferStorage.DepthComponent16;
                            break;

                        case DepthFormat.Depth24:
                            if (GL.IsES)
                            {
                                if (Capabilities.SupportsDepth24)
                                    depthInternalFormat = RenderbufferStorage.DepthComponent24Oes;
                                else if (Capabilities.SupportsDepthNonLinear)
                                    depthInternalFormat = (RenderbufferStorage)0x8E2C;
                                else
                                    depthInternalFormat = RenderbufferStorage.DepthComponent16;
                            }
                            else
                            {
                                depthInternalFormat = RenderbufferStorage.DepthComponent24;
                            }
                            break;

                        case DepthFormat.Depth24Stencil8:
                            if (GL.IsES)
                            {
                                if (Capabilities.SupportsPackedDepthStencil)
                                    depthInternalFormat = RenderbufferStorage.Depth24Stencil8Oes;
                                else
                                {
                                    if (Capabilities.SupportsDepth24)
                                        depthInternalFormat = RenderbufferStorage.DepthComponent24Oes;
                                    else if (Capabilities.SupportsDepthNonLinear)
                                        depthInternalFormat = (RenderbufferStorage)0x8E2C;
                                    else
                                        depthInternalFormat = RenderbufferStorage.DepthComponent16;
                                    stencilInternalFormat = RenderbufferStorage.StencilIndex8;
                                }
                            }
                            else
                            {
                                depthInternalFormat = RenderbufferStorage.Depth24Stencil8;
                            }
                            break;
                    }

                    if (depthInternalFormat != 0)
                    {
                        depth = _framebufferHelper.GenRenderbuffer();
                        _framebufferHelper.BindRenderbuffer(depth);
                        _framebufferHelper.RenderbufferStorageMultisample(
                            preferredMultiSampleCount, (int)depthInternalFormat, width, height);

                        if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                        {
                            stencil = depth;
                            if (stencilInternalFormat != 0)
                            {
                                stencil = _framebufferHelper.GenRenderbuffer();
                                _framebufferHelper.BindRenderbuffer(stencil);
                                _framebufferHelper.RenderbufferStorageMultisample(
                                    preferredMultiSampleCount, (int)stencilInternalFormat, width, height);
                            }
                        }
                    }
                }

                renderTarget.GLColorBuffer = color != 0 ? color : renderTarget.GLTexture;
                renderTarget.GLDepthBuffer = depth;
                renderTarget.GLStencilBuffer = stencil;
            }

            if (Threading.IsOnMainThread)
                Construct();
            else
                Threading.BlockOnMainThread(Construct);
        }

        internal void PlatformDeleteRenderTarget(IRenderTarget renderTarget)
        {
            void Delete()
            {
                int color = renderTarget.GLColorBuffer;
                if (color == 0)
                    return;

                int depth = renderTarget.GLDepthBuffer;
                int stencil = renderTarget.GLStencilBuffer;
                bool colorIsRenderbuffer = color != renderTarget.GLTexture;

                if (colorIsRenderbuffer)
                    _framebufferHelper.DeleteRenderbuffer(color);
                if (stencil != 0 && stencil != depth)
                    _framebufferHelper.DeleteRenderbuffer(stencil);
                if (depth != 0)
                    _framebufferHelper.DeleteRenderbuffer(depth);

                var bindingsToDelete = new List<RenderTargetBinding[]>();
                foreach (var bindings in _glFramebuffers.Keys)
                {
                    foreach (var binding in bindings)
                    {
                        if (binding.RenderTarget == renderTarget)
                        {
                            bindingsToDelete.Add(bindings);
                            break;
                        }
                    }
                }

                foreach (var bindings in bindingsToDelete)
                {
                    if (_glFramebuffers.TryGetValue(bindings, out int fbo))
                    {
                        _framebufferHelper.DeleteFramebuffer(fbo);
                        _glFramebuffers.Remove(bindings);
                    }

                    if (_glResolveFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        _framebufferHelper.DeleteFramebuffer(fbo);
                        _glResolveFramebuffers.Remove(bindings);
                    }
                }
            }

            if (Threading.IsOnMainThread)
                Delete();
            else
                Threading.BlockOnMainThread(Delete);
        }

        private void PlatformResolveRenderTargets()
        {
            if (RenderTargetCount == 0)
                return;

            var renderTargetBinding = _currentRenderTargetBindings[0];
            var renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
            if (renderTarget.MultiSampleCount > 0 && _framebufferHelper.SupportsBlitFramebuffer)
            {
                if (!_glResolveFramebuffers.TryGetValue(_currentRenderTargetBindings, out int glResolveFramebuffer))
                {
                    _framebufferHelper.GenFramebuffer(out glResolveFramebuffer);
                    _framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                    for (var i = 0; i < RenderTargetCount; ++i)
                    {
                        var rt = (IRenderTarget)_currentRenderTargetBindings[i].RenderTarget;
                        var texTarget = (int)rt.GetFramebufferTarget(renderTargetBinding);
                        _framebufferHelper.FramebufferTexture2D((int)(
                            FramebufferAttachment.ColorAttachment0 + i), texTarget, rt.GLTexture);
                    }
                    _glResolveFramebuffers.Add(
                        (RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glResolveFramebuffer);
                }
                else
                {
                    _framebufferHelper.BindFramebuffer(glResolveFramebuffer);
                }

                // The only fragment operations which affect the resolve are 
                // the pixel ownership test, the scissor test, and dithering.
                if (_lastRasterizerState.ScissorTestEnable)
                {
                    GL.Disable(EnableCap.ScissorTest);
                    GL.CheckError();
                }

                var glFramebuffer = _glFramebuffers[_currentRenderTargetBindings];
                _framebufferHelper.BindReadFramebuffer(glFramebuffer);
                for (var i = 0; i < RenderTargetCount; ++i)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                    _framebufferHelper.BlitFramebuffer(i, renderTarget.Width, renderTarget.Height);
                }

                if (renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents &&
                    _framebufferHelper.SupportsInvalidateFramebuffer)
                    _framebufferHelper.InvalidateReadFramebuffer();

                if (_lastRasterizerState.ScissorTestEnable)
                {
                    GL.Enable(EnableCap.ScissorTest);
                    GL.CheckError();
                }
            }
            for (var i = 0; i < RenderTargetCount; ++i)
            {
                renderTargetBinding = _currentRenderTargetBindings[i];
                renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                if (renderTarget.LevelCount > 1)
                {
                    GL.BindTexture(renderTarget.GLTarget, renderTarget.GLTexture);
                    GL.CheckError();
                    _framebufferHelper.GenerateMipmap((int)renderTarget.GLTarget);
                }
            }
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            if (!_glFramebuffers.TryGetValue(_currentRenderTargetBindings, out int glFramebuffer))
            {
                _framebufferHelper.GenFramebuffer(out glFramebuffer);
                _framebufferHelper.BindFramebuffer(glFramebuffer);

                var renderTargetBinding = _currentRenderTargetBindings[0];
                var renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                var depthBuffer = renderTarget.GLDepthBuffer;
                _framebufferHelper.FramebufferRenderbuffer((int)FramebufferAttachment.DepthAttachment, depthBuffer, 0);
                _framebufferHelper.FramebufferRenderbuffer((int)FramebufferAttachment.StencilAttachment, depthBuffer, 0);

                for (int i = 0; i < RenderTargetCount; ++i)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                    var attachement = (int)(FramebufferAttachment.ColorAttachment0 + i);

                    if (renderTarget.GLColorBuffer != renderTarget.GLTexture)
                        _framebufferHelper.FramebufferRenderbuffer(attachement, renderTarget.GLColorBuffer, 0);
                    else
                        _framebufferHelper.FramebufferTexture2D(
                            attachement, (int)renderTarget.GetFramebufferTarget(renderTargetBinding),
                            renderTarget.GLTexture, 0, renderTarget.MultiSampleCount);
                }

#if DEBUG
                _framebufferHelper.CheckFramebufferStatus();
#endif
                _glFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                _framebufferHelper.BindFramebuffer(glFramebuffer);
            }

            if (!GL.IsES)
                GL.DrawBuffers(RenderTargetCount, _drawBuffers);

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.MarkDirty();

            return (IRenderTarget)_currentRenderTargetBindings[0].RenderTarget;
        }

        private static GLPrimitiveType GetGLPrimitiveType(PrimitiveType primitiveType)
        {
            return primitiveType switch
            {
                PrimitiveType.LineList => GLPrimitiveType.Lines,
                PrimitiveType.LineStrip => GLPrimitiveType.LineStrip,
                PrimitiveType.TriangleList => GLPrimitiveType.Triangles,
                PrimitiveType.TriangleStrip => GLPrimitiveType.TriangleStrip,
                _ => throw new ArgumentOutOfRangeException(nameof(primitiveType)),
            };
        }

        /// <summary>
        /// Activates the current vertex/pixel shader pair into a program.         
        /// </summary>
        private unsafe void ActivateShaderProgram()
        {
            // Lookup the shader program.
            var shaderProgram = _programCache.GetProgram(VertexShader, PixelShader);
            if (shaderProgram.Program.IsNull)
                return;

            // Set the new program if it has changed.
            if (_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GL.CheckError();
                _shaderProgram = shaderProgram;
            }

            var posFixupLoc = shaderProgram.GetUniformLocation("posFixup");
            if (posFixupLoc == -1)
                return;

            // Apply vertex shader fix:
            // The following two lines are appended to the end of vertex shaders
            // to account for rendering differences between OpenGL and DirectX:
            //
            // gl_Position.y = gl_Position.y * posFixup.y;
            // gl_Position.xy += posFixup.zw * gl_Position.ww;
            //
            // (the following paraphrased from wine, wined3d/state.c and wined3d/glsl_shader.c)
            //
            // - We need to flip along the y-axis in case of offscreen rendering.
            // - D3D coordinates refer to pixel centers while GL coordinates refer
            //   to pixel corners.
            // - D3D has a top-left filling convention. We need to maintain this
            //   even after the y-flip mentioned above.
            // In order to handle the last two points, we translate by
            // (63.0 / 128.0) / VPw and (63.0 / 128.0) / VPh. This is equivalent to
            // translating slightly less than half a pixel. We want the difference to
            // be large enough that it doesn't get lost due to rounding inside the
            // driver, but small enough to prevent it from interfering with any
            // anti-aliasing.
            //
            // OpenGL coordinates specify the center of the pixel while d3d coords specify
            // the corner. The offsets are stored in z and w in posFixup. posFixup.y contains
            // 1.0 or -1.0 to turn the rendering upside down for offscreen rendering. PosFixup.x
            // contains 1.0 to allow a mad.

            Span<float> posFixup = stackalloc float[4] { 1, 1, 0, 0 };

            //If we have a render target bound (rendering offscreen)
            if (IsRenderTargetBound)
            {
                //flip vertically
                posFixup[1] *= -1f;
                posFixup[3] *= -1f;
            }

            GL.Uniform4(posFixupLoc, 1, posFixup);
            GL.CheckError();
        }

        internal void PlatformBeginApplyState()
        {
            Threading.AssertMainThread();
        }

        private void PlatformApplyBlend(bool force = false)
        {
            _actualBlendState.PlatformApplyState(this, force);
            ApplyBlendFactor(force);
        }

        private void ApplyBlendFactor(bool force)
        {
            if (force || BlendFactor != _lastBlendState.BlendFactor)
            {
                GL.BlendColor(
                    BlendFactor.R / 255f,
                    BlendFactor.G / 255f,
                    BlendFactor.B / 255f,
                    BlendFactor.A / 255f);

                GL.CheckError();
                _lastBlendState.BlendFactor = BlendFactor;
            }
        }

        internal void PlatformApplyState(bool applyShaders)
        {
            if (_scissorRectangleDirty)
            {
                var scissorRect = _scissorRectangle;
                if (!IsRenderTargetBound)
                    scissorRect.Y = PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);

                GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
                GL.CheckError();
                _scissorRectangleDirty = false;
            }

            // If we're not applying shaders then early out now.
            if (!applyShaders)
                return;


            if (_indexBufferDirty && _indexBuffer != null)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer._glBuffer);
                GL.CheckError();
            }
            _indexBufferDirty = false;

            if (_vertexShader == null)
                throw new InvalidOperationException("A vertex shader must be set.");
            if (_pixelShader == null)
                throw new InvalidOperationException("A pixel shader must be set.");

            if (VertexShaderDirty || PixelShaderDirty)
            {
                ActivateShaderProgram();

                unchecked
                {
                    if (VertexShaderDirty)
                        _graphicsMetrics._vertexShaderCount++;

                    if (PixelShaderDirty)
                        _graphicsMetrics._pixelShaderCount++;
                }

                VertexShaderDirty = false;
                PixelShaderDirty = false;
            }

            Debug.Assert(_shaderProgram != null);
            _vertexConstantBuffers.SetConstantBuffers(this, _shaderProgram);
            _pixelConstantBuffers.SetConstantBuffers(this, _shaderProgram);

            Textures.SetTextures(this);
            SamplerStates.PlatformSetSamplers(this);
        }

        private void PlatformDrawIndexedPrimitives(
            PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            ApplyState(true);
            ApplyAttribs(_vertexShader, baseVertex);

            int elementCount = GetElementCountForType(primitiveType, primitiveCount);
            var elementType = _indexBuffer!.ElementType.ToGLElementType();
            var indexOffsetInBytes = new IntPtr(startIndex * (int)elementType);

            GL.DrawElements(
                GetGLPrimitiveType(primitiveType), elementCount, elementType, indexOffsetInBytes);
            GL.CheckError();
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            ApplyState(true);
            ApplyAttribs(_vertexShader, 0);

            if (vertexStart < 0)
                vertexStart = 0;

            GL.DrawArrays(GetGLPrimitiveType(primitiveType), vertexStart, vertexCount);
            GL.CheckError();
        }

        private unsafe void PlatformDrawUserPrimitives<T>(
            PrimitiveType type, ReadOnlySpan<T> vertices, VertexDeclaration declaration)
            where T : unmanaged
        {
            ApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.CheckError();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.CheckError();
            _indexBufferDirty = true;

            fixed (T* vertexPtr = vertices)
            {
                // Setup the vertex declaration to point at the VB data.
                declaration.GraphicsDevice = this;
                declaration.Apply(_vertexShader, (IntPtr)vertexPtr, ShaderProgramHash);

                GL.DrawArrays(GetGLPrimitiveType(type), 0, vertices.Length);
                GL.CheckError();
            }
        }

        private unsafe void PlatformDrawUserIndexedPrimitives(
            PrimitiveType primitiveType,
            IndexElementType elementType,
            ReadOnlySpan<byte> vertices,
            ReadOnlySpan<byte> indices,
            int primitiveCount,
            VertexDeclaration declaration)
        {
            ApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.CheckError();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.CheckError();
            _indexBufferDirty = true;

            var pType = GetGLPrimitiveType(primitiveType);
            int elementCount = GetElementCountForType(primitiveType, primitiveCount);
            var glElementType = elementType.ToGLElementType();

            fixed (byte* vertexPtr = vertices)
            {
                // Setup the vertex declaration to point at the VB data.
                declaration.GraphicsDevice = this;
                declaration.Apply(_vertexShader, (IntPtr)vertexPtr, ShaderProgramHash);

                fixed (byte* indexPtr = indices)
                {
                    GL.DrawElements(pType, elementCount, glElementType, (IntPtr)indexPtr);
                    GL.CheckError();
                }
            }
        }

        private void PlatformDrawInstancedPrimitives(
            PrimitiveType primitiveType, int baseVertex,
            int startIndex, int primitiveCount,
            int baseInstance, int instanceCount)
        {
            AssertSupportsInstancing();
            ApplyState(true);

            var elementCount = GetElementCountForType(primitiveType, primitiveCount);
            var elementType = _indexBuffer!.ElementType.ToGLElementType();
            var indexOffsetInBytes = new IntPtr(startIndex * (int)elementType);

            ApplyAttribs(_vertexShader, baseVertex);

            if (baseInstance > 0)
            {
                if (!Capabilities.SupportsBaseIndexInstancing)
                    throw new PlatformNotSupportedException(
                        "Instanced geometry drawing with base instance requires at least OpenGL 4.2. " +
                        "Try upgrading your graphics card drivers.");

                GL.DrawElementsInstancedBaseInstance(
                    GetGLPrimitiveType(primitiveType),
                    elementCount,
                    elementType,
                    indexOffsetInBytes,
                    instanceCount,
                    baseInstance);
            }
            else
            {
                GL.DrawElementsInstanced(
                    GetGLPrimitiveType(primitiveType),
                    elementCount,
                    elementType,
                    indexOffsetInBytes,
                    instanceCount);
            }
            GL.CheckError();
        }

        private void PlatformGetBackBufferData<T>(Span<T> destination, Rectangle rect)
            where T : unmanaged
        {
            unsafe
            {
                fixed (T* ptr = destination)
                {
                    int flippedY = PresentationParameters.BackBufferHeight - rect.Bottom;
                    GL.ReadPixels(
                        rect.X, flippedY, rect.Width, rect.Height,
                        PixelFormat.Rgba, PixelType.UnsignedByte, (IntPtr)ptr);
                }
            }

            // ReadPixels returns data upside down, so we must swap rows around
            int rowBytes = rect.Width * PresentationParameters.BackBufferFormat.GetSize();
            int rowSize = rowBytes / Unsafe.SizeOf<T>();
            int count = destination.Length;

            Span<byte> buffer = stackalloc byte[Math.Min(4096, rowBytes)];
            var rowBuffer = MemoryMarshal.Cast<byte, T>(buffer);

            for (int dy = 0; dy < rect.Height / 2; dy++)
            {
                int left = Math.Min(count, rowSize);
                if (left == 0)
                    break;

                int offset = 0;
                var topRow = destination.Slice(dy * rowSize, rowSize);
                var bottomRow = destination.Slice((rect.Height - dy - 1) * rowSize, rowSize);

                while (left > 0)
                {
                    int toCopy = Math.Min(left, rowBuffer.Length);

                    var bottomRowSlice = bottomRow.Slice(offset, toCopy);
                    bottomRowSlice.CopyTo(rowBuffer);

                    var topRowSlice = topRow.Slice(offset, toCopy);
                    topRowSlice.CopyTo(bottomRowSlice);

                    rowBuffer.Slice(0, toCopy).CopyTo(topRowSlice);

                    count -= toCopy;
                    offset += toCopy;
                    left -= toCopy;
                }
            }
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }

        internal void PlatformSetMultiSamplingToMaximum(
            PresentationParameters presentationParameters, out int quality)
        {
            presentationParameters.MultiSampleCount = 4;
            quality = 0;
        }

        internal void OnPresentationChanged()
        {
#if DESKTOPGL || ANGLE
            Context.MakeCurrent(SDLGameWindow.Instance);
            Context.SwapInterval = PresentationParameters.PresentationInterval.GetSwapInterval();
#endif

            ApplyRenderTargets(null, null);
        }

        // Holds information for caching
        private struct BufferBindingInfo
        {
            public IntPtr VertexOffset;
            public int InstanceFrequency;
            public GLHandle Vbo;
            public VertexDeclaration.AttributeInfo AttributeInfo;
        }

        private void AssertSupportsInstancing()
        {
            if (!Capabilities.SupportsInstancing)
                throw new PlatformNotSupportedException(
                    "Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. " +
                    "Try upgrading your graphics card drivers.");
        }

        // FIXME: why is this even here
        //private void GetModeSwitchedSize(out int width, out int height)
        //{
        //    var mode = new SDL.Display.Mode
        //    {
        //        Width = PresentationParameters.BackBufferWidth,
        //        Height = PresentationParameters.BackBufferHeight,
        //        Format = 0,
        //        RefreshRate = 0,
        //        DriverData = IntPtr.Zero
        //    };
        //    SDL.Display.GetClosestDisplayMode(0, mode, out SDL.Display.Mode closest);
        //    width = closest.Width;
        //    height = closest.Height;
        //}
        //
        //private void GetDisplayResolution(out int width, out int height)
        //{
        //    SDL.Display.GetCurrentDisplayMode(0, out SDL.Display.Mode mode);
        //    width = mode.Width;
        //    height = mode.Height;
        //}
    }
}