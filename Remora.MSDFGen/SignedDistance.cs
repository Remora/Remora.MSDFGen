//
//  SPDX-FileName: SignedDistance.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;

namespace Remora.MSDFGen;

/// <summary>
/// Represents a signed distance between a point on a plane and the edge of a shape.
/// </summary>
public struct SignedDistance
{
    /// <summary>
    /// Gets or sets the distance between the point and the shape.
    /// </summary>
    public double Distance { get; set; }

    /// <summary>
    /// Gets or sets the dot product of the distance, indicating whether the measured point is inside or outside the
    /// shape.
    /// </summary>
    public double Dot { get; set; }

    /// <summary>
    /// Gets a value representing an infinite distance from the shape.
    /// </summary>
    public static SignedDistance Infinite { get; } = new(-1e240, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="SignedDistance"/> struct.
    /// </summary>
    /// <param name="distance">The distance between the point and the shape.</param>
    /// <param name="dot">The dot product of the distance.</param>
    public SignedDistance(double distance, double dot)
    {
        this.Distance = distance;
        this.Dot = dot;
    }

    /// <summary>
    /// Compares two distances, determining if the left operand is less than the right operand.
    /// </summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>true if the left operand is less than the right operand; otherwise, false.</returns>
    public static bool operator <(SignedDistance a, SignedDistance b)
    {
        return Math.Abs(a.Distance) < Math.Abs(b.Distance) ||
               (Math.Abs(a.Distance) == Math.Abs(b.Distance) && a.Dot < b.Dot);
    }

    /// <summary>
    /// Compares two distances, determining if the left operand is greater than the right operand.
    /// </summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>true if the left operand is less than the greater operand; otherwise, false.</returns>
    public static bool operator >(SignedDistance a, SignedDistance b)
    {
        return Math.Abs(a.Distance) > Math.Abs(b.Distance) ||
               (Math.Abs(a.Distance) == Math.Abs(b.Distance) && a.Dot > b.Dot);
    }

    /// <summary>
    /// Compares two distances, determining if the left operand is less than or equal to the right operand.
    /// </summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>true if the left operand is less than or equal to the right operand; otherwise, false.</returns>
    public static bool operator <=(SignedDistance a, SignedDistance b)
    {
        return Math.Abs(a.Distance) < Math.Abs(b.Distance) ||
               (Math.Abs(a.Distance) == Math.Abs(b.Distance) && a.Dot <= b.Dot);
    }

    /// <summary>
    /// Compares two distances, determining if the left operand is greater than or equal to the right operand.
    /// </summary>
    /// <param name="a">The left operand.</param>
    /// <param name="b">The right operand.</param>
    /// <returns>true if the left operand is greater than or equal to the right operand; otherwise, false.</returns>
    public static bool operator >=(SignedDistance a, SignedDistance b)
    {
        return Math.Abs(a.Distance) > Math.Abs(b.Distance) ||
               (Math.Abs(a.Distance) == Math.Abs(b.Distance) && a.Dot >= b.Dot);
    }
}
