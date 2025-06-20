// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using MonoGame.Framework.Input.Touch;

namespace MonoGame.Framework.Windows
{
    internal static class MessageExtensions
    {
        public static int GetPointerId(this Message msg)
        {
            return (short)msg.WParam;
        }

        public static System.Drawing.Point GetPointerLocation(this Message msg)
        {
            var lowword = msg.LParam.ToInt32();

            return new System.Drawing.Point()
            {
                X = (short)lowword,
                Y = (short)(lowword >> 16),
            };
        }
    }

    [System.ComponentModel.DesignerCategory("Code")]
    internal partial class WinFormsGameForm : Form
    {
        public WinFormsGameWindow Window { get; }

        public event DataEvent<WinFormsGameForm, HorizontalMouseWheelEvent>? MouseHorizontalWheel;

        internal bool IsResizing { get; set; }

        public WinFormsGameForm(WinFormsGameWindow window)
        {
            Window = window ?? throw new ArgumentNullException(nameof(window));
        }

        public void CenterOnPrimaryMonitor()
        {
            Location = new System.Drawing.Point(
                (Screen.PrimaryScreen.WorkingArea.Width - Width) / 2,
                (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
        }

        [DllImport("user32.dll")]
        private static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        public void SendTextInput(Rune rune)
        {
            // Don't post text events for unprintable characters
            if (rune.Value < ' ' || rune.Value == 127)
                return;

            Span<char> tmp = stackalloc char[1];
            var nkey = rune.TryEncodeToUtf16(tmp, out _)
                ? (Input.Keys)(VkKeyScanEx(tmp[0], InputLanguage.CurrentInputLanguage.Handle) & 0xff)
                : (Input.Keys?)null;

            Window.OnTextInput(new TextInputEventArgs(rune, nkey));
        }

        protected override void WndProc(ref Message m)
        {
            var state = TouchLocationState.Invalid;
            
            switch ((WM)m.Msg)
            {
                case WM.TABLET_QUERYSYSTEMGESTURESTA:
                {
                    // This disables the windows touch helpers, popups, and 
                    // guides that get in the way of touch gameplay.
                    const int flags =
                        0x00000001 | // TABLET_DISABLE_PRESSANDHOLD
                        0x00000008 | // TABLET_DISABLE_PENTAPFEEDBACK
                        0x00000010 | // TABLET_DISABLE_PENBARRELFEEDBACK
                        0x00000100 | // TABLET_DISABLE_TOUCHUIFORCEON
                        0x00000200 | // TABLET_DISABLE_TOUCHUIFORCEOFF
                        0x00008000 | // TABLET_DISABLE_TOUCHSWITCH
                        0x00010000 | // TABLET_DISABLE_FLICKS
                        0x00080000 | // TABLET_DISABLE_SMOOTHSCROLLING 
                        0x00100000; // TABLET_DISABLE_FLICKFALLBACKKEYS

                    m.Result = new IntPtr(flags);
                    return;
                }

#if WINDOWS && DIRECTX
                case WM.KEYDOWN:
                    HandleKeyMessage(ref m);
                    switch (m.WParam.ToInt32())
                    {
                        case 0x5B:  // Left Windows Key
                        case 0x5C: // Right Windows Key
                            if (Window.IsFullScreen && Window.HardwareModeSwitch)
                                WindowState = FormWindowState.Minimized;
                            break;
                    }
                    break;

                case WM.SYSKEYDOWN:
                case WM.KEYUP:
                case WM.SYSKEYUP:
                    HandleKeyMessage(ref m);
                    break;
#endif
                case WM.SYSCOMMAND:
                    int wParam = m.WParam.ToInt32();
                    if (!Window.AllowAltF4 && 
                        wParam == 0xF060 &&
                        m.LParam.ToInt32() == 0 &&
                        Focused)
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }

                    // Disable the system menu from being toggled by
                    // keyboard input so we can own the ALT key.
                    if (wParam == 0xF100) // SC_KEYMENU
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM.POINTERUP:
                    state = TouchLocationState.Released;
                    break;

                case WM.POINTERDOWN:
                    state = TouchLocationState.Pressed;
                    break;

                case WM.POINTERUPDATE:
                    state = TouchLocationState.Moved;
                    break;

                case WM.MOUSEHWHEEL:
                    var delta = (short)(((ulong)m.WParam >> 16) & 0xffff);
                    MouseHorizontalWheel?.Invoke(this, new HorizontalMouseWheelEvent(delta));
                    break;

                case WM.ENTERSIZEMOVE:
                    IsResizing = true;
                    break;

                case WM.EXITSIZEMOVE:
                    IsResizing = false;
                    break;
            }

            if (state != TouchLocationState.Invalid)
            {
                var id = m.GetPointerId();

                var position = m.GetPointerLocation();
                position = PointToClient(position);
                var vec = new Vector2(position.X, position.Y);

                Window.TouchPanel.AddEvent(id, state, vec, false);
            }

            base.WndProc(ref m);
        }

        private void HandleKeyMessage(ref Message m)
        {
            long virtualKeyCode = m.WParam.ToInt64();
            bool extended = (m.LParam.ToInt64() & 0x01000000) != 0;
            long scancode = (m.LParam.ToInt64() & 0x00ff0000) >> 16;

            var key = KeyCodeTranslate(
                (Keys)virtualKeyCode,
                extended,
                scancode);

            switch ((WM)m.Msg)
            {
                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                {
                    var nkey = Input.KeysHelper.IsKey((int)key) ? key : (Input.Keys?)null;
                    Window.OnKeyDown(new TextInputEventArgs(new System.Text.Rune((char)key), nkey));
                    break;
                }

                case WM.KEYUP:
                case WM.SYSKEYUP:
                {
                    var nkey = Input.KeysHelper.IsKey((int)key) ? key : (Input.Keys?)null;
                    Window.OnKeyUp(new TextInputEventArgs(new System.Text.Rune((char)key), nkey));
                    break;
                }
            }
        }

        private static Input.Keys KeyCodeTranslate(
            Keys keyCode, bool extended, long scancode)
        {
            switch (keyCode)
            {
                // WinForms does not distinguish between left/right keys
                // We have to check for special keys such as control/shift/alt/ etc
                case Keys.ControlKey:
                    return extended
                        ? Input.Keys.RightControl
                        : Input.Keys.LeftControl;

                case Keys.ShiftKey:
                    // left shift is 0x2A, right shift is 0x36. IsExtendedKey is always false.
                    return ((scancode & 0x1FF) == 0x36)
                               ? Input.Keys.RightShift
                                : Input.Keys.LeftShift;

                // Note that the Alt key is now refered to as Menu.
                case Keys.Menu:
                case Keys.Alt:
                    return extended
                        ? Input.Keys.RightAlt
                        : Input.Keys.LeftAlt;

                default:
                    return (Input.Keys)keyCode;
            }
        }

        public enum WM
        {
            MOUSEHWHEEL = 0x020E,
            POINTERUP = 0x0247,
            POINTERDOWN = 0x0246,
            POINTERUPDATE = 0x0245,
            KEYDOWN = 0x0100,
            KEYUP = 0x0101,
            SYSKEYDOWN = 0x0104,
            SYSKEYUP = 0x0105,
            GESTURE = 0x0119,
            GESTURENOTIFY = 0x011A,   
            TABLET_QUERYSYSTEMGESTURESTA = 0x02C0 + 12,

            ENTERSIZEMOVE = 0x0231,
            EXITSIZEMOVE = 0x0232,

            SYSCOMMAND = 0x0112,
            INPUTLANGCHANGE = 0x0051
        }

        public enum GCS
        {
            COMPREADSTR = 0x0001,
            COMPREADATTR = 0x0002,
            COMPREADCLAUSE = 0x0004,
            COMPSTR = 0x0008,
            COMPATTR = 0x0010,
            COMPCLAUSE = 0x0020,
            CURSORPOS = 0x0080,
            DELTASTART = 0x0100,
            RESULTREADSTR = 0x0200,
            RESULTREADCLAUSE = 0x0400,
            RESULTSTR = 0x0800,
            RESULTCLAUSE = 0x1000,
        }
    }
}
