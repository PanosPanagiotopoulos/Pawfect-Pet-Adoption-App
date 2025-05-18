using System;
using System.Collections.Concurrent;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class BuilderFactory : IBuilderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<Type, Type> _constructorCache;

        public BuilderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _constructorCache = new ConcurrentDictionary<Type, Type>();
        }

        public T Builder<T>() where T : IBuilder
        {
            Type builderType = typeof(T);

            // Check cache (mirroring QueryFactory, though not used beyond storage here)
            if (!_constructorCache.TryGetValue(builderType, out _))
            {
                _constructorCache[builderType] = builderType;
            }

            // Get all public constructors
            System.Reflection.ConstructorInfo[] constructors = builderType.GetConstructors();
            if (constructors.Length == 0)
                throw new InvalidOperationException($"No public constructor found for {builderType.Name}");

            // Select constructor with the most parameters
            System.Reflection.ConstructorInfo constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();

            // Resolve constructor parameters
            System.Reflection.ParameterInfo[] parameters = constructor.GetParameters();
            object[] resolvedParameters = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                Type parameterType = parameters[i].ParameterType;
                object service = _serviceProvider.GetRequiredService(parameterType);
                resolvedParameters[i] = service;
            }

            // Instantiate the builder
            return (T)constructor.Invoke(resolvedParameters);
        }
    }
}