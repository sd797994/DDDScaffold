using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
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
                // 1. 定义原类型/目标类型
                var srcType = sourceItem.PropertyType;
                var tgtType = targetItem.PropertyType;
                // 2. 找到底层枚举类型（如果不是 Nullable，则就是自己）
                var srcEnum = Nullable.GetUnderlyingType(srcType) ?? srcType;
                var tgtEnum = Nullable.GetUnderlyingType(tgtType) ?? tgtType;
                // 3. 只要任一侧是枚举，就走这里
                if (srcEnum.IsEnum || tgtEnum.IsEnum)
                {
                    Expression expr;
                    // 3.1 源是 Nullable<Enum>：要先检查 HasValue
                    if (Nullable.GetUnderlyingType(srcType) != null)
                    {
                        var hasValue = Property(sourceProperty, nameof(Nullable<int>.HasValue));
                        var value = Property(sourceProperty, nameof(Nullable<int>.Value));
                        var converted = Convert(value, tgtType);
                        // false 分支：目标如果也可空，就 null，否则 default(T)
                        Expression defaultValue = Nullable.GetUnderlyingType(tgtType) != null
                            ? Constant(null, tgtType)
                            : Default(tgtType);
                        expr = Condition(hasValue, converted, defaultValue);
                    }
                    else
                    {
                        // 3.2 其它所有枚举↔值类型或枚举↔枚举，直接 Convert
                        expr = Convert(sourceProperty, tgtType);
                    }
                    memberBindings.Add(Bind(targetItem, expr));
                    continue;
                }
                else if (targetItem.PropertyType != sourceItem.PropertyType)
                {
                    if (targetItem.PropertyType.IsGenericType
                        && targetItem.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                        && !sourceItem.PropertyType.IsGenericType)
                    {
                        var converted = Convert(sourceProperty, targetItem.PropertyType);
                        memberBindings.Add(Bind(targetItem, converted));
                    }
                    else if (sourceItem.PropertyType.IsGenericType
                             && sourceItem.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                             && !targetItem.PropertyType.IsGenericType)
                    {
                        var underlying = Nullable.GetUnderlyingType(sourceItem.PropertyType);
                        if (underlying != null && !underlying.IsEnum)
                        {
                            var method = ConvertMethodDir.GetConvertMethod(sourceItem.PropertyType);
                            var converted = Convert(sourceProperty, targetItem.PropertyType, method);
                            memberBindings.Add(Bind(targetItem, converted));
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                    memberBindings.Add(Bind(targetItem, sourceProperty));
            }

            //创建一个if条件表达式
            var test = NotEqual(parameter, Constant(null, sourceType));// p==null;
            var ifTrue = MemberInit(New(targetType), memberBindings);
            Expression defaultTarget = Nullable.GetUnderlyingType(targetType) != null
            ? Constant(null, targetType)
            : Default(targetType);
            var condition = Condition(test, ifTrue, defaultTarget);

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
                var srcType = sp.PropertyType;
                var tgtType = tp.PropertyType;
                var srcEnum = Nullable.GetUnderlyingType(srcType) ?? srcType;
                var tgtEnum = Nullable.GetUnderlyingType(tgtType) ?? tgtType;
                if (srcEnum.IsEnum || tgtEnum.IsEnum)
                {
                    Expression expr;
                    if (Nullable.GetUnderlyingType(srcType) != null)
                    {
                        var hasValue = Property(srcExpr, nameof(Nullable<int>.HasValue));
                        var value = Property(srcExpr, nameof(Nullable<int>.Value));
                        var converted = Convert(value, tgtType);
                        Expression defaultValue = Nullable.GetUnderlyingType(tgtType) != null
                            ? Constant(null, tgtType)
                            : Default(tgtType);
                        expr = Condition(hasValue, converted, defaultValue);
                    }
                    else
                    {
                        expr = Convert(srcExpr, tgtType);
                    }
                    assigns.Add(Assign(tgtExpr, expr));
                    continue;
                }
                if (sp.PropertyType.IsGenericType && sp.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && !tp.PropertyType.IsGenericType && Nullable.GetUnderlyingType(sp.PropertyType) is Type underlying && !underlying.IsEnum)
                {
                    var method = ConvertMethodDir.GetConvertMethod(sp.PropertyType);
                    var converted = Convert(srcExpr, tp.PropertyType, method);
                    assigns.Add(Assign(tgtExpr, converted));
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
