//
//  SPDX-FileName: LinearSegment.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Numerics;
using Remora.MSDFGen.Extensions;

namespace Remora.MSDFGen;

/// <summary>
/// Represents a straight line between two points.
/// </summary>
public class LinearSegment : EdgeSegment
{
    private Vector2 _start;
    private Vector2 _end;

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearSegment"/> class.
    /// </summary>
    /// <param name="start">The start point of the line.</param>
    /// <param name="end">The end point of the line.</param>
    /// <param name="color">The color of the segment.</param>
    public LinearSegment(Vector2 start, Vector2 end, EdgeColor color)
        : base(color)
    {
        _start = start;
        _end = end;
    }

    /// <inheritdoc />
    public override EdgeSegment Clone()
    {
        return new LinearSegment(_start, _end, this.Color);
    }

    /// <inheritdoc />
    public override Vector2 GetPoint(double normalizedEdgeDistance)
    {
        return Vector2.Lerp(_start, _end, (float)normalizedEdgeDistance);
    }

    /// <inheritdoc />
    public override Vector2 GetDirection(double normalizedEdgeDistance)
    {
        return _end - _start;
    }

    /// <inheritdoc />
    public override SignedDistance GetSignedDistance(Vector2 origin, out double normalizedEdgeDistance)
    {
        var toStart = origin - _start;
        var segmentDirection = _end - _start;

        normalizedEdgeDistance = Vector2.Dot(toStart, segmentDirection) / Vector2.Dot(segmentDirection, segmentDirection);
        var eq = (normalizedEdgeDistance > 0.5d ? _end : _start) - origin;
        double endPointDistance = eq.Length();

        if (normalizedEdgeDistance is <= 0 or >= 1)
        {
            return new SignedDistance
            (
                NonZeroSign(toStart.Cross(segmentDirection)) * endPointDistance,
                Math.Abs(Vector2.Dot(Vector2.Normalize(segmentDirection), Vector2.Normalize(eq)))
            );
        }

        double orthoDistance = Vector2.Dot(GetOrthonormal(segmentDirection, false, false), toStart);
        if (Math.Abs(orthoDistance) < endPointDistance)
        {
            return new SignedDistance(orthoDistance, 0);
        }

        return new SignedDistance(
            NonZeroSign(toStart.Cross(segmentDirection)) * endPointDistance,
            Math.Abs(Vector2.Dot(Vector2.Normalize(segmentDirection), Vector2.Normalize(eq)))
        );
    }

    /// <inheritdoc />
    public override void GetBounds(ref double left, ref double bottom, ref double right, ref double top)
    {
        PointBounds(_start, ref left, ref bottom, ref right, ref top);
        PointBounds(_end, ref left, ref bottom, ref right, ref top);
    }

    /// <inheritdoc />
    public override void MoveStartPoint(Vector2 newStart)
    {
        _start = newStart;
    }

    /// <inheritdoc />
    public override void MoveEndPoint(Vector2 newEnd)
    {
        _end = newEnd;
    }

    /// <inheritdoc />
    public override void SplitInThirds(out EdgeSegment first, out EdgeSegment second, out EdgeSegment third)
    {
        first = new LinearSegment(_start, GetPoint(1 / 3d), this.Color);
        second = new LinearSegment(GetPoint(1 / 3d), GetPoint(2 / 3d), this.Color);
        third = new LinearSegment(GetPoint(2 / 3d), _end, this.Color);
    }

    private static Vector2 GetOrthonormal(Vector2 v, bool polarity, bool allowZero)
    {
        var len = v.Length();

        if (len == 0)
        {
            return polarity ? new Vector2(0, !allowZero ? 1 : 0) : new Vector2(0, -(!allowZero ? 1 : 0));
        }

        return polarity ? new Vector2(-v.Y / len, v.X / len) : new Vector2(v.Y / len, -v.X / len);
    }
}
