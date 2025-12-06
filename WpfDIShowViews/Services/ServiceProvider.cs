using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WpfDIShowViews.Services
{
    public enum ServiceLifetime
    {
        Singleton,
        Transient
    }
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider provider)
            where T : class
            => (T)provider.GetService(typeof(T));

        public static T GetRequiredService<T>(this IServiceProvider provider)
            where T : class
        {
            var service = provider.GetService(typeof(T));
            if (service == null)
            {
                throw new InvalidOperationException($"Service of type {typeof(T).FullName} not found.");
            }
            return (T)service;
        }
    }

    public sealed class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public Func<IServiceProvider, object> ImplementationFactory { get; }
        public object ImplementationInstance { get; internal set; }
        public ServiceLifetime Lifetime { get; }

        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            ServiceType = serviceType;
            ImplementationFactory = factory;
            Lifetime = lifetime;
        }

        public ServiceDescriptor(Type serviceType, object instance)
        {
            ServiceType = serviceType;
            ImplementationInstance = instance;
            Lifetime = ServiceLifetime.Singleton;
        }
    }

    public interface IServiceCollection : IList<ServiceDescriptor>
    {
    }

    public class ServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
    }

    public static class ServiceCollectionExtensions
    {
        // 인터페이스 -> 구현 타입 싱글톤
        public static IServiceCollection AddSingleton<TService, TImpl>(this IServiceCollection services)
            where TService : class
            where TImpl : class, TService
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TImpl), ServiceLifetime.Singleton));
            return services;
        }

        // self-binding 싱글톤 (MainView, MainViewModel 같은 경우)
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services)
            where TService : class
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), ServiceLifetime.Singleton));
            return services;
        }

        // 인스턴스 싱글톤
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService instance)
            where TService : class
        {
            services.Add(new ServiceDescriptor(typeof(TService), instance));
            return services;
        }

        // 인터페이스 -> 구현 타입 트랜지언트
        public static IServiceCollection AddTransient<TService, TImpl>(this IServiceCollection services)
            where TService : class
            where TImpl : class, TService
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TImpl), ServiceLifetime.Transient));
            return services;
        }

        // self-binding 트랜지언트 (SubView, SubViewModel 같은 경우)
        public static IServiceCollection AddTransient<TService>(this IServiceCollection services)
            where TService : class
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), ServiceLifetime.Transient));
            return services;
        }

        // ServiceProvider 생성
        public static IServiceProvider BuildServiceProvider(this IServiceCollection services)
        {
            return new SimpleServiceProvider(services);
        }
    }

    internal sealed class SimpleServiceProvider : IServiceProvider
    {
        // 등록 정보
        private readonly Dictionary<Type, ServiceDescriptor> _descriptors;
        // 싱글톤 캐시
        private readonly Dictionary<Type, object> _singletons = new();

        public SimpleServiceProvider(IEnumerable<ServiceDescriptor> descriptors)
        {
            _descriptors = descriptors.ToDictionary(d => d.ServiceType);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
                return this;

            if (!_descriptors.TryGetValue(serviceType, out var descriptor))
            {
                // 등록 안 돼 있지만 구체 타입이면 그냥 생성해 줄지 여부는 선택사항
                if (!serviceType.IsAbstract && !serviceType.IsInterface)
                    return CreateInstance(serviceType);

                return null;
            }

            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                // 인스턴스로 등록된 경우
                if (descriptor.ImplementationInstance != null)
                    return descriptor.ImplementationInstance;

                // 이미 생성된 싱글톤 있으면 재사용
                if (_singletons.TryGetValue(serviceType, out var instance))
                    return instance;

                // 처음이면 생성 후 캐시
                instance = CreateFromDescriptor(descriptor);
                _singletons[serviceType] = instance;
                descriptor.ImplementationInstance = instance;
                return instance;
            }

            // Transient
            return CreateFromDescriptor(descriptor);
        }

        private object CreateFromDescriptor(ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
                return descriptor.ImplementationInstance;

            if (descriptor.ImplementationFactory != null)
                return descriptor.ImplementationFactory(this);

            var implType = descriptor.ImplementationType ?? descriptor.ServiceType;
            return CreateInstance(implType);
        }

        private object CreateInstance(Type implementationType)
        {
            var ctors = implementationType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderByDescending(c => c.GetParameters().Length);

            foreach (var ctor in ctors)
            {
                var parameters = ctor.GetParameters();
                var args = new object[parameters.Length];

                var canUse = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    var dependency = GetService(paramType); // 재귀적으로 의존성 해결

                    if (dependency == null)
                    {
                        canUse = false;
                        break;
                    }

                    args[i] = dependency;
                }

                if (canUse)
                    return ctor.Invoke(args);
            }

            throw new InvalidOperationException(
                $"생성자를 선택할 수 없습니다: {implementationType.FullName}");
        }
    }
}
