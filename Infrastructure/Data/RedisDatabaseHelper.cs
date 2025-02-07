using StackExchange.Redis;

namespace Infrastructure.Data
{
    public class RedisDatabaseHelper
    {
        private static ConnectionMultiplexer redis;
        private static IDatabase db;

        public static void Initialize(string connectionString)
        {
            redis = ConnectionMultiplexer.Connect(connectionString);
            db = redis.GetDatabase();
        }

        // 添加或更新字符串键值对
        public static void SetString(string key, string value, TimeSpan? expire = null)
        {
            db.StringSet(key, value, expire);
        }

        // 获取字符串键的值
        public static string GetString(string key)
        {
            return db.StringGet(key);
        }

        // 删除键
        public static bool DeleteKey(string key)
        {
            return db.KeyDelete(key);
        }

        // 向列表添加元素
        public static void AddToList(string listKey, string value)
        {
            db.ListRightPush(listKey, value);
        }

        // 获取列表中的所有元素
        public static string[] GetList(string listKey)
        {
            var length = db.ListLength(listKey);
            return db.ListRange(listKey, 0, length - 1).ToStringArray();
        }

        // 删除列表
        public static void DeleteList(string listKey)
        {
            db.KeyDelete(listKey);
        }
    }
}
