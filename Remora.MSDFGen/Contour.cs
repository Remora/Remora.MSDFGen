//
//  SPDX-FileName: Contour.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Remora.MSDFGen;

/// <summary>
/// Represents a contour in a glyph.
/// </summary>
public class Contour
{
    /// <summary>
    /// Gets the segments take makes up the contour.
    /// </summary>
    public List<EdgeSegment> Edges { get; } = new();

    /// <summary>
    /// Gets the winding direction of the contour.
    /// </summary>
    public int Winding
    {
        get
        {
            if (this.Edges.Count == 0)
            {
                return 0;
            }

            double total = 0;

            switch (this.Edges.Count)
            {
                case 1:
                {
                    var a = this.Edges[0].GetPoint(0);
                    var b = this.Edges[0].GetPoint(1 / 3f);
                    var c = this.Edges[0].GetPoint(2 / 3f);

                    total += Shoelace(a, b);
                    total += Shoelace(b, c);
                    total += Shoelace(c, a);
                    break;
                }
                case 2:
                {
                    var a = this.Edges[0].GetPoint(0);
                    var b = this.Edges[0].GetPoint(0.5f);
                    var c = this.Edges[1].GetPoint(0);
                    var d = this.Edges[1].GetPoint(0.5f);

                    total += Shoelace(a, b);
                    total += Shoelace(b, c);
                    total += Shoelace(c, d);
                    total += Shoelace(d, a);
                    break;
                }
                default:
                {
                    var prev = this.Edges[^1].GetPoint(0);
                    foreach (var edge in this.Edges)
                    {
                        var cur = edge.GetPoint(0);
                        total += Shoelace(prev, cur);
                        prev = cur;
                    }

                    break;
                }
            }

            return Math.Sign(total);
        }
    }

    /// <summary>
    /// Calculates the bounding box of the contour.
    /// </summary>
    /// <param name="left">The left limit of the contour.</param>
    /// <param name="bottom">The bottom limit of the contour.</param>
    /// <param name="right">The right limit of the contour.</param>
    /// <param name="top">The top limit of the contour.</param>
    public void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        foreach (var edge in this.Edges)
        {
            edge.GetBounds(ref left, ref bottom, ref right, ref top);
        }
    }

    private static double Shoelace(Vector2 a, Vector2 b)
    {
        return (b.X - a.X) * (a.Y + b.Y);
    }
}
