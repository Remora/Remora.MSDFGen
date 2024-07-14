//
//  SPDX-FileName: QuadraticSegment.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Numerics;
using Remora.MSDFGen.Extensions;

namespace Remora.MSDFGen;

public class QuadraticSegment : SplineSegment
{
    private Vector2 p0;
    private Vector2 p1;
    private Vector2 p2;

    public QuadraticSegment(Vector2 p0, Vector2 p1, Vector2 p2, EdgeColor color)
        : base(color)
    {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
    }

    public override EdgeSegment Clone()
    {
        return new QuadraticSegment(p0, p1, p2, this.Color);
    }

    public override Vector2 GetPoint(double normalizedEdgeDistance)
    {
        return Vector2.Lerp(
            Vector2.Lerp(p0, p1, (float)normalizedEdgeDistance),
            Vector2.Lerp(p1, p2, (float)normalizedEdgeDistance),
            (float)normalizedEdgeDistance
        );
    }

    public override Vector2 GetDirection(double normalizedEdgeDistance)
    {
        return Vector2.Lerp(
            p1 - p0,
            p2 - p1,
            (float)normalizedEdgeDistance
        );
    }

    public override SignedDistance GetSignedDistance(Vector2 origin, out double normalizedEdgeDistance)
    {
        var qa = p0 - origin;
        var ab = p1 - p0;
        var br = p0 + p2 - p1 - p1;
        double a = Vector2.Dot(br, br);
        double b = 3 * Vector2.Dot(ab, br);
        double c = (2 * Vector2.Dot(ab, ab)) + Vector2.Dot(qa, br);
        double d = Vector2.Dot(qa, ab);

        var roots = new Roots();
        var solutions = SolveCubic(ref roots, a, b, c, d);

        double minDistance = NonZeroSign(ab.Cross(qa)) * qa.Length();
        normalizedEdgeDistance = -Vector2.Dot(qa, ab) / Vector2.Dot(ab, ab);

        double distance = NonZeroSign((p2 - p1).Cross(p2 - origin)) * (p2 - origin).Length();
        if (Math.Abs(distance) < Math.Abs(minDistance))
        {
            minDistance = distance;
            normalizedEdgeDistance = Vector2.Dot(origin - p1, p2 - p1) / Vector2.Dot(p2 - p1, p2 - p1);
        }

        for (var i = 0; i < solutions; i++)
        {
            if (!(roots[i] > 0) || !(roots[i] < 1))
            {
                continue;
            }

            var endPoint = p0 + ((float)(2 * roots[i]) * ab) + ((float)(roots[i] * roots[i]) * br);
            double solutionDistance = NonZeroSign((p2 - p0).Cross(endPoint - origin)) * (endPoint - origin).Length();

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
                Math.Abs(Vector2.Dot(Vector2.Normalize(p2 - p1), Vector2.Normalize(p2 - origin)))
            )
        };
    }

    public override void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        PointBounds(p0, ref left, ref bottom, ref right, ref top);
        PointBounds(p2, ref left, ref bottom, ref right, ref top);
        var bot = p1 - p0 - (p2 - p1);

        if (bot.X != 0)
        {
            double param = (p1.X - p0.X) / bot.X;
            if (param is > 0 and < 1)
            {
                PointBounds(GetPoint(param), ref left, ref bottom, ref right, ref top);
            }
        }

        if (bot.Y != 0)
        {
            double param = (p1.Y - p0.Y) / bot.Y;
            if (param is > 0 and < 1)
            {
                PointBounds(GetPoint(param), ref left, ref bottom, ref right, ref top);
            }
        }
    }

    public override void MoveStartPoint(Vector2 newStart)
    {
        var originalStartDirection = p0 - p1;
        var origP1 = p1;

        p1 += (float)(originalStartDirection.Cross(newStart - p0) / originalStartDirection.Cross(p2 - p1)) * (p2 - p1);
        p0 = newStart;
        if (Vector2.Dot(originalStartDirection, p0 - p1) < 0)
        {
            p1 = origP1;
        }
    }

    public override void MoveEndPoint(Vector2 newEnd)
    {
        var originalEndDirection = p2 - p1;
        var origP1 = p1;

        p1 += (float)(originalEndDirection.Cross(newEnd - p2) / originalEndDirection.Cross(p0 - p1)) * (p0 - p1);
        p2 = newEnd;
        if (Vector2.Dot(originalEndDirection, p2 - p1) < 0)
        {
            p1 = origP1;
        }
    }

    public override void SplitInThirds(out EdgeSegment first, out EdgeSegment second, out EdgeSegment third)
    {
        first = new QuadraticSegment
        (
            p0,
            Vector2.Lerp(p0, p1, 1 / 3f),
            GetPoint(1 / 3d),
            this.Color
        );

        second = new QuadraticSegment
        (
            GetPoint(1 / 3d),
            Vector2.Lerp(
                Vector2.Lerp(p0, p1, 5 / 9f),
                Vector2.Lerp(p1, p2, 4 / 9f),
                0.5f
            ),
            GetPoint(2 / 3d),
            this.Color
        );

        third = new QuadraticSegment
        (
            GetPoint(2 / 3d),
            Vector2.Lerp(p1, p2, 2 / 3f),
            p2,
            this.Color
        );
    }
}
