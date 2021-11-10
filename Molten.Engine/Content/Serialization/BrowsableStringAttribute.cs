using System;

namespace Molten
{
    /// <summary>Flags a property as a browsable string. In a property grid, this will allow the user to browse for a file path to fill the string.
    /// Attribute is ignored on all but properties of type <see cref="String"/></summary>
    public class BrowsableStringAttribute : Attribute
    {

        public BrowsableStringAttribute(string title, string fileFilter)
        {
            FileFilter = fileFilter;
        }

        public string FileFilter { get; private set; }

        public string Title { get; private set; }
    }
}
