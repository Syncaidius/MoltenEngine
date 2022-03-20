namespace Molten.Font
{
    public partial class GPOS
    {
        public class EntryExitRecord
        {
            public AnchorTable EntryAnchor { get; internal set; }

            public AnchorTable ExitAnchor { get; internal set; }
        }
    }
}
