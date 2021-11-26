using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Net
{
    internal static class DeliveryMethodExtension
    {
        /// <summary>
        /// Convert lidgren delivery method to molten equivalent.
        /// </summary>
        /// <param name="deliveryMethod"></param>
        /// <returns></returns>
        internal static DeliveryMethod ToMolten(this NetDeliveryMethod deliveryMethod)
        {
            switch (deliveryMethod)
            {
                case NetDeliveryMethod.Unreliable:
                    return DeliveryMethod.Unreliable;

                case NetDeliveryMethod.UnreliableSequenced:
                    return DeliveryMethod.UnreliableSequenced;

                case NetDeliveryMethod.ReliableUnordered:
                    return DeliveryMethod.ReliableUnordered;

                case NetDeliveryMethod.ReliableSequenced:
                    return DeliveryMethod.ReliableSequenced;

                case NetDeliveryMethod.ReliableOrdered:
                    return DeliveryMethod.ReliableOrdered;

                case NetDeliveryMethod.Unknown:
                default:
                    return DeliveryMethod.Unknown;
            }
        }

        /// <summary>
        /// Convert molten delivery method to lidgren equivalent.
        /// </summary>
        /// <param name="deliveryMethod"></param>
        /// <returns></returns>
        internal static NetDeliveryMethod ToLidgren(this DeliveryMethod deliveryMethod)
        {
            switch (deliveryMethod)
            {
                case DeliveryMethod.Unreliable:
                    return NetDeliveryMethod.Unreliable;

                case DeliveryMethod.UnreliableSequenced:
                    return NetDeliveryMethod.UnreliableSequenced;

                case DeliveryMethod.ReliableUnordered:
                    return NetDeliveryMethod.ReliableUnordered;

                case DeliveryMethod.ReliableSequenced:
                    return NetDeliveryMethod.ReliableSequenced;

                case DeliveryMethod.ReliableOrdered:
                    return NetDeliveryMethod.ReliableOrdered;

                case DeliveryMethod.Unknown:
                default:
                    return NetDeliveryMethod.Unknown;
            }
        }

    }
}