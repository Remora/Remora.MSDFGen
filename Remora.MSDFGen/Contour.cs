//
//  SPDX-FileName: Contour.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Remora.MSDFGen;

public class Contour
{
    public List<EdgeSegment> Edges { get; private set; }

    public Contour()
    {
        Edges = new List<EdgeSegment>();
    }

    public void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        for (var i = 0; i < Edges.Count; i++)
        {
            Edges[i].GetBounds(ref left, ref bottom, ref right, ref top);
        }
    }

    public int Winding
    {
        get
        {
            if (Edges.Count == 0)
            {
                return 0;
            }

            double total = 0;

            if (Edges.Count == 1)
            {
                var a = Edges[0].GetPoint(0);
                var b = Edges[0].GetPoint(1 / 3f);
                var c = Edges[0].GetPoint(2 / 3f);

                total += Shoelace(a, b);
                total += Shoelace(b, c);
                total += Shoelace(c, a);
            }
            else if (Edges.Count == 2)
            {
                var a = Edges[0].GetPoint(0);
                var b = Edges[0].GetPoint(0.5f);
                var c = Edges[1].GetPoint(0);
                var d = Edges[1].GetPoint(0.5f);

                total += Shoelace(a, b);
                total += Shoelace(b, c);
                total += Shoelace(c, d);
                total += Shoelace(d, a);
            }
            else
            {
                var prev = Edges[Edges.Count - 1].GetPoint(0);
                for (var i = 0; i < Edges.Count; i++)
                {
                    var cur = Edges[i].GetPoint(0);
                    total += Shoelace(prev, cur);
                    prev = cur;
                }
            }

            return Math.Sign(total);
        }
    }

    double Shoelace(Vector2 a, Vector2 b)
    {
        return (b.X - a.X) * (a.Y + b.Y);
    }
}
