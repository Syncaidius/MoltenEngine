﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a page of the main font texture array
    /// </summary>
    internal class FontPage
    {
        ConcurrentDictionary<char, FontGlyphBinding> _bindings;
        BinPacker _packer;
        Interlocker _interlocker;

        internal FontPage(FontManager manager, int pageID, int width, int height)
        {
            Manager = manager;
            ID = pageID;
            _bindings = new ConcurrentDictionary<char, FontGlyphBinding>(); 
            _packer = new BinPacker(width, height);
            _interlocker = new Interlocker();
        }

        /// <summary>
        /// Attempts to pack a <see cref="FontGlyphBinding"/> onto the current <see cref="FontPage"/>.
        /// </summary>
        /// <param name="binding">The <see cref="FontGlyphBinding"/> to pack.</param>
        /// <returns>True if the <see cref="FontGlyphBinding"/> was successfully given a place on the page. False if there was no space available.</returns>
        internal bool Pack(FontGlyphBinding binding)
        {
            Rectangle gBounds = binding.Glyph.Bounds;
            int padding2 = Manager.Padding * 2;

            Vector2F glyphScale = new Vector2F()
            {
                X = (float)binding.PWidth / gBounds.Width,
                Y = (float)binding.PHeight / gBounds.Height,
            };

            Rectangle? paddedLoc = null;
            _interlocker.Lock(() => paddedLoc = _packer.Insert(binding.PWidth + padding2, binding.PHeight + padding2));

            if (paddedLoc == null)
                return false;

            Rectangle loc = new Rectangle()
            {
                X = paddedLoc.Value.X + Manager.Padding,
                Y = paddedLoc.Value.Y + Manager.Padding,
                Width = binding.PWidth,
                Height = binding.PHeight,
            };

            binding.Location = loc;
            binding.Page = ID;
            return true;
        }

        /// <summary>
        /// Gets the <see cref="FontManager"/> that the current <see cref="FontPage"/> is bound to.
        /// </summary>
        internal FontManager Manager { get; }

        /// <summary>
        /// Gets the page ID.
        /// </summary>
        internal int ID { get; }
    }
}