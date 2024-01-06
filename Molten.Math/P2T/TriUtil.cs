namespace Molten;

internal static class TriUtil
{
    public const double PI_3div4 = 3 * Math.PI / 4;
    public const double PI_div2 = 1.57079632679489661923;
    public const double EPSILON = 1e-12;

    internal static Winding Orient2d(TriPoint pa, TriPoint pb, TriPoint pc)
    {
        double detleft = (pa.X - pc.X) * (pb.Y - pc.Y);
        double detright = (pa.Y - pc.Y) * (pb.X - pc.X);
        double val = detleft - detright;

        // Using a tolerance here fails on concave-by-subepsilon boundaries
        //   if (val > -EPSILON && val < EPSILON) {
        // Using == on double makes -Wfloat-equal warnings yell at us
        if (val == 0.0 || val == -0.0)
            return Winding.Collinear;
        else if (val > 0)
            return Winding.CounterClockwise;

        return Winding.Clockwise;
    }

    internal static bool InScanArea(TriPoint pa, TriPoint pb, TriPoint pc, TriPoint pd)
    {
        double oadb = (pa.X - pb.X) * (pd.Y - pb.Y) - (pd.X - pb.X) * (pa.Y - pb.Y);
        if (oadb >= -EPSILON)
            return false;


        double oadc = (pa.X - pc.X) * (pd.Y - pc.Y) - (pd.X - pc.X) * (pa.Y - pc.Y);
        if (oadc <= EPSILON)
            return false;

        return true;
    }
}
