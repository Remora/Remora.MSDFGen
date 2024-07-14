//
//  SPDX-FileName: CubicSegment.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Numerics;
using Remora.MSDFGen.Extensions;

namespace Remora.MSDFGen;

/// <summary>
/// Represents a cubic spline segment, containing a start and end point with two intermediate continuous derivatives.
/// </summary>
public class CubicSegment : SplineSegment
{
    private Vector2 _start;
    private Vector2 _p1;
    private Vector2 _p2;
    private Vector2 _end;

    /// <summary>
    /// Initializes a new instance of the <see cref="CubicSegment"/> class.
    /// </summary>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="p1">The first intermediate derivative.</param>
    /// <param name="p2">The second intermediate derivative.</param>
    /// <param name="end">The end point.</param>
    /// <param name="color">The color of the segment.</param>
    public CubicSegment(Vector2 start, Vector2 p1, Vector2 p2, Vector2 end, EdgeColor color)
        : base(color)
    {
        _start = start;
        _p1 = p1;
        _p2 = p2;
        _end = end;
    }

    /// <inheritdoc />
    public override EdgeSegment Clone()
    {
        return new CubicSegment(_start, _p1, _p2, _end, this.Color);
    }

    /// <inheritdoc />
    public override Vector2 GetPoint(double normalizedEdgeDistance)
    {
        var p12 = Vector2.Lerp(_p1, _p2, (float)normalizedEdgeDistance);
        return Vector2.Lerp
        (
            Vector2.Lerp
            (
                Vector2.Lerp(_start, _p1, (float)normalizedEdgeDistance),
                _p1,
                (float)normalizedEdgeDistance
            ),
            Vector2.Lerp
            (
                p12,
                Vector2.Lerp(_p2, _end, (float)normalizedEdgeDistance),
                (float)normalizedEdgeDistance
            ),
            (float)normalizedEdgeDistance
        );
    }

    /// <inheritdoc />
    public override Vector2 GetDirection(double normalizedEdgeDistance)
    {
        var tangent = Vector2.Lerp
        (
            Vector2.Lerp(_p1 - _start, _p2 - _p1, (float)normalizedEdgeDistance),
            Vector2.Lerp(_p2 - _p1, _end - _p2, (float)normalizedEdgeDistance),
            (float)normalizedEdgeDistance
        );

        if (tangent != Vector2.Zero)
        {
            return tangent;
        }

        return normalizedEdgeDistance switch
        {
            0 => _p2 - _start,
            1 => _end - _p1,
            _ => tangent
        };
    }

    /// <inheritdoc />
    public override SignedDistance GetSignedDistance(Vector2 origin, out double normalizedEdgeDistance)
    {
        var qa = _start - origin;
        var ab = _p1 - _start;
        var br = _p2 - _p1 - ab;
        var @as = _end - _p2 - (_p2 - _p1) - br;

        var startDirection = GetDirection(0);
        double minDistance = NonZeroSign(startDirection.Cross(qa)) * qa.Length();
        normalizedEdgeDistance = -Vector2.Dot(qa, startDirection) / Vector2.Dot(startDirection, startDirection);

        startDirection = GetDirection(1);
        double distance = NonZeroSign(startDirection.Cross(_end - origin)) * (_end - origin).Length();

        if (Math.Abs(distance) < Math.Abs(minDistance))
        {
            minDistance = distance;
            normalizedEdgeDistance = Vector2.Dot(origin + startDirection - _end, startDirection) /
                                     Vector2.Dot(startDirection, startDirection);
        }

        for (var i = 0; i < 4; i++)
        {
            var t = (double)i / 4;
            var step = 0;
            while (true)
            {
                var qpt = GetPoint(t) - origin;
                double stepDistance = NonZeroSign(GetDirection(t).Cross(qpt)) * qpt.Length();

                if (Math.Abs(stepDistance) < Math.Abs(minDistance))
                {
                    minDistance = stepDistance;
                    normalizedEdgeDistance = t;
                }

                if (step == 4)
                {
                    break;
                }

                var d1 = (3 * @as * (float)(normalizedEdgeDistance * normalizedEdgeDistance)) +
                         (6 * br * (float)normalizedEdgeDistance) + (3 * ab);

                var d2 = (6 * @as * (float)normalizedEdgeDistance) + (6 * br);
                t -= Vector2.Dot(qpt, d1) / (Vector2.Dot(d1, d1) + Vector2.Dot(qpt, d2));
                if (normalizedEdgeDistance is < 0 or > 1)
                {
                    break;
                }

                step++;
            }
        }

        return normalizedEdgeDistance switch
        {
            >= 0 and <= 1 => new SignedDistance(minDistance, 0),
            < 0.5 => new SignedDistance
            (
                minDistance,
                Math.Abs(Vector2.Dot(Vector2.Normalize(GetDirection(0)), Vector2.Normalize(qa)))
            ),
            _ => new SignedDistance
            (
                minDistance,
                Math.Abs(Vector2.Dot(Vector2.Normalize(GetDirection(1)), Vector2.Normalize(_end - origin)))
            )
        };
    }

    /// <inheritdoc />
    public override void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        PointBounds(_start, ref left, ref bottom, ref right, ref top);
        PointBounds(_end, ref left, ref bottom, ref right, ref top);

        var a0 = _p1 - _start;
        var a1 = 2 * (_p2 - _p1 - a0);
        var a2 = _end - (3 * _p2) + (3 * _p1) - _start;

        var roots = default(Roots);
        var solutions = SolveQuadratic(ref roots, a2.X, a1.X, a0.X);
        for (var i = 0; i < solutions; i++)
        {
            if (roots[i] > 0 && roots[i] < 1)
            {
                PointBounds(GetPoint(roots[i]), ref left, ref bottom, ref right, ref top);
            }
        }

        solutions = SolveQuadratic(ref roots, a2.Y, a1.Y, a0.Y);
        for (var i = 0; i < solutions; i++)
        {
            if (roots[i] > 0 && roots[i] < 1)
            {
                PointBounds(GetPoint(roots[i]), ref left, ref bottom, ref right, ref top);
            }
        }
    }

    /// <inheritdoc />
    public override void MoveStartPoint(Vector2 newStart)
    {
        _p1 += newStart - _start;
        _start = newStart;
    }

    /// <inheritdoc />
    public override void MoveEndPoint(Vector2 newEnd)
    {
        _p2 += newEnd - _end;
        _end = newEnd;
    }

    /// <inheritdoc />
    public override void SplitInThirds(out EdgeSegment first, out EdgeSegment second, out EdgeSegment third)
    {
        first = new CubicSegment
        (
            _start,
            _start == _p1 ? _start : Vector2.Lerp(_start, _p1, 1 / 3f),
            Vector2.Lerp
            (
                Vector2.Lerp(_start, _p1, 1 / 3f),
                Vector2.Lerp(_p1, _p2, 1 / 3f),
                1 / 3f
            ),
            GetPoint(1 / 3d),
            this.Color
        );

        second = new CubicSegment
        (
            GetPoint(1 / 3d),
            Vector2.Lerp
            (
                Vector2.Lerp
                (
                    Vector2.Lerp(_start, _p1, 1 / 3f),
                    Vector2.Lerp(_p1, _p2, 1 / 3f),
                    1 / 3f
                ),
                Vector2.Lerp
                (
                    Vector2.Lerp(_p1, _p2, 1 / 3f),
                    Vector2.Lerp(_p2, _end, 1 / 3f),
                    1 / 3f
                ),
                2 / 3f
            ),
            Vector2.Lerp
            (
                Vector2.Lerp
                (
                    Vector2.Lerp(_start, _p1, 2 / 3f),
                    Vector2.Lerp(_p1, _p2, 2 / 3f),
                    2 / 3f
                ),
                Vector2.Lerp
                (
                    Vector2.Lerp(_p1, _p2, 2 / 3f),
                    Vector2.Lerp(_p2, _end, 2 / 3f),
                    2 / 3f
                ),
                1 / 3f
            ),
            GetPoint(2 / 3d),
            this.Color
        );

        third = new CubicSegment
        (
            GetPoint(2 / 3d),
            Vector2.Lerp
            (
                Vector2.Lerp(_p1, _p2, 2 / 3f),
                Vector2.Lerp(_p2, _end, 2 / 3f),
                2 / 3f
            ),
            _p2 == _end ? _end : Vector2.Lerp(_p2, _end, 2 / 3f),
            _end,
            this.Color
        );
    }
}
