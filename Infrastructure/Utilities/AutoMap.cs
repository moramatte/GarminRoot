using AutoMapper;

namespace Infrastructure.Utilities
{
    public static class AutoMap
    {
        public static T To<T>(this object source)
        {
            var originalType = source.GetType();
            var config = new MapperConfiguration(cfg => cfg.CreateMap(originalType, typeof(T)));
            var mapper = config.CreateMapper();

            return mapper.Map<T>(source);
        }
    }
}
