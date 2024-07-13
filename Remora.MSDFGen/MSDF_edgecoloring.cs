//
//  SPDX-FileName: MSDF_edgecoloring.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Remora.MSDFGen;

public static partial class MSDF
{
    private static bool IsCorner(Vector2 aDir, Vector2 bDir, double crossThreshold)
    {
        return Vector2.Dot(aDir, bDir) <= 0 || Math.Abs(EdgeSegment.Cross(aDir, bDir)) > crossThreshold;
    }

    private static EdgeColor[] switchColors = { EdgeColor.Cyan, EdgeColor.Magenta, EdgeColor.Yellow };

    private static void SwitchColor(ref EdgeColor color, ref ulong seed, EdgeColor banned)
    {
        var combined = color & banned;

        if (combined == EdgeColor.Red || combined == EdgeColor.Green || combined == EdgeColor.Blue)
        {
            color = combined ^ EdgeColor.White;
            return;
        }

        if (color == EdgeColor.Black || color == EdgeColor.White)
        {
            color = switchColors[seed % 3];
            seed /= 3;
            return;
        }

        var shifted = (int)color << (int)(1 + (seed & 1));
        color = (EdgeColor)((shifted | shifted >> 3) & (int)EdgeColor.White);
        seed >>= 1;
    }

    public static void EdgeColoringSimple(Shape shape, double angleThreshold, ulong seed)
    {
        var crossThreshold = Math.Sin(angleThreshold);
        var corners = new List<int>();
        for (var i = 0; i < shape.Contours.Count; i++)
        {
            var contour = shape.Contours[i];
            corners.Clear();

            if (!(contour.Edges.Count == 0))
            {
                var prevDirection = contour.Edges[^1].GetDirection(1);

                for (var j = 0; j < contour.Edges.Count; j++)
                {
                    var edge = contour.Edges[j];
                    if (IsCorner(
                            Vector2.Normalize(prevDirection),
                            Vector2.Normalize(edge.GetDirection(0)),
                            crossThreshold))
                    {
                        corners.Add(j);
                    }

                    prevDirection = edge.GetDirection(1);
                }
            }

            if (corners.Count == 0)
            {
                for (var j = 0; j < contour.Edges.Count; j++)
                {
                    contour.Edges[j].Color = EdgeColor.White;
                }
            }
            else if (corners.Count == 1)
            {
                EdgeColor[] colors = { EdgeColor.White, EdgeColor.White, EdgeColor.Black };
                SwitchColor(ref colors[0], ref seed, EdgeColor.Black);
                SwitchColor(ref colors[2], ref seed, EdgeColor.Black);

                var corner = corners[0];

                if (contour.Edges.Count >= 3)
                {
                    var m = contour.Edges.Count;
                    for (var j = 0; j < m; j++)
                    {
                        var magic = (int)(3 + 2.875f * j / (m - 1) - 1.4375f + 0.5f) -
                                    3; //see edge-coloring.cpp in the original msdfgen
                        contour.Edges[(corner + j) % m].Color = colors[1 + magic];
                    }
                }
                else if (contour.Edges.Count >= 1)
                {
                    EdgeSegment[] parts = new EdgeSegment[7];
                    contour.Edges[0].SplitInThirds(
                        out parts[0 + 3 * corner],
                        out parts[1 + 3 * corner],
                        out parts[2 + 3 * corner]);
                    if (contour.Edges.Count >= 2)
                    {
                        contour.Edges[1].SplitInThirds(
                            out parts[3 - 3 * corner],
                            out parts[4 - 3 * corner],
                            out parts[5 - 3 * corner]);
                        parts[0].Color = colors[0];
                        parts[1].Color = colors[0];
                        parts[2].Color = colors[1];
                        parts[3].Color = colors[1];
                        parts[4].Color = colors[2];
                        parts[5].Color = colors[2];
                    }
                    else
                    {
                        parts[0].Color = colors[0];
                        parts[1].Color = colors[1];
                        parts[2].Color = colors[2];
                    }

                    contour.Edges.Clear();
                    for (var j = 0; parts[j] != null; j++)
                    {
                        contour.Edges.Add(parts[j]);
                    }
                }
            }
            else
            {
                var cornerCount = corners.Count;
                var spline = 0;
                var start = corners[0];
                var m = contour.Edges.Count;
                var color = EdgeColor.White;
                SwitchColor(ref color, ref seed, EdgeColor.Black);
                var initialColor = color;
                for (var j = 0; j < m; j++)
                {
                    var index = (start + j) % m;
                    if (spline + 1 < cornerCount && corners[spline + 1] == index)
                    {
                        spline++;
                        SwitchColor(
                            ref color,
                            ref seed,
                            (EdgeColor)(((spline == cornerCount - 1) ? 1 : 0) * (int)initialColor));
                    }

                    contour.Edges[index].Color = color;
                }
            }
        }
    }
}