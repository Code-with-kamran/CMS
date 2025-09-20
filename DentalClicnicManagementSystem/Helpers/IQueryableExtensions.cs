using System.Linq.Expressions;

namespace CMS.Extensions
{
    public static class IQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderByDynamic<T>(this IQueryable<T> source, string propertyName, bool ascending)
        {
            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "x");
            var property = Expression.Property(parameter, propertyName);
            var converted = Expression.Convert(property, typeof(object));
            var selector = Expression.Lambda<Func<T, object>>(converted, parameter);

            return ascending ? source.OrderBy(selector) : source.OrderByDescending(selector);
        }

        public static IOrderedQueryable<T> ThenByDynamic<T>(this IOrderedQueryable<T> source, string propertyName, bool ascending)
        {
            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "x");
            var property = Expression.Property(parameter, propertyName);
            var converted = Expression.Convert(property, typeof(object));
            var selector = Expression.Lambda<Func<T, object>>(converted, parameter);

            return ascending ? source.ThenBy(selector) : source.ThenByDescending(selector);
        }
    }
}
