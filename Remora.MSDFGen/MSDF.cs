//
//  SPDX-FileName: MSDF.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Remora.MSDFGen.Extensions;
using Remora.MSDFGen.Graphics;
using Remora.MSDFGen.Utility;

namespace Remora.MSDFGen;

/// <summary>
/// Defines functions for calculated multichannel signed distance fields.
/// </summary>
public static class MSDF
{
    /// <summary>
    /// Generates a multichannel signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateMSDF
    (
        Pixmap<Color3> output,
        Shape shape,
        double range,
        Vector2 scale,
        Vector2 translate
    ) => GenerateMSDF
    (
        output,
        c => c,
        shape,
        new Rectangle(0, 0, output.Width, output.Height),
        range,
        scale,
        translate
    );

    /// <summary>
    /// Generates a multichannel signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateMSDF
    (
        Pixmap<Color3b> output,
        Shape shape,
        double range,
        Vector2 scale,
        Vector2 translate
    ) => GenerateMSDF
    (
        output,
        c => new Color3b(c),
        shape,
        new Rectangle(0, 0, output.Width, output.Height),
        range,
        scale,
        translate
    );

    /// <summary>
    /// Generates a multi-channel signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="region">The region within the shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateMSDF
    (
        Pixmap<Color3> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate
    ) => GenerateMSDF(output, c => c, shape, region, range, scale, translate);

    /// <summary>
    /// Generates a multi-channel signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="region">The region within the shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateMSDF
    (
        Pixmap<Color3b> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate
    ) => GenerateMSDF(output, c => new Color3b(c), shape, region, range, scale, translate);

    /// <summary>
    /// Generates a multi-channel signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="region">The region within the shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateMSDF
    (
        Pixmap<Color> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate
    ) => GenerateMSDF(output, c => c.ToColor(), shape, region, range, scale, translate);

    private static void GenerateMSDF<TPixel>
    (
        Pixmap<TPixel> output,
        Func<Color3, TPixel> converter,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate
    ) where TPixel : struct
    {
        var contourCount = shape.Contours.Count;
        var windings = new int[contourCount];

        for (var i = 0; i < shape.Contours.Count; i++)
        {
            windings[i] = shape.Contours[i].Winding;
        }

        var horizontalStart = Math.Min(Math.Max(0, region.Left), output.Width);
        var verticalStart = Math.Min(Math.Max(0, region.Top), output.Height);
        var horizontalEnd = Math.Min(Math.Max(0, region.Right), output.Width);
        var verticalEnd = Math.Min(Math.Max(0, region.Bottom), output.Height);

        var contourSignedDistance = new MultiDistance[contourCount];

        for (var y = verticalStart; y < verticalEnd; y++)
        {
            var row = shape.InverseYAxis ? verticalEnd - (y - verticalStart) - 1 : y;
            for (var x = horizontalStart; x < horizontalEnd; x++)
            {
                var p = (new Vector2(x, y) - region.Location.ToVector() - translate) / scale;
                output[x, row] = converter(EvaluateMSDF(shape, windings, contourSignedDistance, p, range));
            }
        }
    }

    private struct EdgePoint
    {
        public SignedDistance MinDistance;
        public EdgeSegment NearEdge;
        public double NearParam;
    }

    private static Color3 EvaluateMSDF(Shape shape, int[] windings, MultiDistance[] contourSignedDistance, Vector2 p, double range)
    {
        var contourCount = contourSignedDistance.Length;
        p += new Vector2(0.5f, 0.5f);

        var sr = new EdgePoint
        {
            MinDistance = new SignedDistance(-1e240, 1)
        };
        var sg = new EdgePoint
        {
            MinDistance = new SignedDistance(-1e240, 1)
        };
        var sb = new EdgePoint
        {
            MinDistance = new SignedDistance(-1e240, 1)
        };

        var d = Math.Abs(SignedDistance.Infinite.Distance);
        var negDist = -SignedDistance.Infinite.Distance;
        var posDist = SignedDistance.Infinite.Distance;
        var winding = 0;

        for (var i = 0; i < contourCount; i++)
        {
            var contour = shape.Contours[i];
            var r = new EdgePoint
            {
                MinDistance = new SignedDistance(-1e240, 1)
            };
            var g = new EdgePoint
            {
                MinDistance = new SignedDistance(-1e240, 1)
            };
            var b = new EdgePoint
            {
                MinDistance = new SignedDistance(-1e240, 1)
            };

            foreach (var edge in contour.Edges)
            {
                var distance = edge.GetSignedDistance(p, out var param);
                if ((edge.Color & EdgeColor.Red) == EdgeColor.Red && distance < r.MinDistance)
                {
                    r.MinDistance = distance;
                    r.NearEdge = edge;
                    r.NearParam = param;
                }

                if ((edge.Color & EdgeColor.Green) == EdgeColor.Green && distance < g.MinDistance)
                {
                    g.MinDistance = distance;
                    g.NearEdge = edge;
                    g.NearParam = param;
                }

                if ((edge.Color & EdgeColor.Blue) != EdgeColor.Blue || distance >= b.MinDistance)
                {
                    continue;
                }

                b.MinDistance = distance;
                b.NearEdge = edge;
                b.NearParam = param;
            }

            if (r.MinDistance < sr.MinDistance)
            {
                sr = r;
            }

            if (g.MinDistance < sg.MinDistance)
            {
                sg = g;
            }

            if (b.MinDistance < sb.MinDistance)
            {
                sb = b;
            }

            var medMinDistance =
                Math.Abs(ExtraMath.Median(r.MinDistance.Distance, g.MinDistance.Distance, b.MinDistance.Distance));

            if (medMinDistance < d)
            {
                d = medMinDistance;
                winding = -windings[i];
            }

            r.NearEdge?.DistanceToPseudoDistance(ref r.MinDistance, p, r.NearParam);
            g.NearEdge?.DistanceToPseudoDistance(ref g.MinDistance, p, g.NearParam);
            b.NearEdge?.DistanceToPseudoDistance(ref b.MinDistance, p, b.NearParam);

            medMinDistance = ExtraMath.Median(r.MinDistance.Distance, g.MinDistance.Distance, b.MinDistance.Distance);

            contourSignedDistance[i].R = r.MinDistance.Distance;
            contourSignedDistance[i].G = g.MinDistance.Distance;
            contourSignedDistance[i].B = b.MinDistance.Distance;
            contourSignedDistance[i].Median = medMinDistance;

            if (windings[i] > 0 && medMinDistance >= 0 && Math.Abs(medMinDistance) < Math.Abs(posDist))
            {
                posDist = medMinDistance;
            }

            if (windings[i] < 0 && medMinDistance <= 0 && Math.Abs(medMinDistance) < Math.Abs(negDist))
            {
                negDist = medMinDistance;
            }
        }

        sr.NearEdge?.DistanceToPseudoDistance(ref sr.MinDistance, p, sr.NearParam);
        sg.NearEdge?.DistanceToPseudoDistance(ref sg.MinDistance, p, sg.NearParam);
        sb.NearEdge?.DistanceToPseudoDistance(ref sb.MinDistance, p, sb.NearParam);

        var msd = new MultiDistance
        {
            R = SignedDistance.Infinite.Distance,
            G = SignedDistance.Infinite.Distance,
            B = SignedDistance.Infinite.Distance,
            Median = SignedDistance.Infinite.Distance
        };

        if (posDist >= 0 && Math.Abs(posDist) <= Math.Abs(negDist))
        {
            msd.Median = SignedDistance.Infinite.Distance;
            winding = 1;
            for (var i = 0; i < contourCount; i++)
            {
                if (windings[i] > 0 && contourSignedDistance[i].Median > msd.Median &&
                    Math.Abs(contourSignedDistance[i].Median) < Math.Abs(negDist))
                {
                    msd = contourSignedDistance[i];
                }
            }
        }
        else if (negDist <= 0 && Math.Abs(negDist) <= Math.Abs(posDist))
        {
            msd.Median = -SignedDistance.Infinite.Distance;
            winding = -1;
            for (var i = 0; i < contourCount; i++)
            {
                if (windings[i] < 0 && contourSignedDistance[i].Median < msd.Median &&
                    Math.Abs(contourSignedDistance[i].Median) < Math.Abs(posDist))
                {
                    msd = contourSignedDistance[i];
                }
            }
        }

        for (var i = 0; i < contourCount; i++)
        {
            if (windings[i] != winding && Math.Abs(contourSignedDistance[i].Median) < Math.Abs(msd.Median))
            {
                msd = contourSignedDistance[i];
            }
        }

        if (ExtraMath.Median(sr.MinDistance.Distance, sg.MinDistance.Distance, sb.MinDistance.Distance) != msd.Median)
        {
            return new Color3
            (
                (float)(msd.R / range) + 0.5f,
                (float)(msd.G / range) + 0.5f,
                (float)(msd.B / range) + 0.5f
            );
        }

        msd.R = sr.MinDistance.Distance;
        msd.G = sg.MinDistance.Distance;
        msd.B = sb.MinDistance.Distance;

        return new Color3((float)(msd.R / range) + 0.5f, (float)(msd.G / range) + 0.5f, (float)(msd.B / range) + 0.5f);
    }

    private static readonly EdgeColor[] _switchColors = { EdgeColor.Cyan, EdgeColor.Magenta, EdgeColor.Yellow };

    /// <summary>
    /// Assigns colours to edges of the given shape in accordance to the multi-channel distance field algorithm. May
    /// split some edges if necessary.
    /// </summary>
    /// <param name="shape">The shape.</param>
    /// <param name="angleThreshold">
    /// The maximum angle (in radians) to be considered a corner. Values below 1/2 Pi will be treated as the external
    /// angle.
    /// </param>
    /// <param name="seed">The seed value used as the initial starting point for colour switching.</param>
    public static void EdgeColoringSimple(Shape shape, double angleThreshold, ulong seed = 0)
    {
        var crossThreshold = Math.Sin(angleThreshold);
        var corners = new List<int>();
        foreach (var contour in shape.Contours)
        {
            corners.Clear();

            if (contour.Edges.Count != 0)
            {
                var prevDirection = contour.Edges[^1].GetDirection(1);

                for (var j = 0; j < contour.Edges.Count; j++)
                {
                    var edge = contour.Edges[j];
                    if
                    (
                        IsCorner
                        (
                            Vector2.Normalize(prevDirection),
                            Vector2.Normalize(edge.GetDirection(0)),
                            crossThreshold
                        )
                    )
                    {
                        corners.Add(j);
                    }

                    prevDirection = edge.GetDirection(1);
                }
            }

            switch (corners.Count)
            {
                case 0:
                {
                    foreach (var edge in contour.Edges)
                    {
                        edge.Color = EdgeColor.White;
                    }

                    break;
                }
                case 1:
                {
                    EdgeColor[] colors = { EdgeColor.White, EdgeColor.White, EdgeColor.Black };
                    SwitchColor(ref colors[0], ref seed, EdgeColor.Black);
                    SwitchColor(ref colors[2], ref seed, EdgeColor.Black);

                    var corner = corners[0];

                    switch (contour.Edges.Count)
                    {
                        case >= 3:
                        {
                            var m = contour.Edges.Count;
                            for (var j = 0; j < m; j++)
                            {
                                // see edge-coloring.cpp in the original msdfgen
                                var magic = (int)(3 + (2.875f * j / (m - 1)) - 1.4375f + 0.5f) - 3;
                                contour.Edges[(corner + j) % m].Color = colors[1 + magic];
                            }

                            break;
                        }
                        case >= 1:
                        {
                            var parts = new EdgeSegment?[7];
                            contour.Edges[0].SplitInThirds
                            (
                                out parts[0 + (3 * corner)],
                                out parts[1 + (3 * corner)],
                                out parts[2 + (3 * corner)]
                            );

                            if (contour.Edges.Count >= 2)
                            {
                                contour.Edges[1].SplitInThirds
                                (
                                    out parts[3 - (3 * corner)],
                                    out parts[4 - (3 * corner)],
                                    out parts[5 - (3 * corner)]
                                );

                                parts[0]!.Color = colors[0];
                                parts[1]!.Color = colors[0];
                                parts[2]!.Color = colors[1];
                                parts[3]!.Color = colors[1];
                                parts[4]!.Color = colors[2];
                                parts[5]!.Color = colors[2];
                            }
                            else
                            {
                                parts[0]!.Color = colors[0];
                                parts[1]!.Color = colors[1];
                                parts[2]!.Color = colors[2];
                            }

                            contour.Edges.Clear();
                            for (var j = 0; parts[j] != null; j++)
                            {
                                contour.Edges.Add(parts[j]!);
                            }

                            break;
                        }
                    }

                    break;
                }
                default:
                {
                    var cornerCount = corners.Count;
                    var spline = 0;
                    var start = corners[0];
                    var m = contour.Edges.Count;
                    var color = EdgeColor.White;
                    SwitchColor(ref color, ref seed, EdgeColor.Black);
                    var initialColor = color;
                    for (var j = 0; j < m; j++)
                    {
                        var index = (start + j) % m;
                        if (spline + 1 < cornerCount && corners[spline + 1] == index)
                        {
                            spline++;
                            SwitchColor
                            (
                                ref color,
                                ref seed,
                                (EdgeColor)((spline == cornerCount - 1 ? 1 : 0) * (int)initialColor)
                            );
                        }

                        contour.Edges[index].Color = color;
                    }

                    break;
                }
            }
        }
    }

    private static void SwitchColor(ref EdgeColor color, ref ulong seed, EdgeColor banned)
    {
        var combined = color & banned;

        if (combined is EdgeColor.Red or EdgeColor.Green or EdgeColor.Blue)
        {
            color = combined ^ EdgeColor.White;
            return;
        }

        if (color is EdgeColor.Black or EdgeColor.White)
        {
            color = _switchColors[seed % 3];
            seed /= 3;
            return;
        }

        var shifted = (int)color << (int)(1 + (seed & 1));
        color = (EdgeColor)((shifted | (shifted >> 3)) & (int)EdgeColor.White);
        seed >>= 1;
    }

    private static bool IsCorner(Vector2 firstDirection, Vector2 secondDirection, double crossThreshold)
    {
        return Vector2.Dot
        (
            firstDirection,
            secondDirection
        ) <= 0 || Math.Abs(firstDirection.Cross(secondDirection)) > crossThreshold;
    }

    private static bool PixelClash(Color4 first, Color4 second, double threshold)
    {
        var firstIn = (first.R > .5f ? 1 : 0) + (first.G > .5f ? 1 : 0) + (first.B > .5f ? 1 : 0) >= 2;
        var secondIn = (second.R > .5f ? 1 : 0) + (second.G > .5f ? 1 : 0) + (second.B > .5f ? 1 : 0) >= 2;
        if (firstIn != secondIn)
        {
            return false;
        }

        if
        (
            (first.R > .5f && first is { G: > .5f, B: > .5f }) ||
            (first.R < .5f && first is { G: < .5f, B: < .5f }) ||
            (second.R > .5f && second is { G: > .5f, B: > .5f }) ||
            (second.R < .5f && second is { G: < .5f, B: < .5f })
        )
        {
            return false;
        }

        float aa, ab, ba, bb, ac, bc;

        if (first.R > .5f != second.R > .5f && first.R < .5f != second.R < .5f)
        {
            aa = first.R;
            ba = second.R;
            if (first.G > .5f != second.G > .5f && first.G < .5f != second.G < .5f)
            {
                ab = first.G;
                bb = second.G;
                ac = first.B;
                bc = second.B;
            }
            else if (first.B > .5f != second.B > .5f && first.B < .5f != second.B < .5f)
            {
                ab = first.B;
                bb = second.B;
                ac = first.G;
                bc = second.G;
            }
            else
            {
                return false;
            }
        }
        else if
        (
            first.G > .5f != second.G > .5f &&
            first.G < .5f != second.G < .5f &&
            first.B > .5f != second.B > .5f &&
            first.B < .5f != second.B < .5f
        )
        {
            aa = first.G;
            ba = second.G;
            ab = first.B;
            bb = second.B;
            ac = first.R;
            bc = second.R;
        }
        else
        {
            return false;
        }

        return Math.Abs(aa - ba) >= threshold &&
               Math.Abs(ab - bb) >= threshold &&
               Math.Abs(ac - .5f) >= Math.Abs(bc - .5f);
    }

    private static bool PixelClash(Color a, Color b, double threshold)
    {
        var af = new Color4(a);
        var bf = new Color4(b);

        return PixelClash(af, bf, threshold);
    }

    private struct Clash
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// Predicts potential artifacts caused by the interpolation of the MSDF and corrects them by converting nearby
    /// texels to single-channel.
    /// </summary>
    /// <param name="output">The multi-channel signed distance field.</param>
    /// <param name="region">The region within which to apply the correction.</param>
    /// <param name="threshold">The threshold distance between texels.</param>
    public static void CorrectErrors(Pixmap<Color4> output, Rectangle region, Vector2 threshold)
    {
        var clashes = new List<Clash>();
        var w = output.Width;
        var h = output.Height;

        var horizontalStart = Math.Min(Math.Max(0, region.Left), output.Width);
        var verticalStart = Math.Min(Math.Max(0, region.Top), output.Height);
        var horizontalEnd = Math.Min(Math.Max(0, region.Right), output.Width);
        var verticalEnd = Math.Min(Math.Max(0, region.Bottom), output.Height);

        for (var y = verticalStart; y < verticalEnd; y++)
        {
            for (var x = horizontalStart; x < horizontalEnd; x++)
            {
                if
                (
                    (x > 0 && PixelClash(output[x, y], output[x - 1, y], threshold.X)) ||
                    (x < w - 1 && PixelClash(output[x, y], output[x + 1, y], threshold.X)) ||
                    (y > 0 && PixelClash(output[x, y], output[x, y - 1], threshold.Y)) ||
                    (y < h - 1 && PixelClash(output[x, y], output[x, y + 1], threshold.Y))
                )
                {
                    clashes.Add(new Clash { X = x, Y = y });
                }
            }
        }

        for (var i = 0; i < clashes.Count; i++)
        {
            var pixel = output[clashes[i].X, clashes[i].Y];
            var med = ExtraMath.Median(pixel.R, pixel.G, pixel.B);
            pixel.R = med;
            pixel.G = med;
            pixel.B = med;
            output[clashes[i].X, clashes[i].Y] = pixel;
        }
    }

    /// <summary>
    /// Predicts potential artifacts caused by the interpolation of the MSDF and corrects them by converting nearby
    /// texels to single-channel.
    /// </summary>
    /// <param name="output">The multi-channel signed distance field.</param>
    /// <param name="region">The region within which to apply the correction.</param>
    /// <param name="threshold">The threshold distance between texels.</param>
    public static void CorrectErrors(Pixmap<Color> output, Rectangle region, Vector2 threshold)
    {
        var clashes = new List<Clash>();
        var w = output.Width;
        var h = output.Height;

        var horizontalStart = Math.Min(Math.Max(0, region.Left), output.Width);
        var verticalStart = Math.Min(Math.Max(0, region.Top), output.Height);
        var horizontalEnd = Math.Min(Math.Max(0, region.Right), output.Width);
        var verticalEnd = Math.Min(Math.Max(0, region.Bottom), output.Height);

        for (var y = verticalStart; y < verticalEnd; y++)
        {
            for (var x = horizontalStart; x < horizontalEnd; x++)
            {
                if
                (
                    (x > 0 && PixelClash(output[x, y], output[x - 1, y], threshold.X)) ||
                    (x < w - 1 && PixelClash(output[x, y], output[x + 1, y], threshold.X)) ||
                    (y > 0 && PixelClash(output[x, y], output[x, y - 1], threshold.Y)) ||
                    (y < h - 1 && PixelClash(output[x, y], output[x, y + 1], threshold.Y))
                )
                {
                    clashes.Add(new Clash { X = x, Y = y });
                }
            }
        }

        for (var i = 0; i < clashes.Count; i++)
        {
            var pixel = output[clashes[i].X, clashes[i].Y];
            var median = ExtraMath.Median(pixel.R, pixel.G, pixel.B);

            pixel = Color.FromArgb(median, median, median, pixel.A);
            output[clashes[i].X, clashes[i].Y] = pixel;
        }
    }
}
