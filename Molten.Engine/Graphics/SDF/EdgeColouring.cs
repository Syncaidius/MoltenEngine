using System.Collections;
using Molten.DoublePrecision;
using Molten.Shapes;

namespace Molten.Graphics.SDF
{
    internal class EdgeColouring
    {
        internal unsafe class DoublePtrComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                IntPtr a = (IntPtr)x;
                IntPtr b = (IntPtr)y;
                return Math.Sign((double*)a.ToPointer() - (double*)b.ToPointer());
            }
        }

        class EdgeColoringInkTrapCorner
        {
            public int index;
            public double prevEdgeLengthEstimate;
            public bool minor;
            public EdgeColor color;

            public EdgeColoringInkTrapCorner()
            {
                index = 0; prevEdgeLengthEstimate = 0; minor = false;
                color = EdgeColor.Black;
            }

            public EdgeColoringInkTrapCorner(int i, double len)
            {
                index = i;
                prevEdgeLengthEstimate = len;
                minor = false;
                color = EdgeColor.Black;
            }
        }

        public const int MSDFGEN_EDGE_LENGTH_PRECISION = 4;
        public const int MAX_RECOLOR_STEPS = 16;
        public const int EDGE_DISTANCE_PRECISION = 16;
        static int[] FIRST_POSSIBLE_COLOR = { -1, 0, 1, 0, 2, 2, 1, 0 };

        static DoublePtrComparer _doubleComparer = new DoublePtrComparer();

        static bool IsCorner(Vector2D aDir, Vector2D bDir, double crossThreshold)
        {
            return Vector2D.Dot(ref aDir, ref bDir) <= 0 || Math.Abs(Vector2D.Cross(ref aDir, ref bDir)) > crossThreshold;
        }

        static double EstimateEdgeLength(Edge edge)
        {
            double len = 0;
            Vector2D prev = edge.Point(0);
            for (int i = 1; i <= MSDFGEN_EDGE_LENGTH_PRECISION; ++i)
            {
                Vector2D cur = edge.Point(1.0 / MSDFGEN_EDGE_LENGTH_PRECISION * i);
                len += (cur - prev).Length();
                prev = cur;
            }
            return len;
        }

        static unsafe void SwitchColor(ref EdgeColor color, ref int seed, EdgeColor banned = EdgeColor.Black)
        {
            EdgeColor combined = color & banned;
            if (combined == EdgeColor.Red || combined == EdgeColor.Green || combined == EdgeColor.Blue)
            {
                color = combined ^ EdgeColor.White;
                return;
            }
            if (color == EdgeColor.Black || color == EdgeColor.White)
            {
                EdgeColor* start = stackalloc[] { EdgeColor.Cyan, EdgeColor.Magenta, EdgeColor.Yellow };
                color = start[seed % 3];
                seed /= 3;
                return;
            }
            int shifted = (int)color << (1 + (seed & 1));
            color = (EdgeColor)(shifted | shifted >> 3) & EdgeColor.White;
            seed >>= 1;
        }

        public static unsafe void EdgeColoringSimple(Shape shape, double angleThreshold, int seed)
        {
            double crossThreshold = Math.Sin(angleThreshold);
            List<int> corners = new List<int>();

            foreach (Contour contour in shape.Contours)
            {
                // Identify corners
                corners.Clear();
                if (contour.Edges.Count > 0)
                {
                    Vector2D prevDirection = contour.Edges.Last().GetDirection(1);
                    int index = 0;
                    foreach (Edge edge in contour.Edges)
                    {
                        if (IsCorner(prevDirection.GetNormalized(), edge.GetDirection(0).GetNormalized(), crossThreshold))
                            corners.Add(index);
                        prevDirection = edge.GetDirection(1);
                    }
                }

                // Smooth contour
                if (corners.Count == 0)
                {
                    foreach (Edge edge in contour.Edges)
                        edge.Color = EdgeColor.White;
                }
                else if (corners.Count == 1) // "Teardrop" case
                {
                    EdgeColor* colors = stackalloc[] { EdgeColor.White, EdgeColor.White, EdgeColor.Black };

                    SwitchColor(ref colors[0], ref seed);
                    colors[2] = colors[0];
                    SwitchColor(ref colors[2], ref seed);
                    int corner = corners[0];
                    if (contour.Edges.Count >= 3)
                    {
                        int m = contour.Edges.Count;
                        for (int i = 0; i < m; ++i)
                            contour.Edges[(corner + i) % m].Color = (colors + 1)[(int)(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3];
                    }
                    else if (contour.Edges.Count >= 1)
                    {
                        // Less than three edge segments for three colors => edges must be split
                        Edge[] parts = new Edge[7];
                        contour.Edges[0].SplitInThirds(ref parts[0 + 3 * corner], ref parts[1 + 3 * corner], ref parts[2 + 3 * corner]);
                        if (contour.Edges.Count >= 2)
                        {
                            contour.Edges[1].SplitInThirds(ref parts[3 - 3 * corner], ref parts[4 - 3 * corner], ref parts[5 - 3 * corner]);
                            parts[0].Color = parts[1].Color = colors[0];
                            parts[2].Color = parts[3].Color = colors[1];
                            parts[4].Color = parts[5].Color = colors[2];
                        }
                        else
                        {
                            parts[0].Color = colors[0];
                            parts[1].Color = colors[1];
                            parts[2].Color = colors[2];
                        }
                        contour.Edges.Clear();
                        for (int i = 0; i < parts.Length && parts[i] != null; ++i)
                            contour.Edges.Add(parts[i]);
                    }
                }
                // Multiple corners
                else
                {
                    int cornerCount = corners.Count;
                    int spline = 0;
                    int start = corners[0];
                    int m = contour.Edges.Count;
                    EdgeColor color = EdgeColor.White;
                    SwitchColor(ref color, ref seed);
                    EdgeColor initialColor = color;
                    for (int i = 0; i < m; ++i)
                    {
                        int index = (start + i) % m;
                        if (spline + 1 < cornerCount && corners[spline + 1] == index)
                        {
                            ++spline;
                            SwitchColor(ref color, ref seed, (EdgeColor)((spline == cornerCount - 1 ? 1 : 0) * (int)initialColor));
                        }
                        contour.Edges[index].Color = color;
                    }
                }
            }
        }

        public static unsafe void EdgeColoringInkTrap(Shape shape, double angleThreshold, int seed)
        {
            double crossThreshold = Math.Sin(angleThreshold);
            List<EdgeColoringInkTrapCorner> corners = new List<EdgeColoringInkTrapCorner>();

            foreach (Contour contour in shape.Contours)
            {
                // Identify corners
                double splineLength = 0;
                corners.Clear();

                if (contour.Edges.Count > 0)
                {
                    Vector2D prevDirection = contour.Edges.Last().GetDirection(1);
                    int index = 0;
                    foreach (Edge edge in contour.Edges)
                    {
                        if (IsCorner(prevDirection.GetNormalized(), edge.GetDirection(0).GetNormalized(), crossThreshold))
                        {
                            EdgeColoringInkTrapCorner corner = new EdgeColoringInkTrapCorner(index, splineLength);
                            corners.Add(corner);
                            splineLength = 0;
                        }

                        splineLength += EstimateEdgeLength(edge);
                        prevDirection = edge.GetDirection(1);
                    }
                }

                // Smooth contour
                if (corners.Count == 0)
                {
                    foreach (Edge edge in contour.Edges)
                        edge.Color = EdgeColor.White;
                }
                else if (corners.Count == 1) // "Teardrop" case
                {
                    EdgeColor* colors = stackalloc[] { EdgeColor.White, EdgeColor.White, EdgeColor.Black };

                    SwitchColor(ref colors[0], ref seed);
                    colors[2] = colors[0];
                    SwitchColor(ref colors[2], ref seed);

                    int corner = corners[0].index;
                    if (contour.Edges.Count >= 3)
                    {
                        int m = contour.Edges.Count;
                        for (int i = 0; i < m; ++i)
                            contour.Edges[(corner + i) % m].Color = (colors + 1)[(int)(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3];
                    }
                    else if (contour.Edges.Count >= 1)
                    {
                        // Less than three edge segments for three colors => edges must be split
                        Edge[] parts = new Edge[7];
                        contour.Edges[0].SplitInThirds(ref parts[0 + 3 * corner], ref parts[1 + 3 * corner], ref parts[2 + 3 * corner]);
                        if (contour.Edges.Count >= 2)
                        {
                            contour.Edges[1].SplitInThirds(ref parts[3 - 3 * corner], ref parts[4 - 3 * corner], ref parts[5 - 3 * corner]);
                            parts[0].Color = parts[1].Color = colors[0];
                            parts[2].Color = parts[3].Color = colors[1];
                            parts[4].Color = parts[5].Color = colors[2];
                        }
                        else
                        {
                            parts[0].Color = colors[0];
                            parts[1].Color = colors[1];
                            parts[2].Color = colors[2];
                        }
                        contour.Edges.Clear();
                        for (int i = 0; i < parts.Length && parts[i] != null; ++i)
                            contour.Edges.Add(parts[i]);
                    }
                }
                // Multiple corners
                else
                {
                    int cornerCount = corners.Count;
                    int majorCornerCount = cornerCount;
                    if (cornerCount > 3)
                    {
                        corners.First().prevEdgeLengthEstimate += splineLength;
                        for (int i = 0; i < cornerCount; ++i)
                        {
                            if (
                                corners[i].prevEdgeLengthEstimate > corners[(i + 1) % cornerCount].prevEdgeLengthEstimate &&
                                corners[(i + 1) % cornerCount].prevEdgeLengthEstimate < corners[(i + 2) % cornerCount].prevEdgeLengthEstimate
                            )
                            {
                                corners[i].minor = true;
                                --majorCornerCount;
                            }
                        }
                    }
                    EdgeColor color = EdgeColor.White;
                    EdgeColor initialColor = EdgeColor.Black;

                    for (int i = 0; i < cornerCount; ++i)
                    {
                        if (!corners[i].minor)
                        {
                            --majorCornerCount;
                            SwitchColor(ref color, ref seed, (EdgeColor)(majorCornerCount == 0 ? 1 : 0 * (int)initialColor));
                            corners[i].color = color;
                            if (initialColor == 0)
                                initialColor = color;
                        }
                    }

                    for (int i = 0; i < cornerCount; ++i)
                    {
                        if (corners[i].minor)
                        {
                            EdgeColor nextColor = corners[(i + 1) % cornerCount].color;
                            corners[i].color = ((color & nextColor) ^ EdgeColor.White);
                        }
                        else
                            color = corners[i].color;
                    }

                    int spline = 0;
                    int start = corners[0].index;
                    color = corners[0].color;
                    int m = contour.Edges.Count;

                    for (int i = 0; i < m; ++i)
                    {
                        int index = (start + i) % m;
                        if (spline + 1 < cornerCount && corners[spline + 1].index == index)
                            color = corners[++spline].color;
                        contour.Edges[index].Color = color;
                    }
                }
            }
        }

        /// <summary>
        /// EDGE COLORING BY DISTANCE - EXPERIMENTAL IMPLEMENTATION - WORK IN PROGRESS
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        static double EdgeToEdgeDistance(Edge a, Edge b, int precision)
        {
            if (a.Point(0) == b.Point(0) || a.Point(0) == b.Point(1) || a.Point(1) == b.Point(0) || a.Point(1) == b.Point(1))
                return 0;
            double iFac = 1.0 / precision;
            double minDistance = (b.Point(0) - a.Point(0)).Length();
            for (int i = 0; i <= precision; ++i)
            {
                double t = iFac * i;
                double d = Math.Abs(a.SignedDistance(b.Point(t), out t).Distance);
                minDistance = Math.Min(minDistance, d);
            }
            for (int i = 0; i <= precision; ++i)
            {
                double t = iFac * i;
                double d = Math.Abs(b.SignedDistance(a.Point(t), out t).Distance);
                minDistance = Math.Min(minDistance, d);
            }
            return minDistance;
        }

        static double SplineToSplineDistance(List<Edge> edgeSegments, int aStart, int aEnd, int bStart, int bEnd, int precision)
        {
            double minDistance = double.MaxValue;
            for (int ai = aStart; ai < aEnd; ++ai)
            {
                for (int bi = bStart; bi < bEnd && minDistance != 0; ++bi)
                {
                    double d = EdgeToEdgeDistance(edgeSegments[ai], edgeSegments[bi], precision);
                    minDistance = Math.Min(minDistance, d);
                }
            }
            return minDistance;
        }

        static unsafe void ColorSecondDegreeGraph(int* coloring, int** edgeMatrix, int vertexCount, int seed)
        {
            for (int i = 0; i < vertexCount; ++i)
            {
                int possibleColors = 7;
                for (int j = 0; j < i; ++j)
                {
                    if (edgeMatrix[i][j] != 0)
                        possibleColors &= ~(1 << coloring[j]);
                }
                int color = 0;
                switch (possibleColors)
                {
                    case 1:
                        color = 0;
                        break;
                    case 2:
                        color = 1;
                        break;
                    case 3:
                        color = seed & 1;
                        seed >>= 1;
                        break;
                    case 4:
                        color = 2;
                        break;
                    case 5:
                        color = (seed + 1 & 1) << 1;
                        seed >>= 1;
                        break;
                    case 6:
                        color = (seed & 1) + 1;
                        seed >>= 1;
                        break;
                    case 7:
                        color = (seed + i) % 3;
                        seed /= 3;
                        break;
                }
                coloring[i] = color;
            }
        }

        static unsafe int VertexPossibleColors(int* coloring, int* edgeVector, int vertexCount)
        {
            int usedColors = 0;
            for (int i = 0; i < vertexCount; ++i)
                if (edgeVector[i] != 0)
                    usedColors |= 1 << coloring[i];
            return 7 & ~usedColors;
        }

        static unsafe void UncolorSameNeighbors(Queue<int> uncolored, int* coloring, int** edgeMatrix, int vertex, int vertexCount)
        {
            for (int i = vertex + 1; i < vertexCount; ++i)
            {
                if (edgeMatrix[vertex][i] != 0 && coloring[i] == coloring[vertex])
                {
                    coloring[i] = -1;
                    uncolored.Enqueue(i);
                }
            }
            for (int i = 0; i < vertex; ++i)
            {
                if (edgeMatrix[vertex][i] != 0 && coloring[i] == coloring[vertex])
                {
                    coloring[i] = -1;
                    uncolored.Enqueue(i);
                }
            }
        }

        static unsafe bool TryAddEdge(int* coloring, int** edgeMatrix, int vertexCount, int vertexA, int vertexB, int* coloringBuffer)
        {
            edgeMatrix[vertexA][vertexB] = 1;
            edgeMatrix[vertexB][vertexA] = 1;
            if (coloring[vertexA] != coloring[vertexB])
                return true;
            int bPossibleColors = VertexPossibleColors(coloring, edgeMatrix[vertexB], vertexCount);
            if (bPossibleColors != 0)
            {
                coloring[vertexB] = FIRST_POSSIBLE_COLOR[bPossibleColors];
                return true;
            }

            Buffer.MemoryCopy(coloring, coloringBuffer, sizeof(int) * vertexCount, sizeof(int) * vertexCount);


            Queue<int> uncolored = new Queue<int>();
            {
                int* subColoring = coloringBuffer;
                subColoring[vertexB] = FIRST_POSSIBLE_COLOR[7 & ~(1 << subColoring[vertexA])];
                UncolorSameNeighbors(uncolored, subColoring, edgeMatrix, vertexB, vertexCount);
                int step = 0;
                while (uncolored.Count == 0 && step < MAX_RECOLOR_STEPS)
                {
                    int i = uncolored.Dequeue();
                    int possibleColors = VertexPossibleColors(subColoring, edgeMatrix[i], vertexCount);
                    if (possibleColors != 0)
                    {
                        subColoring[i] = FIRST_POSSIBLE_COLOR[possibleColors];
                        continue;
                    }
                    do
                    {
                        subColoring[i] = step++ % 3;
                    } while (edgeMatrix[i][vertexA] != 0 && subColoring[i] == subColoring[vertexA]);
                    UncolorSameNeighbors(uncolored, subColoring, edgeMatrix, i, vertexCount);
                }
            }
            if (uncolored.Count != 0)
            {
                edgeMatrix[vertexA][vertexB] = 0;
                edgeMatrix[vertexB][vertexA] = 0;
                return false;
            }
            Buffer.MemoryCopy(coloringBuffer, coloring, sizeof(int) * vertexCount, sizeof(int) * vertexCount);
            return true;
        }

        public static unsafe void EdgeColoringByDistance(Shape shape, double angleThreshold, int seed)
        {
            List<Edge> edgeSegments = new List<Edge>();
            List<int> splineStarts = new List<int>();

            double crossThreshold = Math.Sin(angleThreshold);
            List<int> corners = new List<int>();
            foreach (Contour contour in shape.Contours)
                if (contour.Edges.Count != 0)
                {
                    // Identify corners
                    corners.Clear();
                    Vector2D prevDirection = contour.Edges.Last().GetDirection(1);
                    int index = 0;
                    foreach (Edge edge in contour.Edges)
                    {
                        if (IsCorner(prevDirection.GetNormalized(), edge.GetDirection(0).GetNormalized(), crossThreshold))
                            corners.Add(index);
                        prevDirection = edge.GetDirection(1);
                    }

                    splineStarts.Add(edgeSegments.Count);

                    // Smooth contour
                    if (corners.Count == 0)
                    {
                        foreach (Edge edge in contour.Edges)
                            edgeSegments.Add(edge);
                    }
                    else if (corners.Count == 1) // "Teardrop" case
                    {
                        int corner = corners[0];
                        if (contour.Edges.Count >= 3)
                        {
                            int m = contour.Edges.Count;
                            for (int i = 0; i < m; ++i)
                            {
                                if (i == m / 2)
                                    splineStarts.Add(edgeSegments.Count);
                                if ((int)(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3 != 0)
                                    edgeSegments.Add(contour.Edges[(corner + i) % m]);
                                else
                                    contour.Edges[(corner + i) % m].Color = EdgeColor.White;
                            }
                        }
                        else if (contour.Edges.Count >= 1)
                        {
                            // Less than three edge segments for three colors => edges must be split
                            Edge[] parts = new Edge[7];
                            contour.Edges[0].SplitInThirds(ref parts[0 + 3 * corner], ref parts[1 + 3 * corner], ref parts[2 + 3 * corner]);
                            if (contour.Edges.Count >= 2)
                            {
                                contour.Edges[1].SplitInThirds(ref parts[3 - 3 * corner], ref parts[4 - 3 * corner], ref parts[5 - 3 * corner]);
                                edgeSegments.Add(parts[0]);
                                edgeSegments.Add(parts[1]);
                                parts[2].Color = parts[3].Color = EdgeColor.White;
                                splineStarts.Add(edgeSegments.Count);
                                edgeSegments.Add(parts[4]);
                                edgeSegments.Add(parts[5]);
                            }
                            else
                            {
                                edgeSegments.Add(parts[0]);
                                parts[1].Color = EdgeColor.White;
                                splineStarts.Add(edgeSegments.Count);
                                edgeSegments.Add(parts[2]);
                            }
                            contour.Edges.Clear();
                            for (int i = 0; i < parts.Length && parts[i] != null; ++i)
                                contour.Edges.Add(parts[i]);
                        }
                    }
                    else // Multiple corners
                    {
                        int cornerCount = corners.Count;
                        int subSpline = 0;
                        int start = corners[0];
                        int m = contour.Edges.Count;

                        for (int i = 0; i < m; ++i)
                        {
                            int subIndex = (start + i) % m;
                            if (subSpline + 1 < cornerCount && corners[subSpline + 1] == subIndex)
                            {
                                splineStarts.Add(edgeSegments.Count);
                                ++subSpline;
                            }
                            edgeSegments.Add(contour.Edges[subIndex]);
                        }
                    }
                }
            splineStarts.Add(edgeSegments.Count);

            int segmentCount = edgeSegments.Count;
            int splineCount = splineStarts.Count - 1;
            if (splineCount == 0)
                return;

            double* distanceMatrixStorage = stackalloc double[splineCount * splineCount];
            double** distanceMatrix = stackalloc double*[splineCount];
            double* distanceMatrixBase = &distanceMatrixStorage[0];

            for (int i = 0; i < splineCount; ++i)
                distanceMatrix[i] = &distanceMatrixStorage[i * splineCount];

            for (int i = 0; i < splineCount; ++i)
            {
                distanceMatrix[i][i] = -1;
                for (int j = i + 1; j < splineCount; ++j)
                {
                    double dist = SplineToSplineDistance(edgeSegments, splineStarts[i], splineStarts[i + 1], splineStarts[j], splineStarts[j + 1], EDGE_DISTANCE_PRECISION);
                    distanceMatrix[i][j] = dist;
                    distanceMatrix[j][i] = dist;
                }
            }

            double*[] graphEdgeDistances = new double*[splineCount * (splineCount - 1) / 2];
            int graphEdgePos = 0;

            for (int i = 0; i < splineCount; ++i)
            {
                for (int j = i + 1; j < splineCount; ++j)
                    graphEdgeDistances[graphEdgePos++] = &distanceMatrix[i][j];
            }

            if (graphEdgePos > 0)
                Array.Sort(graphEdgeDistances, 0, graphEdgePos, _doubleComparer);

            int* edgeMatrixStorage = stackalloc int[splineCount * splineCount];
            int** edgeMatrix = stackalloc int*[splineCount];

            for (int i = 0; i < splineCount; ++i)
                edgeMatrix[i] = &edgeMatrixStorage[i * splineCount];

            int nextEdge = 0;
            for (; nextEdge < graphEdgePos && graphEdgeDistances[nextEdge] == null; ++nextEdge)
            {
                int elem = (int)(graphEdgeDistances[nextEdge] - distanceMatrixBase);
                int row = elem / splineCount;
                int col = elem % splineCount;
                edgeMatrix[row][col] = 1;
                edgeMatrix[col][row] = 1;
            }

            int* coloring = stackalloc int[2 * splineCount];
            ColorSecondDegreeGraph(&coloring[0], &edgeMatrix[0], splineCount, seed);
            for (; nextEdge < graphEdgePos; ++nextEdge)
            {
                int elem = (int)(graphEdgeDistances[nextEdge] - distanceMatrixBase);
                TryAddEdge(&coloring[0], &edgeMatrix[0], splineCount, elem / splineCount, elem % splineCount, &coloring[splineCount]);
            }

            EdgeColor* colors = stackalloc[] { EdgeColor.Yellow, EdgeColor.Cyan, EdgeColor.Magenta };
            int spline = -1;
            for (int i = 0; i < segmentCount; ++i)
            {
                if (splineStarts[spline + 1] == i)
                    ++spline;
                edgeSegments[i].Color = colors[coloring[spline]];
            }
        }

        public static void ParseColoring(Shape shape, string edgeAssignment = "cmwyCMWY")
        {
            int c = 0, e = 0;
            if (string.IsNullOrEmpty(edgeAssignment)) 
                return;

            Contour contour = shape.Contours[c];
            bool change = false;
            bool clear = true;

            foreach (char cin in edgeAssignment)
            {
                switch (cin)
                {
                    case ',':
                        if (change)
                            ++e;

                        if (clear)
                        {
                            while (e < contour.Edges.Count)
                            {
                                contour.Edges[e].Color = EdgeColor.White;
                                ++e;
                            }
                        }

                        ++c; 
                        e = 0;

                        if (shape.Contours.Count <= c)
                            return;

                        contour = shape.Contours[c];
                        change = false;
                        clear = true;
                        break;

                    case '?':
                        clear = false;
                        break;

                    case 'C':
                    case 'M':
                    case 'W':
                    case 'Y':
                    case 'c':
                    case 'm':
                    case 'w':
                    case 'y':
                        if (change)
                        {
                            ++e;
                            change = false;
                        }

                        if (e < contour.Edges.Count)
                        {
                            contour.Edges[e].Color = (EdgeColor)(
                                ((cin == 'C' || cin == 'c') ? 1 : 0) * (int)EdgeColor.Cyan |
                                ((cin == 'M' || cin == 'm') ? 1 : 0) * (int)EdgeColor.Magenta |
                                ((cin == 'Y' || cin == 'y') ? 1 : 0) * (int)EdgeColor.Yellow |
                                ((cin == 'W' || cin == 'w') ? 1 : 0) * (int)EdgeColor.White);
                            change = true;
                        }
                        break;
                }
            }
        }
    }
}
