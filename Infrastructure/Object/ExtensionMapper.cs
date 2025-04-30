using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace InfrastructureBase.Object
{
    public static class ExtensionMapper<TSource, TTarget> where TSource : class where TTarget : class
    {
        public readonly static Func<TSource, TTarget> MapFunc = GetMapFunc();
        public readonly static Action<TSource, TTarget> MapAction = GetMapAction();

        /// <summary>
        /// 将对象TSource转换为TTarget
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TTarget Map(TSource source) => MapFunc(source);

        public static List<TTarget> MapList(IEnumerable<TSource> sources) => sources.Select(MapFunc).ToList();



        /// <summary>
        /// 将对象TSource的值赋给给TTarget
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Map(TSource source, TTarget target) => MapAction(source, target);
        private static Func<TSource, TTarget> GetMapFunc()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);
            //Func委托传入变量
            var parameter = Parameter(sourceType, "p");

            var memberBindings = new List<MemberBinding>();
            var targetTypes = targetType.GetProperties().Where(x => x.PropertyType.IsPublic && x.CanWrite);
            foreach (var targetItem in targetTypes)
            {
                var sourceItem = sourceType.GetProperty(targetItem.Name);

                //判断实体的读写权限
                if (sourceItem == null || !sourceItem.CanRead || sourceItem.PropertyType.IsNotPublic)
                    continue;

                ////标注NotMapped特性的属性忽略转换
                //if (sourceItem.GetCustomAttribute<NotMappedAttribute>() != null)
                //    continue;

                var sourceProperty = Property(parameter, sourceItem);

                //当非值类型且类型不相同时
                if (!sourceItem.PropertyType.IsValueType && sourceItem.PropertyType != targetItem.PropertyType)
                {
                    if (sourceItem.PropertyType == typeof(string) || targetItem.PropertyType == typeof(string))
                    {
                        continue;
                    }
                    //判断都是(非泛型)class
                    if (sourceItem.PropertyType.IsClass && targetItem.PropertyType.IsClass &&
                    !sourceItem.PropertyType.IsGenericType && !targetItem.PropertyType.IsGenericType)
                    {
                        var expression = GetClassExpression(sourceProperty, sourceItem.PropertyType, targetItem.PropertyType);
                        memberBindings.Add(Bind(targetItem, expression));
                    }
                    //集合数组类型的转换
                    if (typeof(IEnumerable).IsAssignableFrom(sourceItem.PropertyType) && typeof(IEnumerable).IsAssignableFrom(targetItem.PropertyType))
                    {
                        var expression = GetListExpression(sourceProperty, sourceItem.PropertyType, targetItem.PropertyType);
                        memberBindings.Add(Bind(targetItem, expression));
                    }
                    continue;
                }
                //当两者是枚举 - 值类型关系时
                var srcUnderlying = Nullable.GetUnderlyingType(sourceItem.PropertyType);
                var tgtUnderlying = Nullable.GetUnderlyingType(targetItem.PropertyType);
                bool srcIsNullableEnum = srcUnderlying != null && srcUnderlying.IsEnum;
                bool tgtIsNullableEnum = tgtUnderlying != null && tgtUnderlying.IsEnum;
                bool srcIsEnum = sourceItem.PropertyType.IsEnum;
                bool tgtIsEnum = targetItem.PropertyType.IsEnum;

                if (srcIsNullableEnum || tgtIsEnum || tgtIsNullableEnum || srcIsEnum)
                {
                    if (srcIsNullableEnum || srcUnderlying != null)
                    {
                        var nullableType = typeof(Nullable<>).MakeGenericType(srcUnderlying);
                        var hasValueProp = nullableType.GetProperty(nameof(Nullable<int>.HasValue));
                        var valueProp = nullableType.GetProperty(nameof(Nullable<int>.Value));
                        var hasValue = Property(sourceProperty, hasValueProp);
                        var getValue = Property(sourceProperty, valueProp);
                        var trueBranch = Convert(getValue, targetItem.PropertyType);
                        Expression falseBranch;
                        if (tgtIsNullableEnum)
                        {
                            falseBranch = Constant(null, targetItem.PropertyType);
                        }
                        else
                        {
                            falseBranch = Default(targetItem.PropertyType);
                        }
                        var enumConvertExpr = Condition(
                            hasValue,
                            trueBranch,
                            falseBranch
                        );

                        memberBindings.Add(Bind(targetItem, enumConvertExpr));
                    }
                    else
                    {
                        var directConvert = Convert(sourceProperty, targetItem.PropertyType);
                        memberBindings.Add(Bind(targetItem, directConvert));
                    }
                    continue;
                }
                if ((sourceItem.PropertyType.IsValueType && targetItem.PropertyType.IsEnum)
                    || (targetItem.PropertyType.IsValueType && sourceItem.PropertyType.IsEnum))
                {
                    var expression = Convert(sourceProperty, targetItem.PropertyType);
                    memberBindings.Add(Bind(targetItem, expression));
                    continue;
                }
                else if (targetItem.PropertyType != sourceItem.PropertyType)
                {
                    if (targetItem.PropertyType.IsGenericType && targetItem.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && !sourceItem.PropertyType.IsGenericType)
                    {
                        var converted = Convert(sourceProperty, targetItem.PropertyType);
                        memberBindings.Add(Bind(targetItem, converted));
                    }
                    else if (sourceItem.PropertyType.IsGenericType && sourceItem.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && !targetItem.PropertyType.IsGenericType)
                    {
                        memberBindings.Add(Bind(targetItem, Convert(sourceProperty, targetItem.PropertyType, ConvertMethodDir.GetConvertMethod(sourceItem.PropertyType))));
                    }
                    else
                        continue;
                }
                else
                    memberBindings.Add(Bind(targetItem, sourceProperty));
            }

            //创建一个if条件表达式
            var test = NotEqual(parameter, Constant(null, sourceType));// p==null;
            var ifTrue = MemberInit(New(targetType), memberBindings);
            var condition = Condition(test, ifTrue, Constant(null, targetType));

            var lambda = Lambda<Func<TSource, TTarget>>(condition, parameter);
            return lambda.Compile();
        }

        /// <summary>
        /// 类型是clas时赋值
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static Expression GetClassExpression(Expression sourceProperty, Type sourceType, Type targetType)
        {
            //条件p.Item!=null    
            var testItem = NotEqual(sourceProperty, Constant(null, sourceType));

            //构造回调 Mapper<TSource, TTarget>.Map()
            var mapperType = typeof(ExtensionMapper<,>).MakeGenericType(sourceType, targetType);
            var iftrue = Call(mapperType.GetMethod(nameof(Map), new[] { sourceType }), sourceProperty);

            var conditionItem = Condition(testItem, iftrue, Constant(null, targetType));

            return conditionItem;
        }

        /// <summary>
        /// 类型为集合时赋值
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static Expression GetListExpression(Expression sourceProperty, Type sourceType, Type targetType)
        {
            //条件p.Item!=null    
            var testItem = NotEqual(sourceProperty, Constant(null, sourceType));

            //构造回调 Mapper<TSource, TTarget>.MapList()
            var sourceArg = sourceType.IsArray ? sourceType.GetElementType() : sourceType.GetGenericArguments()[0];
            var targetArg = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];
            var mapperType = typeof(ExtensionMapper<,>).MakeGenericType(sourceArg, targetArg);

            var mapperExecMap = Call(mapperType.GetMethod(nameof(MapList), new[] { sourceType }), sourceProperty);

            Expression iftrue;
            if (targetType == mapperExecMap.Type)
            {
                iftrue = mapperExecMap;
            }
            else if (targetType.IsArray)//数组类型调用ToArray()方法
            {
                iftrue = Call(mapperExecMap, mapperExecMap.Type.GetMethod("ToArray"));
            }
            else if (typeof(IDictionary).IsAssignableFrom(targetType))
            {
                iftrue = Constant(null, targetType);//字典类型不转换
            }
            else
            {
                iftrue = Convert(mapperExecMap, targetType);
            }

            var conditionItem = Condition(testItem, iftrue, Constant(null, targetType));

            return conditionItem;
        }

        private static Action<TSource, TTarget> GetMapAction()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var sourceParam = Parameter(sourceType, "p");   // source
            var targetParam = Parameter(targetType, "t");   // target

            var assigns = new List<Expression>();

            var targetProps = targetType.GetProperties()
                                        .Where(x => x.CanWrite && x.PropertyType.IsPublic);

            foreach (var tp in targetProps)
            {
                var sp = sourceType.GetProperty(tp.Name);
                if (sp == null || !sp.CanRead || sp.PropertyType.IsNotPublic) continue;

                var srcExpr = Property(sourceParam, sp);
                var tgtExpr = Property(targetParam, tp);
                if (!sp.PropertyType.IsValueType && sp.PropertyType != tp.PropertyType)
                {
                    if (sp.PropertyType == typeof(string) || tp.PropertyType == typeof(string))
                        continue;

                    if (sp.PropertyType.IsClass && tp.PropertyType.IsClass &&
                        !sp.PropertyType.IsGenericType && !tp.PropertyType.IsGenericType)
                    {
                        var expr = GetClassExpression(srcExpr, sp.PropertyType, tp.PropertyType);
                        assigns.Add(Assign(tgtExpr, expr));
                        continue;
                    }
                    if (typeof(IEnumerable).IsAssignableFrom(sp.PropertyType) &&
                        typeof(IEnumerable).IsAssignableFrom(tp.PropertyType))
                    {
                        var expr = GetListExpression(srcExpr, sp.PropertyType, tp.PropertyType);
                        assigns.Add(Assign(tgtExpr, expr));
                        continue;
                    }
                }

                var srcUnderlying = Nullable.GetUnderlyingType(sp.PropertyType);
                var tgtUnderlying = Nullable.GetUnderlyingType(tp.PropertyType);
                bool srcIsNullableEnum = srcUnderlying != null && srcUnderlying.IsEnum;
                bool tgtIsNullableEnum = tgtUnderlying != null && tgtUnderlying.IsEnum;
                bool srcIsEnum = sp.PropertyType.IsEnum;
                bool tgtIsEnum = tp.PropertyType.IsEnum;

                if ((srcIsNullableEnum || tgtIsEnum) || (tgtIsNullableEnum || srcIsEnum))
                {
                    if (srcIsNullableEnum || srcUnderlying != null)
                    {
                        var nullableType = typeof(Nullable<>).MakeGenericType(srcUnderlying);
                        var hasValueProp = nullableType.GetProperty(nameof(Nullable<int>.HasValue));
                        var hasValue = Property(srcExpr, hasValueProp);
                        var valueProp = nullableType.GetProperty(nameof(Nullable<int>.Value));
                        var getValue = Property(srcExpr, valueProp);
                        var trueBranch = Convert(getValue, tp.PropertyType);
                        Expression falseBranch;
                        if (tgtIsNullableEnum)
                        {
                            falseBranch = Constant(null, tp.PropertyType);
                        }
                        else
                        {
                            falseBranch = Default(tp.PropertyType);
                        }
                        var enumConvertExpr = Condition(
                            hasValue,
                            trueBranch,
                            falseBranch
                        );
                        assigns.Add(Assign(tgtExpr, enumConvertExpr));


                    }
                    else
                    {
                        var directConvert = Convert(srcExpr, tp.PropertyType);
                        assigns.Add(Assign(tgtExpr, directConvert));
                    }
                    continue;
                }
                if ((sp.PropertyType.IsValueType && tp.PropertyType.IsEnum)
                    || (tp.PropertyType.IsValueType && sp.PropertyType.IsEnum))
                {
                    var expression = Convert(srcExpr, tp.PropertyType);
                    assigns.Add(Assign(tgtExpr, expression));
                    continue;
                }
                if (tp.PropertyType != sp.PropertyType)
                {
                    if (tp.PropertyType.IsGenericType &&
                        tp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                        !sp.PropertyType.IsGenericType)
                    {
                        var converted = Convert(srcExpr, tp.PropertyType);
                        assigns.Add(Assign(tgtExpr, converted));
                        continue;
                    }
                    if (sp.PropertyType.IsGenericType &&
                        sp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                        !tp.PropertyType.IsGenericType)
                    {
                        var method = ConvertMethodDir.GetConvertMethod(sp.PropertyType);
                        var converted = Convert(srcExpr, tp.PropertyType, method);
                        assigns.Add(Assign(tgtExpr, converted));
                        continue;
                    }

                    continue;
                }

                assigns.Add(Assign(tgtExpr, srcExpr));
            }

            var guardSource = IfThen(
                NotEqual(sourceParam, Constant(null, sourceType)),
                Block(assigns));

            var guardTarget = IfThen(
                NotEqual(targetParam, Constant(null, targetType)),
                guardSource);

            var lambda = Lambda<Action<TSource, TTarget>>(
                            guardTarget, sourceParam, targetParam);

            return lambda.Compile();
        }
    }
    public static class ConvertMethodDir
    {
        static Dictionary<Type, MethodInfo> keyValues = new Dictionary<Type, MethodInfo>();
        static ConvertMethodDir()
        {
            keyValues.Add(typeof(Guid?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(Guid) }));
            keyValues.Add(typeof(decimal?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(decimal?) }));
            keyValues.Add(typeof(float?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(float?) }));
            keyValues.Add(typeof(double?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(double?) }));
            keyValues.Add(typeof(int?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(int?) }));
            keyValues.Add(typeof(sbyte?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(sbyte?) }));
            keyValues.Add(typeof(short?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(short?) }));
            keyValues.Add(typeof(long?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(long?) }));
            keyValues.Add(typeof(byte?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(byte?) }));
            keyValues.Add(typeof(uint?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(uint?) }));
            keyValues.Add(typeof(ulong?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(ulong?) }));
            keyValues.Add(typeof(bool?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(bool?) }));
            keyValues.Add(typeof(char?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(char?) }));
            keyValues.Add(typeof(DateTime?), typeof(ConvertMethodDir).GetMethod(nameof(ConvertMethodDir.InitConv), new Type[] { typeof(DateTime?) }));
        }
        public static MethodInfo GetConvertMethod(Type type)
        {
            return keyValues[type];
        }
        public static Guid InitConv(Guid? val) => val ?? default;
        public static decimal InitConv(decimal? val) => val ?? default;
        public static float InitConv(float? val) => val ?? default;
        public static double InitConv(double? val) => val ?? default;
        public static int InitConv(int? val) => val ?? default;
        public static sbyte InitConv(sbyte? val) => val ?? default;
        public static short InitConv(short? val) => val ?? default;
        public static long InitConv(long? val) => val ?? default;
        public static byte InitConv(byte? val) => val ?? default;
        public static uint InitConv(uint? val) => val ?? default;
        public static ulong InitConv(ulong? val) => val ?? default;
        public static bool InitConv(bool? val) => val ?? default;
        public static char InitConv(char? val) => val ?? default;
        public static DateTime InitConv(DateTime? val) => val ?? default;
    }
}