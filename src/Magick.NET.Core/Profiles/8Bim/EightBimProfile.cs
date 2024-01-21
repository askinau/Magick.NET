﻿// Copyright Dirk Lemstra https://github.com/dlemstra/Magick.NET.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace ImageMagick;

/// <summary>
/// Class that can be used to access an 8bim profile.
/// </summary>
public sealed class EightBimProfile : ImageProfile, IEightBimProfile
{
    private readonly int _height;
    private readonly int _width;

    private Collection<IEightBimValue>? _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="EightBimProfile"/> class.
    /// </summary>
    /// <param name="data">The byte array to read the 8bim profile from.</param>
    public EightBimProfile(byte[] data)
      : base("8bim", data)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EightBimProfile"/> class.
    /// </summary>
    /// <param name="fileName">The fully qualified name of the 8bim profile file, or the relative
    /// 8bim profile file name.</param>
    public EightBimProfile(string fileName)
      : base("8bim", fileName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EightBimProfile"/> class.
    /// </summary>
    /// <param name="stream">The stream to read the 8bim profile from.</param>
    public EightBimProfile(Stream stream)
      : base("8bim", stream)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EightBimProfile"/> class.
    /// </summary>
    /// <param name="image">The image that contains the profile.</param>
    /// <param name="data">The byte array to read the 8bim profile from.</param>
    public EightBimProfile(IMagickImage image, byte[] data)
      : base("8bim", data)
    {
        Throw.IfNull(nameof(image), image);

        _width = image.Width;
        _height = image.Height;
    }

    /// <summary>
    /// Gets the clipping paths this image contains.
    /// </summary>
    public IReadOnlyCollection<IClipPath> ClipPaths
    {
        get
        {
            Initialize();

            var clipPaths = new List<IClipPath>();
            foreach (var value in _values)
            {
                var clipPath = CreateClipPath(value);
                if (clipPath is not null)
                    clipPaths.Add(clipPath);
            }

            return clipPaths;
        }
    }

    /// <summary>
    /// Gets the values of this 8bim profile.
    /// </summary>
    public IReadOnlyCollection<IEightBimValue> Values
    {
        get
        {
            Initialize();

            return _values;
        }
    }

    private ClipPath? CreateClipPath(IEightBimValue value)
    {
        if (value.Name is null)
            return null;

        var d = GetClipPath(0, value.ToByteArray());
        if (string.IsNullOrEmpty(d))
            return null;

        var doc = XmlHelper.CreateDocument();
        doc.CreateXmlDeclaration("1.0", "iso-8859-1", null);

        var svg = XmlHelper.CreateElement(doc, "svg");
        XmlHelper.SetAttribute(svg, "width", _width);
        XmlHelper.SetAttribute(svg, "height", _height);

        var g = XmlHelper.CreateElement(svg, "g");

        var path = XmlHelper.CreateElement(g, "path");
        XmlHelper.SetAttribute(path, "fill", "#00000000");
        XmlHelper.SetAttribute(path, "stroke", "#00000000");
        XmlHelper.SetAttribute(path, "stroke-width", "0");
        XmlHelper.SetAttribute(path, "stroke-antialiasing", "false");
        XmlHelper.SetAttribute(path, "d", d);

        return new ClipPath(value.Name, doc.CreateNavigator());
    }

    private string? GetClipPath(int offset, byte[] data)
    {
        if (_width == 0 || _height == 0)
            return null;

        return ClipPathReader.Read(_width, _height, data);
    }

    [MemberNotNull(nameof(_values))]
    private void Initialize()
    {
        if (_values is not null)
            return;

        _values = new Collection<IEightBimValue>();

        var data = GetData();
        if (data is null)
            return;

        var i = 0;
        while (i < data.Length)
        {
            if (data[i++] != '8')
                continue;
            if (data[i++] != 'B')
                continue;
            if (data[i++] != 'I')
                continue;
            if (data[i++] != 'M')
                continue;

            if (i + 7 > data.Length)
                return;

            var id = ByteConverter.ToShort(data, ref i);

            string? name = null;
            int length = data[i++];
            if (length != 0)
            {
                if (id > 1999 && id < 2998 && i + length < data.Length)
                    name = Encoding.ASCII.GetString(data, i, length);

                i += length;
            }

            if ((length & 0x01) == 0)
                i++;

            length = ByteConverter.ToUInt(data, ref i);
            if (i + length > data.Length)
                return;

            if (length < 0)
                return;

            if (length != 0)
            {
                var value = new byte[length];
                Array.Copy(data, i, value, 0, length);
                _values.Add(new EightBimValue(id, name, value));
            }

            i += length;
        }
    }
}
