using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace Molten.Audio.OpenAL
{
    internal unsafe class OutputDevice : AudioDevice, IAudioOutput
    {
        Context* _context;

        internal OutputDevice(AudioServiceAL service, string specifier, bool isDefault) :
            base(service, specifier, isDefault, AudioDeviceType.Output)
        {
        }


        protected override unsafe ContextError OnOpen()
        {
            Ptr = Service.Alc.OpenDevice(Name);
            ContextError result = Service.Alc.GetError(Ptr);
            if (result == ContextError.NoError)
            {
                _context = Service.Alc.CreateContext(Ptr, null);
                result = Service.Alc.GetError(Ptr);
                if (result == ContextError.NoError)
                {
                    Service.Alc.MakeContextCurrent(_context);
                    result = Service.Alc.GetError(Ptr);
                }
            }

            return result;
        }

        protected override ContextError OnClose()
        {
            Context* curContext = Service.Alc.GetCurrentContext();
            if (curContext == _context)
                Service.Alc.MakeContextCurrent(null);

            Service.Alc.DestroyContext(_context);
            Service.Alc.CloseDevice(Ptr);
            return Service.Alc.GetError(Ptr);
        }

        protected override void OnTransferTo(AudioDevice other)
        {
            
        }

        protected override void OnUpdate(Timing time)
        {
            
        }
    }
}
