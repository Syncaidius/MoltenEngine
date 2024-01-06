namespace Molten;

/// <summary>
/// A Bin packer implementation based on the 'MAXRECTS' method developed by Jukka Jylänki: http://clb.demon.fi/files/RectangleBinPack.pdf
/// </summary>
public class BinPacker
{
    List<Rectangle> freeList;

    /// <summary>
    /// Creates a new instance of <see cref="BinPacker"/> with the specified width and height.
    /// </summary>
    /// <param name="width">The width of the area available for packing, in pixels.</param>
    /// <param name="height">The height of the area available for packing, in pixels.</param>
    public BinPacker(int width, int height)
    {
        Width = width;
        Height = height;
        freeList = new List<Rectangle>();
        freeList.Add(new Rectangle(0, 0, width, height));
    }

    /// <summary>
    /// Clears the current <see cref="BinPacker"/> instance.
    /// </summary>
    public void Clear()
    {
        Clear(Width, Height);
    }

    /// <summary>
    /// Clears the current <see cref="BinPacker"/> instance and resizes it to the specified size.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void Clear(int width, int height)
    {
        freeList.Clear();
        Width = width;
        Height = height;
        freeList.Add(new Rectangle(0, 0, width, height));
    }

    /// <summary>
    /// Inserts an area of the specified width and height into the packer area, then return's its bounds.
    /// </summary>
    /// <param name="width">The width of the area to be inserted.</param>
    /// <param name="height">The height of the area to be inserted.</param>
    /// <returns></returns>
    public Rectangle? Insert(int width, int height)
    {
        Rectangle bestNode = new Rectangle();
        int bestShortFit = int.MaxValue;
        int bestLongFit = int.MaxValue;
        int count = freeList.Count;

        for (int i = 0; i < count; i++)
        {
            // try to place the rect
            Rectangle rect = freeList[i];
            if (rect.Width < width || rect.Height < height)
                continue;

            int leftoverX = Math.Abs(rect.Width - width);
            int leftoverY = Math.Abs(rect.Height - height);
            int shortFit = Math.Min(leftoverX, leftoverY);
            int longFit = Math.Max(leftoverX, leftoverY);

            if (shortFit < bestShortFit || (shortFit == bestShortFit && longFit < bestLongFit))
            {
                bestNode = new Rectangle(rect.X, rect.Y, width, height);
                bestShortFit = shortFit;
                bestLongFit = longFit;
            }
        }

        if (bestNode.Height == 0)
            return null;

        // split out free areas into smaller ones
        for (int i = 0; i < count; i++)
        {
            if (SplitFreeNode(freeList[i], bestNode))
            {
                freeList.RemoveAt(i--);
                count--;
            }
        }

        // prune the freelist
        for (int i = 0; i < freeList.Count; i++)
        {
            for (int j = i + 1; j < freeList.Count; j++)
            {
                Rectangle idata = freeList[i];
                Rectangle jdata = freeList[j];
                if (jdata.Contains(idata))
                {
                    freeList.RemoveAt(i--);
                    break;
                }

                if (idata.Contains(jdata))
                    freeList.RemoveAt(j--);
            }
        }

        return bestNode;
    }

    private bool SplitFreeNode(Rectangle freeNode, Rectangle usedNode)
    {
        // test if the rects even intersect
        var insideX = usedNode.X < freeNode.Right && usedNode.Right > freeNode.X;
        var insideY = usedNode.Y < freeNode.Bottom && usedNode.Bottom > freeNode.Y;
        if (!insideX || !insideY)
            return false;

        if (insideX)
        {
            // new node at the top side of the used node
            if (usedNode.Y > freeNode.Y && usedNode.Y < freeNode.Bottom)
            {
                var newNode = freeNode;
                newNode.Height = usedNode.Y - newNode.Y;
                freeList.Add(newNode);
            }

            // new node at the bottom side of the used node
            if (usedNode.Bottom < freeNode.Bottom)
            {
                var newNode = freeNode;
                newNode.Y = usedNode.Bottom;
                newNode.Height = freeNode.Bottom - usedNode.Bottom;
                freeList.Add(newNode);
            }
        }

        if (insideY)
        {
            // new node at the left side of the used node
            if (usedNode.X > freeNode.X && usedNode.X < freeNode.Right)
            {
                var newNode = freeNode;
                newNode.Width = usedNode.X - newNode.X;
                freeList.Add(newNode);
            }

            // new node at the right side of the used node
            if (usedNode.Right < freeNode.Right)
            {
                var newNode = freeNode;
                newNode.X = usedNode.Right;
                newNode.Width = freeNode.Right - usedNode.Right;
                freeList.Add(newNode);
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the width of the bin-packed area.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the height of the bin-packed area.
    /// </summary>
    public int Height { get; private set; }
}
