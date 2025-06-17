using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Point = Microsoft.Xna.Framework.Point;
using System.Diagnostics.CodeAnalysis;

namespace Moe.ResourcesManager;

public sealed class TextureAtlasManager<T> : IDisposable where T : class
{
    public Texture2D Atlas { get; init; }

    public ResourceID ResourceId { get; init; }

    public IReadOnlyDictionary<T, Rectangle> ReadonlyAtlasIndex => AtlasIndex;

    private Dictionary<T, Rectangle> AtlasIndex { get; init; } = [];

    private readonly int maxX;

    private readonly int maxY;

    private int _xPen = 0;

    private int _currentLinePen = 0;

    private int _currentLinePitch = 0;

    public TextureAtlasManager(ResourceID resourceId, GraphicsDevice device)
        : this(resourceId, device, 2048, 2048)
    {
    }

    public TextureAtlasManager(ResourceID resourceId, GraphicsDevice device, int width, int height)
    {
        ResourceId = resourceId;
        Atlas = new(device, width, height, false, SurfaceFormat.Color);
        maxX = Atlas.Width;
        maxY = Atlas.Height;
    }

    private bool TryAlloc(Point size, [NotNullWhen(true)] out Point? place)
    {
        place = null;

        // out of range
        if (_currentLinePen > maxY)
        {
            return false;
        }

        int x = size.X;
        int y = size.Y;

        // width is too big
        if (x > maxX)
        {
            return false;
        }
        // the y is not enough
        if (_currentLinePen + y > maxY)
        {
            return false;
        }

        // the y is enough
        // try old line
        if (_xPen + x <= maxX)
        {
            // old line is enough
            place = new(_xPen, _currentLinePen);

            _currentLinePitch = int.Max(_currentLinePitch, y);
            _xPen += x;

            return true;
        }

        // try new line
        _currentLinePen += _currentLinePitch;
        _currentLinePitch = y;
        _xPen = 0;

        if (_currentLinePen + y > maxY)
        {
            // new line has not enough height
            return false;
        }

        // ok,we have ensure the x <= xMax above
        // so newline is enough
        place = new(_xPen, _currentLinePen);

        _xPen += x;

        return true;
    }

    public bool TryAdd(T key, Texture2D texture)
    {
        if (TryAlloc(new(texture.Width, texture.Height), out var dst))
        {
            TextureHelper.CopyTextureColor(texture, Atlas, null, dst.Value);
        }

        return false;
    }

    public bool TryAlloc(T key, Point size, out Rectangle? rect)
    {
        rect = null;

        var currentXPen = _xPen;
        var currentLinePen = _currentLinePen;
        var currentPitch = _currentLinePitch;

        if (TryAlloc(new(size.X, size.Y), out Point? dst))
        {
            rect = new(dst.Value.X, dst.Value.Y, size.X, size.Y);
            AtlasIndex.Add(key, rect.Value);
            return true;
        }

        // go back
        _xPen = currentXPen;
        _currentLinePen = currentLinePen;
        _currentLinePitch = currentPitch;

        return false;
    }

    public void Dispose()
    {
        Atlas.Dispose();
        GC.SuppressFinalize(this);
    }
}
