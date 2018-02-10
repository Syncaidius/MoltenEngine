//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port)
//MIT, 2018, James Yarwood (Adapted for Molten Engine)

using System;
using System.Collections.Generic; 

namespace Msdfgen
{
    public static class EdgeColoring
    {
        static bool isCorner(Vector2 aDir, Vector2 bDir, double crossThreshold)
        {
            return Vector2.Dot(aDir, bDir) <= 0 || Math.Abs(Vector2.Cross(aDir, bDir)) > crossThreshold;
        }

        public static void edgeColoringSimple(Shape shape, double angleThreshold)
        {
            double crossThreshold = Math.Sin(angleThreshold);
            List<int> corners = new List<int>();

            // for (std::vector<Contour>::iterator contour = shape.contours.begin(); contour != shape.contours.end(); ++contour)
            foreach (Contour contour in shape.contours)
            {
                // Identify corners 
                corners.Clear();
                List<EdgeHolder> edges = contour.Edges;
                int edgeCount = edges.Count;
                if (edgeCount != 0)
                {
                    Vector2 prevDirection = edges[edgeCount - 1].Direction(1);// (*(contour->edges.end() - 1))->direction(1); 
                    for (int i = 0; i < edgeCount; ++i)
                    {
                        EdgeHolder edge = edges[i];
                        if (isCorner(prevDirection.Normalize(),
                            edge.Direction(0).Normalize(), crossThreshold))
                        {
                            corners.Add(i);
                        }
                        prevDirection = edge.Direction(1);
                    }
                }

                // Smooth contour
                if (corners.Count == 0) //is empty
                {
                    for (int i = edgeCount - 1; i >= 0; --i)
                    {
                        edges[i].color = EdgeColor.WHITE;
                    }

                }
                else if (corners.Count == 1)
                {
                    // "Teardrop" case
                    EdgeColor[] colors = { EdgeColor.MAGENTA, EdgeColor.WHITE, EdgeColor.YELLOW };
                    int corner = corners[0];
                    if (edgeCount >= 3)
                    {
                        int m = edgeCount;
                        for (int i = 0; i < m; ++i)
                        {
                            //TODO: review here 
                            contour.Edges[(corner + i) % m].color = colors[((int)(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3) + 1];
                            //(colors + 1)[int(3 + 2.875 * i / (m - 1) - 1.4375 + .5) - 3];
                        }
                    }
                    else if (edgeCount >= 1)
                    {
                        // Less than three edge segments for three colors => edges must be split
                        EdgeSegment[] parts = new EdgeSegment[7]; //empty array
                        edges[0].edgeSegment.splitInThirds(
                            out parts[0 + 3 * corner],
                            out parts[1 + 3 * corner],
                            out parts[2 + 3 * corner]);

                        if (edgeCount >= 2)
                        {
                            edges[1].edgeSegment.splitInThirds(
                                out parts[3 - 3 * corner],
                                out parts[4 - 3 * corner],
                                out parts[5 - 3 * corner]
                                );
                            parts[0].color = parts[1].color = colors[0];
                            parts[2].color = parts[3].color = colors[1];
                            parts[4].color = parts[5].color = colors[2];
                        }
                        else
                        {
                            parts[0].color = colors[0];
                            parts[1].color = colors[1];
                            parts[2].color = colors[2];
                        }
                        contour.Edges.Clear();
                        for (int i = 0; i < 7; ++i)
                        {
                            edges.Add(new EdgeHolder(parts[i]));
                        }
                    }
                }
                // Multiple corners
                else
                {
                    int cornerCount = corners.Count;
                    // CMYCMYCMYCMY / YMYCMYC if corner count % 3 == 1
                    EdgeColor[] colors = { cornerCount % 3 == 1 ? EdgeColor.YELLOW : EdgeColor.CYAN, EdgeColor.CYAN, EdgeColor.MAGENTA, EdgeColor.YELLOW };
                    int spline = 0;
                    int start = corners[0];
                    int m = contour.Edges.Count;
                    for (int i = 0; i < m; ++i)
                    {
                        int index = (start + i) % m;
                        if (cornerCount > spline + 1 && corners[spline + 1] == index)
                        {
                            ++spline;
                        }

                        int tmp = (spline % 3 - ((spline == 0) ? 1 : 0));
                        edges[index].color = colors[tmp + 1];
                        //contour->edges[index]->color = (colors + 1)[spline % 3 - !spline];
                    }
                }
            }
        }

    }
}
