//
//  SPDX-FileName: Shape.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System.Collections.Generic;

namespace Remora.MSDFGen;

/// <summary>
/// Represents a shape made up of multiple contours. Multiple contours combine to form a single glyph.
/// </summary>
public class Shape
{
    /// <summary>
    /// Gets the contours in the shape.
    /// </summary>
    public List<Contour> Contours { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the Y-axis of the shape is inverted.
    /// </summary>
    public bool InverseYAxis { get; set; }

    /// <summary>
    /// Validates the shape, ensuring it is... something.
    /// </summary>
    /// <returns>true if the shape is valid; otherwise, false.</returns>
    public bool Validate()
    {
        foreach (var contour in this.Contours)
        {
            if (contour.Edges.Count <= 0)
            {
                continue;
            }

            var corner = contour.Edges[^1].GetPoint(1);
            foreach (var edge in contour.Edges)
            {
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

        return true;
    }

    /// <summary>
    /// Normalizes the shape, doing... something.
    /// </summary>
    public void Normalize()
    {
        foreach (var contour in this.Contours)
        {
            if (contour.Edges.Count != 1)
            {
                continue;
            }

            contour.Edges[0].SplitInThirds(out var e1, out var e2, out var e3);
            contour.Edges.Clear();
            contour.Edges.Add(e1);
            contour.Edges.Add(e2);
            contour.Edges.Add(e3);
        }
    }

    /// <summary>
    /// Calculates the bounding box of the shape.
    /// </summary>
    /// <param name="left">The left limit of the shape.</param>
    /// <param name="bottom">The bottom limit of the shape.</param>
    /// <param name="right">The right limit of the shape.</param>
    /// <param name="top">The top limit of the shape.</param>
    public void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        foreach (var contour in this.Contours)
        {
            contour.GetBounds(ref left, ref bottom, ref right, ref top);
        }
    }
}
