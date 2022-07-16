using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public abstract class ParameterizedContentHandle : ContentHandle
    {
        internal ParameterizedContentHandle(ContentManager manager, IContentProcessor processor, IContentParameters parameters, Type contentType) : 
            base(manager, contentType)
        {
            Processor = processor;
            Parameters = parameters;
        }

        private void ValidateParameters()
        {
            Type pExpectedType = Processor.GetParameterType();

            if (Parameters != null)
            {
                Type pType = Parameters.GetType();

                if (!pExpectedType.IsAssignableFrom(pType))
                    Manager.Log.Warning($"[CONTENT] {Info}: Invalid parameter type provided. Expected '{pExpectedType.Name}' but received '{pType.Name}'. Using defaults instead.");
                else
                    return;
            }

            Parameters = Activator.CreateInstance(pExpectedType) as IContentParameters;
        }

        protected override bool OnProcess()
        {
            ValidateParameters();

            return true;
        }

        internal IContentProcessor Processor { get; }

        internal IContentParameters Parameters { get; set; }
    }
}
