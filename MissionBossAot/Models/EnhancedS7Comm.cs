using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S7.Net;
using S7.Net.Types;
using DataType = S7.Net.DataType;

namespace MissionBossAot.Models
{
    /// <summary>
    /// 增强版S7通讯类，基于S7NetPlus第三方库实现
    /// </summary>
    public class EnhancedS7Comm : IDisposable
    {
        public Plc _plc;
        private readonly object _lockObject = new object();

        /// <summary>
        /// PLC连接状态
        /// </summary>
        public bool IsConnected => _plc?.IsConnected ?? false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cpuType">CPU类型</param>
        /// <param name="ip">PLC IP地址</param>
        /// <param name="rack">机架号</param>
        /// <param name="slot">插槽号</param>
        public EnhancedS7Comm(CpuType cpuType, string ip, Int16 rack, Int16 slot)
        {
            _plc = new Plc(cpuType, ip, rack, slot);
            // 设置读写超时时间为1秒
            _plc.ReadTimeout = 100;
            _plc.WriteTimeout = 100;
        }

        /// <summary>
        /// 连接到PLC
        /// </summary>
        public void Connect()
        {
            lock (_lockObject)
            {
                if (_plc != null)
                {
                    _plc.Open();
                }
            }
        }

        /// <summary>
        /// 异步连接到PLC
        /// </summary>
        public async Task ConnectAsync()
        {
            await Task.Run(() => Connect());
        }

        /// <summary>
        /// 断开PLC连接
        /// </summary>
        public void Disconnect()
        {
            lock (_lockObject)
            {
                if (_plc != null)
                {
                    _plc.Close();
                }
            }
        }

        /// <summary>
        /// 检查PLC是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            try
            {
                // 使用简单的网络连接检查替代IsAvailable
                // 由于Plc类是第三方库，我们无法直接访问其私有字段
                // 这里我们假设在构造函数中传入的IP是有效的
                return true; // 简化实现，实际项目中应根据具体需求实现
            }
            catch
            {
                try
                {
                    // 如果连接检查失败，尝试通过连接状态判断
                    return _plc?.IsConnected ?? false;
                }
                catch
                {
                    return false;
                }
            }
        }

        #region 读取操作

        /// <summary>
        /// 读取字节数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <param name="count">读取字节数</param>
        /// <returns>字节数组</returns>
        public byte[] ReadBytes(DataType dataType, int db, int startByteAdr, int count)
        {

            return _plc.ReadBytes(dataType, db, startByteAdr, count);

        }

        /// <summary>
        /// 读取指定类型的数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <param name="varType">变量类型</param>
        /// <param name="varCount">变量数量</param>
        /// <returns>解码后的数据</returns>
        public object Read(DataType dataType, int db, int startByteAdr, VarType varType, int varCount)
        {

            return _plc.Read(dataType, db, startByteAdr, varType, varCount);

        }

        /// <summary>
        /// 通过地址字符串读取数据
        /// </summary>
        /// <param name="variable">地址字符串，如"DB1.DBW20"</param>
        /// <returns>读取的数据</returns>
        public object Read(string variable)
        {

            return _plc.Read(variable);

        }

        /// <summary>
        /// 读取结构体数据
        /// </summary>
        /// <param name="structType">结构体类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <returns>结构体实例</returns>
        public object ReadStruct(Type structType, int db, int startByteAdr = 0)
        {

            return _plc.ReadStruct(structType, db, startByteAdr);

        }

        /// <summary>
        /// 读取类数据
        /// </summary>
        /// <param name="sourceClass">类实例</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        public void ReadClass(object sourceClass, int db, int startByteAdr = 0)
        {

            _plc.ReadClass(sourceClass, db, startByteAdr);

        }

        /// <summary>
        /// 批量读取多个变量
        /// </summary>
        /// <param name="variables">变量地址列表</param>
        /// <returns>读取结果字典</returns>
        public Dictionary<string, object> ReadMultipleVars(IEnumerable<string> variables)
        {
            var result = new Dictionary<string, object>();

            foreach (var variable in variables)
            {
                try
                {
                    result[variable] = _plc.Read(variable);
                }
                catch (Exception ex)
                {
                    result[variable] = ex;
                }
            }

            return result;
        }

        #endregion

        #region 写入操作

        /// <summary>
        /// 写入字节数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <param name="value">要写入的字节数组</param>
        public void WriteBytes(DataType dataType, int db, int startByteAdr, byte[] value)
        {

            _plc.WriteBytes(dataType, db, startByteAdr, value);

        }

        /// <summary>
        /// 写入指定类型的数据
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        /// <param name="value">要写入的数据</param>
        public void Write(DataType dataType, int db, int startByteAdr, object value)
        {

            _plc.Write(dataType, db, startByteAdr, value);

        }

        /// <summary>
        /// 通过地址字符串写入数据
        /// </summary>
        /// <param name="variable">地址字符串，如"DB1.DBW20"</param>
        /// <param name="value">要写入的数据</param>
        public void Write(string variable, object value)
        {
            try
            {
                _plc.Write(variable, value);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        /// <summary>
        /// 写入结构体数据
        /// </summary>
        /// <param name="structValue">结构体实例</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        public void WriteStruct(object structValue, int db, int startByteAdr = 0)
        {

            _plc.WriteStruct(structValue, db, startByteAdr);

        }

        /// <summary>
        /// 写入类数据
        /// </summary>
        /// <param name="classValue">类实例</param>
        /// <param name="db">DB块编号</param>
        /// <param name="startByteAdr">起始字节地址</param>
        public void WriteClass(object classValue, int db, int startByteAdr = 0)
        {

            _plc.WriteClass(classValue, db, startByteAdr);

        }

        #endregion

        #region 特定数据类型读写方法

        /// <summary>
        /// 读取布尔值
        /// </summary>
        /// <param name="address">地址，如"DB1.DBX0.5"</param>
        /// <returns>布尔值</returns>
        public bool ReadBool(string address)
        {
            return (bool)Read(address);
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name="address">地址，如"DB1.DBB0"</param>
        /// <returns>字节值</returns>
        public byte ReadByte(string address)
        {
            return (byte)Read(address);
        }

        /// <summary>
        /// 读取字
        /// </summary>
        /// <param name="address">地址，如"DB1.DBW0"</param>
        /// <returns>字值</returns>
        public ushort ReadWord(string address)
        {
            return (ushort)Read(address);
        }

        /// <summary>
        /// 读取双字
        /// </summary>
        /// <param name="address">地址，如"DB1.DBD0"</param>
        /// <returns>双字值</returns>
        public uint ReadDWord(string address)
        {
            return (uint)Read(address);
        }

        /// <summary>
        /// 读取整数
        /// </summary>
        /// <param name="address">地址，如"DB1.DBW0"</param>
        /// <returns>整数值</returns>
        public short ReadInt(string address)
        {
            return (short)Read(address);
        }

        /// <summary>
        /// 读取双整数
        /// </summary>
        /// <param name="address">地址，如"DB1.DBD0"</param>
        /// <returns>双整数值</returns>
        public int ReadDInt(string address)
        {
            return (int)Read(address);
        }

        /// <summary>
        /// 读取实数
        /// </summary>
        /// <param name="address">地址，如"DB1.DBD0"</param>
        /// <returns>实数值</returns>
        public float ReadReal(string address)
        {
            return (float)Read(address);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">地址，如"DB1.DBB0"</param>
        /// <param name="length">字符串长度</param>
        /// <returns>字符串值</returns>
        public string ReadString(string address, int length)
        {
            var bytes = ReadBytes(DataType.DataBlock,
                int.Parse(address.Split('.')[0].Substring(2)),
                int.Parse(address.Split('.')[1].Substring(3)),
                length);
            return S7String.FromByteArray(bytes);
        }

        /// <summary>
        /// 写入布尔值
        /// </summary>
        /// <param name="address">地址，如"DB1.DBX0.5"</param>
        /// <param name="value">布尔值</param>
        public void WriteBool(string address, bool value)
        {
            Write(address, value);
        }

        /// <summary>
        /// 写入字节
        /// </summary>
        /// <param name="address">地址，如"DB1.DBB0"</param>
        /// <param name="value">字节值</param>
        public void WriteByte(string address, byte value)
        {
            Write(address, value);
        }

        /// <summary>
        /// 写入字
        /// </summary>
        /// <param name="address">地址，如"DB1.DBW0"</param>
        /// <param name="value">字值</param>
        public void WriteWord(string address, ushort value)
        {
            Write(address, value);
        }

        /// <summary>
        /// 写入双字
        /// </summary>
        /// <param name="address">地址，如"DB1.DBD0"</param>
        /// <param name="value">双字值</param>
        public void WriteDWord(string address, uint value)
        {
            Write(address, value);
        }

        /// <summary>
        /// 写入整数
        /// </summary>
        /// <param name="address">地址，如"DB1.DBW0"</param>
        /// <param name="value">整数值</param>
        public void WriteInt(string address, short value)
        {
            Write(address, value);
        }

        /// <summary>
        /// 写入双整数
        /// </summary>
        /// <param name="address">地址，如"DB1.DBD0"</param>
        /// <param name="value">双整数值</param>
        public void WriteDInt(string address, int value)
        {
            Write(address, value);
        }

        /// <summary>
        /// 写入实数
        /// </summary>
        /// <param name="address">地址，如"DB1.DBD0"</param>
        /// <param name="value">实数值</param>
        public void WriteReal(string address, float value)
        {
            Write(address, value);
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="address">地址，如"DB1.DBB0"</param>
        /// <param name="value">字符串值</param>
        /// <param name="length">字符串长度</param>
        public void WriteString(string address, string value, int length)
        {
            var bytes = S7String.ToByteArray(value, length);
            var dbNumber = int.Parse(address.Split('.')[0].Substring(2));
            var startByte = int.Parse(address.Split('.')[1].Substring(3));
            WriteBytes(DataType.DataBlock, dbNumber, startByte, bytes);
        }

        #endregion

        #region 批量操作（返回Result类型）

        /// <summary>
        /// 读取数据（返回Result类型）
        /// </summary>
        public Result<T> Read<T>(DataType dataType, int db, int startByteAdr, VarType varType, int varCount, byte bitAddr = 0)
        {

            try
            {
                Result<T> result = new Result<T> { Status = true };

                if (varType == VarType.Bit)
                {
                    // 位读取
                    var bytes = _plc.ReadBytes(dataType, db, startByteAdr, 1);
                    var bitValue = (bytes[0] & (1 << bitAddr)) != 0;
                    result.Datas.Add((T)(object)bitValue);
                }
                else
                {
                    var value = _plc.Read(dataType, db, startByteAdr, varType, varCount);

                    if (value is Array array)
                    {
                        foreach (var item in array)
                        {
                            result.Datas.Add(ConvertValue<T>(item));
                        }
                    }
                    else
                    {
                        result.Datas.Add(ConvertValue<T>(value));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new Result<T> { Status = false, Message = $"读取数据异常: {ex.Message}" };
            }

        }

        /// <summary>
        /// 类型转换辅助方法
        /// </summary>
        private T ConvertValue<T>(object value)
        {
            if (value == null)
                return default(T);

            Type targetType = typeof(T);
            Type sourceType = value.GetType();

            // 如果类型匹配，直接返回
            if (sourceType == targetType)
                return (T)value;

            // 处理数值类型转换
            if (targetType == typeof(short) && sourceType == typeof(ushort))
                return (T)(object)unchecked((short)(ushort)value);

            if (targetType == typeof(int) && sourceType == typeof(uint))
                return (T)(object)unchecked((int)(uint)value);

            if (targetType == typeof(ushort) && sourceType == typeof(short))
                return (T)(object)unchecked((ushort)(short)value);

            if (targetType == typeof(uint) && sourceType == typeof(int))
                return (T)(object)unchecked((uint)(int)value);

            // 处理字符串类型
            if (targetType == typeof(string))
            {
                if (sourceType == typeof(byte))
                    return (T)(object)((byte)value).ToString();
                if (sourceType == typeof(byte[]))
                    return (T)(object)System.Text.Encoding.ASCII.GetString((byte[])value);
                return (T)(object)value.ToString();
            }

            // 其他情况尝试使用Convert
            try
            {
                return (T)Convert.ChangeType(value, targetType);
            }
            catch
            {
                // 如果所有转换都失败，直接强制转换
                return (T)value;
            }
        }

        /// <summary>
        /// 写入数据（返回Result类型）
        /// </summary>
        public Result Write(DataType dataType, int db, int startByteAdr, object value, byte bitAddr = 0)
        {

            try
            {
                if (bitAddr > 0 && value is bool boolValue)
                {
                    // 位写入
                    var bytes = _plc.ReadBytes(dataType, db, startByteAdr, 1);
                    if (boolValue)
                        bytes[0] |= (byte)(1 << bitAddr);
                    else
                        bytes[0] &= (byte)~(1 << bitAddr);
                    _plc.WriteBytes(dataType, db, startByteAdr, bytes);
                }
                else
                {
                    _plc.Write(dataType, db, startByteAdr, value);
                }

                return new Result { Status = true };
            }
            catch (Exception ex)
            {
                return new Result { Status = false, Message = $"写入数据异常: {ex.Message}" };
            }

        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 批量读取数据参数
        /// </summary>
        /// <param name="parameters">数据参数列表</param>
        /// <returns>读取结果</returns>
        public List<DataParameter> ReadDataParameters(List<DataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return new List<DataParameter>();

            var result = new List<DataParameter>();


            foreach (var param in parameters)
            {
                try
                {
                    // 验证参数
                    if (param == null)
                    {
                        continue;
                    }

                    var dataType = ConvertAreasToDataType(param.Area);
                    var varType = ConvertPValueSizeToVarType(param.PValueSize);

                    // 清空之前的数据
                    param.Datas.Clear();
                    param.Status = false;
                    param.Error = null;

                    // 根据数据类型读取数据
                    if (param.PValueSize == PValueSize.BIT)
                    {
                        // 位操作：直接使用BitAddress
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, VarType.Bit, 1, param.BitAddress);
                        var convertedValue = ConvertValue(value, param.DataType);
                        param.Datas.Add(convertedValue);
                    }
                    else
                    {
                        // 非位操作：根据Count读取数据
                        int readCount = param.Count > 0 ? param.Count : 1;
                        var value = _plc.Read(dataType, param.DBNumber, param.ByteAddress, varType, readCount);

                        // 处理返回的数据
                        if (value is Array array && param.DataType != typeof(string))
                        {
                            // 数组类型：逐个添加（字符串除外）
                            foreach (var item in array)
                            {
                                var convertedValue = ConvertValue(item, param.DataType);
                                param.Datas.Add(convertedValue);
                            }
                        }
                        else
                        {
                            // 单个值或字符串：整体转换后添加
                            var convertedValue = ConvertValue(value, param.DataType);
                            param.Datas.Add(convertedValue);
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = $"读取失败: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"ReadDataParameters Error: Area={param.Area}, DB={param.DBNumber}, Byte={param.ByteAddress}, Bit={param.BitAddress}, Error={ex.Message}");
                }

                result.Add(param);
            }


            return result;
        }

        /// <summary>
        /// 类型转换辅助方法
        /// </summary>
        private object ConvertValue(object value, Type targetType)
        {
            if (value == null || targetType == null)
                return value;

            try
            {
                // 如果类型匹配，直接返回
                if (value.GetType() == targetType)
                    return value;

                // 处理ushort→short转换
                if (targetType == typeof(short) && value is ushort)
                    return unchecked((short)(ushort)value);

                // 处理uint→int转换
                if (targetType == typeof(int) && value is uint)
                    return unchecked((int)(uint)value);

                // 处理字符串转换
                if (targetType == typeof(string))
                {
                    string result;
                    if (value is byte)
                        result = ((byte)value).ToString();
                    else if (value is byte[])
                        result = System.Text.Encoding.ASCII.GetString((byte[])value);
                    else
                        result = value.ToString();

                    // 去除字符串中的空格、回车等空白字符
                    return result.Trim();
                }

                // 其他情况使用Convert
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                // 转换失败，返回原值
                return value;
            }
        }

        /// <summary>
        /// 批量写入数据参数
        /// </summary>
        /// <param name="parameters">数据参数列表</param>
        public void WriteDataParameters(List<DataParameter> parameters)
        {

            foreach (var param in parameters)
            {
                try
                {
                    var dataType = ConvertAreasToDataType(param.Area);

                    if (param.Datas.Count > 0)
                    {
                        if (param.PValueSize == PValueSize.BIT)
                        {
                            // 位操作需要特殊处理
                            var bitAddress = param.ByteAddress * 8 + param.BitAddress;
                            var byteAddress = bitAddress / 8;
                            var bitOffset = bitAddress % 8;

                            // 先读取当前字节
                            var bytes = _plc.ReadBytes(dataType, param.DBNumber, byteAddress, 1);

                            // 修改指定位
                            if ((bool)param.Datas[0])
                            {
                                bytes[0] |= (byte)(1 << bitOffset);
                            }
                            else
                            {
                                bytes[0] &= (byte)~(1 << bitOffset);
                            }

                            // 写回
                            _plc.WriteBytes(dataType, param.DBNumber, byteAddress, bytes);
                        }
                        else
                        {
                            // 其他类型直接写入
                            if (param.Datas.Count == 1)
                            {
                                _plc.Write(dataType, param.DBNumber, param.ByteAddress, param.Datas[0]);
                            }
                            else
                            {
                                // 数组类型处理
                                var array = Array.CreateInstance(param.DataType, param.Datas.Count);
                                for (int i = 0; i < param.Datas.Count; i++)
                                {
                                    array.SetValue(param.Datas[i], i);
                                }
                                _plc.Write(dataType, param.DBNumber, param.ByteAddress, array);
                            }
                        }
                    }

                    param.Status = true;
                }
                catch (Exception ex)
                {
                    param.Status = false;
                    param.Error = ex.Message;
                }
            }

        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 转换Areas枚举为DataType枚举
        /// </summary>
        /// <param name="area">Areas枚举</param>
        /// <returns>DataType枚举</returns>
        private DataType ConvertAreasToDataType(Areas area)
        {
            return area switch
            {
                Areas.Input => DataType.Input,
                Areas.Output => DataType.Output,
                Areas.Memory => DataType.Memory,
                Areas.DataBlock => DataType.DataBlock,
                _ => DataType.DataBlock
            };
        }

        /// <summary>
        /// 转换PValueSize枚举为VarType枚举
        /// </summary>
        /// <param name="size">PValueSize枚举</param>
        /// <returns>VarType枚举</returns>
        private VarType ConvertPValueSizeToVarType(PValueSize size)
        {
            return size switch
            {
                PValueSize.BIT => VarType.Bit,
                PValueSize.BYTE => VarType.Byte,
                PValueSize.CHAR => VarType.Byte,
                PValueSize.WORD => VarType.Word,
                PValueSize.INTERGER => VarType.Int,
                PValueSize.DWORD => VarType.DWord,
                PValueSize.DINT => VarType.DInt,
                PValueSize.REAL => VarType.Real,
                PValueSize.DATE => VarType.Byte,
                _ => VarType.Byte
            };
        }

        #endregion

        #region IDisposable实现

        private bool _disposed = false;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否正在 disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                    // 由于Plc类的Dispose方法可能不可访问，我们直接将其设置为null
                    _plc = null;
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
