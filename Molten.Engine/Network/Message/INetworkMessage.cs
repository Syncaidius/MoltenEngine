namespace Molten.Net.Message;

public interface INetworkMessage
{
    byte[] Data { get; }
    int Sequence { get; }
    DeliveryMethod DeliveryMethod { get; }
}
