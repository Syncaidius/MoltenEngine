namespace Molten.Comparers;

public class IntPtrComparer : IComparer<IntPtr>
{
    public unsafe int Compare(IntPtr x, IntPtr y)
    {
        nuint ix = (nuint)x.ToPointer();
        nuint iy = (nuint)y.ToPointer();

        if (ix < iy)
            return -1;
        else if (ix > iy)
            return 1;
        else
            return 0;
    }
}
