using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;

namespace InfrastructureBase.Object
{
    public static class ObjectExtension
    {
        private static readonly ConcurrentDictionary<(Type source, Type target), Func<object, object>> MapperCache
        = new ConcurrentDictionary<(Type, Type), Func<object, object>>();

        /// <summary>
        /// 只需指定目标类型，内部通过反射和包装生成统一的委托调用映射方法
        /// </summary>
        public static TTarget CopyTo<TTarget>(this object model)
            where TTarget : class
        {
            if (model == null) return null;

            Type sourceType = model.GetType();
            Type targetType = typeof(TTarget);
            var key = (sourceType, targetType);

            var mapper = MapperCache.GetOrAdd(key, k =>
            {
                Type mapperGenericType = typeof(ExtensionMapper<,>).MakeGenericType(sourceType, targetType);
                MethodInfo mapMethod = mapperGenericType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static, null, new Type[] { sourceType }, null);
                if (mapMethod == null)
                {
                    throw new InvalidOperationException("未能找到对应的 Map 方法");
                }
                var srcParameter = Expression.Parameter(typeof(object), "src");
                var typedSrc = Expression.Convert(srcParameter, sourceType);
                var callExpr = Expression.Call(mapMethod, typedSrc);
                var castResult = Expression.Convert(callExpr, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(castResult, srcParameter);
                return lambda.Compile();
            });

            return (TTarget)mapper(model);
        }
        public static IEnumerable<TTarget> CopyToList<TTarget>(this IEnumerable model)
        where TTarget : class
        {
            foreach (var item in model)
            {
                yield return item.CopyTo<TTarget>();
            }
        }
        public static object GetPropertyValue<T>(this T t, string name)
        {
            Type type = t.GetType();
            PropertyInfo p = type.GetProperty(name);
            if (p == null)
            {
                return null;
            }
            ParameterExpression param_obj = Expression.Parameter(typeof(T));
            UnaryExpression body;
            if (typeof(T).BaseType == null)
            {
                body = Expression.Convert(Expression.Property(Expression.Convert(param_obj, type), name), typeof(object));
            }
            else
            {
                body = Expression.Convert(Expression.Property(param_obj, p), typeof(object));
            }
            var result = Expression.Lambda<Func<T, object>>(body, param_obj);
            var getValue = result.Compile();
            return getValue(t);
        }
    }
}
