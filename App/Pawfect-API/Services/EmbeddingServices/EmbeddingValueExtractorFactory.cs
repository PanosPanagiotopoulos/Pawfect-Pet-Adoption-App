using Microsoft.Extensions.AI;
using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Services.EmbeddingServices
{
    public class EmbeddingValueExtractorFactory
    {
        public class EmbeddingValueExtractorFactoryConfig
        {
            // Main mapper from diagram node types to their provider objects implemented. 
            // For config used with Abstract Type object 
            public Dictionary<EmbeddingValueExtractorType, Type> Accessors { get; } = new Dictionary<EmbeddingValueExtractorType, Type>();
            public EmbeddingValueExtractorFactoryConfig Add(EmbeddingValueExtractorType key, Type type)
            {
                this.Accessors[key] = type;
                return this;
            }
        }
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<EmbeddingValueExtractorType, Func<IEmbeddingValueExtractorAbstraction>> _accessorsMap = null;
        public EmbeddingValueExtractorFactory(IServiceProvider serviceProvider, Microsoft.Extensions.Options.IOptions<EmbeddingValueExtractorFactoryConfig> config)
        {
            this._serviceProvider = serviceProvider;
            // Fill in the factory accessor map with the actual objects of the diagram node types providers
            this._accessorsMap = new Dictionary<EmbeddingValueExtractorType, Func<IEmbeddingValueExtractorAbstraction>>();
            foreach (KeyValuePair<EmbeddingValueExtractorType, Type> pair in config?.Value?.Accessors)
            {
                this._accessorsMap.Add(pair.Key, () =>
                {
                    IEmbeddingValueExtractorAbstraction obj = this._serviceProvider.GetRequiredService(pair.Value) as IEmbeddingValueExtractorAbstraction;
                    return obj;
                });
            }
        }
        // UnSafe access to providers
        public IEmbeddingValueExtractorAbstraction Extractor(EmbeddingValueExtractorType type)
        {
            if (this._accessorsMap.TryGetValue(type, out Func<IEmbeddingValueExtractorAbstraction> obj)) return obj();
            throw new ApplicationException("unrecognized form helper " + type.ToString());
        }
        // Safe access to providers
        public IEmbeddingValueExtractorAbstraction ExtractorSafe(EmbeddingValueExtractorType type)
        {
            if (this._accessorsMap.TryGetValue(type, out Func<IEmbeddingValueExtractorAbstraction> obj)) return obj();
            return null;
        }
        // Access with array like syntax
        public IEmbeddingValueExtractorAbstraction this[EmbeddingValueExtractorType key]
        {
            get
            {
                Func<IEmbeddingValueExtractorAbstraction> obj = null;
                if (this._accessorsMap.TryGetValue(key, out obj)) return obj();
                throw new Exception("unrecognized form helper " + key.ToString());
            }
        }
    }
}
