namespace Molten;

public abstract class ContentParameters : ICloneable
{
    public int PartCount = 1;

    public abstract object Clone();
}
