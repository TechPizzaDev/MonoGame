// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Graphics
{
    public sealed partial class DepthStencilState : GraphicsResource
    {
        public static DepthStencilState Default { get; } = 
            new("DepthStencilState.Default", true, true);

        public static DepthStencilState DepthRead { get; } = 
            new("DepthStencilState.DepthRead", true, false);

        public static DepthStencilState None { get; } = 
            new("DepthStencilState.None", false, false);

        private readonly bool _defaultStateObject;

        private bool _depthBufferEnable;
        private bool _depthBufferWriteEnable;
        private StencilOperation _counterClockwiseStencilDepthBufferFail;
        private StencilOperation _counterClockwiseStencilFail;
        private CompareFunction _counterClockwiseStencilFunction;
        private StencilOperation _counterClockwiseStencilPass;
        private CompareFunction _depthBufferFunction;
        private int _referenceStencil;
        private StencilOperation _stencilDepthBufferFail;
        private bool _stencilEnable;
        private StencilOperation _stencilFail;
        private CompareFunction _stencilFunction;
        private int _stencilMask;
        private StencilOperation _stencilPass;
        private int _stencilWriteMask;
        private bool _twoSidedStencilMode;

        #region Properties

        protected override bool IsDefaultStateObject => _defaultStateObject;

        public bool DepthBufferEnable
        {
            get => _depthBufferEnable;
            set
            {
                ThrowIfBound();
                _depthBufferEnable = value;
            }
        }

        public bool DepthBufferWriteEnable
        {
            get => _depthBufferWriteEnable;
            set
            {
                ThrowIfBound();
                _depthBufferWriteEnable = value;
            }
        }

        public StencilOperation CounterClockwiseStencilDepthBufferFail
        {
            get => _counterClockwiseStencilDepthBufferFail;
            set
            {
                ThrowIfBound();
                _counterClockwiseStencilDepthBufferFail = value;
            }
        }

        public StencilOperation CounterClockwiseStencilFail
        {
            get => _counterClockwiseStencilFail;
            set
            {
                ThrowIfBound();
                _counterClockwiseStencilFail = value;
            }
        }

        public CompareFunction CounterClockwiseStencilFunction
        {
            get => _counterClockwiseStencilFunction;
            set
            {
                ThrowIfBound();
                _counterClockwiseStencilFunction = value;
            }
        }

        public StencilOperation CounterClockwiseStencilPass
        {
            get => _counterClockwiseStencilPass;
            set
            {
                ThrowIfBound();
                _counterClockwiseStencilPass = value;
            }
        }

        public CompareFunction DepthBufferFunction
        {
            get => _depthBufferFunction;
            set
            {
                ThrowIfBound();
                _depthBufferFunction = value;
            }
        }

        public int ReferenceStencil
        {
            get => _referenceStencil;
            set
            {
                ThrowIfBound();
                _referenceStencil = value;
            }
        }

        public StencilOperation StencilDepthBufferFail
        {
            get => _stencilDepthBufferFail;
            set
            {
                ThrowIfBound();
                _stencilDepthBufferFail = value;
            }
        }

        public bool StencilEnable
        {
            get => _stencilEnable;
            set
            {
                ThrowIfBound();
                _stencilEnable = value;
            }
        }

        public StencilOperation StencilFail
        {
            get => _stencilFail;
            set
            {
                ThrowIfBound();
                _stencilFail = value;
            }
        }

        public CompareFunction StencilFunction
        {
            get => _stencilFunction;
            set
            {
                ThrowIfBound();
                _stencilFunction = value;
            }
        }

        public int StencilMask
        {
            get => _stencilMask;
            set
            {
                ThrowIfBound();
                _stencilMask = value;
            }
        }

        public StencilOperation StencilPass
        {
            get => _stencilPass;
            set
            {
                ThrowIfBound();
                _stencilPass = value;
            }
        }

        public int StencilWriteMask
        {
            get => _stencilWriteMask;
            set
            {
                ThrowIfBound();
                _stencilWriteMask = value;
            }
        }

        public bool TwoSidedStencilMode
        {
            get => _twoSidedStencilMode;
            set
            {
                ThrowIfBound();
                _twoSidedStencilMode = value;
            }
        }

        #endregion

        #region Constructors

        public DepthStencilState()
        {
            DepthBufferEnable = true;
            DepthBufferWriteEnable = true;
            DepthBufferFunction = CompareFunction.LessEqual;
            StencilEnable = false;
            StencilFunction = CompareFunction.Always;
            StencilPass = StencilOperation.Keep;
            StencilFail = StencilOperation.Keep;
            StencilDepthBufferFail = StencilOperation.Keep;
            TwoSidedStencilMode = false;
            CounterClockwiseStencilFunction = CompareFunction.Always;
            CounterClockwiseStencilFail = StencilOperation.Keep;
            CounterClockwiseStencilPass = StencilOperation.Keep;
            CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
            StencilMask = int.MaxValue;
            StencilWriteMask = int.MaxValue;
            ReferenceStencil = 0;
        }

        private DepthStencilState(string name, bool depthBufferEnable, bool depthBufferWriteEnable)
            : this()
        {
            Name = name;
            _depthBufferEnable = depthBufferEnable;
            _depthBufferWriteEnable = depthBufferWriteEnable;
            _defaultStateObject = true;
        }

        private DepthStencilState(DepthStencilState cloneSource)
        {
            Name = cloneSource.Name;
            _depthBufferEnable = cloneSource._depthBufferEnable;
            _depthBufferWriteEnable = cloneSource._depthBufferWriteEnable;
            _counterClockwiseStencilDepthBufferFail = cloneSource._counterClockwiseStencilDepthBufferFail;
            _counterClockwiseStencilFail = cloneSource._counterClockwiseStencilFail;
            _counterClockwiseStencilFunction = cloneSource._counterClockwiseStencilFunction;
            _counterClockwiseStencilPass = cloneSource._counterClockwiseStencilPass;
            _depthBufferFunction = cloneSource._depthBufferFunction;
            _referenceStencil = cloneSource._referenceStencil;
            _stencilDepthBufferFail = cloneSource._stencilDepthBufferFail;
            _stencilEnable = cloneSource._stencilEnable;
            _stencilFail = cloneSource._stencilFail;
            _stencilFunction = cloneSource._stencilFunction;
            _stencilMask = cloneSource._stencilMask;
            _stencilPass = cloneSource._stencilPass;
            _stencilWriteMask = cloneSource._stencilWriteMask;
            _twoSidedStencilMode = cloneSource._twoSidedStencilMode;
        }

#endregion

        internal DepthStencilState Clone()
        {
            return new DepthStencilState(this);
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

