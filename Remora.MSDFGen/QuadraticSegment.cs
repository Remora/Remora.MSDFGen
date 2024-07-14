//
//  SPDX-FileName: QuadraticSegment.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Numerics;
using Remora.MSDFGen.Extensions;

namespace Remora.MSDFGen;

/// <summary>
/// Represents a quadratic spline segment, containing a start and end point with one intermediate continuous derivative.
/// </summary>
public class QuadraticSegment : SplineSegment
{
    private Vector2 _start;
    private Vector2 _p1;
    private Vector2 _end;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuadraticSegment"/> class.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="p1">The intermediate derivative.</param>
    /// <param name="end">The end point.</param>
    /// <param name="color">The color of the segment.</param>
    public QuadraticSegment(Vector2 start, Vector2 p1, Vector2 end, EdgeColor color)
        : base(color)
    {
        _start = start;
        _p1 = p1;
        _end = end;
    }

    /// <inheritdoc />
    public override EdgeSegment Clone()
    {
        return new QuadraticSegment(_start, _p1, _end, this.Color);
    }

    /// <inheritdoc />
    public override Vector2 GetPoint(double normalizedEdgeDistance)
    {
        return Vector2.Lerp
        (
            Vector2.Lerp(_start, _p1, (float)normalizedEdgeDistance),
            Vector2.Lerp(_p1, _end, (float)normalizedEdgeDistance),
            (float)normalizedEdgeDistance
        );
    }

    /// <inheritdoc />
    public override Vector2 GetDirection(double normalizedEdgeDistance)
    {
        return Vector2.Lerp
        (
            _p1 - _start,
            _end - _p1,
            (float)normalizedEdgeDistance
        );
    }

    /// <inheritdoc />
    public override SignedDistance GetSignedDistance(Vector2 origin, out double normalizedEdgeDistance)
    {
        var qa = _start - origin;
        var ab = _p1 - _start;
        var br = _start + _end - _p1 - _p1;
        double a = Vector2.Dot(br, br);
        double b = 3 * Vector2.Dot(ab, br);
        double c = (2 * Vector2.Dot(ab, ab)) + Vector2.Dot(qa, br);
        double d = Vector2.Dot(qa, ab);

        var roots = default(Roots);
        var solutions = SolveCubic(ref roots, a, b, c, d);

        double minDistance = NonZeroSign(ab.Cross(qa)) * qa.Length();
        normalizedEdgeDistance = -Vector2.Dot(qa, ab) / Vector2.Dot(ab, ab);

        double distance = NonZeroSign((_end - _p1).Cross(_end - origin)) * (_end - origin).Length();
        if (Math.Abs(distance) < Math.Abs(minDistance))
        {
            minDistance = distance;
            normalizedEdgeDistance = Vector2.Dot(origin - _p1, _end - _p1) / Vector2.Dot(_end - _p1, _end - _p1);
        }

        for (var i = 0; i < solutions; i++)
        {
            if (!(roots[i] > 0) || !(roots[i] < 1))
            {
                continue;
            }

            var endPoint = _start + ((float)(2 * roots[i]) * ab) + ((float)(roots[i] * roots[i]) * br);
            double solutionDistance = NonZeroSign((_end - _start).Cross(endPoint - origin)) *
                                      (endPoint - origin).Length();

            if (!(Math.Abs(solutionDistance) <= Math.Abs(minDistance)))
            {
                continue;
            }

            minDistance = solutionDistance;
            normalizedEdgeDistance = roots[i];
        }

        return normalizedEdgeDistance switch
        {
            >= 0 and <= 1 => new SignedDistance(minDistance, 0),
            < .5 => new SignedDistance
            (
                minDistance,
                Math.Abs(Vector2.Dot(Vector2.Normalize(ab), Vector2.Normalize(qa)))
            ),
            _ => new SignedDistance
            (
                minDistance,
                Math.Abs(Vector2.Dot(Vector2.Normalize(_end - _p1), Vector2.Normalize(_end - origin)))
            )
        };
    }

    /// <inheritdoc />
    public override void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        PointBounds(_start, ref left, ref bottom, ref right, ref top);
        PointBounds(_end, ref left, ref bottom, ref right, ref top);
        var bot = _p1 - _start - (_end - _p1);

        if (bot.X != 0)
        {
            double param = (_p1.X - _start.X) / bot.X;
            if (param is > 0 and < 1)
            {
                PointBounds(GetPoint(param), ref left, ref bottom, ref right, ref top);
            }
        }

        if (bot.Y != 0)
        {
            double param = (_p1.Y - _start.Y) / bot.Y;
            if (param is > 0 and < 1)
            {
                PointBounds(GetPoint(param), ref left, ref bottom, ref right, ref top);
            }
        }
    }

    /// <inheritdoc />
    public override void MoveStartPoint(Vector2 newStart)
    {
        var originalStartDirection = _start - _p1;
        var origP1 = _p1;

        _p1 += (float)(originalStartDirection.Cross(newStart - _start) / originalStartDirection.Cross(_end - _p1)) * (_end - _p1);
        _start = newStart;
        if (Vector2.Dot(originalStartDirection, _start - _p1) < 0)
        {
            _p1 = origP1;
        }
    }

    /// <inheritdoc />
    public override void MoveEndPoint(Vector2 newEnd)
    {
        var originalEndDirection = _end - _p1;
        var origP1 = _p1;

        _p1 += (float)(originalEndDirection.Cross(newEnd - _end) / originalEndDirection.Cross(_start - _p1)) * (_start - _p1);
        _end = newEnd;
        if (Vector2.Dot(originalEndDirection, _end - _p1) < 0)
        {
            _p1 = origP1;
        }
    }

    /// <inheritdoc />
    public override void SplitInThirds(out EdgeSegment first, out EdgeSegment second, out EdgeSegment third)
    {
        first = new QuadraticSegment
        (
            _start,
            Vector2.Lerp(_start, _p1, 1 / 3f),
            GetPoint(1 / 3d),
            this.Color
        );

        second = new QuadraticSegment
        (
            GetPoint(1 / 3d),
            Vector2.Lerp
            (
                Vector2.Lerp(_start, _p1, 5 / 9f),
                Vector2.Lerp(_p1, _end, 4 / 9f),
                0.5f
            ),
            GetPoint(2 / 3d),
            this.Color
        );

        third = new QuadraticSegment
        (
            GetPoint(2 / 3d),
            Vector2.Lerp(_p1, _end, 2 / 3f),
            _end,
            this.Color
        );
    }
}
