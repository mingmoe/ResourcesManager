using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Moe.ResourcesManager;

public sealed class AtlasManager<T> : IDisposable where T : class
{
    public const int DEFAULT_ATLAS_X = 2048;
    public const int DEFAULT_ATLAS_Y = 2048;

    public GraphicsDevice Device { get; init; }

    public ResourceID ResourceID { get; init; }

    private readonly List<TextureAtlasManager<T>> managers = [];

    private readonly Dictionary<T, TextureAtlasManager<T>> index = [];

    public AtlasManager(GraphicsDevice device, ResourceID id)
    {
        Device = device;
        ResourceID = id;
        managers.Add(new(id, device, DEFAULT_ATLAS_X, DEFAULT_ATLAS_Y));
    }

    public bool TryGet(T key, [NotNullWhen(true)] out (Texture2D, Rectangle)? result)
    {
        if (index.TryGetValue(key, out var manager))
        {
            var got = manager.ReadonlyAtlasIndex.TryGetValue(key, out var rect);

            Trace.Assert(got);

            result = (manager.Atlas, rect);
            return true;
        }

        result = null;
        return false;
    }

    public (Texture2D, Rectangle) Set(T key, Texture2D texture)
    {
        var allocated = Alloc(key, new(texture.Width, texture.Height));
        TextureHelper.CopyTextureColor(
            texture,
            allocated.Item1,
            null,
            new(allocated.Item2.X, allocated.Item2.Y));
        return allocated;
    }

    public (Texture2D, Rectangle) Alloc(T key, Point size)
    {
        if (index.ContainsKey(key))
        {
            throw new InvalidOperationException("the key exists");
        }

        foreach (var manager in ListHelper.FastReverse(managers))
        {
            var added = manager.TryAlloc(key, size, out var rect1);

            if (added)
            {
                index[key] = manager;

                return (manager.Atlas, rect1!.Value);
            }
        }

        // create new
        int x = int.Max(DEFAULT_ATLAS_X, size.X);
        int y = int.Max(DEFAULT_ATLAS_Y, size.Y);

        var mgr = new TextureAtlasManager<T>(ResourceID, Device, x, y);
        Trace.Assert(mgr.TryAlloc(key, size, out var rect2));
        index[key] = mgr;
        managers.Add(mgr);

        return (mgr.Atlas, rect2!.Value);
    }

    public void Dispose()
    {
        foreach (var manager in managers)
        {
            manager.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
