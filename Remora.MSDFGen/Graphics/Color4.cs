//
//  SPDX-FileName: Color4.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System.Drawing;
using System.Numerics;

namespace Remora.MSDFGen.Graphics;

/// <summary>
/// Represents a floating-point RGBA colour.
/// </summary>
public struct Color4
{
    private Vector4 _value;

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
    /// Gets or sets the alpha component of the colour.
    /// </summary>
    public float A
    {
        get => _value.W;
        set => _value.W = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color4"/> struct.
    /// </summary>
    /// <param name="color">The byte-packed colour.</param>
    public Color4(Color color)
    {
        _value = new Vector4(color.R, color.G, color.B, color.A);
    }
}
