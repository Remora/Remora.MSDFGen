//
//  SPDX-FileName: SDF.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;
using System.Drawing;
using System.Numerics;
using JetBrains.Annotations;
using Remora.MSDFGen.Extensions;
using Remora.MSDFGen.Graphics;

namespace Remora.MSDFGen;

/// <summary>
/// Defines functions for generating signed distance fields.
/// </summary>
[PublicAPI]
public static class SDF
{
    /// <summary>
    /// Generates a signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateSDF(Pixmap<float> output, Shape shape, double range, Vector2 scale, Vector2 translate)
        => GenerateSDF(output, shape, new Rectangle(0, 0, output.Width, output.Height), range, scale, translate);

    /// <summary>
    /// Generates a signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateSDF(Pixmap<byte> output, Shape shape, double range, Vector2 scale, Vector2 translate)
        => GenerateSDF(output, shape, new Rectangle(0, 0, output.Width, output.Height), range, scale, translate);

    /// <summary>
    /// Generates a signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="region">The region within the shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateSDF
    (
        Pixmap<float> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate
    ) => GenerateSDF
    (
        output,
        p => p,
        shape,
        region,
        range,
        scale,
        translate
    );

    /// <summary>
    /// Generates a signed distance field for the given shape.
    /// </summary>
    /// <param name="output">The pixmap where the resulting signed distance field is generated.</param>
    /// <param name="shape">The shape to generate the field for.</param>
    /// <param name="region">The region within the shape to generate the field for.</param>
    /// <param name="range">
    /// The width of the range around the shape between the minimum and maximum representable signed distance.
    /// </param>
    /// <param name="scale">The scale used to convert shape units to distance field pixels.</param>
    /// <param name="translate">The translation of the shape in shape units.</param>
    public static void GenerateSDF
    (
        Pixmap<byte> output,
        Shape shape,
        Rectangle region,
        double range,
        Vector2 scale,
        Vector2 translate
    ) => GenerateSDF
    (
        output,
        p => (byte)Math.Min(Math.Min((int)(p * 255), 0), 255),
        shape,
        region,
        range,
        scale,
        translate
    );

    private static void GenerateSDF<TPixel>
    (
        Pixmap<TPixel> output,
        Func<float, TPixel> converter,
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

        var contourSignedDistance = new double[contourCount];

        for (var y = verticalStart; y < verticalEnd; y++)
        {
            var row = shape.InverseYAxis ? verticalEnd - (y - verticalStart) - 1 : y;
            for (var x = horizontalStart; x < horizontalEnd; x++)
            {
                output[x, row] = converter
                (
                    EvaluateSDF
                    (
                        shape,
                        windings,
                        contourSignedDistance,
                        x,
                        y,
                        range,
                        scale,
                        region.Location.ToVector() + translate
                    )
                );
            }
        }
    }

    private static float EvaluateSDF
    (
        Shape shape,
        int[] windings,
        double[] contourSignedDistance,
        int x,
        int y,
        double range,
        Vector2 scale,
        Vector2 translate
    )
    {
        var contourCount = contourSignedDistance.Length;

        var p = (new Vector2(x + 0.5f, y + 0.5f) / scale) - translate;
        var negativeDistance = -SignedDistance.Infinite.Distance;
        var positiveDistance = SignedDistance.Infinite.Distance;
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

            contourSignedDistance[i] = minDistance.Distance;
            if (windings[i] > 0 && minDistance.Distance >= 0 && Math.Abs(minDistance.Distance) < Math.Abs(positiveDistance))
            {
                positiveDistance = minDistance.Distance;
            }

            if (windings[i] < 0 && minDistance.Distance <= 0 && Math.Abs(minDistance.Distance) < Math.Abs(negativeDistance))
            {
                negativeDistance = minDistance.Distance;
            }
        }

        var sd = SignedDistance.Infinite.Distance;

        if (positiveDistance >= 0 && Math.Abs(positiveDistance) <= Math.Abs(negativeDistance))
        {
            sd = positiveDistance;
            winding = 1;
            for (var i = 0; i < contourCount; i++)
            {
                if (windings[i] > 0 && contourSignedDistance[i] > sd && Math.Abs(contourSignedDistance[i]) < Math.Abs(negativeDistance))
                {
                    sd = contourSignedDistance[i];
                }
            }
        }
        else if (negativeDistance <= 0 && Math.Abs(negativeDistance) <= Math.Abs(positiveDistance))
        {
            sd = negativeDistance;
            winding = -1;
            for (var i = 0; i < contourCount; i++)
            {
                if (windings[i] < 0 && contourSignedDistance[i] < sd && Math.Abs(contourSignedDistance[i]) < Math.Abs(positiveDistance))
                {
                    sd = contourSignedDistance[i];
                }
            }
        }

        for (var i = 0; i < contourCount; i++)
        {
            if (windings[i] != winding && Math.Abs(contourSignedDistance[i]) < Math.Abs(sd))
            {
                sd = contourSignedDistance[i];
            }
        }

        return (float)(sd / range) + 0.5f;
    }
}
