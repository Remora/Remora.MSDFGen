//
//  SPDX-FileName: ExtraMath.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;

namespace Remora.MSDFGen.Utility;

/// <summary>
/// Defines various additional math functions.
/// </summary>
public static class ExtraMath
{
    /// <summary>
    /// Calculates the median value of the three given values.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <param name="c">The third value.</param>
    /// <returns>The median.</returns>
    public static float Median(float a, float b, float c) => Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));

    /// <summary>
    /// Calculates the median value of the three given values.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <param name="c">The third value.</param>
    /// <returns>The median.</returns>
    public static double Median(double a, double b, double c) => Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));

    /// <summary>
    /// Calculates the median value of the three given values.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <param name="c">The third value.</param>
    /// <returns>The median.</returns>
    public static int Median(int a, int b, int c) => Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
}
