// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Graphics
{
    public sealed partial class SamplerState : GraphicsResource
    {
        public static SamplerState AnisotropicClamp { get; } =
            new("SamplerState.AnisotropicClamp", TextureFilter.Anisotropic, TextureAddressMode.Clamp);

        public static SamplerState AnisotropicWrap { get; } =
            new("SamplerState.AnisotropicWrap", TextureFilter.Anisotropic, TextureAddressMode.Wrap);

        public static SamplerState LinearClamp { get; } =
            new("SamplerState.LinearClamp", TextureFilter.Linear, TextureAddressMode.Clamp);

        public static SamplerState LinearWrap { get; } =
            new("SamplerState.LinearWrap", TextureFilter.Linear, TextureAddressMode.Wrap);

        public static SamplerState PointClamp { get; } =
            new("SamplerState.PointClamp", TextureFilter.Point, TextureAddressMode.Clamp);

        public static SamplerState PointWrap { get; } =
            new("SamplerState.PointWrap", TextureFilter.Point, TextureAddressMode.Wrap);

        private readonly bool _defaultStateObject;

        private TextureAddressMode _addressU;
        private TextureAddressMode _addressV;
        private TextureAddressMode _addressW;
        private Color _borderColor;
        private TextureFilter _filter;
        private int _maxAnisotropy;
        private int _maxMipLevel;
        private float _mipMapLevelOfDetailBias;
        private TextureFilterMode _filterMode;
        private CompareFunction _comparisonFunction;

        #region Properties

        protected override bool IsDefaultStateObject => _defaultStateObject;

        public TextureAddressMode AddressU
        {
            get => _addressU;
            set
            {
                ThrowIfBound();
                _addressU = value;
            }
        }

        public TextureAddressMode AddressV
        {
            get => _addressV;
            set
            {
                ThrowIfBound();
                _addressV = value;
            }
        }

        public TextureAddressMode AddressW
        {
            get => _addressW;
            set
            {
                ThrowIfBound();
                _addressW = value;
            }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                ThrowIfBound();
                _borderColor = value;
            }
        }

        public TextureFilter Filter
        {
            get => _filter;
            set
            {
                ThrowIfBound();
                _filter = value;
            }
        }

        public int MaxAnisotropy
        {
            get => _maxAnisotropy;
            set
            {
                ThrowIfBound();
                _maxAnisotropy = value;
            }
        }

        public int MaxMipLevel
        {
            get => _maxMipLevel;
            set
            {
                ThrowIfBound();
                _maxMipLevel = value;
            }
        }

        public float MipMapLevelOfDetailBias
        {
            get => _mipMapLevelOfDetailBias;
            set
            {
                ThrowIfBound();
                _mipMapLevelOfDetailBias = value;
            }
        }

        /// <summary>
        /// When using comparison sampling, also set <see cref="FilterMode"/> to <see cref="TextureFilterMode.Comparison"/>.
        /// </summary>
        public CompareFunction ComparisonFunction
        {
            get => _comparisonFunction;
            set
            {
                ThrowIfBound();
                _comparisonFunction = value;
            }
        }

        public TextureFilterMode FilterMode
        {
            get => _filterMode;
            set
            {
                ThrowIfBound();
                _filterMode = value;
            }
        }

        #endregion

        #region Constructors

        public SamplerState()
        {
            Filter = TextureFilter.Linear;
            AddressU = TextureAddressMode.Wrap;
            AddressV = TextureAddressMode.Wrap;
            AddressW = TextureAddressMode.Wrap;
            BorderColor = Color.White;
            MaxAnisotropy = 4;
            MaxMipLevel = 0;
            MipMapLevelOfDetailBias = 0f;
            ComparisonFunction = CompareFunction.Never;
            FilterMode = TextureFilterMode.Default;
        }

        private SamplerState(string name, TextureFilter filter, TextureAddressMode addressMode) : this()
        {
            Name = name;
            _filter = filter;
            _addressU = addressMode;
            _addressV = addressMode;
            _addressW = addressMode;
            _defaultStateObject = true;
        }

        private SamplerState(SamplerState cloneSource)
        {
            Name = cloneSource.Name;
            _filter = cloneSource._filter;
            _addressU = cloneSource._addressU;
            _addressV = cloneSource._addressV;
            _addressW = cloneSource._addressW;
            _borderColor = cloneSource._borderColor;
            _maxAnisotropy = cloneSource._maxAnisotropy;
            _maxMipLevel = cloneSource._maxMipLevel;
            _mipMapLevelOfDetailBias = cloneSource._mipMapLevelOfDetailBias;
            _comparisonFunction = cloneSource._comparisonFunction;
            _filterMode = cloneSource._filterMode;
        }

        #endregion

        internal SamplerState Clone()
        {
            return new SamplerState(this);
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