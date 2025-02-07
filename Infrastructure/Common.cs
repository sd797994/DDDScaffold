using ApplicaionServiceInterface.Dtos.Responses;
using Autofac;
using Domain.Entities;
using InfrastructureBase.Object;
using Microsoft.Extensions.DependencyModel;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Infrastructure
{
    public class Common
    {
        public static string GetMD5SaltCode(string origin, params object[] salt)
        {
            using var md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(origin + string.Join("", salt)))).Replace("-", "");
        }
        static Lazy<IEnumerable<Assembly>> Assemblies = new Lazy<IEnumerable<Assembly>>(() => DependencyContext.Default.CompileLibraries.Where(lib => !lib.Serviceable && lib.Type != "package" && lib.Type != "referenceassembly").Select(lib =>
        {
            try
            {
                return AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
            }
            catch (Exception)
            {
                return default;
            }
        }).Where(x => x != default));
        public static IEnumerable<Type> GetTypesByAttributes(bool isInterface, params Type[] attributes)
        {
            var t = DependencyContext.Default.CompileLibraries.ToList();
            return Assemblies.Value.SelectMany(a => a.GetTypes().Where(t => (isInterface ? t.IsInterface : !t.IsInterface) && t.GetCustomAttributes().Select(x => x.GetType()).Intersect(attributes).Count() == attributes.Count())).ToArray();
        }
        public static Assembly[] GetProjectAssembliesArray()
        {
            return Assemblies.Value.ToArray();
        }
        static string[] SystemAssemblyQualifiedName = new string[] { "Microsoft", "System" };
        public static bool IsSystemType(Type type, bool checkBaseType = true, bool checkInterfaces = true)
        {
            if (SystemAssemblyQualifiedName.Any(x => type.AssemblyQualifiedName.StartsWith(x)))
                return true;
            else
            {
                if (checkBaseType && type.BaseType != null && type.BaseType != typeof(object) && SystemAssemblyQualifiedName.Any(x => type.BaseType.AssemblyQualifiedName.StartsWith(x)))
                    return true;
                if (checkInterfaces && type.GetInterfaces().Any())
                    if (type.GetInterfaces().Any(i => SystemAssemblyQualifiedName.Any(x => i.AssemblyQualifiedName.StartsWith(x))))
                        return true;
            }
            return false;
        }
        public static IEnumerable<dynamic> GetEnumValue<T>() where T : Enum
        {
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                yield return new { key = Enum.GetName(typeof(T), item), value = (int)item };
            }
        }

        static AsyncLocal<User> currentUser = new AsyncLocal<User>();
        public static User GetCurrentUser() => currentUser.Value ?? new User();
        public static void SetCurrentUser(User user)
        {
            currentUser.Value = user;
        }
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        public static string CalculateSHA256ByBytes(byte[] filebuffer)
        {
            try
            {
                semaphoreSlim.Wait();
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(filebuffer);
                    return BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
        public static MenuRespVo BuildTree(MenuRespVo root, HashSet<int> ids)
        {
            if (root == null)
                return null;

            // 如果当前节点或其任一子节点在目标列表中，则保留
            bool isIncluded = ids.Contains(root.Id) || (root.Children != null && root.Children.Any(c => ids.Contains(c.Id)));

            // 递归构建子树
            var newChildren = new List<MenuRespVo>();
            if (root.Children != null)
                foreach (var child in root.Children)
                {
                    var result = BuildTree(child, ids);
                    if (result != null)
                        newChildren.Add(result);
                }
            root.Children = newChildren;

            // 如果当前节点或其子树包含目标节点，返回当前节点的副本
            if (isIncluded || newChildren.Count > 0)
            {
                return root.CopyTo<MenuRespVo, MenuRespVo>();
            }
            return null;
        }


        static SemaphoreSlim versionSlim = new SemaphoreSlim(1);
        public static string GetVersionNumber()
        {
            // 构建文件的完整路径
            try
            {
                versionSlim.Wait();
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CustomerRelease", "WinFormApp.dll.config");
                if (File.Exists(filePath))
                {
                    XDocument doc = XDocument.Load(filePath);
                    var versionNum = doc.Descendants("add")
                                        .Where(x => (string)x.Attribute("key") == "versionNum")
                                        .Select(x => (string)x.Attribute("value"))
                                        .FirstOrDefault();
                    return versionNum;
                }
            }
            catch (Exception) { }
            finally
            {
                versionSlim.Release();
            }
            return null;
        }
        public static async Task<bool> IsPortOpenAsync(string host, int port, int timeoutMs)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var task = client.ConnectAsync(host, port);
                    if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // 忽略异常，端口不可达或连接被拒绝等情况
            }
            return false;
        }
        public static string GetFullPath(string relativePath)
        {
            // 假定应用的根目录位于当前工作目录下的wwwroot目录
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (relativePath.StartsWith("/"))
                relativePath = relativePath.Substring(1);
            string correctedPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            // 直接使用Path.Combine，它会自动处理不同操作系统下的路径分隔符问题
            string fullPath = Path.Combine(webRootPath, correctedPath);
            return fullPath;
        }
        public static string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));  // x2表示将byte值转换为两位十六进制字符
                }
                return sb.ToString();
            }
        }

    }
}
