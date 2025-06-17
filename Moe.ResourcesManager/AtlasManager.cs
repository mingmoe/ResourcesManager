using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Moe.ResourcesManager;

public sealed class AtlasManager<T> where T : class
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

    public AtlasManager(ResourceID resourceId, GraphicsDevice device)
        : this(resourceId, device, 2048, 2048)
    {
    }

    public AtlasManager(ResourceID resourceId, GraphicsDevice device, int width, int height)
    {
        ResourceId = resourceId;
        Atlas = new(device, width, height, false, SurfaceFormat.Color);
        maxX = Atlas.Width;
        maxY = Atlas.Height;
    }

    private bool _TryAddInternal(T key, Texture2D texture)
    {
        // out of range
        if (_currentLinePen > maxY)
        {
            return false;
        }

        int x = texture.Width;
        int y = texture.Height;

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
            _currentLinePitch = int.Max(_currentLinePitch, y);

            TextureHelper.CopyTextureColor(texture, Atlas, null, new(_xPen, _currentLinePen));
            AtlasIndex.Add(key, new(_xPen, _currentLinePen, x, y));
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
        TextureHelper.CopyTextureColor(texture, Atlas, null, new(_xPen, _currentLinePen));
        AtlasIndex.Add(key, new(_xPen, _currentLinePen, x, y));
        _xPen += x;

        return true;
    }

    public bool TryAdd(T key, Texture2D texture)
    {
        var currentXPen = _xPen;
        var currentLinePen = _currentLinePen;
        var currentPitch = _currentLinePitch;

        var result = _TryAddInternal(key, texture);

        if (result)
        {
            return result;
        }

        // go back
        _xPen = currentXPen;
        _currentLinePen = currentLinePen;
        _currentLinePitch = currentPitch;

        return false;
    }
}
