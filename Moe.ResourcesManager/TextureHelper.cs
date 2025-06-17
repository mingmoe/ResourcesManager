using Microsoft.Xna.Framework.Graphics;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Moe.ResourcesManager;
public static class TextureHelper
{
    public static void CopyTextureColor(Texture2D src, Texture2D dst, Rectangle? srcRectangle, Point dstLocation)
    {
        Trace.Assert(src.Format == SurfaceFormat.Color);
        Trace.Assert(dst.Format == SurfaceFormat.Color);

        var srcRect = srcRectangle ?? new(0, 0, src.Width, src.Height);

        int srcX = srcRect.Width;
        int srcY = srcRect.Height;
        int dstX = dst.Width;
        int dstY = dst.Height;
        int locationX = dstLocation.X;
        int locationY = dstLocation.Y;

        if (locationX + srcX > dstX || locationY + srcY > dstY)
        {
            throw new ArgumentOutOfRangeException(
                nameof(src),
                "the copy operation will out of range!");
        }
        if (srcX + srcRect.X > src.Width || srcY + srcRect.Y > src.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(srcRectangle),
                "the srcRectangle is out of src!");
        }

        // copy
        uint[]? buffer = null;
        try
        {
            buffer = ArrayPool<uint>.Shared.Rent(srcX * srcY);

            src.GetData(0, srcRectangle, buffer, 0, srcX * srcY);

            dst.SetData(0, new(locationX, locationY, srcX, srcY), buffer, 0, srcX * srcY);
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<uint>.Shared.Return(buffer);
            }
        }
    }
}