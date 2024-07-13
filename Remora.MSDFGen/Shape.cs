//
//  SPDX-FileName: Shape.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System.Collections.Generic;
using System.Numerics;

namespace Remora.MSDFGen;

public class Shape
{
    public List<Contour> Contours { get; private set; }

    public bool InverseYAxis { get; set; }

    public Shape()
    {
        Contours = new List<Contour>();
    }

    public bool Validate()
    {
        for (var i = 0; i < Contours.Count; i++)
        {
            var contour = Contours[i];
            if (contour.Edges.Count > 0)
            {
                var corner = contour.Edges[contour.Edges.Count - 1].GetPoint(1);
                for (var j = 0; j < contour.Edges.Count; j++)
                {
                    var edge = contour.Edges[j];
                    if (edge == null)
                    {
                        return false;
                    }

                    if (edge.GetPoint(0) != corner)
                    {
                        return false;
                    }

                    corner = edge.GetPoint(1);
                }
            }
        }

        return true;
    }

    public void Normalize()
    {
        for (var i = 0; i < Contours.Count; i++)
        {
            var contour = Contours[i];
            if (contour.Edges.Count == 1)
            {
                EdgeSegment e1, e2, e3;
                contour.Edges[0].SplitInThirds(out e1, out e2, out e3);
                contour.Edges.Clear();
                contour.Edges.Add(e1);
                contour.Edges.Add(e2);
                contour.Edges.Add(e3);
            }
        }
    }

    public void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        for (var i = 0; i < Contours.Count; i++)
        {
            Contours[i].GetBounds(ref left, ref bottom, ref right, ref top);
        }
    }
}