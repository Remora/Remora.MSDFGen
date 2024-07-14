//
//  SPDX-FileName: EdgeSegment.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Numerics;
using Remora.MSDFGen.Extensions;

namespace Remora.MSDFGen;

/// <summary>
/// Represents a segment in an edge.
/// </summary>
public abstract class EdgeSegment
{
    /// <summary>
    /// Gets or sets the color of the segment.
    /// </summary>
    public EdgeColor Color { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EdgeSegment"/> class.
    /// </summary>
    /// <param name="color">The color of the segment.</param>
    protected EdgeSegment(EdgeColor color)
    {
        this.Color = color;
    }

    /// <summary>
    /// Clones the segment, producing a new identical copy.
    /// </summary>
    /// <returns>The clone.</returns>
    public abstract EdgeSegment Clone();

    /// <summary>
    /// Gets a point along the segment at the given normalized distance from the segment's starting point.
    /// </summary>
    /// <param name="normalizedEdgeDistance">The normalized distance.</param>
    /// <returns>The point.</returns>
    public abstract Vector2 GetPoint(double normalizedEdgeDistance);

    /// <summary>
    /// Gets the direction vector of a point along the segment at the given normalized distanced from the segment's
    /// starting point.
    /// </summary>
    /// <param name="normalizedEdgeDistance">The normalized distance.</param>
    /// <returns>The direction vector.</returns>
    public abstract Vector2 GetDirection(double normalizedEdgeDistance);

    /// <summary>
    /// Calculates the signed distance from the given origin to the segment, producing the signed distance at the origin
    /// to the segment, along with the normalized distance along the segment from the segment's starting point of the
    /// closest point on the segment.
    /// </summary>
    /// <param name="origin">The origin of the measurement.</param>
    /// <param name="normalizedEdgeDistance">The normalized distance.</param>
    /// <returns>The signed distance at the origin.</returns>
    public abstract SignedDistance GetSignedDistance(Vector2 origin, out double normalizedEdgeDistance);

    /// <summary>
    /// Calculates the bounding box of the segment.
    /// </summary>
    /// <param name="left">The left limit of the segment.</param>
    /// <param name="bottom">The bottom limit of the segment.</param>
    /// <param name="right">The right limit of the segment.</param>
    /// <param name="top">The top limit of the segment.</param>
    public abstract void GetBounds(ref double left, ref double bottom, ref double right, ref double top);

    /// <summary>
    /// Moves the starting point of the segment.
    /// </summary>
    /// <param name="newStart">The new starting point.</param>
    public abstract void MoveStartPoint(Vector2 newStart);

    /// <summary>
    /// Moves the end point of the segment.
    /// </summary>
    /// <param name="newEnd">The new end point.</param>
    public abstract void MoveEndPoint(Vector2 newEnd);

    /// <summary>
    /// Splits the segment into thirds, producing new segments for each part.
    /// </summary>
    /// <param name="first">The first part.</param>
    /// <param name="second">The second part.</param>
    /// <param name="third">The third part.</param>
    public abstract void SplitInThirds(out EdgeSegment first, out EdgeSegment second, out EdgeSegment third);

    /// <summary>
    /// Converts the given signed distance to a pseudo-distance from the origin to a point along the path the segment
    /// would have traced, had it continued outside its bounds. This function only modifies the distance for values
    /// of <paramref name="normalizedEdgeDistance"/> below zero or above 1.
    /// </summary>
    /// <param name="distance">The distance to convert.</param>
    /// <param name="origin">The origin point to measure from.</param>
    /// <param name="normalizedEdgeDistance">The normalized edge distance.</param>
    public void DistanceToPseudoDistance(ref SignedDistance distance, Vector2 origin, double normalizedEdgeDistance)
    {
        switch (normalizedEdgeDistance)
        {
            // before the start point
            case < 0:
            {
                var startDirection = Vector2.Normalize(GetDirection(0));
                var toStart = origin - GetPoint(0);
                double ts = Vector2.Dot(toStart, startDirection);

                if (ts < 0)
                {
                    var pseudoDistance = toStart.Cross(startDirection);
                    if (Math.Abs(pseudoDistance) <= Math.Abs(distance.Distance))
                    {
                        distance.Distance = pseudoDistance;
                        distance.Dot = 0;
                    }
                }

                break;
            }

            // after the end point
            case > 1:
            {
                var endDirection = Vector2.Normalize(GetDirection(1));
                var toEnd = origin - GetPoint(1);
                double ts = Vector2.Dot(toEnd, endDirection);

                if (ts > 0)
                {
                    var pseudoDistance = toEnd.Cross(endDirection);
                    if (Math.Abs(pseudoDistance) <= Math.Abs(distance.Distance))
                    {
                        distance.Distance = pseudoDistance;
                        distance.Dot = 0;
                    }
                }

                break;
            }
        }
    }

    /// <summary>
    /// Shifts the given bounding coordinates to include the given point.
    /// </summary>
    /// <param name="p">The point.</param>
    /// <param name="left">The left limit of the bounding box.</param>
    /// <param name="bottom">The bottom limit of the bounding box.</param>
    /// <param name="right">The right limit of the bounding box.</param>
    /// <param name="top">The top limit of the bounding box.</param>
    protected static void PointBounds(Vector2 p, ref double left, ref double bottom, ref double right, ref double top)
    {
        if (p.X < left)
        {
            left = p.X;
        }

        if (p.Y < bottom)
        {
            bottom = p.Y;
        }

        if (p.X > right)
        {
            right = p.X;
        }

        if (p.Y > top)
        {
            top = p.Y;
        }
    }

    /// <summary>
    /// Determines the nonzero sign of the given value, producing a value of 1 for zero and the original value for all
    /// others.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The non-zero sign of the given value.</returns>
    protected static int NonZeroSign(double value)
    {
        var result = Math.Sign(value);
        return result == 0 ? 1 : result;
    }
}
