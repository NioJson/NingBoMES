using HandyControl.Data;
using S7.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DataType = S7.Net.DataType;

namespace MissionBossAot.Models
{
    /// <summary>
    /// S7协议通信类，用于与西门子PLC进行数据交互
    /// 内部使用EnhancedS7Comm实现，保持向后兼容
    /// </summary>
    public class S7Comm : IDisposable
    {
        #region 字段和属性

        private EnhancedS7Comm _enhancedComm;
        private string _ip;
        private byte _rack;
        private byte _slot;
        private readonly object _lockObject = new object();
        private int PduSize = 240; // 限制PLC一次处理的字节数
        private bool _disposed = false;

        /// <summary>
        /// 读取超时时间（毫秒）
        /// </summary>
        public int ReadTimeout { get; set; } = 5000;

        #endregion

        #region 构造函数和析构函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">PLC IP地址</param>
        public S7Comm(string ip)
        {
            _ip = ip;
            _rack = 0;
            _slot = 1;
            // 延迟初始化EnhancedS7Comm，直到Connect被调用
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~S7Comm()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Stop()
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
                    // 释放托管资源
                    try
                    {
                        _enhancedComm?.Disconnect();
                        _enhancedComm?.Dispose();
                    }
                    catch
                    {
                        // 忽略关闭连接时的异常
                    }
                    _enhancedComm = null;
                }

                _disposed = true;
            }
        }

        #endregion

        #region 连接管理

        /// <summary>
        /// 连接到PLC
        /// </summary>
        /// <param name="rack">机架号</param>
        /// <param name="slot">插槽号</param>
        /// <param name="timeout">超时时间（毫秒）</param>
        /// <returns>连接结果</returns>
        public Result<bool> Connect(byte rack, byte slot, int timeout = 50)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(S7Comm));


            try
            {
                _rack = rack;
                _slot = slot;

                // 初始化EnhancedS7Comm
                if (_enhancedComm == null)
                {
                    _enhancedComm = new EnhancedS7Comm(CpuType.S71200, _ip, (short)rack, (short)slot);
                }

                // 连接PLC
                _enhancedComm.Connect();

                return new Result<bool> { Status = true, Message = "连接成功" };
            }
            catch (Exception ex)
            {
                return new Result<bool> { Status = false, Message = $"连接异常: {ex.Message}" };
            }

        }

        /// <summary>
        /// COTP连接（已废弃，保留以保持兼容性）
        /// </summary>
        private Result<bool> COTP(byte rack, byte slot)
        {
            // EnhancedS7Comm内部已处理COTP连接
            return new Result<bool> { Status = true };
        }

        /// <summary>
        /// 建立通信（已废弃，保留以保持兼容性）
        /// </summary>
        private Result<bool> SetupCommunication()
        {
            // EnhancedS7Comm内部已处理通信设置
            return new Result<bool> { Status = true };
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取数据类型的字节大小
        /// </summary>
        private int GetTypeSize(Type dataType)
        {
            if (dataType == typeof(bool)) return 1;
            if (dataType == typeof(byte)) return 1;
            if (dataType == typeof(short)) return 2;
            if (dataType == typeof(ushort)) return 2;
            if (dataType == typeof(int)) return 4;
            if (dataType == typeof(uint)) return 4;
            if (dataType == typeof(float)) return 4;
            if (dataType == typeof(double)) return 8;
            if (dataType == typeof(string)) return 1;
            return Marshal.SizeOf(dataType);
        }

        /// <summary>
        /// 根据运行时类型读取数据
        /// </summary>
        private Result<object> ReadByType(DataType dataType, int db, int offset, VarType varType, int count, Type type)
        {
            try
            {
                if (type == typeof(bool))
                    return ConvertResult(_enhancedComm.Read<bool>(dataType, db, offset, varType, count));
                else if (type == typeof(byte))
                    return ConvertResult(_enhancedComm.Read<byte>(dataType, db, offset, varType, count));
                else if (type == typeof(short))
                    return ConvertResult(_enhancedComm.Read<short>(dataType, db, offset, varType, count));
                else if (type == typeof(ushort))
                    return ConvertResult(_enhancedComm.Read<ushort>(dataType, db, offset, varType, count));
                else if (type == typeof(int))
                    return ConvertResult(_enhancedComm.Read<int>(dataType, db, offset, varType, count));
                else if (type == typeof(uint))
                    return ConvertResult(_enhancedComm.Read<uint>(dataType, db, offset, varType, count));
                else if (type == typeof(float))
                    return ConvertResult(_enhancedComm.Read<float>(dataType, db, offset, varType, count));
                else if (type == typeof(double))
                    return ConvertResult(_enhancedComm.Read<double>(dataType, db, offset, varType, count));
                else if (type == typeof(string))
                    return ConvertResult(_enhancedComm.Read<string>(dataType, db, offset, varType, count));
                else
                    return new Result<object> { Status = false, Message = $"不支持的数据类型: {type.Name}" };
            }
            catch (Exception ex)
            {
                return new Result<object> { Status = false, Message = $"读取数据异常: {ex.Message}" };
            }
        }

        /// <summary>
        /// 转换Result<T>为Result<object>
        /// </summary>
        private Result<object> ConvertResult<T>(Result<T> source)
        {
            var result = new Result<object>
            {
                Status = source.Status,
                Message = source.Message
            };
            foreach (var item in source.Datas)
            {
                result.Datas.Add(item);
            }
            return result;
        }

        #endregion

        #region 读取操作

        /// <summary>
        /// 读取PLC数据
        /// </summary>
        public Result<T> Read<T>(Areas area, ushort db, PValueSize pValueSize, int byteAddr, ushort count, byte bitAddr = 0)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(S7Comm));

            if (_enhancedComm == null || !_enhancedComm.IsConnected)
                return new Result<T> { Status = false, Message = "PLC未连接" };


            try
            {
                Result<T> result = new Result<T>();
                var dataType = ConvertAreasToDataType(area);
                var varType = ConvertPValueSizeToVarType(pValueSize);

                // 位读取特殊处理
                if (pValueSize == PValueSize.BIT)
                {
                    var readResult = _enhancedComm.Read<T>(dataType, db, byteAddr, VarType.Bit, 1, bitAddr);
                    if (readResult.Status && readResult.Datas.Count > 0)
                    {
                        result.Datas.Add(readResult.Datas[0]);
                        result.Status = true;
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = readResult.Message;
                    }
                    return result;
                }

                // 数组读取
                for (int i = 0; i < count; i++)
                {
                    int offset = byteAddr + i * GetTypeSize(typeof(T));
                    var readResult = _enhancedComm.Read<T>(dataType, db, offset, varType, 1);

                    if (readResult.Status && readResult.Datas.Count > 0)
                    {
                        result.Datas.Add(readResult.Datas[0]);
                    }
                    else
                    {
                        result.Status = false;
                        result.Message = $"读取第{i}个数据失败: {readResult.Message}";
                        return result;
                    }
                }

                result.Status = true;
                return result;
            }
            catch (Exception ex)
            {
                return new Result<T> { Status = false, Message = $"读取数据异常: {ex.Message}" };
            }

        }

        /// <summary>
        /// 获取参数项
        /// </summary>
        /// <param name="areas">存储区域</param>
        /// <param name="db">DB块编号</param>
        /// <param name="pTransportSize">传输大小</param>
        /// <param name="byteAddr">字节地址</param>
        /// <param name="count">数据数量</param>
        /// <param name="bitAddr">位地址</param>
        /// <returns>参数项字节列表</returns>
        private List<byte> GetParameterItem(Areas areas, ushort db, PValueSize pTransportSize, int byteAddr, ushort count, byte bitAddr)
        {
            List<byte> items = new List<byte>();
            items.Add(0x12);

            items.Add(0x0a);
            items.Add(0x10);

            items.Add((byte)pTransportSize);

            items.Add((byte)(count / 256));
            items.Add((byte)(count % 256));

            items.Add((byte)(db / 256));
            items.Add((byte)(db % 256));// DB编号

            items.Add((byte)areas);

            // 地址计算
            byteAddr = (byteAddr << 3) + bitAddr;

            items.Add((byte)(byteAddr / 256 / 256 % 256));
            items.Add((byte)(byteAddr / 256 % 256));
            items.Add((byte)(byteAddr % 256));

            return items;
        }

        /// <summary>
        /// 分组地址
        /// </summary>
        /// <param name="parameters">数据参数列表</param>
        /// <param name="index">索引</param>
        /// <param name="doneCount">已处理量</param>
        /// <returns>分组结果</returns>
        private Result<DataParameter> GroupAddress(List<DataParameter> parameters, ref int index, ref int doneCount)
        {
            Result<DataParameter> result = new Result<DataParameter>();

            int byteCount = 0;// 意思：指是当前这个分组中已经有多个字节

            try
            {
                for (int i = index; i < parameters.Count; i++)
                {
                    DataParameter dataParameter = new DataParameter();
                    int size = 0;

                    // 处理不同数据类型的大小计算
                    if (parameters[i].DataType == typeof(bool))
                    {
                        size = 1;
                    }
                    else if (parameters[i].DataType == typeof(string))
                    {
                        // 字符串类型使用Count作为长度
                        size = 1; // 每个字符占1个字节
                    }
                    else
                    {
                        // 其他类型使用Marshal.SizeOf
                        size = Marshal.SizeOf(parameters[i].DataType);
                    }

                    dataParameter.id = parameters[i].id;
                    dataParameter.Area = parameters[i].Area;
                    dataParameter.DBNumber = parameters[i].DBNumber;
                    dataParameter.BitAddress = parameters[i].BitAddress; // BIT    1Byte
                    dataParameter.PValueSize = parameters[i].PValueSize;
                    dataParameter.DataType = parameters[i].DataType;

                    // 第一次进来
                    // 第二次进来   需要知道前一次处理的量
                    dataParameter.ByteAddress = parameters[i].ByteAddress + doneCount * size;
                    dataParameter.Count = (ushort)(parameters[i].Count - doneCount);

                    // 判断长度
                    // 指当前Parameter的请求长度在PDU范围内
                    if (byteCount + (parameters[i].Count - doneCount) * size <= PduSize - 40)
                    {
                        // 当前Parameter剩余长度
                        byteCount += (parameters[i].Count - doneCount) * size;
                        // 新的分组对象添加到返回集合中，以待数据处理
                        result.Datas.Add(dataParameter);
                    }
                    else
                    {
                        // Parameter的请求长度超出了允许范围
                        // 还有空间可以处理
                        if (PduSize - 40 - byteCount > 0)
                        {
                            // 原因：保证返回的字节与数据类型匹配   10个字节   float 4    8
                            ushort len = (ushort)((PduSize - 40 - byteCount) / size * size);
                            byteCount += len;
                            dataParameter.Count = (ushort)(len / size);// 当前临时对象只能请求部分数据

                            doneCount += dataParameter.Count;// 已处理过的数据量
                            result.Datas.Add(dataParameter);
                        }

                        break;
                    }

                    index++;
                    doneCount = 0;
                }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = $"分组地址异常: {ex.Message}";
            }
            return result;
        }

        /// <summary>
        /// 读取PLC数据
        /// </summary>
        public Result Read(List<DataParameter> parameters)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(S7Comm));

            if (_enhancedComm == null || !_enhancedComm.IsConnected)
                return new Result { Status = false, Message = "PLC未连接" };


            try
            {
                Result result = new Result { Status = true };
                List<DataParameter> dataParameters = _enhancedComm.ReadDataParameters(parameters);

                return result;
            }
            catch (Exception ex)
            {
                return new Result { Status = false, Message = $"读取PLC数据异常: {ex.Message}" };
            }

        }

        private Dictionary<string, PValueSize> PValudDic = new Dictionary<string, PValueSize>()
        {
            { "X",PValueSize.BIT},
            { "B",PValueSize.BYTE},
            { "W",PValueSize.WORD},
            { "D",PValueSize.DWORD},
        };
        private Dictionary<string, Areas> AreasDic = new Dictionary<string, Areas>()
        {
            { "I",Areas.Input},
            { "Q",Areas.Output},
            { "M",Areas.Memory},
            { "V",Areas.DataBlock},
        };

        /// <summary>
        /// 地址解析，建议尽量与博途、Step7保持一致
        ///       位(bit)      字节(Byte)       字（Word）     双字（DWord）
        /// I     I0.0         IB0              IW0            ID0
        /// Q     Q0.0         QB0
        /// M     M0.0         MB0
        /// V     V0.0         VB0
        /// DB    DB1.DBX0.0   DB1.DBB110        DBW           DBD
        /// 
        /// 注意：字符串大小写问题，自行处理
        /// </summary>
        /// <param name="variable">变量地址</param>
        /// <returns>解析结果</returns>
        private Result<DataParameter> AnalysisAddress(string variable)
        {
            Result<DataParameter> result = new Result<DataParameter>();
            try
            {
                DataParameter parameter = new DataParameter();

                string str = variable.Substring(0, 2);

                //IsReadOnly  IsEnable
                //stopwatch.Restart();
                //if (str.ToUpper() == "DB") 
                if (str.ToUpperInvariant() == "DB") // ToUpper考虑文化环境
                //if (str == "DB")
                {
                    string[] arrays = variable.Split('.');
                    // [0]  DB1  ----   DB100   DB2000
                    // [1]  DBX100
                    // [2]  0

                    // 处理区域类型
                    parameter.Area = Areas.DataBlock;

                    // 处理DB编号
                    if (ushort.TryParse(arrays[0].Substring(2), out ushort db))
                    {
                        parameter.DBNumber = db;
                    }
                    else
                    {
                        throw new ArgumentException("DB编号无法解析");
                    }
                    // 处理字节地址
                    if (int.TryParse(arrays[1].Substring(3), out int byteAddr))
                    {
                        parameter.ByteAddress = byteAddr;
                    }
                    else
                    {
                        throw new ArgumentException("字节位置无法解析");
                    }

                    // 处理参数类型
                    string typeStr = arrays[1].Substring(2, 1);// X  B   W  D
                    if (!PValudDic.ContainsKey(typeStr)) throw new ArgumentException("数据类型无法解析");
                    parameter.PValueSize = PValudDic[typeStr];

                    // 处理位地址
                    if (arrays.Length == 3)
                    {
                        if (typeStr == "X")
                        {
                            if (Byte.TryParse(arrays[2], out byte bitAddr))
                            {
                                if (bitAddr > 7)
                                    throw new ArgumentException("位地址无效");
                                parameter.BitAddress = bitAddr;
                            }
                            else
                            {
                                throw new ArgumentException("位地址无法解析");
                            }
                        }
                        else
                        {
                            throw new ArgumentException("地址无法解析");
                        }

                    }
                }
                //else if (new string[] { "I", "Q", "M", "V", "T", "C" }.Contains(variable[0].ToString()))
                //else if ("IQMV".Contains(variable[0]))
                else if (variable[0] == 'V')
                {
                    //stopwatch.Stop();
                    //Console.WriteLine("2:" + stopwatch.ElapsedMilliseconds);// 100+    
                    //  以下部分所有可能的异常判断   自行处理


                    string[] arrays = variable.Split('.');
                    // [0]  I0      IB0
                    // [1]  0       --
                    parameter.Area = AreasDic[arrays[0][0].ToString()];// 判断下是否存在
                    if (arrays[0][0] == 'V') parameter.DBNumber = 1;

                    if (arrays.Length == 1)
                    {
                        parameter.PValueSize = PValudDic[arrays[0][1].ToString()];// 需要判断是否存在
                        parameter.ByteAddress = int.Parse(arrays[0].Substring(2));
                    }
                    else if (arrays.Length == 2)// 位处理
                    {
                        parameter.PValueSize = PValueSize.BIT;
                        parameter.ByteAddress = int.Parse(arrays[0].Substring(1));

                        parameter.BitAddress = Byte.Parse(arrays[1]);
                    }
                    else
                    {
                        throw new ArgumentException("地址无法解析");
                    }
                }
                else
                {
                    throw new ArgumentException("地址无法解析");
                }

                result.Datas.Add(parameter);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.Message = $"地址解析异常: {ex.Message}";
            }
            return result;
        }

        /// <summary>
        /// 读取PLC数据
        /// </summary>
        public Result<T> Read<T>(string variable, ushort count)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(S7Comm));


            try
            {
                // string => 实际地址的解析
                var addrResult = AnalysisAddress(variable);//

                if (!addrResult.Status) return new Result<T> { Status = false, Message = addrResult.Message };

                Areas area = addrResult.Datas[0].Area;
                ushort db = addrResult.Datas[0].DBNumber;
                PValueSize pValueSize = addrResult.Datas[0].PValueSize;
                int byteAddr = addrResult.Datas[0].ByteAddress;
                byte bitAddr = addrResult.Datas[0].BitAddress;

                return Read<T>(area, db, pValueSize, byteAddr, count, bitAddr);
            }
            catch (Exception ex)
            {
                return new Result<T> { Status = false, Message = $"读取PLC数据异常: {ex.Message}" };
            }

        }

        #endregion

        #region 写入操作

        /// <summary>
        /// 往PLC里写入数据
        /// </summary>
        public Result Write(List<DataParameter> parameters)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(S7Comm));

            if (_enhancedComm == null || !_enhancedComm.IsConnected)
                return new Result { Status = false, Message = "PLC未连接" };


            try
            {
                Result result = new Result { Status = true };

                // 使用EnhancedS7Comm逐个写入参数
                foreach (var param in parameters)
                {
                    try
                    {
                        var dataType = ConvertAreasToDataType(param.Area);
                        var varType = ConvertPValueSizeToVarType(param.PValueSize);

                        // 位写入
                        if (param.PValueSize == PValueSize.BIT)
                        {
                            bool writeSuccess = true;
                            foreach (var data in param.Datas)
                            {
                                var writeResult = _enhancedComm.Write(dataType, param.DBNumber, param.ByteAddress, (bool)data, param.BitAddress);
                                if (!writeResult.Status)
                                {
                                    param.Status = false;
                                    param.Error = writeResult.Message;
                                    result.Status = false;
                                    writeSuccess = false;
                                    break;
                                }
                            }
                            if (writeSuccess)
                            {
                                param.Status = true;
                            }
                        }
                        // 数组写入
                        else if (param.Datas.Count > 1)
                        {
                            bool writeSuccess = true;
                            for (int i = 0; i < param.Datas.Count; i++)
                            {
                                int offset = param.ByteAddress + i * GetTypeSize(param.DataType);
                                var writeResult = _enhancedComm.Write(dataType, param.DBNumber, offset, param.Datas[i], 0);

                                if (!writeResult.Status)
                                {
                                    param.Status = false;
                                    param.Error = writeResult.Message;
                                    result.Status = false;
                                    writeSuccess = false;
                                    break;
                                }
                            }
                            if (writeSuccess)
                            {
                                param.Status = true;
                            }
                        }
                        // 单个值写入
                        else if (param.Datas.Count == 1)
                        {
                            var writeResult = _enhancedComm.Write(dataType, param.DBNumber, param.ByteAddress, param.Datas[0], 0);
                            if (writeResult.Status)
                            {
                                param.Status = true;
                            }
                            else
                            {
                                param.Status = false;
                                param.Error = writeResult.Message;
                                result.Status = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        param.Status = false;
                        param.Error = $"写入参数异常: {ex.Message}";
                        result.Status = false;
                    }
                }

                // 检查是否有失败的参数
                var failedParams = parameters.Where(p => !p.Status).ToList();
                if (failedParams.Count > 0)
                {
                    result.Status = false;
                    result.Message = $"部分参数写入失败: {string.Join(", ", failedParams.Select(p => p.Error))}";
                }

                return result;
            }
            catch (Exception ex)
            {
                return new Result { Status = false, Message = $"写入PLC数据异常: {ex.Message}" };
            }

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
        /// 创建参数项（多个）
        /// </summary>
        /// <param name="parameters">数据参数列表</param>
        /// <returns>参数项字节列表</returns>
        private List<byte> ParameterItemMulit(List<DataParameter> parameters)
        {
            List<byte> items = new List<byte>();
            items.Add(0x05);//功能码：写入动作
            items.Add((byte)parameters.Count);// Items的个数  Data的Item个数据与Parameter的Item个数匹配

            foreach (var item in parameters)
            {
                items.Add(0x12);

                items.Add(0x0a);
                items.Add(0x10);
                items.Add((byte)item.PValueSize);// 类型 02 Byte   03  Char   04 Word   06  DWord

                ushort vcount = (ushort)item.Datas.Count;
                items.Add(BitConverter.GetBytes(vcount)[1]);
                items.Add(BitConverter.GetBytes(vcount)[0]);// 写一个值

                items.Add(BitConverter.GetBytes(item.DBNumber)[1]);
                items.Add(BitConverter.GetBytes(item.DBNumber)[0]);

                items.Add((byte)item.Area);  //V

                // 地址计算
                int byteAddr = item.ByteAddress;
                byte bitAddr = item.BitAddress;
                byteAddr = (byteAddr << 3) + bitAddr;

                items.Add((byte)(byteAddr / 256 / 256 % 256));
                items.Add((byte)(byteAddr / 256 % 256));
                items.Add((byte)(byteAddr % 256));
            }

            return items;
        }

        /// <summary>
        /// 创建数据项（多个）
        /// </summary>
        /// <param name="parameters">数据参数列表</param>
        /// <returns>数据项字节列表</returns>
        private List<byte> DataItemMulitDecimal(List<DataParameter> parameters)
        {
            // VB10   Byte  
            List<byte> items = new List<byte>();

            foreach (var item in parameters)
            {
                items.Add(0x00);
                items.Add((byte)item.DValueSize);//Byte/Word/DWord

                // Count需要与Size匹配
                int size = 0;

                // 处理不同数据类型的大小计算
                if (item.DataType == typeof(string))
                {
                    // 字符串类型使用Count作为长度
                    size = 1; // 每个字符占1个字节
                }
                else
                {
                    // 其他类型使用Marshal.SizeOf
                    size = Marshal.SizeOf(item.DataType);
                }

                // bit  char     other * 8
                ushort vcount = (ushort)(item.Datas.Count * size * 8);
                if (item.PValueSize == PValueSize.BIT)
                    vcount = (ushort)item.Datas.Count;
                items.Add(BitConverter.GetBytes(vcount)[1]);
                items.Add(BitConverter.GetBytes(vcount)[0]);// 写入的位数

                if (item.PValueSize == PValueSize.BIT)
                {
                    item.Datas.ForEach(d => items.Add((byte)(bool.Parse(d.ToString()) ? 0x01 : 0x00)));
                }
                else
                {
                    item.Datas.ForEach(d =>
                    {
                        dynamic v = d;
                        byte[] vBytes = BitConverter.GetBytes(v);
                        items.AddRange(vBytes);
                    });
                }
                if (item.Count % 2 != 0)
                    items.Add(0x00);// fill byte
            }
            return items;
        }

        /// <summary>
        /// 创建数据项（多个），并将数据转换为10进制格式
        /// </summary>
        /// <param name="parameters">数据参数列表</param>
        /// <returns>数据项字节列表</returns>
        private List<byte> DataItemMulit(List<DataParameter> parameters)
        {
            // VB10   Byte  
            List<byte> items = new List<byte>();

            foreach (var item in parameters)
            {
                items.Add(0x00);
                items.Add((byte)item.DValueSize);//Byte/Word/DWord

                // Count需要与Size匹配
                int size = 0;

                // 处理不同数据类型的大小计算
                if (item.DataType == typeof(string))
                {
                    // 字符串类型使用Count作为长度
                    size = 1; // 每个字符占1个字节
                }
                else
                {
                    // 其他类型使用Marshal.SizeOf
                    size = Marshal.SizeOf(item.DataType);
                }

                // bit  char     other * 8
                ushort vcount = (ushort)(item.Datas.Count * size * 8);
                if (item.PValueSize == PValueSize.BIT)
                    vcount = (ushort)item.Datas.Count;
                items.Add(BitConverter.GetBytes(vcount)[1]);
                items.Add(BitConverter.GetBytes(vcount)[0]);// 写入的位数

                if (item.PValueSize == PValueSize.BIT)
                {
                    item.Datas.ForEach(d => items.Add((byte)(bool.Parse(d.ToString()) ? 0x01 : 0x00)));
                }
                else
                {
                    item.Datas.ForEach(d =>
                    {
                        // 特殊处理字符串类型，按照西门子PLC格式要求
                        if (item.DataType == typeof(string) && item.PValueSize == PValueSize.CHAR)
                        {
                            string stringValue = d.ToString();
                            // 创建PLC格式的字符串数据（包含长度信息）
                            byte[] stringBytes = new byte[item.Count + 2];
                            // 第一个字节：最大长度
                            stringBytes[0] = (byte)(item.Count);
                            // 第二个字节：实际长度
                            int actualLength = Math.Min(stringValue.Length, item.Count);
                            stringBytes[1] = (byte)actualLength;
                            // 后续字节：字符串内容（ASCII编码）
                            byte[] contentBytes = Encoding.ASCII.GetBytes(stringValue);
                            Array.Copy(contentBytes, 0, stringBytes, 2, Math.Min(actualLength, contentBytes.Length));
                            items.AddRange(stringBytes);
                        }
                        else
                        {
                            // 使用标准的二进制格式
                            dynamic v = d;
                            byte[] vBytes = ConvertToBinaryBytes(v);
                            items.AddRange(vBytes);
                        }
                    });
                }
                if (item.Count % 2 != 0)
                    items.Add(0x00);// fill byte
            }
            return items;
        }

        /// <summary>
        /// 创建数据项（多个），使用标准二进制格式
        /// </summary>
        /// <param name="parameters">数据参数列表</param>
        /// <returns>数据项字节列表</returns>
        private List<byte> DataItemMulitBinary(List<DataParameter> parameters)
        {
            // VB10   Byte  
            List<byte> items = new List<byte>();

            foreach (var item in parameters)
            {
                items.Add(0x00);
                items.Add((byte)item.DValueSize);//Byte/Word/DWord

                // Count需要与Size匹配
                int size = 0;

                if (item.DataType == typeof(string) && item.PValueSize == PValueSize.CHAR)
                {
                    // 对于字符串类型，直接使用Datas中的数据（已按西门子格式准备）
                    // 第一个字节：最大长度
                    // 第二个字节：实际长度
                    // 后续字节：字符串内容
                    foreach (var data in item.Datas)
                    {
                        items.Add((byte)data);
                    }

                    // 对于字符串类型，不需要再添加位数信息和填充字节
                    // 因为这些已经在WriteString1方法中处理过了
                    continue;
                }
                else
                {
                    // 其他类型使用Marshal.SizeOf
                    size = Marshal.SizeOf(item.DataType);

                    // bit  char     other * 8
                    ushort vcount = (ushort)(item.Datas.Count * size * 8);
                    if (item.PValueSize == PValueSize.BIT)
                        vcount = (ushort)item.Datas.Count;
                    items.Add(BitConverter.GetBytes(vcount)[1]);
                    items.Add(BitConverter.GetBytes(vcount)[0]);// 写入的位数
                    if (item.PValueSize == PValueSize.BIT)
                    {
                        item.Datas.ForEach(d => items.Add((byte)(bool.Parse(d.ToString()) ? 0x01 : 0x00)));
                    }
                    else
                    {
                        item.Datas.ForEach(d =>
                        {
                            // 使用标准的二进制格式
                            dynamic v = d;
                            byte[] vBytes = ConvertToBinaryBytes(v);
                            items.AddRange(vBytes);
                        });
                    }
                }

                // 只有非字符串类型才需要填充字节
                if (item.DataType != typeof(string) && item.Count % 2 != 0)
                    items.Add(0x00);// fill byte
            }
            return items;
        }

        /// <summary>
        /// 将数据转换为10进制字节格式
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>字节数组</returns>
        private byte[] ConvertToDecimalBytes(dynamic value)
        {
            // 如果是数值类型，转换为10进制字符串再转为字节
            if (value is int || value is long || value is short || value is byte ||
                value is uint || value is ulong || value is ushort)
            {
                string decimalStr = value.ToString();
                return Encoding.ASCII.GetBytes(decimalStr);
            }
            // 如果是浮点数类型
            else if (value is float || value is double)
            {
                // 为了确保浮点数在PLC中正确显示，保留一定的小数位数
                string decimalStr = value.ToString("F2"); // 保留2位小数
                return Encoding.ASCII.GetBytes(decimalStr);
            }
            // 如果是字符串类型
            else if (value is string stringValue)
            {
                return Encoding.ASCII.GetBytes(stringValue);
            }
            // 其他类型保持原有转换方式
            else
            {
                byte[] vBytes = BitConverter.GetBytes(value);
                return vBytes;
            }
        }

        /// <summary>
        /// 将数据转换为二进制字节格式
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <returns>字节数组</returns>
        private byte[] ConvertToBinaryBytes(dynamic value)
        {
            // 对于数值类型，直接使用BitConverter转换为二进制格式
            if (value is int intValue)
            {
                byte[] bytes = BitConverter.GetBytes(intValue);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return bytes;
            }
            else if (value is short shortValue)
            {
                byte[] bytes = BitConverter.GetBytes(shortValue);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return bytes;
            }
            else if (value is long longValue)
            {
                byte[] bytes = BitConverter.GetBytes(longValue);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return bytes;
            }
            else if (value is byte byteValue)
            {
                return new byte[] { byteValue };
            }
            else if (value is uint uintValue)
            {
                byte[] bytes = BitConverter.GetBytes(uintValue);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return bytes;
            }
            else if (value is ushort ushortValue)
            {
                byte[] bytes = BitConverter.GetBytes(ushortValue);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return bytes;
            }
            else if (value is ulong ulongValue)
            {
                byte[] bytes = BitConverter.GetBytes(ulongValue);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return bytes;
            }
            // 浮点数类型保持原有转换方式
            else if (value is float floatValue)
            {
                byte[] bytes = BitConverter.GetBytes(floatValue);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return bytes;
            }
            else if (value is double doubleValue)
            {
                byte[] bytes = BitConverter.GetBytes(doubleValue);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                return bytes;
            }
            // 其他类型保持原有转换方式
            else
            {
                byte[] vBytes = BitConverter.GetBytes(value);
                // 确保使用大端序格式
                if (BitConverter.IsLittleEndian && vBytes.Length > 1)
                    Array.Reverse(vBytes);
                return vBytes;
            }
        }

        #endregion

    }
}
