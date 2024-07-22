//
//  SPDX-FileName: MultiDistance.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

namespace Remora.MSDFGen;

/// <summary>
/// Represents a multi-channel signed distance.
/// </summary>
internal struct MultiDistance
{
    /// <summary>
    /// Gets the red component of the distance.
    /// </summary>
    public double R;

    /// <summary>
    /// Gets the green component of the distance.
    /// </summary>
    public double G;

    /// <summary>
    /// Gets the blue component of the distance.
    /// </summary>
    public double B;

    /// <summary>
    /// Gets the median value of the three components.
    /// </summary>
    public double Median;
}
