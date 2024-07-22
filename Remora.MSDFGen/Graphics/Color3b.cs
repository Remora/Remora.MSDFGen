//
//  SPDX-FileName: Color3b.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System.Drawing;

namespace Remora.MSDFGen.Graphics;

/// <summary>
/// Represents a byte-packed RGB colour.
/// </summary>
public struct Color3b
{
    /// <summary>
    /// Gets or sets the red component of the colour.
    /// </summary>
    public byte R { get; set; }

    /// <summary>
    /// Gets or sets the green component of the colour.
    /// </summary>
    public byte G { get; set; }

    /// <summary>
    /// Gets or sets the blue component of the colour.
    /// </summary>
    public byte B { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color3b"/> struct.
    /// </summary>
    /// <param name="color">The byte-packed colour.</param>
    public Color3b(Color color)
    {
        this.R = color.R;
        this.G = color.G;
        this.B = color.B;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color3b"/> struct.
    /// </summary>
    /// <param name="color">The floating-point colour.</param>
    public Color3b(Color3 color)
    {
        this.R = (byte)(color.R / 255f);
        this.G = (byte)(color.G / 255f);
        this.B = (byte)(color.B / 255f);
    }

    /// <summary>
    /// Converts the floating-point RGB colour to a packed 8-bit element colour.
    /// </summary>
    /// <returns>The packed colour.</returns>
    public Color ToColor() => Color.FromArgb((int)this.R, (int)this.G, (int)this.B);
}
