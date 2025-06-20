// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Threading;

#if IOS
using Foundation;
using OpenGLES;
#endif

namespace MonoGame.Framework
{
    internal class Threading
    {
        // nothing should stall the main thread for more than a second
        public static readonly TimeSpan MaxWaitForMainThread = TimeSpan.FromMilliseconds(1000);

        private static int _mainThreadId;

#if IOS
        public static EAGLContext BackgroundContext;
#endif

        static Threading()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

#if ANDROID
        internal static void ResetThread(int id)
        {
            _mainThreadId = id;
        }
#endif

        /// <summary>
        /// Gets whether the caller is running on the main thread.
        /// </summary>
        /// <returns><see langword="true"/> if the caller is running on the main thread.</returns>
        public static bool IsOnMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        /// <summary>
        /// Throws an exception if the caller is not running on the main thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The caller is not running on the main thread.
        /// </exception>
        public static void AssertMainThread()
        {
            if (!IsOnMainThread)
                throw new OffThreadNotSupportedException();
        }

        /// <summary>
        /// Runs the given action on the main thread and blocks while the action is running.
        /// </summary>
        /// <param name="action">The action to be run on the main thread.</param>
        internal static void BlockOnMainThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            // If we are already on the main thread, just call the action and be done with it.
            if (IsOnMainThread)
            {
                action.Invoke();
                return;
            }

            var resetEvent = new ManualResetEventSlim(false);
            Add(() =>
            {
#if ANDROID
                //if (!Game.Instance.Window.GraphicsContext.IsCurrent)
                    ((AndroidGameWindow)AndroidGameActivity.Instance.Game.Window).GameView.MakeCurrent();
#endif
                action.Invoke();
                resetEvent.Set();
            });

            if (!resetEvent.Wait(MaxWaitForMainThread))
                throw new TimeoutException();
        }

        private static readonly List<Action> _actionList = new();

        private static void Add(Action action)
        {
            lock (_actionList)
                _actionList.Add(action);
        }

        /// <summary>
        /// Runs all pending actions. Must be called from the main thread.
        /// </summary>
        internal static void Run()
        {
            AssertMainThread();

#if IOS
            lock (BackgroundContext)
            {
                // Make the context current on this thread if it is not already
                if (!Object.ReferenceEquals(EAGLContext.CurrentContext, BackgroundContext))
                    EAGLContext.SetCurrentContext(BackgroundContext);
#endif
            lock (_actionList)
            {

                foreach (Action action in _actionList)
                    action.Invoke();

                _actionList.Clear();
            }
#if IOS
                // Must flush the GL calls so the GPU asset is ready for the main context to use it
                GL.Flush();
                GL.CheckError();
            }
#endif
        }
    }
}
