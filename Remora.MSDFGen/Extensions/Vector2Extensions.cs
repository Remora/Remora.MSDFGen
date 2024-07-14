//
//  SPDX-FileName: Vector2Extensions.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System.Numerics;

namespace Remora.MSDFGen.Extensions;

/// <summary>
/// Defines extension methods for the <see cref="Vector2"/> struct.
/// </summary>
public static class Vector2Extensions
{
    /// <summary>
    /// Calculates the cross product between the vector and the given other vector.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The other vector.</param>
    /// <returns>The cross product.</returns>
    public static double Cross(this Vector2 a, Vector2 b)
    {
        return (a.X * b.Y) - (a.Y * b.X);
    }
}
