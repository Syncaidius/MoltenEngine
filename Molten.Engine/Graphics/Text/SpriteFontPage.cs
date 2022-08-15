using System;
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
    internal class SpriteFontPage
    {
        List<SpriteFontGlyphBinding> _bindings;
        BinPacker _packer;
        Interlocker _interlocker;

        internal SpriteFontPage(SpriteFontManager manager, int pageID)
        {
            Manager = manager;
            ID = pageID;
            _bindings = new List<SpriteFontGlyphBinding>();
            _packer = new BinPacker(manager.PageSize, manager.PageSize);
            _interlocker = new Interlocker();
        }

        /// <summary>
        /// Attempts to pack a <see cref="SpriteFontGlyphBinding"/> onto the current <see cref="SpriteFontPage"/>.
        /// </summary>
        /// <param name="binding">The <see cref="SpriteFontGlyphBinding"/> to pack.</param>
        /// <returns>True if the <see cref="SpriteFontGlyphBinding"/> was successfully given a place on the page. False if there was no space available.</returns>
        internal bool Pack(SpriteFontGlyphBinding binding)
        {
            int padding2 = Manager.Padding * 2;

            Rectangle? paddedLoc = null;
            _interlocker.Lock(() => paddedLoc = _packer.Insert(binding.PWidth + padding2, binding.PHeight + padding2));

            if (paddedLoc == null)
                return false;

            binding.PageID = ID;
            binding.Location = new Rectangle()
            {
                X = paddedLoc.Value.X + Manager.Padding,
                Y = paddedLoc.Value.Y + Manager.Padding,
                Width = binding.PWidth,
                Height = binding.PHeight,
            };

            _bindings.Add(binding);

            return true;
        }

        internal void Remove(SpriteFontGlyphBinding binding)
        {
            _bindings.Remove(binding);
        }

        /// <summary>
        /// Gets the <see cref="SpriteFontManager"/> that the current <see cref="SpriteFontPage"/> is bound to.
        /// </summary>
        internal SpriteFontManager Manager { get; }

        /// <summary>
        /// Gets the page ID.
        /// </summary>
        internal int ID { get; }
    }
}
