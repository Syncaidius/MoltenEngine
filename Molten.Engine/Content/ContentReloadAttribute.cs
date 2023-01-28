namespace Molten
{
    /// <summary>
    /// Used to determine how a content asset behaves when reloaded after changes.
    /// </summary>
    public class ContentReloadAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="ContentReloadAttribute"/>.
        /// </summary>
        /// <param name="recreate">If true, the content should be re-instantiated if reloaded.</param>
        public ContentReloadAttribute(bool recreate)
        {
            Recreate = recreate;
        }

        /// <summary>
        /// Gets whether the content should be re-instantiated if it is reloaded.
        /// </summary>
        public bool Recreate { get; }
    }
}
