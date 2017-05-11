using System;
using System.Collections.Generic;
using System.Linq;

namespace DeviceMonitor.Infrastructure.IoC
{
    public class IoCContainer
    {
        private readonly Dictionary<Type, ResolvedTypeWithLifeTimeOptions> _iocMap = new Dictionary<Type, ResolvedTypeWithLifeTimeOptions>();

        public void Register<TTypeToResolve, TResolvedType>()
        {
            Register<TTypeToResolve, TResolvedType>(LifeTimeOptions.TransientLifeTimeOption);
        }

        public void Register<TTypeToResolve, TResolvedType>(LifeTimeOptions options)
        {
            if (_iocMap.ContainsKey(typeof(TTypeToResolve)))
            {
                throw new Exception($"Type {typeof(TTypeToResolve).FullName} already registered.");
            }

            var targetType = new ResolvedTypeWithLifeTimeOptions(typeof(TResolvedType), options);

            _iocMap.Add(typeof(TTypeToResolve), targetType);
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type typeToResolve)
        {
            if (!_iocMap.ContainsKey(typeToResolve))
            {
                throw new Exception($"Can't resolve {typeToResolve.FullName}. Type is not registered.");
            }

            var resolvedType = _iocMap[typeToResolve];

            if (resolvedType.LifeTimeOption == LifeTimeOptions.ContainerControlledLifeTimeOption &&
                resolvedType.InstanceValue != null)
            {
                return resolvedType.InstanceValue;
            }

            var constructorInfo = resolvedType.ResolvedType.GetConstructors().First();

            var paramsInfo = constructorInfo.GetParameters().ToList();
            var resolvedParams = new List<object>();

            foreach (var param in paramsInfo)
            {
                var t = param.ParameterType;
                var res = Resolve(t);
                resolvedParams.Add(res);
            }

            var retOjbect = constructorInfo.Invoke(resolvedParams.ToArray());

            if (resolvedType.LifeTimeOption == LifeTimeOptions.ContainerControlledLifeTimeOption)
            {
                resolvedType.InstanceValue = retOjbect;
            }

            return retOjbect;
        }
    }
}