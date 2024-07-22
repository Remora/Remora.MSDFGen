//
//  SPDX-FileName: Pixmap.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: MIT
//

using System;

namespace Remora.MSDFGen.Graphics;

/// <summary>
/// Represents a pixel map.
/// </summary>
/// <typeparam name="T">The pixel type of the map.</typeparam>
public class Pixmap<T> where T : struct
{
    /// <summary>
    /// Gets the width of the pixmap.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of the pixmap.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the raw data of the pixmap.
    /// </summary>
    public T[] Data { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pixmap{T}"/> class.
    /// </summary>
    /// <param name="width">The width of the pixmap.</param>
    /// <param name="height">The height of the pixmap.</param>
    public Pixmap(int width, int height)
    {
        this.Width = width;
        this.Height = height;

        this.Data = new T[width * height];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Pixmap{T}"/> class.
    /// </summary>
    /// <param name="width">The width of the pixmap.</param>
    /// <param name="height">The height of the pixmap.</param>
    /// <param name="data">The raw data of the pixmap.</param>
    /// <exception cref="ArgumentException">Thrown if the data is of an invalid size.</exception>
    public Pixmap(int width, int height, T[] data)
    {
        if (data.Length != width * height)
        {
            throw new ArgumentException("data.Length must equal (width * height)", nameof(data));
        }

        this.Width = width;
        this.Height = height;
        this.Data = data;
    }

    /// <summary>
    /// Gets the index into the data array of the pixel at the given coordinates.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel.</param>
    /// <param name="y">The vertical coordinate of the pixel.</param>
    /// <returns>The index into the data array.</returns>
    public int GetIndex(int x, int y)
    {
        return x + (y * this.Width);
    }

    /// <summary>
    /// Gets the pixel at the given coordinate.
    /// </summary>
    /// <param name="x">The horizontal coordinate of the pixel.</param>
    /// <param name="y">The vertical coordinate of the pixel.</param>
    public T this[int x, int y]
    {
        get => this.Data[GetIndex(x, y)];
        set => this.Data[GetIndex(x, y)] = value;
    }
}
