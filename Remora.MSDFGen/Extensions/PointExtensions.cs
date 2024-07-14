//
//  SPDX-FileName: PointExtensions.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System.Drawing;
using System.Numerics;

namespace Remora.MSDFGen.Extensions;

/// <summary>
/// Defines extension methods for the <see cref="Point"/> struct.
/// </summary>
public static class PointExtensions
{
    /// <summary>
    /// Converts the given point to a floating-point vector.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The vector.</returns>
    public static Vector2 ToVector(this Point point) => new(point.X, point.Y);
}
