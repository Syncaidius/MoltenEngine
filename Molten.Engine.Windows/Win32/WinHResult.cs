using System.Runtime.InteropServices;

namespace Molten.Windows32
{
    public struct WinHResult
    {
        public int Value { get; }

        public bool IsSuccess => Value >= 0;

        public bool IsFailure => Value < 0;

        public bool IsError => (uint)Value >> 31 == 1;

        public int Code => (Value & 0xFFFF);

        /// <summary>
        /// If true, the hresult value is customer-defined. If false, the value is Microsoft-defined.
        /// </summary>
        public bool IsCustomer => ((Value >> 29) & 1) == 1;

        /// <summary>
        /// If true, <see cref="Code"/> is an NTSTATUS code.
        /// <para>
        /// See: https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55</para>
        /// </summary>
        public bool IsCodeNtStatus => ((Value >> 28) & 1) == 1;

        /// <summary>
        /// Facility code. 
        /// <para>
        /// See: https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/0642cb2f-2075-4469-918c-4441e69c548a?redirectedfrom=MSDN
        /// </para>
        /// </summary>
        public int Facility => ((Value >> 16) & 0x1FFF);

        public int Severity => (Value >> 31) & 1;


        public WinHResult(int hr)
        {
            Value = hr;
        }

        public T ToEnum<T>() where T : struct
        {
            return (T)Enum.ToObject(typeof(T), ((Value << 31) | (Facility << 16) | Code));
        }

        public static implicit operator int(WinHResult hr)
        {
            return hr.Value;
        }

        public static implicit operator WinHResult(int hr)
        {
            return new WinHResult(hr);
        }

        public void Throw()
        {
            Marshal.ThrowExceptionForHR(Value);
        }
    }
}
