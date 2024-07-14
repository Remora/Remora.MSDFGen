//
//  SPDX-FileName: EdgeColor.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;

namespace Remora.MSDFGen;

/// <summary>
/// Represents possible colours for an edge in a contour.
/// </summary>
[Flags]
public enum EdgeColor
{
    /// <summary>
    /// Represents a colourless edge.
    /// </summary>
    Black = 0,

    /// <summary>
    /// Represents a red edge.
    /// </summary>
    Red = 1 << 1,

    /// <summary>
    /// Represents a green edge.
    /// </summary>
    Green = 1 << 2,

    /// <summary>
    /// Represents a blue edge.
    /// </summary>
    Blue = 1 << 3,

    /// <summary>
    /// Represents a yellow edge, which is the bitwise combination of a red and green edge.
    /// </summary>
    Yellow = Red | Green,

    /// <summary>
    /// Represents a magenta edge, which is the bitwise combination of a red and blue edge.
    /// </summary>
    Magenta = Red | Blue,

    /// <summary>
    /// Represents a cyan edge, which is the bitwise combination of a green and blue edge.
    /// </summary>
    Cyan = Green | Blue,

    /// <summary>
    /// Represents a white edge, which is the bitwise combination of a red, green, and blue edge.
    /// </summary>
    White = Red | Green | Blue
}
