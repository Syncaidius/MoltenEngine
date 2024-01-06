using Molten.DoublePrecision;
using Molten.Shapes;

namespace Molten.Graphics.SDF;

/// <summary>A contour combiner.</summary>
public class ContourCombiner
{
    Vector2D _p;
    List<int> _windings;

    internal List<MultiDistanceSelector> EdgeSelectors { get; }

    public ContourCombiner(Shape shape)
    {
        _windings = new List<int>(shape.Contours.Count);
        foreach (Contour contour in shape.Contours)
            _windings.Add(contour.GetWinding());

        EdgeSelectors = new List<MultiDistanceSelector>(shape.Contours.Count);
        for (int i = 0; i < shape.Contours.Count; i++)
            EdgeSelectors.Add(new MultiDistanceSelector());
    }

    public void Reset(ref Vector2D p)
    {
        this._p = p;
        foreach (MultiDistanceSelector contourEdgeSelector in EdgeSelectors)
            contourEdgeSelector.Reset(ref p);
    }

    public Color3D Distance()
    {
        int contourCount = EdgeSelectors.Count;
        MultiDistanceSelector EdgeSelector = new MultiDistanceSelector();
        MultiDistanceSelector innerEdgeSelector = new MultiDistanceSelector();
        MultiDistanceSelector outerEdgeSelector = new MultiDistanceSelector();

        EdgeSelector.Reset(ref _p);
        innerEdgeSelector.Reset(ref _p);
        outerEdgeSelector.Reset(ref _p);

        for (int i = 0; i < contourCount; ++i)
        {
            Color3D edgeDistance = EdgeSelectors[i].Distance();
            EdgeSelector.Merge(EdgeSelectors[i]);
            if (_windings[i] > 0 && EdgeSelectors[i].ResolveDistance(edgeDistance) >= 0)
                innerEdgeSelector.Merge(EdgeSelectors[i]);
            if (_windings[i] < 0 && EdgeSelectors[i].ResolveDistance(edgeDistance) <= 0)
                outerEdgeSelector.Merge(EdgeSelectors[i]);
        }

        Color3D shapeDistance = EdgeSelector.Distance();
        Color3D innerDistance = innerEdgeSelector.Distance();
        Color3D outerDistance = outerEdgeSelector.Distance();

        double innerScalarDistance = EdgeSelector.ResolveDistance(innerDistance);
        double outerScalarDistance = EdgeSelector.ResolveDistance(outerDistance);

        Color3D distance = new Color3D();
        EdgeSelector.InitDistance(ref distance);

        int winding = 0;
        if (innerScalarDistance >= 0 && Math.Abs(innerScalarDistance) <= Math.Abs(outerScalarDistance))
        {
            distance = innerDistance;
            winding = 1;
            for (int i = 0; i < contourCount; ++i)
            {
                if (_windings[i] > 0)
                {
                    Color3D contourDistance = EdgeSelectors[i].Distance();
                    if (Math.Abs(EdgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(outerScalarDistance) &&
                        EdgeSelectors[i].ResolveDistance(contourDistance) > EdgeSelectors[i].ResolveDistance(distance))
                    {
                        distance = contourDistance;
                    }
                }
            }
        }
        else if (outerScalarDistance <= 0 && Math.Abs(outerScalarDistance) < Math.Abs(innerScalarDistance))
        {
            distance = outerDistance;
            winding = -1;
            for (int i = 0; i < contourCount; ++i)
            {
                if (_windings[i] < 0)
                {
                    Color3D contourDistance = EdgeSelectors[i].Distance();
                    if (Math.Abs(EdgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(innerScalarDistance) &&
                        EdgeSelectors[i].ResolveDistance(contourDistance) < EdgeSelectors[i].ResolveDistance(distance))
                    {
                        distance = contourDistance;
                    }
                }
            }
        }
        else
        {
            return shapeDistance;
        }

        for (int i = 0; i < contourCount; ++i)
        {
            if (_windings[i] != winding)
            {
                Color3D contourDistance = EdgeSelectors[i].Distance();
                if (EdgeSelectors[i].ResolveDistance(contourDistance) * EdgeSelectors[i].ResolveDistance(distance) >= 0 &&
                    Math.Abs(EdgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(EdgeSelectors[i].ResolveDistance(distance)))
                {
                    distance = contourDistance;
                }
            }
        }

        if (EdgeSelector.ResolveDistance(distance) == EdgeSelector.ResolveDistance(shapeDistance))
            distance = shapeDistance;

        return distance;
    }
}
