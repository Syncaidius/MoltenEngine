namespace Molten.Graphics.SDF;

public class Scanline
{
    public struct Intersection
    {
        /// X coordinate.
        public double x;

        /// Normalized Y direction of the oriented edge at the point of intersection.
        public int direction;

        public Intersection(double xx, int dir)
        {
            x = xx;
            direction = dir;
        }
    }

    public List<Intersection> Intersections;
    public int LastIndex;

    public int CompareIntersections(Intersection a, Intersection b)
    {
        return Math.Sign(a.x - b.x);
    }

    public bool InterpretFillRule(int intersections, FillRule fillRule)
    {
        switch (fillRule)
        {
            case FillRule.NonZero:
                return intersections != 0;
            case FillRule.Odd:
                return (intersections & 1) == 1;
            case FillRule.Positive:
                return intersections > 0;
            case FillRule.Negative:
                return intersections < 0;
        }
        return false;
    }

    public void Preprocess()
    {
        LastIndex = 0;
        if (Intersections.Count > 0)
        {
            Intersections.Sort(CompareIntersections);
            int totalDirection = 0;
            for (int i = 0; i < Intersections.Count; i++)
            {
                Intersection isec = Intersections[i];
                totalDirection += isec.direction;
                isec.direction = totalDirection;
                Intersections[i] = isec;
            }
        }
    }

    public void SetIntersections(List<Intersection> newIntersections)
    {
        Intersections = newIntersections;
        Preprocess();
    }

    public int MoveTo(double x)
    {
        if (Intersections.Count == 0)
            return -1;
        int index = LastIndex;
        if (x < Intersections[index].x)
        {
            do
            {
                if (index == 0)
                {
                    LastIndex = 0;
                    return -1;
                }
                --index;
            } while (x < Intersections[index].x);
        }
        else
        {
            while (index < Intersections.Count - 1 && x >= Intersections[index + 1].x)
                ++index;
        }
        LastIndex = index;
        return index;
    }

    public int SumIntersections(double x)
    {
        int index = MoveTo(x);
        if (index >= 0)
            return Intersections[index].direction;
        return 0;
    }

    public bool Filled(double x, FillRule fillRule)
    {
        return InterpretFillRule(SumIntersections(x), fillRule);
    }
}
