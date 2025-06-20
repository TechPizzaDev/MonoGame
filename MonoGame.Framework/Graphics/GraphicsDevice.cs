// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Graphics
{
    public partial class GraphicsDevice : IDisposable
    {
        #region Fields

        private Viewport _viewport;
        private Color _blendFactor = Color.White;

        private int _maxVertexBufferSlots;
        internal int MaxTextureSlots;
        internal int MaxVertexTextureSlots;

        private BlendState _blendState;
        private BlendState _actualBlendState;
        private bool _blendStateDirty;

        private BlendState _blendStateAdditive;
        private BlendState _blendStateAlphaBlend;
        private BlendState _blendStateNonPremultiplied;
        private BlendState _blendStateOpaque;

        private DepthStencilState _depthStencilState;
        private DepthStencilState _actualDepthStencilState;
        private bool _depthStencilStateDirty;

        private DepthStencilState _depthStencilStateDefault;
        private DepthStencilState _depthStencilStateDepthRead;
        private DepthStencilState _depthStencilStateNone;

        private RasterizerState _rasterizerState;
        private RasterizerState _actualRasterizerState;
        private bool _rasterizerStateDirty;

        private RasterizerState _rasterizerStateCullClockwise;
        private RasterizerState _rasterizerStateCullCounterClockwise;
        private RasterizerState _rasterizerStateCullNone;

        private Rectangle _scissorRectangle;
        private bool _scissorRectangleDirty;

        internal VertexBufferBindings _vertexBuffers;
        private bool _vertexBuffersDirty;

        private IndexBuffer? _indexBuffer;
        private bool _indexBufferDirty;

        private readonly RenderTargetBinding[] _currentRenderTargetBindings = new RenderTargetBinding[4];
        private readonly RenderTargetBinding[] _tmpRenderTargetBinding = new RenderTargetBinding[1];

        /// <summary>
        /// On Intel Integrated graphics, there is a fast hardware unit for doing
        /// clears to colors where all components are either 0 or 255.
        /// Despite XNA4 using Purple here, we use black (in Release) to avoid
        /// performance warnings on Intel/Mesa.
        /// </summary>
#if DEBUG
        private static Vector4 DiscardColor { get; } = new Color(68, 34, 136, 255).ToScaledVector4();
#else
        private static Vector4 DiscardColor { get; } = new Color(0, 0, 0, 0).ToScaledVector4();
#endif

        private Shader? _vertexShader;
        private Shader? _pixelShader;

        private readonly ConstantBufferCollection _vertexConstantBuffers = new(ShaderStage.Vertex, 16);

        private readonly ConstantBufferCollection _pixelConstantBuffers = new(ShaderStage.Pixel, 16);

        /// <summary>
        /// The cache of effects from unique byte streams.
        /// </summary>
        internal Dictionary<int, Effect> EffectCache { get; } = new();

        /// <summary>
        /// Resources may be added to and removed from the list from many threads.
        /// </summary>
        private object ResourcesLock { get; } = new();

        /// <summary>
        /// Use <see cref="WeakReference"/> for the global resources list as we do not know when
        /// a resource may be disposed and collected. We do not want to prevent a resource from
        /// being collected by holding a strong reference to it in this list.
        /// </summary>
        private readonly List<WeakReference> _resources = new();

        internal GraphicsMetrics _graphicsMetrics;

        #endregion 

        #region Auto Properties

        public GraphicsCapabilities Capabilities { get; private set; }
        public TextureCollection VertexTextures { get; private set; }
        public SamplerStateCollection VertexSamplerStates { get; private set; }
        public TextureCollection Textures { get; private set; }
        public SamplerStateCollection SamplerStates { get; private set; }

        public bool IsDisposed { get; private set; }

        private bool VertexShaderDirty { get; set; }
        private bool PixelShaderDirty { get; set; }

        public GraphicsAdapter Adapter { get; private set; }

        /// <summary>
        /// Access debugging APIs for the graphics subsystem.
        /// </summary>
        public GraphicsDebug GraphicsDebug { get; set; }

        public int RenderTargetCount { get; private set; }
        public bool ResourcesLost { get; set; }
        public bool BlendFactorDirty { get; private set; }

        public PresentationParameters PresentationParameters { get; private set; }
        public GraphicsProfile GraphicsProfile { get; }

        #endregion

        #region Simple Properties

        /// <summary>
        /// Gets whether the resources created by this <see cref="GraphicsDevice"/> were lost.
        /// </summary>
        /// <remarks>
        /// Returns <see cref="IsDisposed"/> for now.
        /// </remarks>
        public bool IsContentLost => IsDisposed; // IsDisposed is the only case we currently now.

        internal bool IsRenderTargetBound => RenderTargetCount > 0;

        internal DepthFormat ActiveDepthFormat
        {
            get => IsRenderTargetBound
                ? _currentRenderTargetBindings[0].DepthFormat
                : PresentationParameters.DepthStencilFormat;
        }

        /// <summary>
        /// The rendering information for debugging and profiling.
        /// The metrics are reset every frame after draw within <see cref="Present"/>. 
        /// </summary>
        public GraphicsMetrics Metrics => _graphicsMetrics;

        public DisplayMode DisplayMode => Adapter.CurrentDisplayMode;

        public GraphicsDeviceStatus GraphicsDeviceStatus => GraphicsDeviceStatus.Normal;

        public IndexBuffer? Indices { set => SetIndexBuffer(value); get => _indexBuffer; }

        internal Shader? VertexShader
        {
            get => _vertexShader;
            set
            {
                if (_vertexShader != value)
                {
                    _vertexShader = value;
                    _vertexConstantBuffers.Clear();
                    VertexShaderDirty = true;
                }
            }
        }

        internal Shader? PixelShader
        {
            get => _pixelShader;
            set
            {
                if (_pixelShader != value)
                {
                    _pixelShader = value;
                    _pixelConstantBuffers.Clear();
                    PixelShaderDirty = true;
                }
            }
        }

        public Viewport Viewport
        {
            get => _viewport;
            set
            {
                _viewport = value;
                PlatformSetViewport(value);
            }
        }

        public Rectangle ScissorRectangle
        {
            get => _scissorRectangle;
            set
            {
                if (_scissorRectangle != value)
                {
                    _scissorRectangle = value;
                    _scissorRectangleDirty = true;
                }
            }
        }

        /// <summary>
        /// The color used as blend factor when alpha blending.
        /// </summary>
        /// <remarks>
        /// When only changing blend factor, use this rather than <see cref="BlendState.BlendFactor"/> to
        /// only update blend factor so the whole <see cref="BlendState"/> does not have to be updated.
        /// </remarks>
        public Color BlendFactor
        {
            get => _blendFactor;
            set
            {
                if (_blendFactor == value)
                    return;

                _blendFactor = value;
                BlendFactorDirty = true;
            }
        }

        #endregion

        #region Complex Properties

        public BlendState BlendState
        {
            get => _blendState;
            [MemberNotNull(nameof(_blendState))]
            [MemberNotNull(nameof(_actualBlendState))]
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                // Don't set the same state twice!
                if (_blendState == value)
                {
                    Debug.Assert(_actualBlendState != null);
                    return;

                _blendState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newBlendState = _blendState;
                if (ReferenceEquals(_blendState, BlendState.Additive))
                    newBlendState = _blendStateAdditive;
                else if (ReferenceEquals(_blendState, BlendState.AlphaBlend))
                    newBlendState = _blendStateAlphaBlend;
                else if (ReferenceEquals(_blendState, BlendState.NonPremultiplied))
                    newBlendState = _blendStateNonPremultiplied;
                else if (ReferenceEquals(_blendState, BlendState.Opaque))
                    newBlendState = _blendStateOpaque;

                // Blend state is now bound to a device... no one should
                // be changing the state of the blend state object now!
                newBlendState.BindToGraphicsDevice(this);

                _actualBlendState = newBlendState;

                BlendFactor = _actualBlendState.BlendFactor;

                _blendStateDirty = true;
            }
        }

        public DepthStencilState DepthStencilState
        {
            get => _depthStencilState;
            [MemberNotNull(nameof(_depthStencilState))]
            [MemberNotNull(nameof(_actualDepthStencilState))]
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                // Don't set the same state twice!
                if (_depthStencilState == value)
                {
                    Debug.Assert(_actualDepthStencilState != null);
                    return;
                }

                _depthStencilState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newDepthStencilState = _depthStencilState;
                if (ReferenceEquals(_depthStencilState, DepthStencilState.Default))
                    newDepthStencilState = _depthStencilStateDefault;
                else if (ReferenceEquals(_depthStencilState, DepthStencilState.DepthRead))
                    newDepthStencilState = _depthStencilStateDepthRead;
                else if (ReferenceEquals(_depthStencilState, DepthStencilState.None))
                    newDepthStencilState = _depthStencilStateNone;

                newDepthStencilState.BindToGraphicsDevice(this);

                _actualDepthStencilState = newDepthStencilState;

                _depthStencilStateDirty = true;
            }
        }

        public RasterizerState RasterizerState
        {
            get => _rasterizerState;
            [MemberNotNull(nameof(_rasterizerState))]
            [MemberNotNull(nameof(_actualRasterizerState))]
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                // Don't set the same state twice!
                if (_rasterizerState == value)
                {
                    Debug.Assert(_actualRasterizerState != null);
                    return;
                }

                if (!value.DepthClipEnable && !Capabilities.SupportsDepthClamp)
                    throw new InvalidOperationException(
                        "Cannot set RasterizerState.DepthClipEnable to false on this graphics device");

                _rasterizerState = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newRasterizerState = _rasterizerState;
                if (ReferenceEquals(_rasterizerState, RasterizerState.CullClockwise))
                    newRasterizerState = _rasterizerStateCullClockwise;
                else if (ReferenceEquals(_rasterizerState, RasterizerState.CullCounterClockwise))
                    newRasterizerState = _rasterizerStateCullCounterClockwise;
                else if (ReferenceEquals(_rasterizerState, RasterizerState.CullNone))
                    newRasterizerState = _rasterizerStateCullNone;

                newRasterizerState.BindToGraphicsDevice(this);

                _actualRasterizerState = newRasterizerState;

                _rasterizerStateDirty = true;
            }
        }

        #endregion

        #region Events

        // TODO: Graphics Device events need implementing
        public event Event<GraphicsDevice>? Disposing;
        public event Event<GraphicsDevice>? DeviceLost;
        public event Event<GraphicsDevice>? DeviceReset;
        public event Event<GraphicsDevice>? DeviceResetting;
        public event DataEvent<GraphicsDevice, object>? ResourceCreated;
        public event DataEvent<GraphicsDevice, ResourceDestroyedEventArgs>? ResourceDestroyed;
        public event DataEvent<GraphicsDevice, PresentationParameters>? PresentationChanged;

        #endregion

        #region Constructors

        internal GraphicsDevice() : this(
            GraphicsAdapter.DefaultAdapter,
            GraphicsProfile.Reach,
            new PresentationParameters { DepthStencilFormat = DepthFormat.Depth24 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="adapter">The graphics adapter.</param>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <param name="presentationParameters">The presentation options.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="presentationParameters"/> is <see langword="null"/>.
        /// </exception>
        public GraphicsDevice(
            GraphicsAdapter adapter, GraphicsProfile graphicsProfile, PresentationParameters presentationParameters)
        {
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

            if (!adapter.IsProfileSupported(graphicsProfile))
            {
                throw new NoSuitableGraphicsDeviceException(
                    $"Adapter '{adapter.Description}' does not support the {graphicsProfile} profile.");
            }

            PresentationParameters = presentationParameters ??
                throw new ArgumentNullException(nameof(presentationParameters));

            GraphicsProfile = graphicsProfile;

#if DEBUG
            if (DisplayMode == null)
            {
                throw new Exception(
                    "Unable to determine the current display mode. This can indicate that the " +
                    "game is not configured to be HiDPI aware under Windows 10 or later. " +
                    "See 'https://github.com/MonoGame/MonoGame/issues/5040' for more information.");
            }
#endif

            // Initialize the main viewport
            _viewport = new Viewport(0, 0, DisplayMode.Width, DisplayMode.Height, 0, 1);

            PlatformSetup();
            GraphicsDebug = new GraphicsDebug(this);

            VertexTextures = new TextureCollection(this, MaxVertexTextureSlots, true);
            VertexSamplerStates = new SamplerStateCollection(this, MaxVertexTextureSlots, true);

            Textures = new TextureCollection(this, MaxTextureSlots, false);
            SamplerStates = new SamplerStateCollection(this, MaxTextureSlots, false);

            _blendStateAdditive = BlendState.Additive.Clone();
            _blendStateAlphaBlend = BlendState.AlphaBlend.Clone();
            _blendStateNonPremultiplied = BlendState.NonPremultiplied.Clone();
            _blendStateOpaque = BlendState.Opaque.Clone();

            BlendState = BlendState.Opaque;

            _depthStencilStateDefault = DepthStencilState.Default.Clone();
            _depthStencilStateDepthRead = DepthStencilState.DepthRead.Clone();
            _depthStencilStateNone = DepthStencilState.None.Clone();

            DepthStencilState = DepthStencilState.Default;

            _rasterizerStateCullClockwise = RasterizerState.CullClockwise.Clone();
            _rasterizerStateCullCounterClockwise = RasterizerState.CullCounterClockwise.Clone();
            _rasterizerStateCullNone = RasterizerState.CullNone.Clone();

            RasterizerState = RasterizerState.CullCounterClockwise;

            Capabilities = new GraphicsCapabilities();
            Capabilities.Initialize(this);

            _vertexBuffers = new VertexBufferBindings(0);
            PlatformInitialize();

            // Force set the default render states.
            _blendStateDirty = _depthStencilStateDirty = _rasterizerStateDirty = true;
            BlendState = BlendState.Opaque;
            DepthStencilState = DepthStencilState.Default;
            RasterizerState = RasterizerState.CullCounterClockwise;

            // Clear the texture and sampler collections forcing
            // the state to be reapplied.
            VertexTextures.Clear();
            VertexSamplerStates.Clear();
            Textures.Clear();
            SamplerStates.Clear();

            // Clear constant buffers
            _vertexConstantBuffers.Clear();
            _pixelConstantBuffers.Clear();

            // Force set the buffers and shaders on next ApplyState() call
            _vertexBuffers = new VertexBufferBindings(_maxVertexBufferSlots);
            _vertexBuffersDirty = true;
            _indexBufferDirty = true;
            VertexShaderDirty = true;
            PixelShaderDirty = true;

            // Set the default scissor rect.
            _scissorRectangleDirty = true;
            ScissorRectangle = _viewport.Bounds;

            // Set the default render target.
            ApplyRenderTargets(null, null);
        }

        #endregion

        #region Clear

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            PlatformClear(options, color, depth, stencil);

            unchecked
            {
                _graphicsMetrics._clearCount++;
            }
        }

        public void Clear(Vector4 color)
        {
            Clear(ClearOptions.Full, color, _viewport.MaxDepth, 0);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear(options, color.ToScaledVector4(), depth, stencil);
        }

        public void Clear(Color color)
        {
            Clear(ClearOptions.Full, color, _viewport.MaxDepth, 0);
        }

        #endregion

        #region Uncategorized

        internal void ApplyState(bool applyShaders)
        {
            PlatformBeginApplyState();
            PlatformApplyBlend();

            if (_depthStencilStateDirty)
            {
                _actualDepthStencilState.PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }

            if (_rasterizerStateDirty)
            {
                _actualRasterizerState.PlatformApplyState(this);
                _rasterizerStateDirty = false;
            }

            PlatformApplyState(applyShaders);
        }

        internal void AddResourceReference(WeakReference resourceReference)
        {
            lock (ResourcesLock)
            {
                _resources.Add(resourceReference);
            }
        }

        internal void RemoveResourceReference(WeakReference resourceReference)
        {
            lock (ResourcesLock)
            {
                _resources.Remove(resourceReference);
            }
        }

        public void Present()
        {
            // We cannot present with a RT set on the device.
            if (RenderTargetCount != 0)
                throw new InvalidOperationException("Cannot call Present while a render target is active.");

            _graphicsMetrics = new GraphicsMetrics();
            PlatformPresent();
        }

        /// <summary>
        /// Sends queued commands in the command buffer to the GPU.
        /// </summary>
        public void Flush()
        {
            PlatformFlush();
        }

        #endregion

        #region Reset

        public void Reset()
        {
            PlatformReset();

            DeviceResetting?.Invoke(this);

            // Update the back buffer.
            OnPresentationChanged();

            PresentationChanged?.Invoke(this, PresentationParameters);
            DeviceReset?.Invoke(this);
        }

        public void Reset(PresentationParameters presentationParameters)
        {
            PresentationParameters = presentationParameters ??
                throw new ArgumentNullException(nameof(presentationParameters));

            Reset();
        }

        /// <summary>
        /// Trigger the DeviceResetting event
        /// Currently internal to allow the various platforms to send the event at the appropriate time.
        /// </summary>
        internal void OnDeviceResetting()
        {
            DeviceResetting?.Invoke(this);

            lock (ResourcesLock)
            {
                foreach (var resource in _resources)
                {
                    if (resource.Target is GraphicsResource target)
                        target.InvokeGraphicsDeviceResetting();
                }

                // Remove references to resources that have been garbage collected.
                _resources.RemoveAll(wr => !wr.IsAlive);
            }
        }

        /// <summary>
        /// Trigger the DeviceReset event to allow games to be notified of a device reset.
        /// Currently internal to allow the various platforms to send the event at the appropriate time.
        /// </summary>
        internal void OnDeviceReset()
        {
            DeviceReset?.Invoke(this);
        }

        #endregion

        #region RenderTargets

        public void SetRenderTarget(
            RenderTarget2D? renderTarget, int arraySlice, Vector4? clearColor = null)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null, clearColor);
            }
            else
            {
                _tmpRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, arraySlice);
                SetRenderTargets(_tmpRenderTargetBinding, clearColor);
            }
        }

        public void SetRenderTarget(RenderTarget2D? renderTarget, Vector4? clearColor = null)
        {
            SetRenderTarget(renderTarget, 0, clearColor);
        }

        public void SetRenderTarget(
            RenderTargetCube? renderTarget, CubeMapFace cubeMapFace, Vector4? clearColor = null)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null, clearColor);
            }
            else
            {
                _tmpRenderTargetBinding[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
                SetRenderTargets(_tmpRenderTargetBinding, clearColor);
            }
        }

        public void SetRenderTargets(
            ReadOnlySpan<RenderTargetBinding> renderTargets, Vector4? clearColor = null)
        {
            if (!Capabilities.SupportsTextureArrays)
            {
                for (int i = 0; i < renderTargets.Length; i++)
                {
                    if (renderTargets[i].ArraySlice != 0)
                        throw new InvalidOperationException("Texture arrays are not supported on this graphics device");
                }
            }

            // Try to early out if the current and new bindings are equal.
            if (RenderTargetCount == renderTargets.Length)
            {
                bool isEqual = true;

                for (int i = 0; i < RenderTargetCount; i++)
                {
                    if (_currentRenderTargetBindings[i].RenderTarget != renderTargets[i].RenderTarget ||
                        _currentRenderTargetBindings[i].ArraySlice != renderTargets[i].ArraySlice)
                    {
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                    return;
            }

            ApplyRenderTargets(renderTargets, clearColor);

            unchecked
            {
                if (renderTargets.Length == 0)
                    _graphicsMetrics._targetCount++;
                else
                    _graphicsMetrics._targetCount += renderTargets.Length;
            }
        }

        public void SetRenderTargets(Vector4? clearColor, params RenderTargetBinding[]? renderTargets)
        {
            SetRenderTargets(renderTargets.AsSpan(), clearColor);
        }

        public void SetRenderTargets(params RenderTargetBinding[]? renderTargets)
        {
            SetRenderTargets(renderTargets.AsSpan(), null);
        }

        internal void ApplyRenderTargets(
            ReadOnlySpan<RenderTargetBinding> renderTargets, Vector4? clearColor)
        {
            PlatformResolveRenderTargets();

            // Clear the current bindings.
            Array.Clear(_currentRenderTargetBindings, 0, _currentRenderTargetBindings.Length);

            bool clearTarget;
            int renderTargetWidth;
            int renderTargetHeight;

            if (renderTargets.IsEmpty)
            {
                RenderTargetCount = 0;

                PlatformApplyDefaultRenderTarget();
                clearTarget = PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = PresentationParameters.BackBufferWidth;
                renderTargetHeight = PresentationParameters.BackBufferHeight;
            }
            else
            {
                // Copy the new bindings.
                renderTargets.CopyTo(_currentRenderTargetBindings);
                RenderTargetCount = renderTargets.Length;

                var renderTarget = PlatformApplyRenderTargets();

                // We clear the render target if asked.
                clearTarget = renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = renderTarget.Width;
                renderTargetHeight = renderTarget.Height;
            }

            // Set the viewport to the size of the first render target.
            Viewport = new Viewport(0, 0, renderTargetWidth, renderTargetHeight);

            // Set the scissor rectangle to the size of the first render target.
            ScissorRectangle = new Rectangle(0, 0, renderTargetWidth, renderTargetHeight);

            // In XNA 4, because of hardware limitations on Xbox, when
            // a render target doesn't have PreserveContents as its usage
            // it is cleared before being rendered to.
            if (clearTarget || clearColor.HasValue)
                Clear(clearColor ?? DiscardColor);
        }

        public ReadOnlySpan<RenderTargetBinding> GetRenderTargets()
        {
            return _currentRenderTargetBindings.AsSpan(0, RenderTargetCount);
        }

        public void GetRenderTargets(Span<RenderTargetBinding> destination)
        {
            GetRenderTargets().CopyTo(destination);
        }

        #endregion

        #region Buffers

        public void SetVertexBuffer(VertexBuffer? vertexBuffer)
        {
            _vertexBuffersDirty |= vertexBuffer == null
                ? _vertexBuffers.Clear()
                : _vertexBuffers.Set(vertexBuffer, 0);
        }

        public void SetVertexBuffer(VertexBuffer? vertexBuffer, int vertexOffset)
        {
            // Validate vertexOffset.
            if (vertexOffset < 0 ||
                vertexBuffer == null && vertexOffset != 0 ||
                vertexBuffer != null && vertexOffset >= vertexBuffer.Capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(vertexOffset));
            }

            _vertexBuffersDirty |= vertexBuffer == null
                ? _vertexBuffers.Clear()
                : _vertexBuffers.Set(vertexBuffer, vertexOffset);
        }

        public void SetVertexBuffers(ReadOnlySpan<VertexBufferBinding> vertexBuffers)
        {
            if (vertexBuffers.IsEmpty)
            {
                _vertexBuffersDirty |= _vertexBuffers.Clear();
            }
            else
            {
                if (vertexBuffers.Length > _maxVertexBufferSlots)
                    throw new ArgumentOutOfRangeException(
                        nameof(vertexBuffers), $"Max number of vertex buffers is {_maxVertexBufferSlots}.");

                _vertexBuffersDirty |= _vertexBuffers.Set(vertexBuffers);
            }
        }

        public void SetVertexBuffers(params VertexBufferBinding[] vertexBuffers)
        {
            SetVertexBuffers(vertexBuffers.AsSpan());
        }

        private void SetIndexBuffer(IndexBuffer? indexBuffer)
        {
            if (_indexBuffer == indexBuffer)
                return;

            _indexBuffer = indexBuffer;
            _indexBufferDirty = true;
        }

        internal void SetConstantBuffer(ShaderStage stage, int slot, ConstantBuffer buffer)
        {
            if (stage == ShaderStage.Vertex)
                _vertexConstantBuffers[slot] = buffer;
            else
                _pixelConstantBuffers[slot] = buffer;
        }

        #endregion

        #region Drawing

        #region DrawIndexedPrimitives

        /// <summary>
        /// Draw geometry by indexing into the vertex buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
        public void DrawIndexedPrimitives(
            PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            if (_vertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling this.");

            if (_vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling this.");

            if (_indexBuffer == null)
                throw new InvalidOperationException("Index buffer must be set before calling this.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(primitiveCount));

            PlatformDrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += primitiveCount;
            }
        }

        #endregion

        #region DrawUserPrimitives

        /// <summary>
        /// Draw primitives of the specified type from the data in the given span of vertices without indexing.
        /// </summary>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">The span of vertices to draw.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        public void DrawUserPrimitives(
            PrimitiveType primitiveType,
            ReadOnlySpan<byte> vertexData,
            int primitiveCount,
            VertexDeclaration vertexDeclaration)
        {
            if (vertexData.IsEmpty)
                throw new ArgumentEmptyException(nameof(vertexData));

            if (vertexDeclaration is null)
                throw new ArgumentNullException(nameof(vertexDeclaration));

            int vertexCount = GetElementCountForType(primitiveType, primitiveCount);
            if (vertexCount > vertexData.Length)
                throw new ArgumentOutOfRangeException(nameof(vertexData));

            PlatformDrawUserPrimitives(primitiveType, vertexData, vertexDeclaration);

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += vertexCount;
            }
        }

        /// <summary>
        /// Draw primitives of the specified type from the data in a span of vertices without indexing.
        /// </summary>
        /// <typeparam name="T">The type of the vertex data.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">The span of vertices to draw.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <remarks>
        /// The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="T"/> and cached for subsequent calls.
        /// </remarks>
        public void DrawUserPrimitives<T>(
            PrimitiveType primitiveType, ReadOnlySpan<T> vertexData, int primitiveCount)
            where T : unmanaged, IVertexType
        {
            var declaration = VertexDeclarationCache<T>.VertexDeclaration;
            DrawUserPrimitives(primitiveType, MemoryMarshal.AsBytes(vertexData), primitiveCount, declaration);
        }

        #endregion

        #region DrawPrimitives

        /// <summary>
        /// Draw primitives of the specified type from the currently bound vertexbuffers without indexing.
        /// </summary>
        /// <param name="primitiveType">The type of primitives to draw.</param>
        /// <param name="vertexStart">Index of the vertex to start at.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            if (_vertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawPrimitives.");

            if (_vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawPrimitives.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(primitiveCount));

            int vertexCount = GetElementCountForType(primitiveType, primitiveCount);
            PlatformDrawPrimitives(primitiveType, vertexStart, vertexCount);

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += primitiveCount;
            }
        }

        #endregion

        #region DrawUserIndexedPrimitives

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given span of vertices with 
        /// 16- or 32-bit indices.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertex data.</typeparam>
        /// <typeparam name="TIndex">The type of the index data.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="elementType">The size of index elements.</param>
        /// <param name="vertexData">The span of vertices to draw.</param>
        /// <param name="indexData">The span of indices for indexing the vertices.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        /// <remarks>
        /// All indices in the span are relative to the first vertex.
        /// An index of zero in the index span points to the first vertex in the vertex span.
        /// </remarks>
        public void DrawUserIndexedPrimitives<TVertex, TIndex>(
            PrimitiveType primitiveType,
            IndexElementType elementType,
            ReadOnlySpan<TVertex> vertexData,
            ReadOnlySpan<TIndex> indexData,
            int primitiveCount,
            VertexDeclaration vertexDeclaration)
            where TVertex : unmanaged
            where TIndex : unmanaged
        {
            if (vertexDeclaration is null)
                throw new ArgumentNullException(nameof(vertexDeclaration));

            if (vertexData.IsEmpty)
                throw new ArgumentEmptyException(nameof(vertexData));
            if (indexData.IsEmpty)
                throw new ArgumentEmptyException(nameof(indexData));

            if (primitiveCount <= 0 || GetElementCountForType(primitiveType, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException(nameof(primitiveCount));

            PlatformDrawUserIndexedPrimitives(
                primitiveType,
                elementType,
                MemoryMarshal.AsBytes(vertexData),
                MemoryMarshal.AsBytes(indexData),
                primitiveCount,
                vertexDeclaration);

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += primitiveCount;
            }
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given span of vertices with 
        /// 16- or 32-bit indices.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertex data.</typeparam>
        /// <typeparam name="TIndex">The type of the index data.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="elementType">The size of index elements.</param>
        /// <param name="vertexData">The span of vertices to draw.</param>
        /// <param name="indexData">The span of indices for indexing the vertices.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <remarks>
        /// The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="TVertex"/> and cached for subsequent calls.
        /// </remarks>
        /// <remarks>
        /// All indices in the span are relative to the first vertex.
        /// An index of zero in the index span points to the first vertex in the vertex span.
        /// </remarks>
        public void DrawUserIndexedPrimitives<TVertex, TIndex>(
            PrimitiveType primitiveType,
            IndexElementType elementType,
            ReadOnlySpan<TVertex> vertexData,
            ReadOnlySpan<TIndex> indexData,
            int primitiveCount)
            where TVertex : unmanaged, IVertexType
            where TIndex : unmanaged
        {
            var declaration = VertexDeclarationCache<TVertex>.VertexDeclaration;

            DrawUserIndexedPrimitives(
                primitiveType,
                elementType,
                MemoryMarshal.AsBytes(vertexData),
                MemoryMarshal.AsBytes(indexData),
                primitiveCount,
                declaration);
        }


        /// <summary>
        /// Draw primitives of the specified type by indexing into the given span of vertices with 
        /// 16- or 32-bit indices.
        /// </summary>
        /// <typeparam name="TVertex">The type of the vertex data.</typeparam>
        /// <typeparam name="TIndex">The type of the index data.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">The span of vertices to draw.</param>
        /// <param name="indexData">The span of indices for indexing the vertices.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <remarks>
        /// The <see cref="VertexDeclaration"/> will be found by getting <see cref="IVertexType.VertexDeclaration"/>
        /// from an instance of <typeparamref name="TVertex"/> and cached for subsequent calls.
        /// </remarks>
        /// <remarks>
        /// All indices in the span are relative to the first vertex.
        /// An index of zero in the index span points to the first vertex in the vertex span.
        /// </remarks>
        public void DrawUserIndexedPrimitives<TVertex, TIndex>(
            PrimitiveType primitiveType,
            ReadOnlySpan<TVertex> vertexData,
            ReadOnlySpan<TIndex> indexData,
            int primitiveCount)
            where TVertex : unmanaged, IVertexType
            where TIndex : unmanaged
        {
            var elementType = IndexBuffer.ToIndexElementType(typeof(TIndex));

            DrawUserIndexedPrimitives(
                primitiveType,
                elementType,
                vertexData,
                indexData,
                primitiveCount);
        }

        #endregion

        #region DrawInstancedPrimitives 

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="baseInstance">Used to offset the instance range indexed from the instance buffer.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
        public void DrawInstancedPrimitives(
            PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            if (_vertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawInstancedPrimitives.");

            if (_vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawInstancedPrimitives.");

            if (_indexBuffer == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawInstancedPrimitives.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(primitiveCount));

            PlatformDrawInstancedPrimitives(
                primitiveType, baseVertex, startIndex, primitiveCount, baseInstance, instanceCount);

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += primitiveCount * instanceCount;
            }
        }

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
        public void DrawInstancedPrimitives(
            PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount)
        {
            DrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, 0, instanceCount);
        }

        #endregion

        #endregion

        #region GetBackBufferData

        /// <summary>
        /// Gets the pixels that are currently drawn on screen.
        /// The format is the backbuffer's current format.
        /// </summary>
        public unsafe void GetBackBufferData<T>(Span<T> destination, Rectangle? rectangle = null)
            where T : unmanaged
        {
            Rectangle rect;
            if (rectangle.HasValue)
            {
                rect = rectangle.Value;
                if (rect.X < 0 || rect.Y < 0 || rect.Width <= 0 || rect.Height <= 0 ||
                    rect.Right > PresentationParameters.BackBufferWidth ||
                    rect.Top > PresentationParameters.BackBufferHeight)
                    throw new ArgumentException(
                        "The rectangle must fit in the backbuffer dimensions.", nameof(rectangle));
            }
            else
            {
                rect = new Rectangle(
                    0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
            }

            int formatSize = PresentationParameters.BackBufferFormat.GetSize();
            if (sizeof(T) > formatSize || formatSize % sizeof(T) != 0)
                throw new InvalidOperationException(
                    $"{nameof(T)} is of an invalid size for the format of the backbuffer.");

            int byteCount = destination.Length * sizeof(T);
            if (byteCount > rect.Width * rect.Height * formatSize)
                throw new ArgumentOutOfRangeException(
                     nameof(destination), "The amount of data requested exceeds the backbuffer size.");

            PlatformGetBackBufferData(MemoryMarshal.AsBytes(destination), rect);
        }

        #endregion

        #region Helpers

        private static int GetElementCountForType(PrimitiveType primitiveType, int primitiveCount)
        {
            return primitiveType switch
            {
                PrimitiveType.LineList => primitiveCount * 2,
                PrimitiveType.LineStrip => primitiveCount + 1,
                PrimitiveType.TriangleList => primitiveCount * 3,
                PrimitiveType.TriangleStrip => primitiveCount + 2,
                _ => throw new NotSupportedException(),
            };
        }

        internal int GetClampedMultisampleCount(int multiSampleCount)
        {
            if (multiSampleCount > 1)
            {
                // Round down MultiSampleCount to the nearest power of two
                // hack from http://stackoverflow.com/a/2681094

                // Note: this will return an incorrect, but large value
                // for very large numbers. That doesn't matter because
                // the number will get clamped below anyway in this case.
                int msc = multiSampleCount;
                msc |= msc >> 1;
                msc |= msc >> 2;
                msc |= msc >> 4;
                msc -= msc >> 1;

                // and clamp it to what the device can handle
                if (msc > Capabilities.MaxMultiSampleCount)
                    msc = Capabilities.MaxMultiSampleCount;

                return msc;
            }
            return 0;
        }

        // uniformly scales down the given rectangle by 10%
        internal static Rectangle GetDefaultTitleSafeArea(int x, int y, int width, int height)
        {
            var marginX = (width + 19) / 20;
            var marginY = (height + 19) / 20;
            x += marginX;
            y += marginY;

            width -= marginX * 2;
            height -= marginY * 2;
            return new Rectangle(x, y, width, height);
        }

        internal static Rectangle GetTitleSafeArea(int x, int y, int width, int height)
        {
            return PlatformGetTitleSafeArea(x, y, width, height);
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose of remaining graphics resources before disposing of the graphics device
                    lock (ResourcesLock)
                    {
                        // Reverse loop as resources remove themselves from the list upon disposal.
                        for (int i = _resources.Count; i-- > 0;)
                        {
                            if (_resources[i].Target is IDisposable target)
                                target.Dispose();
                        }
                        _resources.Clear();
                    }

                    EffectCache.Clear();

                    _blendStateAdditive.Dispose();
                    _blendStateAlphaBlend.Dispose();
                    _blendStateNonPremultiplied.Dispose();
                    _blendStateOpaque.Dispose();

                    _depthStencilStateDefault.Dispose();
                    _depthStencilStateDepthRead.Dispose();
                    _depthStencilStateNone.Dispose();

                    _rasterizerStateCullClockwise.Dispose();
                    _rasterizerStateCullCounterClockwise.Dispose();
                    _rasterizerStateCullNone.Dispose();

                    PlatformDispose();
                }

                IsDisposed = true;
                Disposing?.Invoke(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the <see cref="GraphicsDevice"/>.
        /// </summary>
        ~GraphicsDevice()
        {
            Dispose(false);
        }

        #endregion
    }
}