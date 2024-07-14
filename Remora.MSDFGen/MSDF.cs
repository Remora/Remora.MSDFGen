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

namespace Remora.MSDFGen;

/// <summary>
/// Defines functions for calculated multi-channel signed distance fields.
/// </summary>
public static class MSDF
{
    public static bool PixelClash(Color4 a, Color4 b, double threshold)
    {
        var aIn = (a.R > .5f ? 1 : 0) + (a.G > .5f ? 1 : 0) + (a.B > .5f ? 1 : 0) >= 2;
        var bIn = (b.R > .5f ? 1 : 0) + (b.G > .5f ? 1 : 0) + (b.B > .5f ? 1 : 0) >= 2;
        if (aIn != bIn)
        {
            return false;
        }

        if
        (
            (a.R > .5f && a.G > .5f && a.B > .5f) ||
            (a.R < .5f && a.G < .5f && a.B < .5f) ||
            (b.R > .5f && b.G > .5f && b.B > .5f) ||
            (b.R < .5f && b.G < .5f && b.B < .5f)
        )
        {
            return false;
        }

        float aa, ab, ba, bb, ac, bc;

        if (a.R > .5f != b.R > .5f && a.R < .5f != b.R < .5f)
        {
            aa = a.R;
            ba = b.R;
            if (a.G > .5f != b.G > .5f && a.G < .5f != b.G < .5f)
            {
                ab = a.G;
                bb = b.G;
                ac = a.B;
                bc = b.B;
            }
            else if (a.B > .5f != b.B > .5f && a.B < .5f != b.B < .5f)
            {
                ab = a.B;
                bb = b.B;
                ac = a.G;
                bc = b.G;
            }
            else
            {
                return false;
            }
        }
        else if (a.G > .5f != b.G > .5f &&
                 a.G < .5f != b.G < .5f &&
                 a.B > .5f != b.B > .5f &&
                 a.B < .5f != b.B < .5f)
        {
            aa = a.G;
            ba = b.G;
            ab = a.B;
            bb = b.B;
            ac = a.R;
            bc = b.R;
        }
        else
        {
            return false;
        }

        return Math.Abs(aa - ba) >= threshold &&
               Math.Abs(ab - bb) >= threshold &&
               Math.Abs(ac - .5f) >= Math.Abs(bc - .5f);
    }

    public static bool PixelClash(Color a, Color b, double threshold)
    {
        var af = new Color4(a);
        var bf = new Color4(b);

        return PixelClash(af, bf, threshold);
    }

    private struct Clash
    {
        public int x;
        public int y;
    }

    public static void CorrectErrors(Bitmap<Color4> output, Rectangle region, Vector2 threshold)
    {
        var clashes = new List<Clash>();
        int w = output.Width;
        int h = output.Height;

        int xStart = Math.Min(Math.Max(0, region.Left), output.Width);
        int yStart = Math.Min(Math.Max(0, region.Top), output.Height);
        int xEnd = Math.Min(Math.Max(0, region.Right), output.Width);
        int yEnd = Math.Min(Math.Max(0, region.Bottom), output.Height);

        for (var y = yStart; y < yEnd; y++)
        {
            for (var x = xStart; x < xEnd; x++)
            {
                if ((x > 0 && PixelClash(output[x, y], output[x - 1, y], threshold.X)) ||
                    (x < w - 1 && PixelClash(output[x, y], output[x + 1, y], threshold.X)) ||
                    (y > 0 && PixelClash(output[x, y], output[x, y - 1], threshold.Y)) ||
                    (y < h - 1 && PixelClash(output[x, y], output[x, y + 1], threshold.Y)))
                {
                    clashes.Add(new Clash { x = x, y = y });
                }
            }
        }

        for (var i = 0; i < clashes.Count; i++)
        {
            Color4 pixel = output[clashes[i].x, clashes[i].y];
            float med = Median(pixel.R, pixel.G, pixel.B);
            pixel.R = med;
            pixel.G = med;
            pixel.B = med;
            output[clashes[i].x, clashes[i].y] = pixel;
        }
    }

    public static void CorrectErrors(Bitmap<Color> output, Rectangle region, Vector2 threshold)
    {
        var clashes = new List<Clash>();
        int w = output.Width;
        int h = output.Height;

        int xStart = Math.Min(Math.Max(0, region.Left), output.Width);
        int yStart = Math.Min(Math.Max(0, region.Top), output.Height);
        int xEnd = Math.Min(Math.Max(0, region.Right), output.Width);
        int yEnd = Math.Min(Math.Max(0, region.Bottom), output.Height);

        for (var y = yStart; y < yEnd; y++)
        {
            for (var x = xStart; x < xEnd; x++)
            {
                if ((x > 0 && PixelClash(output[x, y], output[x - 1, y], threshold.X)) ||
                    (x < w - 1 && PixelClash(output[x, y], output[x + 1, y], threshold.X)) ||
                    (y > 0 && PixelClash(output[x, y], output[x, y - 1], threshold.Y)) ||
                    (y < h - 1 && PixelClash(output[x, y], output[x, y + 1], threshold.Y)))
                {
                    clashes.Add(new Clash { x = x, y = y });
                }
            }
        }

        for (var i = 0; i < clashes.Count; i++)
        {
            Color pixel = output[clashes[i].x, clashes[i].y];
            var median = Median(pixel.R, pixel.G, pixel.B);

            pixel = Color.FromArgb(median, median, median, pixel.A);
            output[clashes[i].x, clashes[i].y] = pixel;
        }
    }

    private static float Median(float a, float b, float c)
    {
        return Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
    }

    private static double Median(double a, double b, double c)
    {
        return Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
    }

    private static int Median(int a, int b, int c)
    {
        return Math.Max(Math.Min(a, b), Math.Min(Math.Max(a, b), c));
    }

    public static void GenerateSDF(Bitmap<float> output, Shape shape, double range, Vector2 scale, Vector2 translate)
    {
        GenerateSDF(output, shape, new Rectangle(0, 0, output.Width, output.Height), range, scale, translate);
    }

    public static void GenerateSDF(Bitmap<byte> output, Shape shape, double range, Vector2 scale, Vector2 translate)
    {
        GenerateSDF(output, shape, new Rectangle(0, 0, output.Width, output.Height), range, scale, translate);
    }

    public static void GenerateSDF
    (
        Bitmap<float> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate
    )
    {
        var contourCount = shape.Contours.Count;
        var windings = new int[contourCount];

        for (var i = 0; i < shape.Contours.Count; i++)
        {
            windings[i] = shape.Contours[i].Winding;
        }

        int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
        int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
        int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
        int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

        var contourSD = new double[contourCount];

        for (var y = yStart; y < yEnd; y++)
        {
            var row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
            for (var x = xStart; x < xEnd; x++)
            {
                output[x, row] = EvaluateSDF
                (
                    shape,
                    windings,
                    contourSD,
                    x,
                    y,
                    range,
                    scale,
                    region.Location.ToVector() + translate
                );
            }
        }
    }

    public static void GenerateSDF
    (
        Bitmap<byte> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate
    )
    {
        var contourCount = shape.Contours.Count;
        var windings = new int[contourCount];

        for (var i = 0; i < shape.Contours.Count; i++)
        {
            windings[i] = shape.Contours[i].Winding;
        }

        int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
        int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
        int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
        int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

        var contourSD = new double[contourCount];

        for (var y = yStart; y < yEnd; y++)
        {
            var row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
            for (var x = xStart; x < xEnd; x++)
            {
                output[x, row] = (byte)Math.Min
                (
                    Math.Min
                    (
                        (int)(EvaluateSDF(shape, windings, contourSD, x, y, range, scale, region.Location.ToVector() + translate) * 255),
                        0
                    ),
                    255
                );
            }
        }
    }

    private static float EvaluateSDF
    (
        Shape shape,
        int[] windings,
        double[] contourSD,
        int x,
        int y,
        double range,
        Vector2 scale,
        Vector2 translate
    )
    {
        var contourCount = contourSD.Length;

        var p = (new Vector2(x + 0.5f, y + 0.5f) / scale) - translate;
        var negDist = -SignedDistance.Infinite.Distance;
        var posDist = SignedDistance.Infinite.Distance;
        var winding = 0;

        for (var i = 0; i < contourCount; i++)
        {
            var contour = shape.Contours[i];
            var minDistance = new SignedDistance(-1e240, 1);

            foreach (var edge in contour.Edges)
            {
                var distance = edge.GetSignedDistance(p, out _);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            contourSD[i] = minDistance.Distance;
            if (windings[i] > 0 && minDistance.Distance >= 0 && Math.Abs(minDistance.Distance) < Math.Abs(posDist))
            {
                posDist = minDistance.Distance;
            }

            if (windings[i] < 0 && minDistance.Distance <= 0 && Math.Abs(minDistance.Distance) < Math.Abs(negDist))
            {
                negDist = minDistance.Distance;
            }
        }

        var sd = SignedDistance.Infinite.Distance;

        if (posDist >= 0 && Math.Abs(posDist) <= Math.Abs(negDist))
        {
            sd = posDist;
            winding = 1;
            for (var i = 0; i < contourCount; i++)
            {
                if (windings[i] > 0 && contourSD[i] > sd && Math.Abs(contourSD[i]) < Math.Abs(negDist))
                {
                    sd = contourSD[i];
                }
            }
        }
        else if (negDist <= 0 && Math.Abs(negDist) <= Math.Abs(posDist))
        {
            sd = negDist;
            winding = -1;
            for (var i = 0; i < contourCount; i++)
            {
                if (windings[i] < 0 && contourSD[i] < sd && Math.Abs(contourSD[i]) < Math.Abs(posDist))
                {
                    sd = contourSD[i];
                }
            }
        }

        for (var i = 0; i < contourCount; i++)
        {
            if (windings[i] != winding && Math.Abs(contourSD[i]) < Math.Abs(sd))
            {
                sd = contourSD[i];
            }
        }

        return (float)(sd / range) + 0.5f;
    }

    private struct EdgePoint
    {
        public SignedDistance minDistance;
        public EdgeSegment nearEdge;
        public double nearParam;
    }

    public static void GenerateMSDF
    (
        Bitmap<Color3> output,
        Shape shape,
        double range,
        Vector2 scale,
        Vector2 translate,
        double edgeThreshold
    )
    {
        GenerateMSDF
        (
            output,
            shape,
            new Rectangle(0, 0, output.Width, output.Height),
            range,
            scale,
            translate,
            edgeThreshold
        );
    }

    public static void GenerateMSDF
    (
        Bitmap<Color3b> output,
        Shape shape,
        double range,
        Vector2 scale,
        Vector2 translate,
        double edgeThreshold
    )
    {
        GenerateMSDF
        (
            output,
            shape,
            new Rectangle(0, 0, output.Width, output.Height),
            range,
            scale,
            translate,
            edgeThreshold
        );
    }

    public static void GenerateMSDF
    (
        Bitmap<Color3> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate,
        double edgeThreshold
    )
    {
        var contourCount = shape.Contours.Count;
        var windings = new int[contourCount];

        for (var i = 0; i < shape.Contours.Count; i++)
        {
            windings[i] = shape.Contours[i].Winding;
        }

        int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
        int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
        int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
        int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

        var contourSD = new MultiDistance[contourCount];

        for (var y = yStart; y < yEnd; y++)
        {
            var row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
            for (var x = xStart; x < xEnd; x++)
            {
                var p = (new Vector2(x, y) - region.Location.ToVector() - translate) / scale;
                output[x, row] = EvaluateMSDF(shape, windings, contourSD, p, range);
            }
        }
    }

    public static void GenerateMSDF
    (
        Bitmap<Color3b> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate,
        double edgeThreshold
    )
    {
        var contourCount = shape.Contours.Count;
        var windings = new int[contourCount];

        for (var i = 0; i < shape.Contours.Count; i++)
        {
            windings[i] = shape.Contours[i].Winding;
        }

        int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
        int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
        int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
        int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

        var contourSD = new MultiDistance[contourCount];

        for (var y = yStart; y < yEnd; y++)
        {
            var row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
            for (var x = xStart; x < xEnd; x++)
            {
                var p = (new Vector2(x, y) - region.Location.ToVector() - translate) / scale;
                output[x, row] = new Color3b(EvaluateMSDF(shape, windings, contourSD, p, range));
            }
        }
    }

    public static void GenerateMSDF
    (
        Bitmap<Color> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate,
        double edgeThreshold
    )
    {
        var contourCount = shape.Contours.Count;
        var windings = new int[contourCount];

        for (var i = 0; i < shape.Contours.Count; i++)
        {
            windings[i] = shape.Contours[i].Winding;
        }

        int xStart = Math.Min(Math.Max(0, (int)region.Left), output.Width);
        int yStart = Math.Min(Math.Max(0, (int)region.Top), output.Height);
        int xEnd = Math.Min(Math.Max(0, (int)region.Right), output.Width);
        int yEnd = Math.Min(Math.Max(0, (int)region.Bottom), output.Height);

        var contourSD = new MultiDistance[contourCount];

        for (var y = yStart; y < yEnd; y++)
        {
            var row = shape.InverseYAxis ? yEnd - (y - yStart) - 1 : y;
            for (var x = xStart; x < xEnd; x++)
            {
                var p = (new Vector2(x, y) - region.Location.ToVector() - translate) / scale;
                output[x, row] = new Color(EvaluateMSDF(shape, windings, contourSD, p, range), 255);
            }
        }
    }

    private static Color3 EvaluateMSDF(Shape shape, int[] windings, MultiDistance[] contourSD, Vector2 p, double range)
    {
        var contourCount = contourSD.Length;
        p += new Vector2(0.5f, 0.5f);

        var sr = new EdgePoint
        {
            minDistance = new SignedDistance(-1e240, 1)
        };
        var sg = new EdgePoint
        {
            minDistance = new SignedDistance(-1e240, 1)
        };
        var sb = new EdgePoint
        {
            minDistance = new SignedDistance(-1e240, 1)
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
                minDistance = new SignedDistance(-1e240, 1)
            };
            var g = new EdgePoint
            {
                minDistance = new SignedDistance(-1e240, 1)
            };
            var b = new EdgePoint
            {
                minDistance = new SignedDistance(-1e240, 1)
            };

            foreach (var edge in contour.Edges)
            {
                var distance = edge.GetSignedDistance(p, out var param);
                if ((edge.Color & EdgeColor.Red) == EdgeColor.Red && distance < r.minDistance)
                {
                    r.minDistance = distance;
                    r.nearEdge = edge;
                    r.nearParam = param;
                }

                if ((edge.Color & EdgeColor.Green) == EdgeColor.Green && distance < g.minDistance)
                {
                    g.minDistance = distance;
                    g.nearEdge = edge;
                    g.nearParam = param;
                }

                if ((edge.Color & EdgeColor.Blue) != EdgeColor.Blue || distance >= b.minDistance)
                {
                    continue;
                }

                b.minDistance = distance;
                b.nearEdge = edge;
                b.nearParam = param;
            }

            if (r.minDistance < sr.minDistance)
            {
                sr = r;
            }

            if (g.minDistance < sg.minDistance)
            {
                sg = g;
            }

            if (b.minDistance < sb.minDistance)
            {
                sb = b;
            }

            var medMinDistance =
                Math.Abs(Median(r.minDistance.Distance, g.minDistance.Distance, b.minDistance.Distance));

            if (medMinDistance < d)
            {
                d = medMinDistance;
                winding = -windings[i];
            }

            r.nearEdge?.DistanceToPseudoDistance(ref r.minDistance, p, r.nearParam);
            g.nearEdge?.DistanceToPseudoDistance(ref g.minDistance, p, g.nearParam);
            b.nearEdge?.DistanceToPseudoDistance(ref b.minDistance, p, b.nearParam);

            medMinDistance = Median(r.minDistance.Distance, g.minDistance.Distance, b.minDistance.Distance);

            contourSD[i].R = r.minDistance.Distance;
            contourSD[i].G = g.minDistance.Distance;
            contourSD[i].B = b.minDistance.Distance;
            contourSD[i].Median = medMinDistance;

            if (windings[i] > 0 && medMinDistance >= 0 && Math.Abs(medMinDistance) < Math.Abs(posDist))
            {
                posDist = medMinDistance;
            }

            if (windings[i] < 0 && medMinDistance <= 0 && Math.Abs(medMinDistance) < Math.Abs(negDist))
            {
                negDist = medMinDistance;
            }
        }

        sr.nearEdge?.DistanceToPseudoDistance(ref sr.minDistance, p, sr.nearParam);
        sg.nearEdge?.DistanceToPseudoDistance(ref sg.minDistance, p, sg.nearParam);
        sb.nearEdge?.DistanceToPseudoDistance(ref sb.minDistance, p, sb.nearParam);

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
                if (windings[i] > 0 && contourSD[i].Median > msd.Median && Math.Abs(contourSD[i].Median) < Math.Abs(negDist))
                {
                    msd = contourSD[i];
                }
            }
        }
        else if (negDist <= 0 && Math.Abs(negDist) <= Math.Abs(posDist))
        {
            msd.Median = -SignedDistance.Infinite.Distance;
            winding = -1;
            for (var i = 0; i < contourCount; i++)
            {
                if (windings[i] < 0 && contourSD[i].Median < msd.Median && Math.Abs(contourSD[i].Median) < Math.Abs(posDist))
                {
                    msd = contourSD[i];
                }
            }
        }

        for (var i = 0; i < contourCount; i++)
        {
            if (windings[i] != winding && Math.Abs(contourSD[i].Median) < Math.Abs(msd.Median))
            {
                msd = contourSD[i];
            }
        }

        if (Median(sr.minDistance.Distance, sg.minDistance.Distance, sb.minDistance.Distance) != msd.Median)
        {
            return new Color3
            (
                (float)(msd.R / range) + 0.5f,
                (float)(msd.G / range) + 0.5f,
                (float)(msd.B / range) + 0.5f
            );
        }

        msd.R = sr.minDistance.Distance;
        msd.G = sg.minDistance.Distance;
        msd.B = sb.minDistance.Distance;

        return new Color3((float)(msd.R / range) + 0.5f, (float)(msd.G / range) + 0.5f, (float)(msd.B / range) + 0.5f);
    }

    private static readonly EdgeColor[] _switchColors = [EdgeColor.Cyan, EdgeColor.Magenta, EdgeColor.Yellow];

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

    public static void EdgeColoringSimple(Shape shape, double angleThreshold, ulong seed)
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
                                var magic = (int)(3 + (2.875f * j / (m - 1)) - 1.4375f + 0.5f) - 3; //see edge-coloring.cpp in the original msdfgen
                                contour.Edges[(corner + j) % m].Color = colors[1 + magic];
                            }

                            break;
                        }
                        case >= 1:
                        {
                            var parts = new EdgeSegment[7];
                            contour.Edges[0].SplitInThirds(
                                out parts[0 + (3 * corner)],
                                out parts[1 + (3 * corner)],
                                out parts[2 + (3 * corner)]);
                            if (contour.Edges.Count >= 2)
                            {
                                contour.Edges[1].SplitInThirds(
                                    out parts[3 - (3 * corner)],
                                    out parts[4 - (3 * corner)],
                                    out parts[5 - (3 * corner)]);
                                parts[0].Color = colors[0];
                                parts[1].Color = colors[0];
                                parts[2].Color = colors[1];
                                parts[3].Color = colors[1];
                                parts[4].Color = colors[2];
                                parts[5].Color = colors[2];
                            }
                            else
                            {
                                parts[0].Color = colors[0];
                                parts[1].Color = colors[1];
                                parts[2].Color = colors[2];
                            }

                            contour.Edges.Clear();
                            for (var j = 0; parts[j] != null; j++)
                            {
                                contour.Edges.Add(parts[j]);
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

    private static bool IsCorner(Vector2 aDir, Vector2 bDir, double crossThreshold)
    {
        return Vector2.Dot(aDir, bDir) <= 0 || Math.Abs(aDir.Cross(bDir)) > crossThreshold;
    }
}
