using System.Collections.Concurrent;

namespace Pawfect_API.Censors
{
    public class CensorFactory : ICensorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, Type> _constructorCache;

        public CensorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _constructorCache = new ConcurrentDictionary<Type, Type>();
        }
        public T Censor<T>() where T : ICensor
        {
            Type queryType = typeof(T);

            // Check if we already have the constructor info cached
            if (!_constructorCache.TryGetValue(queryType, out _))
            {
                // Cache the type to avoid reflection overhead on subsequent calls
                _constructorCache[queryType] = queryType;
            }

            // Find constructors for the censor type
            System.Reflection.ConstructorInfo[] constructors = queryType.GetConstructors();
            if (constructors.Length == 0)
                throw new InvalidOperationException($"No public constructor found for {queryType.Name}");

            // Get the constructor with the most parameters (typically the most specific one)
            System.Reflection.ConstructorInfo constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();

            // Get the parameters needed for this constructor
            System.Reflection.ParameterInfo[] parameters = constructor.GetParameters();
            object[] resolvedParameters = new object[parameters.Length];

            // Resolve each parameter from the DI container
            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                object service = _serviceProvider.GetRequiredService(parameterType);
                resolvedParameters[i] = service;
            }

            // Create instance using the resolved parameters
            return (T)constructor.Invoke(resolvedParameters);
        }
    }
}
