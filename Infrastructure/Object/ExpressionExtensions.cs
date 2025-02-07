using Domain.DomainBase;
using DomainBase;
using Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace InfrastructureBase.Object
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<TTo, bool>> ReplaceParameter<TFrom, TTo>(this Expression<Func<TFrom, bool>> target)
        {
            return (Expression<Func<TTo, bool>>)new WhereReplacerVisitor<TFrom, TTo>().Visit(target);
        }
        public static Expression<Func<TTo, TProp>> ReplaceParameter<TFrom, TTo, TProp>(this Expression<Func<TFrom, TProp>> target)
        {
            return (Expression<Func<TTo, TProp>>)new PropertyReplacerVisitor<TFrom, TTo, TProp>().Visit(target);
        }
        public static IOrderedQueryable<T> DataSort<T>(this IQueryable<T> source, List<SortedParams> thenByExpression)
        {
            if (thenByExpression == null || !thenByExpression.Any())
                return (IOrderedQueryable<T>)source;
            var type = typeof(T);
            var properties = type.GetProperties().ToDictionary(p => p.Name.ToLower(), p => p);
            bool first = true;
            thenByExpression.ForEach(x =>
            {
                if (properties.TryGetValue(x.SortName.ToLower(), out var property))
                {
                    var paramName = "x";
                    var param = Expression.Parameter(type, paramName);
                    var propertyAccess = Expression.Property(param, property.Name);
                    var lambda = Expression.Lambda(propertyAccess, param);
                    var methodName = first ? (x.Sort ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending)) : (x.Sort ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending));
                    var methodCallExpression = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { type, property.PropertyType },
                        source.Expression,
                        lambda
                    );
                    source = source.Provider.CreateQuery<T>(methodCallExpression);
                    first = false;
                }
            });
            return (IOrderedQueryable<T>)source;
        }

        private class PropertyReplacerVisitor<TFrom, TTo, TProp> : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TTo));
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return Expression.Lambda<Func<TTo, TProp>>(Visit(node.Body), _parameter);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if ((node.Member.DeclaringType == typeof(Entity) || node.Member.DeclaringType == typeof(TFrom)) && node.Expression is ParameterExpression)
                {
                    return Expression.Property(_parameter, node.Member.Name);
                }
                return base.VisitMember(node);
            }
        }
        private class WhereReplacerVisitor<TFrom, TTo> : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TTo));
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return Expression.Lambda<Func<TTo, bool>>(Visit(node.Body), _parameter);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if ((node.Member.DeclaringType == typeof(Entity) || node.Member.DeclaringType == typeof(TFrom)) && node.Expression is ParameterExpression)
                {
                    return Expression.Property(_parameter, node.Member.Name);
                }
                return base.VisitMember(node);
            }
        }
        private class WhereReplacerDynamicVisitor<TFrom, TTo> : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter = Expression.Parameter(typeof(TTo));
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return Expression.Lambda<Func<TTo, dynamic>>(Visit(node.Body), _parameter);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if ((node.Member.DeclaringType == typeof(Entity) || node.Member.DeclaringType == typeof(TFrom)) && node.Expression is ParameterExpression)
                {
                    return Expression.Property(_parameter, node.Member.Name);
                }
                return base.VisitMember(node);
            }
        }
        internal class ConvertVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression Parameter;

            public ConvertVisitor(ParameterExpression parameter)
            {
                Parameter = parameter;
            }

            protected override Expression VisitParameter(ParameterExpression item)
            {
                // we just check the parameter to return the new value for them
                if (!item.Name.Equals(Parameter.Name))
                    return item;
                return Parameter;
            }
        }

        public static Expression<Func<SetPropertyCalls<PersistenceObject>, SetPropertyCalls<PersistenceObject>>> ExecuteUpdateExtension<PersistenceObject, TProperty>(this Expression<Func<PersistenceObject, TProperty>> setSetProperty, TProperty value) where PersistenceObject : Entity
        {
            var setPropertyMethod = typeof(SetPropertyCalls<>)
                .MakeGenericType(typeof(PersistenceObject))
                .GetMethods()
                .Single(m => m is
                {
                    Name: nameof(SetPropertyCalls<object>.SetProperty),
                    IsGenericMethod: true
                }
                             && m.GetGenericArguments() is [var valueType]
                             && m.GetParameters() is
                             [
                                 _,
                             { ParameterType: var valueParameterType },
                             ]
                             && valueParameterType == valueType)
                .MakeGenericMethod(typeof(TProperty));
            var parameter = Expression.Parameter(typeof(SetPropertyCalls<PersistenceObject>), "c");
            var setPropertyCall = Expression.Call(parameter, setPropertyMethod, setSetProperty, valExp(typeof(TProperty), value));
            setPropertyCall = BuildCall<PersistenceObject>(setPropertyCall, setPropertyMethod, typeof(int?), nameof(Entity.LastUpdateUserId), Common.GetCurrentUser().Id);
            setPropertyCall = BuildCall<PersistenceObject>(setPropertyCall, setPropertyMethod, typeof(DateTime?), nameof(Entity.LastUpdateDate), DateTime.Now);
            return Expression.Lambda<Func<SetPropertyCalls<PersistenceObject>, SetPropertyCalls<PersistenceObject>>>(setPropertyCall, parameter);
        }
        static MethodCallExpression BuildCall<PersistenceObject>(Expression instanse, MethodInfo method, Type type, string name, object value) where PersistenceObject : Entity
        {
            method = method.GetGenericMethodDefinition().MakeGenericMethod(type);
            var setPropertyParameter = Expression.Parameter(typeof(PersistenceObject), "x");
            var memberExpression = Expression.Property(setPropertyParameter, name);
            var setterPropertyLambda = Expression.Lambda(memberExpression, setPropertyParameter);
            return Expression.Call(
                instanse,
                method,
                setterPropertyLambda, valExp(type, value));
        }
        static Expression valExp(Type type, object value) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? Expression.Convert(Expression.Constant(value), type) : Expression.Constant(value);

        // <summary>
        /// 构造动态过滤表达式，支持多种数值类型。
        /// </summary>
        /// <param name="paramType">参数的类型。</param>
        /// <param name="parameter">表达式参数。</param>
        /// <param name="fieldName">要过滤的字段名称。</param>
        /// <param name="minValue">最小值（可选）。</param>
        /// <param name="minOperator">最小值操作符。</param>
        /// <param name="maxValue">最大值（可选）。</param>
        /// <param name="maxOperator">最大值操作符。</param>
        /// <returns>构造的过滤表达式。</returns>
        public static Expression ApplyDynamicFilter(
            Type paramType,
            ParameterExpression parameter,
            string fieldName,
            double? minValue,
            CountQueryOperator minOperator,
            double? maxValue,
            CountQueryOperator maxOperator)
        {
            // 获取指定字段的属性信息，忽略大小写
            PropertyInfo propertyInfo = paramType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

            if (propertyInfo == null)
                throw new ArgumentException($"Property '{fieldName}' not found on type '{paramType.Name}'.");

            // 获取属性的表达式
            Expression property = Expression.Property(parameter, propertyInfo);

            // 确定属性的实际类型，处理可空类型
            Type propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

            // 检查属性是否为数值类型
            if (!IsNumericType(propertyType))
                throw new ArgumentException($"Property '{fieldName}' is not a numeric type.");

            // 创建表达式比较
            Expression comparison1 = null;
            Expression comparison2 = null;

            // 处理最小值条件
            if (minValue.HasValue)
            {
                // 将 minValue 转换为属性的实际类型
                object convertedMinValue = Convert.ChangeType(minValue.Value, propertyType);
                // 创建常量表达式
                ConstantExpression constantMin = Expression.Constant(convertedMinValue, propertyType);

                // 如果属性是可空类型，需要访问其值
                if (IsNullableType(propertyInfo.PropertyType))
                {
                    property = Expression.Property(property, "Value");
                }

                // 构造比较表达式
                comparison1 = GetComparisonExpression(property, constantMin, minOperator);
            }

            // 处理最大值条件
            if (maxValue.HasValue)
            {
                // 将 maxValue 转换为属性的实际类型
                object convertedMaxValue = Convert.ChangeType(maxValue.Value, propertyType);
                // 创建常量表达式
                ConstantExpression constantMax = Expression.Constant(convertedMaxValue, propertyType);

                // 如果属性是可空类型，需要访问其值
                if (IsNullableType(propertyInfo.PropertyType))
                {
                    property = Expression.Property(property, "Value");
                }

                // 构造比较表达式
                comparison2 = GetComparisonExpression(property, constantMax, maxOperator);
            }

            // 组合表达式
            if (comparison1 != null && comparison2 != null)
                return Expression.AndAlso(comparison1, comparison2);
            else if (comparison1 != null)
                return comparison1;
            else
                return comparison2;
        }

        /// <summary>
        /// 判断类型是否为数值类型。
        /// </summary>
        private static bool IsNumericType(Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte) ||
                   type == typeof(short) || type == typeof(ushort) ||
                   type == typeof(int) || type == typeof(uint) ||
                   type == typeof(long) || type == typeof(ulong) ||
                   type == typeof(float) || type == typeof(double) ||
                   type == typeof(decimal);
        }

        /// <summary>
        /// 判断类型是否为可空类型。
        /// </summary>
        private static bool IsNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// 根据操作符构造比较表达式。
        /// </summary>
        private static Expression GetComparisonExpression(Expression property, ConstantExpression constant, CountQueryOperator op)
        {
            switch (op)
            {
                case CountQueryOperator.Equal:
                    return Expression.Equal(property, constant);
                case CountQueryOperator.GreaterThan:
                    return Expression.GreaterThan(property, constant);
                case CountQueryOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(property, constant);
                case CountQueryOperator.LessThan:
                    return Expression.LessThan(property, constant);
                case CountQueryOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(property, constant);
                default:
                    throw new NotSupportedException($"Operator '{op}' is not supported.");
            }
        }
    }

    public enum CountQueryLogic
    {
        /// <summary>
        /// 或
        /// </summary>
        Or = 0,
        /// <summary>
        /// 和
        /// </summary>
        And = 1
    }
    public enum CountQueryOperator
    {
        /// <summary>
        /// 等于
        /// </summary>
        Equal = 0,
        /// <summary>
        /// 大于
        /// </summary>
        GreaterThan = 1,
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterThanOrEqual = 2,
        /// <summary>
        /// 小于等于
        /// </summary>
        LessThanOrEqual = 3,
        /// <summary>
        /// 小于
        /// </summary>
        LessThan = 4,
    }
}
