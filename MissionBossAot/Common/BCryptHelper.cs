using S7.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MissionBossAot.Common
{
    public class BCryptHelper
    {
        public static BCryptHelper sBCryptHelper = new BCryptHelper();
        public BCryptHelper() { }
        public string HashPassword(string plainPassword)
        {
            // 自动生成盐，并返回完整的哈希字符串
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
            // workFactor 推荐 10-12，越大越安全但越慢
            return hashedPassword; // 直接存入数据库
        }
        public bool VerifyPassword(string plainPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
        }
    }
    public class MD5Helper
    {
        public static MD5Helper sMD5Helper = new MD5Helper();
        public MD5Helper()
        {
        }
        //计算字符串的MD5
        public string GetMd5InString(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // 转换为十六进制字符串
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("X2")); // "x2" 表示小写，"X2" 表示大写
                }
                return sb.ToString();
            }
        }
    }
    public class ObjectJson
    {
        private readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // 保持原属性名
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly JsonSerializerOptions IndentedOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static ObjectJson sObjectJson = new ObjectJson();
        /// <summary>
        /// 序列化为 JSON 字符串
        /// </summary>
        public string Serialize<T>(T obj, bool indented = false)
        {
            if (obj == null) return null;

            try
            {
                var options = indented ? IndentedOptions : DefaultOptions;
                return JsonSerializer.Serialize(obj, options);
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 反序列化为对象
        /// </summary>
        private T Deserialize<T>(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json)) return default;
                return JsonSerializer.Deserialize<T>(json, DefaultOptions);
            }
            catch (Exception)
            {
                return default;
            }
        }
        /// <summary>
        /// 尝试反序列化
        /// </summary>
        public bool TryDeserialize<T>(string json, out T result)
        {
            try
            {
                result = Deserialize<T>(json);
                return result != null;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
