//
//  SPDX-FileName: Color3.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System.Drawing;
using System.Numerics;

namespace Remora.MSDFGen.Graphics;

/// <summary>
/// Represents a floating-point RGB colour.
/// </summary>
public struct Color3
{
    private Vector3 _value;

    /// <summary>
    /// Gets or sets the red component of the colour.
    /// </summary>
    public float R
    {
        get => _value.X;
        set => _value.X = value;
    }

    /// <summary>
    /// Gets or sets the green component of the colour.
    /// </summary>
    public float G
    {
        get => _value.Y;
        set => _value.Y = value;
    }

    /// <summary>
    /// Gets or sets the blue component of the colour.
    /// </summary>
    public float B
    {
        get => _value.Z;
        set => _value.Z = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color3"/> struct.
    /// </summary>
    /// <param name="color">The byte-packed colour.</param>
    public Color3(Color color)
    {
        _value = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color3"/> struct.
    /// </summary>
    /// <param name="r">The red component of the colour.</param>
    /// <param name="g">The green component of the colour.</param>
    /// <param name="b">The blue component of the colour.</param>
    public Color3(float r, float g, float b)
    {
        _value = new Vector3(r, g, b);
    }

    /// <summary>
    /// Converts the floating-point RGB colour to a packed 8-bit element colour.
    /// </summary>
    /// <returns>The packed colour.</returns>
    public Color ToColor() => Color.FromArgb((int)(_value.X * 255f), (int)(_value.Y * 255f), (int)(_value.Z * 255f));
}
