// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Graphics
{
    public sealed partial class BlendState : GraphicsResource
    {
        public static BlendState Additive { get; } =
            new("BlendState.Additive", Blend.SourceAlpha, Blend.One);

        public static BlendState AlphaBlend { get; } =
            new("BlendState.AlphaBlend", Blend.One, Blend.InverseSourceAlpha);

        public static BlendState NonPremultiplied { get; } =
            new("BlendState.NonPremultiplied", Blend.SourceAlpha, Blend.InverseSourceAlpha, Blend.One, Blend.InverseSourceAlpha);

        public static BlendState Opaque { get; } =
            new("BlendState.Opaque", Blend.One, Blend.Zero);

        private readonly TargetBlendState[] _targetBlendState;
        private readonly bool _defaultStateObject;

        private Color _blendFactor;
        private int _multiSampleMask;
        private bool _independentBlendEnable;

        protected override bool IsDefaultStateObject => _defaultStateObject;

        /// <summary>
        /// Returns the target specific blend state.
        /// </summary>
        /// <param name="index">The 0 to 3 target blend state index.</param>
        /// <returns>A target blend state.</returns>
        public TargetBlendState this[int index] => _targetBlendState[index];

        #region Constructors

        public BlendState()
        {
            _targetBlendState = new TargetBlendState[4];
            for (int i = 0; i < _targetBlendState.Length; i++)
                _targetBlendState[i] = new TargetBlendState(this);

            _blendFactor = Color.White;
            _multiSampleMask = int.MaxValue;
            _independentBlendEnable = false;
        }

        private BlendState(string name, Blend sourceBlend, Blend destinationBlend)
            : this(name, sourceBlend, destinationBlend, sourceBlend, destinationBlend)
        {
        }

        private BlendState(string name, Blend sourceColorBlend, Blend destinationColorBlend, Blend sourceAlphaBlend, Blend destinationAlphaBlend)
            : this()
        {
            Name = name;
            ColorSourceBlend = sourceColorBlend;
            AlphaSourceBlend = sourceAlphaBlend;
            ColorDestinationBlend = destinationColorBlend;
            AlphaDestinationBlend = destinationAlphaBlend;
            _defaultStateObject = true;
        }

        private BlendState(BlendState cloneSource)
        {
            Name = cloneSource.Name;

            _targetBlendState = new TargetBlendState[cloneSource._targetBlendState.Length];
            for (int i = 0; i < _targetBlendState.Length; i++)
                _targetBlendState[i] = cloneSource[i].Clone(this);

            _blendFactor = cloneSource._blendFactor;
            _multiSampleMask = cloneSource._multiSampleMask;
            _independentBlendEnable = cloneSource._independentBlendEnable;
        }

        #endregion

        #region Properties

        public BlendFunction AlphaBlendFunction
        {
            get => _targetBlendState[0].AlphaBlendFunction;
            set
            {
                ThrowIfBound();
                _targetBlendState[0].AlphaBlendFunction = value;
            }
        }

        public Blend AlphaDestinationBlend
        {
            get => _targetBlendState[0].AlphaDestinationBlend;
            set
            {
                ThrowIfBound();
                _targetBlendState[0].AlphaDestinationBlend = value;
            }
        }

        public Blend AlphaSourceBlend
        {
            get => _targetBlendState[0].AlphaSourceBlend;
            set
            {
                ThrowIfBound();
                _targetBlendState[0].AlphaSourceBlend = value;
            }
        }

        public BlendFunction ColorBlendFunction
        {
            get => _targetBlendState[0].ColorBlendFunction;
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorBlendFunction = value;
            }
        }

        public Blend ColorDestinationBlend
        {
            get => _targetBlendState[0].ColorDestinationBlend;
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorDestinationBlend = value;
            }
        }

        public Blend ColorSourceBlend
        {
            get => _targetBlendState[0].ColorSourceBlend;
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorSourceBlend = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels
        {
            get => _targetBlendState[0].ColorWriteChannels;
            set
            {
                ThrowIfBound();
                _targetBlendState[0].ColorWriteChannels = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels1
        {
            get => _targetBlendState[1].ColorWriteChannels;
            set
            {
                ThrowIfBound();
                _targetBlendState[1].ColorWriteChannels = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels2
        {
            get => _targetBlendState[2].ColorWriteChannels;
            set
            {
                ThrowIfBound();
                _targetBlendState[2].ColorWriteChannels = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels3
        {
            get => _targetBlendState[3].ColorWriteChannels;
            set
            {
                ThrowIfBound();
                _targetBlendState[3].ColorWriteChannels = value;
            }
        }

        /// <summary>
        /// The color used as blend factor when alpha blending.
        /// </summary>
        /// <remarks>
        /// <see cref="GraphicsDevice.BlendFactor"/> is set to this value when 
        /// this <see cref="BlendState"/> is bound to a GraphicsDevice.
        /// </remarks>
        public Color BlendFactor
        {
            get => _blendFactor;
            set
            {
                ThrowIfBound();
                _blendFactor = value;
            }
        }

        public int MultiSampleMask
        {
            get => _multiSampleMask;
            set
            {
                ThrowIfBound();
                _multiSampleMask = value;
            }
        }

        /// <summary>
        /// Enables use of the per-target blend states.
        /// </summary>
        public bool IndependentBlendEnable
        {
            get => _independentBlendEnable;
            set
            {
                ThrowIfBound();
                _independentBlendEnable = value;
            }
        }

        #endregion

        internal BlendState Clone()
        {
            return new BlendState(this);
        }

        partial void PlatformDispose();

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                PlatformDispose();
            }
            base.Dispose(disposing);
        }
    }
}

