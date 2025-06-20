// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Framework;
using System.IO;

namespace MonoGame.OpenAL
{
    using OS = PlatformInfo.OS;

    [CLSCompliant(false)]
    public static unsafe class AL
    {
        public static IntPtr NativeLibrary { get; private set; } = GetNativeLibrary();

        private static IntPtr GetNativeLibrary()
        {
            var loaded = IntPtr.Zero;

#if DESKTOPGL || DIRECTX
            // Load bundled library
            string assemblyLocation = Path.GetDirectoryName(typeof(AL).Assembly.Location) ?? string.Empty;

            if (PlatformInfo.CurrentOS == OS.Windows && Environment.Is64BitProcess)
                loaded = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, "x64/soft_oal.dll"));
            else if (PlatformInfo.CurrentOS == OS.Windows && !Environment.Is64BitProcess)
                loaded = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, "x86/soft_oal.dll"));
            else if (PlatformInfo.CurrentOS == OS.Linux && Environment.Is64BitProcess)
                loaded = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, "x64/libopenal.so.1"));
            else if (PlatformInfo.CurrentOS == OS.Linux && !Environment.Is64BitProcess)
                loaded = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, "x86/libopenal.so.1"));
            else if (PlatformInfo.CurrentOS == OS.MacOSX)
                loaded = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, "libopenal.1.dylib"));

            // Load system library
            if (loaded == IntPtr.Zero)
            {
                if (PlatformInfo.CurrentOS == OS.Windows)
                    loaded = FuncLoader.LoadLibrary("soft_oal.dll");
                else if (PlatformInfo.CurrentOS == OS.Linux)
                    loaded = FuncLoader.LoadLibrary("libopenal.so.1");
                else
                    loaded = FuncLoader.LoadLibrary("libopenal.1.dylib");
            }
#elif ANDROID
            loaded = FuncLoader.LoadLibrary("libopenal32.so");

            if (loaded == IntPtr.Zero)
            {
                string appFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string appDir = Path.GetDirectoryName(appFilesDir);
                string lib = Path.Combine(appDir, "lib", "libopenal32.so");

                loaded = FuncLoader.LoadLibrary(lib);
            }
#else
            loaded = FuncLoader.LoadLibrary("/System/Library/Frameworks/OpenAL.framework/OpenAL");
#endif

            return loaded;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_alenable(int cap);
        public static d_alenable Enable = FuncLoader.LoadFunction<d_alenable>(NativeLibrary, "alEnable");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_albufferdata(uint buffer, int format, IntPtr data, int size, int freq);
        private static d_albufferdata alBufferData = FuncLoader.LoadFunction<d_albufferdata>(NativeLibrary, "alBufferData");

        [CLSCompliant(false)]
        public static void BufferData(uint buffer, ALFormat format, ReadOnlySpan<byte> data, int freq)
        {
            fixed (byte* ptr = data)
                alBufferData(buffer, (int)format, (IntPtr)ptr, data.Length, freq);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_aldeletebuffers(int count, uint* buffers);
        private static d_aldeletebuffers alDeleteBuffers = FuncLoader.LoadFunction<d_aldeletebuffers>(NativeLibrary, "alDeleteBuffers");

        public static void DeleteBuffers(ReadOnlySpan<uint> buffers)
        {
            if (buffers.IsEmpty)
                return;

            fixed (uint* ptr = buffers)
                alDeleteBuffers(buffers.Length, ptr);
        }

        public static void DeleteBuffer(uint buffer)
        {
            Span<uint> buffers = stackalloc uint[] { buffer };
            DeleteBuffers(buffers);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_albufferi(uint buffer, ALBufferi param, int value);
        internal static d_albufferi Bufferi = FuncLoader.LoadFunction<d_albufferi>(NativeLibrary, "alBufferi");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_algetbufferi(uint buffer, ALGetBufferi param, out int value);
        private static d_algetbufferi alGetBufferi = FuncLoader.LoadFunction<d_algetbufferi>(NativeLibrary, "alGetBufferi");

        public static void GetBuffer(uint buffer, ALGetBufferi param, out int value)
        {
            alGetBufferi(buffer, param, out value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_albufferiv(uint buffer, ALBufferi param, int* values);
        private static d_albufferiv alBufferiv = FuncLoader.LoadFunction<d_albufferiv>(NativeLibrary, "alBufferiv");

        public static void Bufferiv(uint buffer, ALBufferi param, ReadOnlySpan<int> values)
        {
            fixed (int* ptr = values)
                alBufferiv(buffer, param, ptr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_algenbuffers(int count, uint* output);
        private static d_algenbuffers alGenBuffers = FuncLoader.LoadFunction<d_algenbuffers>(NativeLibrary, "alGenBuffers");

        public static void GenBuffers(Span<uint> output)
        {
            if (output.IsEmpty)
                return;

            fixed (uint* ptr = output)
                alGenBuffers(output.Length, ptr);
        }

        public static uint GenBuffer()
        {
            Span<uint> output = stackalloc uint[1];
            GenBuffers(output);
            return output[0];
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_algensources(int count, uint* output);
        private static d_algensources alGenSources = FuncLoader.LoadFunction<d_algensources>(NativeLibrary, "alGenSources");

        public static void GenSources(Span<uint> output)
        {
            if (output.IsEmpty)
                return;

            fixed (uint* ptr = output)
                alGenSources(output.Length, ptr);
        }

        public static uint GenSource()
        {
            Span<uint> output = stackalloc uint[1];
            GenSources(output);
            return output[0];
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate ALError d_algeterror();
        public static d_algeterror GetError = FuncLoader.LoadFunction<d_algeterror>(NativeLibrary, "alGetError");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_alisbuffer(uint buffer);
        public static d_alisbuffer IsBuffer = FuncLoader.LoadFunction<d_alisbuffer>(NativeLibrary, "alIsBuffer");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_alsourcepause(uint source);
        public static d_alsourcepause SourcePause = FuncLoader.LoadFunction<d_alsourcepause>(NativeLibrary, "alSourcePause");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_alsourceplay(uint source);
        public static d_alsourceplay SourcePlay = FuncLoader.LoadFunction<d_alsourceplay>(NativeLibrary, "alSourcePlay");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_alissource(uint source);
        public static d_alissource IsSource = FuncLoader.LoadFunction<d_alissource>(NativeLibrary, "alIsSource");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_aldeletesources(int n, uint* sources);
        private static d_aldeletesources alDeleteSources = FuncLoader.LoadFunction<d_aldeletesources>(NativeLibrary, "alDeleteSources");

        public static void DeleteSources(ReadOnlySpan<uint> sources)
        {
            if (sources.IsEmpty)
                return;

            fixed (uint* ptr = sources)
                alDeleteSources(sources.Length, ptr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_alsourcestop(uint source);
        public static d_alsourcestop SourceStop = FuncLoader.LoadFunction<d_alsourcestop>(NativeLibrary, "alSourceStop");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_alsourcei(uint source, int i, int a);
        private static d_alsourcei alSourcei = FuncLoader.LoadFunction<d_alsourcei>(NativeLibrary, "alSourcei");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_alsource3i(uint source, ALSource3i i, int a, int b, int c);
        private static d_alsource3i alSource3i = FuncLoader.LoadFunction<d_alsource3i>(NativeLibrary, "alSource3i");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_alsourcef(uint source, ALSourcef i, float a);
        private static d_alsourcef alSourcef = FuncLoader.LoadFunction<d_alsourcef>(NativeLibrary, "alSourcef");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_alsource3f(uint source, ALSource3f i, float x, float y, float z);
        private static d_alsource3f alSource3f = FuncLoader.LoadFunction<d_alsource3f>(NativeLibrary, "alSource3f");

        public static void Source(uint source, ALSourcei i, int a) => alSourcei(source, (int)i, a);

        public static void Source(uint source, ALSourcei i, uint a) => Source(source, i, (int)a);

        public static void Source(uint source, ALSourceb i, bool a) => alSourcei(source, (int)i, a ? 1 : 0);

        public static void Source(uint source, ALSource3i i, int x, int y, int z) => alSource3i(source, i, x, y, z);

        public static void Source(uint source, ALSourcef i, float dist) => alSourcef(source, i, dist);

        public static void Source(uint source, ALSource3f i, float x, float y, float z) => alSource3f(source, i, x, y, z);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_algetsourcei(uint source, ALGetSourcei i, out int state);
        public static d_algetsourcei GetSource = FuncLoader.LoadFunction<d_algetsourcei>(NativeLibrary, "alGetSourcei");

        public static ALSourceState GetSourceState(uint source)
        {
            GetSource(source, ALGetSourcei.SourceState, out int state);
            return (ALSourceState)state;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_algetlistener3f(ALListener3f param, out float x, out float y, out float z);
        public static d_algetlistener3f GetListener = FuncLoader.LoadFunction<d_algetlistener3f>(NativeLibrary, "alGetListener3f");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_aldistancemodel(ALDistanceModel model);
        public static d_aldistancemodel DistanceModel = FuncLoader.LoadFunction<d_aldistancemodel>(NativeLibrary, "alDistanceModel");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_aldopplerfactor(float value);
        public static d_aldopplerfactor DopplerFactor = FuncLoader.LoadFunction<d_aldopplerfactor>(NativeLibrary, "alDopplerFactor");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_alsourcequeuebuffers(uint source, int numEntries, uint* buffers);
        private static d_alsourcequeuebuffers alSourceQueueBuffers = FuncLoader.LoadFunction<d_alsourcequeuebuffers>(NativeLibrary, "alSourceQueueBuffers");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_alsourceunqueuebuffers(uint source, int numEntries, uint* output);
        private static d_alsourceunqueuebuffers alSourceUnqueueBuffers = FuncLoader.LoadFunction<d_alsourceunqueuebuffers>(NativeLibrary, "alSourceUnqueueBuffers");

        public static void SourceQueueBuffers(uint source, ReadOnlySpan<uint> buffers)
        {
            if (buffers.IsEmpty)
                return;

            fixed (uint* ptr = buffers)
                alSourceQueueBuffers(source, buffers.Length, ptr);
        }

        public static void SourceQueueBuffer(uint source, uint buffer)
        {
            alSourceQueueBuffers(source, 1, &buffer);
        }

        public static void SourceUnqueueBuffers(uint source, Span<uint> salvaged)
        {
            if (salvaged.IsEmpty)
                return;

            fixed (uint* ptr = salvaged)
                alSourceUnqueueBuffers(source, salvaged.Length, ptr);
        }

        public static void SourceUnqueueBuffers(uint source, int count)
        {
            ArgumentGuard.AssertGreaterThanZero(count, nameof(count));

            Span<uint> salvaged = stackalloc uint[count];
            SourceUnqueueBuffers(source, salvaged);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_algetenumvalue(string enumName);
        public static d_algetenumvalue GetEnumValue = FuncLoader.LoadFunction<d_algetenumvalue>(NativeLibrary, "alGetEnumValue");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_alisextensionpresent(string extensionName);
        public static d_alisextensionpresent IsExtensionPresent = FuncLoader.LoadFunction<d_alisextensionpresent>(NativeLibrary, "alIsExtensionPresent");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_algetprocaddress(string functionName);
        public static d_algetprocaddress GetProcAddress = FuncLoader.LoadFunction<d_algetprocaddress>(NativeLibrary, "alGetProcAddress");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_algetstring(int p);
        private static d_algetstring alGetString = FuncLoader.LoadFunction<d_algetstring>(NativeLibrary, "alGetString");

        public static string? GetString(int p) => Marshal.PtrToStringAnsi(alGetString(p));

        public static string? Get(ALGetString p) => GetString((int)p);
    }
}
