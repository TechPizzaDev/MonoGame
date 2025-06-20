using System;
using System.Runtime.InteropServices;
using MonoGame.OpenAL;

namespace MonoGame.Framework.Audio
{
    internal class EffectsExtension
    {
        #region Effect Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alGenEffectsDelegate(int count, out uint effect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alDeleteEffectsDelegate(int count, in uint effect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool alIsEffectDelegate(uint effect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alEffectfDelegate(uint effect, EfxEffectf param, float value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alEffectiDelegate(uint effect, EfxEffecti param, int value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alGenAuxiliaryEffectSlotsDelegate(int count, out uint effectslots);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alDeleteAuxiliaryEffectSlotsDelegate(int count, in uint effectslots);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alAuxiliaryEffectSlotiDelegate(uint slot, EfxEffecti type, uint effect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alAuxiliaryEffectSlotfDelegate(uint slot, EfxEffectSlotf param, float value);

        private alGenEffectsDelegate alGenEffects;
        private alDeleteEffectsDelegate alDeleteEffects;
        private alEffectfDelegate alEffectf;
        private alEffectiDelegate alEffecti;
        private alGenAuxiliaryEffectSlotsDelegate alGenAuxiliaryEffectSlots;
        private alDeleteAuxiliaryEffectSlotsDelegate alDeleteAuxiliaryEffectSlots;
        private alAuxiliaryEffectSlotiDelegate alAuxiliaryEffectSloti;
        private alAuxiliaryEffectSlotfDelegate alAuxiliaryEffectSlotf;

        public alIsEffectDelegate IsEffect;

        #endregion

        #region Filter Delegates

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alGenFiltersDelegate(int count, out uint filters);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alFilteriDelegate(uint fid, EfxFilteri param, int value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alFilterfDelegate(uint fid, EfxFilterf param, float value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alDeleteFiltersDelegate(int n, in uint filters);

        private alGenFiltersDelegate alGenFilters;
        private alFilteriDelegate alFilteri;
        private alFilterfDelegate alFilterf;
        private alDeleteFiltersDelegate alDeleteFilters;

        #endregion

        public bool IsAvailable { get; private set; }

        public EffectsExtension(IntPtr device)
        {
            if (!ALC.IsExtensionPresent(device, "ALC_EXT_EFX"))
                return;

            static TDelegate GetDelegate<TDelegate>(string name)
            {
                var procAddress = AL.GetProcAddress(name);
                return Marshal.GetDelegateForFunctionPointer<TDelegate>(procAddress);
            }

            alGenEffects = GetDelegate<alGenEffectsDelegate>("alGenEffects");
            alDeleteEffects = GetDelegate<alDeleteEffectsDelegate>("alDeleteEffects");
            alEffectf = GetDelegate<alEffectfDelegate>("alEffectf");
            alEffecti = GetDelegate<alEffectiDelegate>("alEffecti");
            alGenAuxiliaryEffectSlots = GetDelegate<alGenAuxiliaryEffectSlotsDelegate>("alGenAuxiliaryEffectSlots");
            alDeleteAuxiliaryEffectSlots = GetDelegate<alDeleteAuxiliaryEffectSlotsDelegate>("alDeleteAuxiliaryEffectSlots");
            alAuxiliaryEffectSloti = GetDelegate<alAuxiliaryEffectSlotiDelegate>("alAuxiliaryEffectSloti");
            alAuxiliaryEffectSlotf = GetDelegate<alAuxiliaryEffectSlotfDelegate>("alAuxiliaryEffectSlotf");

            alGenFilters = GetDelegate<alGenFiltersDelegate>("alGenFilters");
            alFilteri = GetDelegate<alFilteriDelegate>("alFilteri");
            alFilterf = GetDelegate<alFilterfDelegate>("alFilterf");
            alDeleteFilters = GetDelegate<alDeleteFiltersDelegate>("alDeleteFilters");

            IsAvailable = true;
        }

        //alEffecti(effect, EfxEffecti.FilterType, (int) EfxEffectType.Reverb);
        //ALHelper.CheckError("Failed to set Filter Type.");      

        public unsafe void GenAuxiliaryEffectSlots(Span<uint> output)
        {
            if (output.IsEmpty)
                return;

            alGenAuxiliaryEffectSlots(output.Length, out MemoryMarshal.GetReference(output));
            ALHelper.CheckError("Failed to genereate aux slot.");
        }

        public uint GenAuxiliaryEffectSlot()
        {
            Span<uint> output = stackalloc uint[1];
            GenAuxiliaryEffectSlots(output);
            return output[0];
        }

        public unsafe void GenEffects(Span<uint> output)
        {
            if (output.IsEmpty)
                return;

            alGenEffects(output.Length, out MemoryMarshal.GetReference(output));
            ALHelper.CheckError("Failed to generate effects.");
        }

        public uint GenEffect()
        {
            Span<uint> output = stackalloc uint[1];
            GenEffects(output);
            return output[0];
        }

        public unsafe void DeleteAuxiliaryEffectSlots(ReadOnlySpan<uint> slots)
        {
            if (slots.IsEmpty)
                return;

            alDeleteAuxiliaryEffectSlots(slots.Length, MemoryMarshal.GetReference(slots));
        }

        public void DeleteAuxiliaryEffectSlot(uint slot)
        {
            Span<uint> tmp = stackalloc uint[] { slot };
            DeleteAuxiliaryEffectSlots(tmp);
        }

        public unsafe void DeleteEffects(ReadOnlySpan<uint> effects)
        {
            if (effects.IsEmpty)
                return;

            alDeleteEffects(effects.Length, MemoryMarshal.GetReference(effects));
        }

        public void DeleteEffect(uint effect)
        {
            Span<uint> tmp = stackalloc uint[] { effect };
            DeleteEffects(tmp);
        }

        public void BindEffectToAuxiliarySlot(uint slot, uint effect)
        {
            alAuxiliaryEffectSloti(slot, EfxEffecti.SlotEffect, effect);
            ALHelper.CheckError("Failed to bind Effect");
        }

        internal void AuxiliaryEffectSlot(uint slot, EfxEffectSlotf param, float value)
        {
            alAuxiliaryEffectSlotf(slot, param, value);
            ALHelper.CheckError(param, value);
        }

        internal void BindSourceToAuxiliarySlot(uint source, int slot, int slotnumber, int filter)
        {
            AL.Source(source, ALSource3i.EfxAuxilarySendFilter, slot, slotnumber, filter);
        }

        internal void Effect(uint effect, EfxEffectf param, float value)
        {
            alEffectf(effect, param, value);
            ALHelper.CheckError(param, value);
        }

        internal void Effect(uint effect, EfxEffecti param, int value)
        {
            alEffecti(effect, param, value);
            ALHelper.CheckError(param, value);
        }

        public void GenFilters(Span<uint> output)
        {
            if (output.IsEmpty)
                return;

            alGenFilters(output.Length, out MemoryMarshal.GetReference(output));
        }

        public uint GenFilter()
        {
            Span<uint> output = stackalloc uint[1];
            GenFilters(output);
            return output[0];
        }

        public void Filter(uint source, EfxFilteri filter, int filterType)
        {
            alFilteri(source, filter, filterType);
        }

        public void Filter(uint source, EfxFilterf filter, float filterType)
        {
            alFilterf(source, filter, filterType);
        }

        public void BindFilterToSource(uint source, uint filter)
        {
            AL.Source(source, ALSourcei.EfxDirectFilter, filter);
        }

        public unsafe void DeleteFilters(ReadOnlySpan<uint> filters)
        {
            if (filters.IsEmpty)
                return;

            alDeleteFilters(filters.Length, MemoryMarshal.GetReference(filters));
        }

        public void DeleteFilter(uint filter)
        {
            Span<uint> tmp = stackalloc uint[] { filter };
            DeleteFilters(tmp);
        }
    }
}
