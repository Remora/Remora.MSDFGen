//
//  SPDX-FileName: CubicSegment.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Numerics;
using Remora.MSDFGen.Extensions;

namespace Remora.MSDFGen;

public class CubicSegment : SplineSegment
{
    private Vector2 p0;
    private Vector2 p1;
    private Vector2 p2;
    private Vector2 p3;

    public CubicSegment(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, EdgeColor color)
        : base(color)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }

    public override EdgeSegment Clone()
    {
        return new CubicSegment(p0, p1, p2, p3, this.Color);
    }

    public override Vector2 GetPoint(double normalizedEdgeDistance)
    {
        var p12 = Vector2.Lerp(p1, p2, (float)normalizedEdgeDistance);
        return Vector2.Lerp(
            Vector2.Lerp(
                Vector2.Lerp(p0, p1, (float)normalizedEdgeDistance),
                p1,
                (float)normalizedEdgeDistance
            ),
            Vector2.Lerp(
                p12,
                Vector2.Lerp(p2, p3, (float)normalizedEdgeDistance),
                (float)normalizedEdgeDistance
            ),
            (float)normalizedEdgeDistance
        );
    }

    public override Vector2 GetDirection(double normalizedEdgeDistance)
    {
        var tangent = Vector2.Lerp(
            Vector2.Lerp(p1 - p0, p2 - p1, (float)normalizedEdgeDistance),
            Vector2.Lerp(p2 - p1, p3 - p2, (float)normalizedEdgeDistance),
            (float)normalizedEdgeDistance
        );

        if (tangent != Vector2.Zero)
        {
            return tangent;
        }

        return normalizedEdgeDistance switch
        {
            0 => p2 - p0,
            1 => p3 - p1,
            _ => tangent
        };
    }

    public override SignedDistance GetSignedDistance(Vector2 origin, out double normalizedEdgeDistance)
    {
        var qa = p0 - origin;
        var ab = p1 - p0;
        var br = p2 - p1 - ab;
        var _as = p3 - p2 - (p2 - p1) - br;

        var epDir = GetDirection(0);
        double minDistance = NonZeroSign(epDir.Cross(qa)) * qa.Length();
        normalizedEdgeDistance = -Vector2.Dot(qa, epDir) / Vector2.Dot(epDir, epDir);

        epDir = GetDirection(1);
        double distance = NonZeroSign(epDir.Cross(p3 - origin)) * (p3 - origin).Length();

        if (Math.Abs(distance) < Math.Abs(minDistance))
        {
            minDistance = distance;
            normalizedEdgeDistance = Vector2.Dot(origin + epDir - p3, epDir) / Vector2.Dot(epDir, epDir);
        }

        for (var i = 0; i < 4; i++)
        {
            var _t = (double)i / 4;
            var step = 0;
            while (true)
            {
                var qpt = GetPoint(_t) - origin;
                double stepDistance = NonZeroSign(GetDirection(_t).Cross(qpt)) * qpt.Length();

                if (Math.Abs(stepDistance) < Math.Abs(minDistance))
                {
                    minDistance = stepDistance;
                    normalizedEdgeDistance = _t;
                }

                if (step == 4)
                {
                    break;
                }

                var d1 = (3 * _as * (float)(normalizedEdgeDistance * normalizedEdgeDistance)) + (6 * br * (float)normalizedEdgeDistance) + (3 * ab);
                var d2 = (6 * _as * (float)normalizedEdgeDistance) + (6 * br);
                _t -= Vector2.Dot(qpt, d1) / (Vector2.Dot(d1, d1) + Vector2.Dot(qpt, d2));
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
                Math.Abs(Vector2.Dot(Vector2.Normalize(GetDirection(1)), Vector2.Normalize(p3 - origin)))
            )
        };
    }

    public override void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        PointBounds(p0, ref left, ref bottom, ref right, ref top);
        PointBounds(p3, ref left, ref bottom, ref right, ref top);

        var a0 = p1 - p0;
        var a1 = 2 * (p2 - p1 - a0);
        var a2 = p3 - (3 * p2) + (3 * p1) - p0;

        var roots = new Roots();
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

    public override void MoveStartPoint(Vector2 newStart)
    {
        p1 += newStart - p0;
        p0 = newStart;
    }

    public override void MoveEndPoint(Vector2 newEnd)
    {
        p2 += newEnd - p3;
        p3 = newEnd;
    }

    public override void SplitInThirds(out EdgeSegment first, out EdgeSegment second, out EdgeSegment third)
    {
        first = new CubicSegment(
            p0,
            p0 == p1 ? p0 : Vector2.Lerp(p0, p1, 1 / 3f),
            Vector2.Lerp(
                Vector2.Lerp(p0, p1, 1 / 3f),
                Vector2.Lerp(p1, p2, 1 / 3f),
                1 / 3f
            ),
            GetPoint(1 / 3d),
            this.Color
        );
        second = new CubicSegment(
            GetPoint(1 / 3d),
            Vector2.Lerp(
                Vector2.Lerp(
                    Vector2.Lerp(p0, p1, 1 / 3f),
                    Vector2.Lerp(p1, p2, 1 / 3f),
                    1 / 3f
                ),
                Vector2.Lerp(
                    Vector2.Lerp(p1, p2, 1 / 3f),
                    Vector2.Lerp(p2, p3, 1 / 3f),
                    1 / 3f
                ),
                2 / 3f
            ),
            Vector2.Lerp(
                Vector2.Lerp(
                    Vector2.Lerp(p0, p1, 2 / 3f),
                    Vector2.Lerp(p1, p2, 2 / 3f),
                    2 / 3f
                ),
                Vector2.Lerp(
                    Vector2.Lerp(p1, p2, 2 / 3f),
                    Vector2.Lerp(p2, p3, 2 / 3f),
                    2 / 3f
                ),
                1 / 3f
            ),
            GetPoint(2 / 3d),
            this.Color
        );
        third = new CubicSegment(
            GetPoint(2 / 3d),
            Vector2.Lerp(
                Vector2.Lerp(p1, p2, 2 / 3f),
                Vector2.Lerp(p2, p3, 2 / 3f),
                2 / 3f
            ),
            p2 == p3 ? p3 : Vector2.Lerp(p2, p3, 2 / 3f),
            p3,
            this.Color
        );
    }
}
