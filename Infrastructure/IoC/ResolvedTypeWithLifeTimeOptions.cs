using System;

namespace DeviceMonitor.Infrastructure.IoC
{
    public class ResolvedTypeWithLifeTimeOptions
    {
        public Type ResolvedType { get; set; }
        public LifeTimeOptions LifeTimeOption { get; set; }
        public object InstanceValue { get; set; }

        public ResolvedTypeWithLifeTimeOptions(Type resolvedType)
        {
            ResolvedType = resolvedType;
            LifeTimeOption = LifeTimeOptions.TransientLifeTimeOption;
            InstanceValue = null;
        }

        public ResolvedTypeWithLifeTimeOptions(Type resolvedType, LifeTimeOptions lifeTimeOptions)
        {
            ResolvedType = resolvedType;
            LifeTimeOption = lifeTimeOptions;
            InstanceValue = null;
        }
    }
}