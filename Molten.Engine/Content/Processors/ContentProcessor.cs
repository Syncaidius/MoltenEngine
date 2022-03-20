namespace Molten {
    /// <summary>     /// Provides a base implementation for content processors, used by a <see cref="ContentManager"/>.     /// </summary>     public abstract class ContentProcessor     {         internal IReadOnlyList<ContentParameter> Parameters { get; private set; }          internal Dictionary<string, ContentParameter> ParametersByName;         List<ContentParameter> _parameters;          internal void Initialize()
        {
            ParametersByName = new Dictionary<string, ContentParameter>();
            _parameters = new List<ContentParameter>();
            Parameters = _parameters.AsReadOnly();

            OnInitialize();

            if (_parameters != null)
            {
                foreach (ContentParameter p in _parameters)
                    ParametersByName[p.Name.ToLower()] = p;
            }
        }          protected void AddParameter<T>(string name, T defaultValue)
        {
            ContentParameter p = new ContentParameter()
            {
                Name = name.ToLower(),
                DefaultValue = defaultValue,
                ExpectedType = typeof(T),
            };

            _parameters.Add(p);
            ParametersByName.Add(p.Name, p);
        }          /// <summary>
        /// Invoked when the current <see cref="ContentProcessor"/> is first instantiated.
        /// </summary>
        /// <param name="engine"></param>         protected abstract void OnInitialize();          /// <summary>Invoked when a content retrieval request was called with metadata tags (prefixed with @) or when multiple objects are available which were loaded from the same file.</summary>         /// <param name="engine">The <see cref="Engine"/> instance that content should be bound to.</param>         /// <param name="contentType">The type of content being loaded.</param>         /// <param name="parameters">An array of parameters that were attached to the request.</param>         /// <param name="groupContent">A list of viable objects which match the requested filename</param>         /// <returns></returns>         public virtual object OnGet(Engine engine, Type contentType, ContentFileParameters parameters, IList<object> groupContent) { return groupContent[0]; }          /// <summary>         /// Invoked when content matching <see cref="AcceptedTypes"/> needs to be read.         /// </summary>         /// <param name="context">The <see cref="ContentContext"/> with which to read content.</param>         public abstract void OnRead(ContentContext context);          /// <summary>         /// Invoked when content matching <see cref="AcceptedTypes"/> needs to be written.         /// </summary>         /// <param name="context">The <see cref="ContentContext"/> with which to write content.</param>         public abstract void OnWrite(ContentContext context);          /// <summary>Gets a list of accepted </summary>         public abstract Type[] AcceptedTypes { get; }          /// <summary>         /// Gets a list of types for required <see cref="EngineService"/>.          /// If the bound <see cref="Engine"/> instance is missing any of them, the content processor will not be used.         /// </summary>         public abstract Type[] RequiredServices { get; }     } } 