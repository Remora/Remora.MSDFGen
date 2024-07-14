//
//  SPDX-FileName: SplineSegment.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Diagnostics.CodeAnalysis;

namespace Remora.MSDFGen;

/// <summary>
/// Represents a contour segment based on a spline calculation.
/// </summary>
public abstract class SplineSegment : EdgeSegment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SplineSegment"/> class.
    /// </summary>
    /// <param name="color">The color of the segment.</param>
    protected SplineSegment(EdgeColor color)
        : base(color)
    {
    }

    /// <summary>
    /// Represents up to three solutions for a polynomial equation.
    /// </summary>
    protected struct Roots
    {
        /// <summary>
        /// Gets or sets the first root of the function.
        /// </summary>
        public double FirstRoot { get; set; }

        /// <summary>
        /// Gets or sets the second root of the function.
        /// </summary>
        public double SecondRoot { get; set; }

        /// <summary>
        /// Gets or sets the third root of the function.
        /// </summary>
        public double ThirdRoot { get; set; }

        /// <summary>
        /// Gets the root at the given index.
        /// </summary>
        /// <param name="i">The index of the root.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if a value below zero or larger than 2 is indexed.
        /// </exception>
        public double this[int i] => i switch
        {
            0 => this.FirstRoot,
            1 => this.SecondRoot,
            2 => this.ThirdRoot,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Solves a quadratic polynomial, returning up to two roots for the function.
    /// </summary>
    /// <param name="roots">The value to calculate the roots in.</param>
    /// <param name="a">The first coefficient of the function.</param>
    /// <param name="b">The second coefficient of the function.</param>
    /// <param name="c">The third coefficient of the function.</param>
    /// <returns>The number of solutions for the function.</returns>
    protected static int SolveQuadratic(ref Roots roots, double a, double b, double c)
    {
        if (Math.Abs(a) < 1e-14)
        {
            if (Math.Abs(b) < 1e-14)
            {
                if (c == 0)
                {
                    return -1;
                }

                return 0;
            }

            roots.FirstRoot = -c / b;
            return 1;
        }

        var discriminant = (b * b) - (4 * a * c);

        switch (discriminant)
        {
            case > 0:
            {
                discriminant = Math.Sqrt(discriminant);
                roots.FirstRoot = (-b + discriminant) / (2 * a);
                roots.SecondRoot = (-b - discriminant) / (2 * a);
                return 2;
            }
            case 0:
            {
                roots.FirstRoot = -b / (2 * a);
                return 1;
            }
            default:
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Solves a normalized cubic polynomial, returning up to three roots of the function.
    /// </summary>
    /// <param name="roots">The value to calculate the roots in.</param>
    /// <param name="a">The first coefficient of the function.</param>
    /// <param name="b">The second coefficient of the function.</param>
    /// <param name="c">The third coefficient of the function.</param>
    /// <returns>The number of solutions for the function.</returns>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:Field names should not use Hungarian notation", Justification = "Not hungarian notation")]
    protected static int SolveCubicNormed(ref Roots roots, double a, double b, double c)
    {
        var aSquared = a * a;
        var q = (aSquared - (3 * b)) / 9;
        var r = ((a * ((2 * aSquared) - (9 * b))) + (27 * c)) / 54;
        var rSquared = r * r;
        var qCubed = q * q * q;

        if (rSquared < qCubed)
        {
            var t = r / Math.Sqrt(qCubed);
            if (t < -1)
            {
                t = -1;
            }

            if (t > 1)
            {
                t = 1;
            }

            t = Math.Acos(t);
            a /= 3;
            q = -2 * Math.Sqrt(q);

            roots.FirstRoot = (q * Math.Cos(t / 3)) - a;
            roots.SecondRoot = (q * Math.Cos((t + (2 * Math.PI)) / 3)) - a;
            roots.ThirdRoot = (q * Math.Cos((t - (2 * Math.PI)) / 3)) - a;

            return 3;
        }

        var aMajor = -Math.Pow
        (
            Math.Abs(r) + Math.Sqrt(rSquared - qCubed),
            1 / 3d
        );

        if (r < 0)
        {
            aMajor = -aMajor;
        }

        var bMajor = aMajor == 0 ? 0 : q / aMajor;
        a /= 3;

        roots.FirstRoot = aMajor + bMajor - a;
        roots.SecondRoot = (-0.5 * (aMajor + bMajor)) - a;
        roots.ThirdRoot = 0.5 * Math.Sqrt(3) * (aMajor - bMajor);

        return Math.Abs(roots.ThirdRoot) < 1e-14 ? 2 : 1;
    }

    /// <summary>
    /// Solves a cubic polynomial, returning up to three roots of the function.
    /// </summary>
    /// <param name="roots">The value to calculate the roots in.</param>
    /// <param name="a">The first coefficient of the function.</param>
    /// <param name="b">The second coefficient of the function.</param>
    /// <param name="c">The third coefficient of the function.</param>
    /// <param name="d">The fourth coefficient of the function.</param>
    /// <returns>The number of solutions for the function.</returns>
    protected static int SolveCubic(ref Roots roots, double a, double b, double c, double d)
    {
        return Math.Abs(a) < 1e-14
            ? SolveQuadratic(ref roots, b, c, d)
            : SolveCubicNormed(ref roots, b / a, c / a, d / a);
    }
}
