using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Common
{
    public class DataExport
    {
        public static void SetColumnCaptions(DataTable dataTable)//zhan 界面名称
        {
            // 创建字段名到中文说明的映射字典（只使用 PascalCase 格式）
            var columnCaptionMap = new Dictionary<string, string>()
            {
           
                //product_workorder_info 
                { "product_code", "主码" },
                { "product_out_code", "电机码" },
                { "product_third_code", "产品码" },
                // 工单相关 (work_order表)
                { "Id", "主键" },
                { "ReadFlag", "标志位" },
                { "WorkOrderNumber", "工单号" },
                { "Priority", "优先级" },
                { "ContractNumber", "合同编号" },
                { "PersonInCharge", "负责人" },
                { "ProductionLocation", "生产位置" },
                { "ProcessRoute", "工艺路线" },
                { "ProductBatch", "产品批次" },
                { "ProductCode", "产品编码" },
                { "ProductName", "产品名称" },
                { "ProductMode", "产品型号" },
                { "CustomerAbbreviation", "客户简称" },
                { "NumberOfTasks", "任务数量" },
                { "CreateTime", "创建时间" },
                { "DeliveryDate", "交货日期" },
                { "PlannedStartTime", "计划开始时间" },
                { "PlannedCompletionTime", "计划完成时间" },
                { "OrderStartTime", "订单开始时间" },
                { "MarkingInformationTime", "打标信息时间" },
                { "ProcessRecipeName", "配方名称" },
                { "UnfinishedOutput", "未完成产量" },
                { "CompletedOutput", "已完成产量" },
                { "Status", "状态" },
                { "ProductionTeam", "生产班组" },
                { "Remark", "备注" },
                
                // 产品信息相关 (product_info表)
                { "ProductCategory", "产品类别" },
                { "ProductModel", "产品型号" },
                { "ProductType", "产品类型" },
                { "RelatedMaterial", "相关物料" },
                { "ProductImage", "产品图片" },
                { "ProductDocument", "产品文档" },
                { "ProductStatus", "产品状态" },
                
                // 工单记录相关 (product_workorder_info, product_workorder_record表)
                { "OrderNumber", "订单号" },
                { "ProductOutCode", "产品外部编码" },
                { "ProductThirdCode", "产品第三方编码" },
                { "CurrentStation", "当前工站" },
                { "StartTime", "开始时间" },
                { "EndTime", "结束时间" },
                { "CompleteTime", "完成时间" },
                { "Recipe", "配方名称" },
                { "Station1Duration", "工站1消耗时间(秒)" },
                { "Station2Duration", "工站2消耗时间(秒)" },
                { "Station3Duration", "工站3消耗时间(秒)" },
                { "Station4Duration", "工站4消耗时间(秒)" },
                { "Station5Duration", "工站5消耗时间(秒)" },
                { "Station6Duration", "工站6消耗时间(秒)" },
                { "Station7Duration", "工站7消耗时间(秒)" },
                { "Station8Duration", "工站8消耗时间(秒)" },
                { "Station9Duration", "工站9消耗时间(秒)" },
                { "Station10Duration", "工站10消耗时间(秒)" },
                
                // 易损件管理相关 (wear_part_manage表)
                { "PartType", "类型" },
                { "PartName", "名称" },
                { "ServiceLifeDays", "使用寿命(天)" },
                { "NearExpiryDays", "临期日期" },
                { "ExpiryAlert", "到期警示" },
                { "ExpiryMeasures", "到期措施" },
                
                // 工艺路线相关 (Process表)
                { "Number", "编码" },
                { "Name", "名称" },
                { "State", "状态" },
                { "Type", "类型" },
                { "Version", "版本" },
                { "FileName", "文件名" },
                { "OperationTime", "操作时间" },
                
                // 配方相关 (process_recipe表)
                { "RecipeCode", "配方编码" },
                { "RecipeName", "配方名称" },
                { "SupplierCode", "供应商代码" },
                
                // 工站相关 (work_station表)
                { "Description", "描述" },
                { "Index", "序号" },
                { "InspectionStandard", "检验标准" },
                { "OutputProduct", "输出产品" },
                { "StandardWorkingHours", "标准工时" },
                { "WorkStationName", "工站名称" },
                { "IsReworkAllowed", "是否允许返工" },
                
                // 工序流程相关 (process_flow表)
                { "ProcessName", "工序名称" },
                
                // 常用时间字段
                { "日期", "日期" },
                { "时间", "时间" },
                { "datetime", "日期时间" },
                
                // PLC报警相关 (PLC报警日志表)
                { "设备", "设备" },
                { "报警内容", "报警内容" },
                { "报警类型", "报警类型" },
                { "处理内容", "处理内容" },
                { "当前状态", "当前状态" },
                { "报警等级", "报警等级" },
                // P10DB2000ReturnData表 - 轮次压力和高度数据
            //{ "Cycle1Head1FinalPressure1", "轮次1压头1最终压力1" },  //  轮次1压头1最终压力1
            //{ "Cycle1Head1FinalPressure2", "轮次1压头1最终压力2" },  //  轮次1压头1最终压力2
            //{ "Cycle1Head1PressHeight1", "轮次1压头1压装高度1" },    //  轮次1压头1压装高度1
            //{ "Cycle1Head1PressHeight2", "轮次1压头1压装高度2" },    //  轮次1压头1压装高度2
            //{ "Cycle1Head2FinalPressure", "轮次1压头2最终压力" },    //  轮次1压头2最终压力
            //{ "Cycle1Head2PressHeight", "轮次1压头2压装高度" },      //  轮次1压头2压装高度
            //{ "Cycle1Head3FinalPressure", "轮次1压头3最终压力" },    //  轮次1压头3最终压力
            //{ "Cycle1Head3PressHeight", "轮次1压头3压装高度" },      //  轮次1压头3压装高度
            //{ "Cycle1Head4FinalPressure", "轮次1压头4最终压力" },    //  轮次1压头4最终压力
            //{ "Cycle1Head4PressHeight", "轮次1压头4压装高度" },      //  轮次1压头4压装高度
            //{ "Cycle2Head2FinalPressure", "轮次2压头2最终压力" },    //  轮次2压头2最终压力
            //{ "Cycle2Head2PressHeight", "轮次2压头2压装高度" },      //  轮次2压头2压装高度
            //{ "Cycle2Head3FinalPressure", "轮次2压头3最终压力" },    //  轮次2压头3最终压力
            //{ "Cycle2Head3PressHeight", "轮次2压头3压装高度" },      //  轮次2压头3压装高度
            //{ "Cycle2Head4PressHeight", "轮次2压头4压装高度" },      //  轮次2压头4压装高度
            //{ "Cycle3Head2FinalPressure", "轮次3压头2最终压力" },    //  轮次3压头2最终压力
            //{ "Cycle3Head2PressHeight", "轮次3压头2压装高度" },      //  轮次3压头2压装高度
            //{ "Cycle1Head1PressHeight", "轮次1压头1压装高度" },      //  轮次1压头1压装高度
            //{ "Cycle1Head1FinalPressure", "轮次1压头1最终压力" },    //  轮次1压头1最终压力
            //{ "CreatedTime", "记录时间" },                            //  记录时间
            //{ "Cycle3Head3PressHeight", "轮次3压头3压装高度" },      //  轮次3压头3压装高度
            //{ "Cycle3Head3FinalPressure", "轮次3压头3最终压力" },    //  轮次3压头3最终压力
            //{ "Product_BarCode", "主码" },                            //  主码



            { "Cycle1Head1FinalPressure1", "大齿轮压装力" },          //  轮次1压头1最终压力1
            { "Cycle1Head1FinalPressure2", "小齿轮压装力" },          //  轮次1压头1最终压力2
            { "Cycle1Head1PressHeight1", "大齿轮压装高度" },          //  轮次1压头1压装高度1
            { "Cycle1Head1PressHeight2", "小齿轮压装高度" },          //  轮次1压头1压装高度2
            { "Cycle1Head2FinalPressure", "箱体41铜套压装力" },      //  轮次1压头2最终压力
            { "Cycle1Head2PressHeight", "箱体41铜套压装高度" },      //  轮次1压头2压装高度
            { "Cycle1Head3FinalPressure", "箱体40铜套压装力" },      //  轮次1压头3最终压力
            { "Cycle1Head3PressHeight", "箱体40铜套压装高度" },      //  轮次1压头3压装高度
            { "Cycle1Head4FinalPressure", "箱盖右销钉压装力" },      //  轮次1压头4最终压力
            { "Cycle1Head4PressHeight", "箱盖右销钉装高度" },        //  轮次1压头4压装高度
            { "Cycle2Head2FinalPressure", "轮次2压头2最终压力" },        //  轮次2压头2最终压力
            { "Cycle2Head2PressHeight", "轮次2压头2压装高度" },        //  轮次2压头2压装高度
            { "Cycle2Head3FinalPressure", "箱盖40铜套压装力" },      //  轮次2压头3最终压力
            { "Cycle2Head3PressHeight", "箱盖40铜套压装高度" },      //  轮次2压头3压装高度
            { "Cycle2Head4PressHeight", "箱盖左销钉装高度" },        //  轮次2压头4压装高度
            { "Cycle3Head2FinalPressure", "轮次3压头2最终压力" },    //  轮次3压头2最终压力
            { "Cycle3Head2PressHeight", "轮次3压头2压装高度" },      //  轮次3压头2压装高度
            { "Cycle1Head1PressHeight", "下轴承压装高度" },          //  轮次1压头1压装高度
            { "Cycle1Head1FinalPressure", "下轴承压装力" },          //  轮次1压头1最终压力
            { "CreatedTime", "记录时间" },                            //  记录时间
            { "Cycle3Head3PressHeight", "上轴承压装高度" },          //  轮次3压头3压装高度
            { "Cycle3Head3FinalPressure", "上轴承压装力" },          //  轮次3压头3最终压力

            { "Cycle2Head4FinalPressure", "箱盖左销钉压装力" },          //  轮次3压头3最终压力
            { "Product_BarCode", "主码" },                            //  主码
                
                // P30DB3000ReturnData, P40DB3001ReturnData, P50DB3002ReturnData表 - 扭矩和角度数据
                { "CompletedAngle1", "完成角度1" },
                { "CompletedAngle2", "完成角度2" },
                { "CompletedAngle3", "完成角度3" },
                { "CompletedAngle4", "完成角度4" },
                { "CompletedAngle5", "完成角度5" },
                { "CompletedAngle6", "完成角度6" },
                { "CompletedAngle7", "完成角度7" },
                { "CompletedAngle8", "完成角度8" },
                { "CompletedAngle9", "完成角度9" },
                { "CompletedTorque1", "完成扭矩1" },
                { "CompletedTorque2", "完成扭矩2" },
                { "CompletedTorque3", "完成扭矩3" },
                { "CompletedTorque4", "完成扭矩4" },
                { "CompletedTorque5", "完成扭矩5" },
                { "CompletedTorque6", "完成扭矩6" },
                { "CompletedTorque7", "完成扭矩7" },
                { "CompletedTorque8", "完成扭矩8" },
                { "CompletedTorque9", "完成扭矩9" },
                { "GearHeight1", "齿轮高度1" },
                { "GearHeight2", "齿轮高度2" },
                { "MotorCode", "电机码" },


                //{ "CompletedAngle1", "丝杆螺母拧紧角度" },        //  完成角度1
                //{ "CompletedAngle2", "限位块拧紧角度" },          //  完成角度2
                //{ "CompletedAngle3", "电机螺钉3拧紧角度" },        //  完成角度3
                //{ "CompletedAngle4", "箱体螺钉1拧紧角度" },        //  完成角度4
                //{ "CompletedAngle5", "箱体螺钉2拧紧角度" },        //  完成角度5
                //{ "CompletedAngle6", "箱体螺钉3拧紧角度" },        //  完成角度6
                //{ "CompletedAngle7", "箱体螺钉4拧紧角度" },        //  完成角度7
                //{ "CompletedAngle8", "箱体螺钉5拧紧角度" },        //  完成角度8
                //{ "CompletedAngle9", "箱体螺钉6拧紧角度" },        //  完成角度9
                //{ "CompletedTorque1", "丝杆螺母拧紧扭矩" },        //  完成扭矩1
                //{ "CompletedTorque2", "限位块拧紧扭矩" },          //  完成扭矩2
                //{ "CompletedTorque3", "电机螺钉3拧紧扭矩" },        //  完成扭矩3
                //{ "CompletedTorque4", "箱体螺钉1拧紧扭矩" },        //  完成扭矩4
                //{ "CompletedTorque5", "箱体螺钉2拧紧扭矩" },        //  完成扭矩5
                //{ "CompletedTorque6", "箱体螺钉3拧紧扭矩" },        //  完成扭矩6
                //{ "CompletedTorque7", "箱体螺钉4拧紧扭矩" },        //  完成扭矩7
                //{ "CompletedTorque8", "箱体螺钉5拧紧扭矩" },        //  完成扭矩8
                //{ "CompletedTorque9", "箱体螺钉6拧紧扭矩" },        //  完成扭矩9
               // { "GearHeight1", "大齿轮垫片检测高度" },            //  齿轮高度1
               // { "GearHeight2", "小齿轮垫片检测高度" },            //  齿轮高度2

                
                // P60DB3000ReturnData表
                { "LeakRate", "压降" },
                { "TimeDate", "时间日期" },
                
                // P70DB3003ReturnData表 - 老化和标定数据
                { "AmplitudeValue1", "AMPLITUDE值1" },
                { "CalibrationResult1", "标定结果1" },
                { "LeftAgingAvgCurrent4", "左老化平均电流4" },
                { "LeftAgingAvgVoltage4", "左老化平均电压4" },
                { "LeftAgingMaxCurrent4", "左老化最大电流4" },
                { "LeftAgingMaxVoltage4", "左老化最大电压4" },
                { "LeftAgingMinCurrent4", "左老化最小电流4" },
                { "LeftAgingMinVoltage4", "左老化最小电压4" },
                { "MagLowValue1", "MAG_LOW值1" },
                { "MaxLength1", "最大长度1" },
                { "MiddleAgingAvgCurrent3", "中老化平均电流3" },
                { "MiddleAgingAvgVoltage3", "中老化平均电压3" },
                { "MiddleAgingMaxCurrent3", "中老化最大电流3" },
                { "MiddleAgingMaxVoltage3", "中老化最大电压3" },
                { "MiddleAgingMinCurrent3", "中老化最小电流3" },
                { "MiddleAgingMinVoltage3", "中老化最小电压3" },
                { "ReadFlag1", "读取标志位1" },
                { "ReadFlag2", "读取标志位2" },
                { "ReadFlag3", "读取标志位3" },
                { "ReadFlag4", "读取标志位4" },
                { "RightAgingAvgCurrent2", "右老化平均电流2" },
                { "RightAgingAvgVoltage2", "右老化平均电压2" },
                { "RightAgingMaxCurrent2", "右老化最大电流2" },
                { "RightAgingMaxVoltage2", "右老化最大电压2" },
                { "RightAgingMinCurrent2", "右老化最小电流2" },
                { "RightAgingMinVoltage2", "右老化最小电压2" },
                
                // P80DB3004ReturnData表 - 声压数据
                { "SoundPressure1000Hz", "1000HZ声压值" },
                { "SoundPressure125Hz", "125HZ声压值" },
                { "SoundPressure16000Hz", "16000HZ声压值" },
                { "SoundPressure2000Hz", "2000HZ声压值" },
                { "SoundPressure250Hz", "250HZ声压值" },
                { "SoundPressure31Hz", "31HZ声压值" },
                { "SoundPressure4000Hz", "4000HZ声压值" },
                { "SoundPressure500Hz", "500HZ声压值" },
                { "SoundPressure63Hz", "63HZ声压值" },
                { "SoundPressure8000Hz", "8000HZ声压值" },
                { "SoundPressureApa", "APA声压值" },
                { "SoundPressureApc", "APC声压值" },
                { "SoundPressureAplin", "APLIN声压值" },
                { "ProductBarCode", "主码" },

//                SELECT id, force_current_500n, force_current_600n, max_current, max_voltage, max_torque, max_pressure, reserve1, reserve2, reserve3, reserve4, create_time, Product_BarCode
//FROM project2.dbo.signal_data2;
                // P70DB3004ReturnData表 - 推拉力数据
                { "force_current_500n", "600N力电流" },
                { "force_current_600n", "1250N力电流" },
                { "max_current", "最大电流" },
                { "max_voltage", "最大电压" },
                { "max_torque", "最大扭力" },
                { "max_pressure", "最大压力" },
                { "reserve1", "预留值1" },
                { "reserve2", "预留值2" },
                { "reserve3", "预留值3" },
                { "reserve4", "预留值4" },


                                // P70DB3004ReturnData表 - 标定
      
                { "a_slot", "A_SLOT" },
                { "b_slot", "B_SLOT" },
                { "c_slot", "C_SLOT" },
                { "amplitude_a", "AMPLITUDE值A" },
                { "amplitude_b", "AMPLITUDE值B" },
                { "amplitude_c", "AMPLITUDE值C" },
                { "mag_low_a", "MAG_LOW值A" },
                { "mag_low_b", "MAG_LOW值B" },
                { "mag_low_c", "MAG_LOW值C" },
                { "a_last_point", "A倒数第一个点" },
                { "a_second_last_point", "A倒数第二个点" },
                { "b_last_point", "B倒数第一个点" },
                { "b_second_last_point", "B倒数第二个点" },
                { "c_last_point", "C倒数第一个点" },
                { "c_second_last_point", "C倒数第二个点" },
                { "a_first_point", "A第一个点" },
                { "a_second_point", "A第二个点" },
                { "b_first_point", "B第一个点" },
                { "b_second_point", "B第二个点" },
                { "c_first_point", "C第一个点" },
                { "c_second_point", "C第二个点" },
                { "induction_distance", "最大长度" },
                { "actual_distance", "感应距离" },
                { "outermost_distance", "最外距离" },
                { "innermost_distance", "最内距离" },



                            //                 = (int)p70Data.ASLOT,
                            // = cleanedCalibrationBarcode07,
                            //BSlot = (int)p70Data.BSLOT,
                            //CSlot = (int)p70Data.CSLOT,
                            //AmplitudeA = (int)p70Data.AmplitudeValueA,
                            //AmplitudeB = (int)p70Data.AmplitudeValueB,
                            //AmplitudeC = (int)p70Data.AmplitudeValueC,
                            //MagLowA = (int)p70Data.MagLowValueA,
                            //MagLowB = (decimal)p70Data.MagLowValueB,
                            //MagLowC = (decimal)p70Data.MagLowValueC,
                            //ALastPoint = (decimal)p70Data.APenultimatePoint,
                            //ASecondLastPoint = (decimal)p70Data.AThirdLastPoint,
                            //BLastPoint = (decimal)p70Data.BPenultimatePoint,
                            //BSecondLastPoint = (decimal)p70Data.BThirdLastPoint,
                            //CLastPoint = (decimal)p70Data.CPenultimatePoint,
                            //CSecondLastPoint = (decimal)p70Data.CThirdLastPoint,
                            //AFirstPoint = (decimal)p70Data.AFirstPoint,
                            //ASecondPoint = (decimal)p70Data.ASecondPoint,
                            //BFirstPoint = (decimal)p70Data.BFirstPoint,
                            //BSecondPoint = (decimal)p70Data.BSecondPoint,
                            //CFirstPoint = (decimal)p70Data.CFirstPoint,
                            //CSecondPoint = (decimal)p70Data.CSecondPoint,
                            //InductionDistance = (decimal)p70Data.SensingDistance,
                            //ActualDistance = (decimal)p70Data.ActualDistance,




            };
            // 创建字段名到中文说明的映射字典（只使用 PascalCase 格式）

            // 创建字段名到中文说明的映射字典（只使用 PascalCase 格式）
            var columnCaptionMap1 = new Dictionary<string, string>()
            {

                { "Id", "主键" },
                { "ReadFlag", "标志位" },

            { "CreatedTime", "记录时间" },                            //  记录时间 
            { "Product_BarCode", "主码" },                            //  主码
                
                // P30DB3000ReturnData, P40DB3001ReturnData, P50DB3002ReturnData表 - 扭矩和角度数据
                { "CompletedAngle1", "30第一颗角度" },
                { "CompletedAngle2", "30第二颗角度" },
                { "CompletedAngle3", "35第一颗角度" },
                { "CompletedAngle4", "35第二颗角度" },
                { "CompletedAngle5", "35第三颗角度" },
                { "CompletedAngle6", "预留角度6" },
                { "CompletedAngle7", "预留角度7" },
                { "CompletedAngle8", "预留角度8" },
                { "CompletedAngle9", "预留角度9" },
                { "CompletedTorque1", "30第一颗扭矩" },
                { "CompletedTorque2", "30第二颗扭矩" },
                { "CompletedTorque3", "35第一颗扭矩" },
                { "CompletedTorque4", "35第二颗扭矩" },
                { "CompletedTorque5", "35第三颗扭矩" },
                { "CompletedTorque6", "预留扭矩6" },
                { "CompletedTorque7", "预留扭矩7" },
                { "CompletedTorque8", "预留扭矩8" },
                { "CompletedTorque9", "预留扭矩9" },
            };
            // 创建字段名到中文说明的映射字典（只使用 PascalCase 格式）
            var columnCaptionMap2 = new Dictionary<string, string>()
            {

                { "Id", "主键" },
                { "ReadFlag", "标志位" },

            { "CreatedTime", "记录时间" },                            //  记录时间 
            { "Product_BarCode", "主码" },                            //  主码
                
                // P30DB3000ReturnData, P40DB3001ReturnData, P50DB3002ReturnData表 - 扭矩和角度数据
                { "CompletedAngle1", "40第一颗角度" },
                { "CompletedAngle2", "40第二颗角度" },
                { "CompletedAngle3", "40第三颗角度" },
                { "CompletedAngle4", "40第四颗角度" },
                { "CompletedAngle5", "40第五颗角度" },
                { "CompletedAngle6", "40第六颗角度" },
                { "CompletedAngle7", "40第七颗角度" },
                { "CompletedAngle8", "40第八颗角度" },
                { "CompletedAngle9", "40第九颗角度" },
                { "CompletedTorque1", "40第一颗扭矩" },
                { "CompletedTorque2", "40第二颗扭矩" },
                { "CompletedTorque3", "40第三颗扭矩" },
                { "CompletedTorque4", "40第四颗扭矩" },
                { "CompletedTorque5", "40第五颗扭矩" },
                { "CompletedTorque6", "40第六颗扭矩" },
                { "CompletedTorque7", "40第七颗扭矩" },
                { "CompletedTorque8", "40第八颗扭矩" },
                { "CompletedTorque9", "40第九颗扭矩" },
                { "GearHeight1", "大齿轮高度" },
                { "GearHeight2", "小齿轮高度" },
            };
            // 创建字段名到中文说明的映射字典（只使用 PascalCase 格式）
            var columnCaptionMap3 = new Dictionary<string, string>()
            {

                { "Id", "主键" },
                { "ReadFlag", "标志位" },

            { "CreatedTime", "记录时间" },                            //  记录时间 
            { "Product_BarCode", "主码" },                            //  主码
                
                   { "force_current_500n", "600N力电流" },
                { "force_current_600n", "1250N力电流" },
                { "max_current", "600N平均电流" },
                { "max_voltage", "600N最大电压" },
                { "max_torque", "600N最大扭力" },
                { "max_pressure", "600N最大压力" },
                { "reserve1", "1250N平均电流" },
                { "reserve2", "1250N最大电压" },
                { "reserve3", "1250N最大扭力" },
                { "reserve4", "1250N最大压力" },
            };
            if (dataTable.TableName == "P30DB3000ReturnData")
            {
                // 为每个列设置Caption
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (columnCaptionMap1.TryGetValue(column.ColumnName, out string caption))
                    {
                        column.ColumnName = caption;
                    }
                    // 如果没有找到映射，保持原列名
                }
            }
            else if (dataTable.TableName == "P40DB3001ReturnData")
            {
                // 为每个列设置Caption
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (columnCaptionMap2.TryGetValue(column.ColumnName, out string caption))
                    {
                        column.ColumnName = caption;
                    }
                    // 如果没有找到映射，保持原列名
                }
            }
            else if (dataTable.TableName == "signal_data2")
            {
                // 为每个列设置Caption
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (columnCaptionMap3.TryGetValue(column.ColumnName, out string caption))
                    {
                        column.ColumnName = caption;
                    }
                    // 如果没有找到映射，保持原列名
                }
            }
            else
            {
                // 为每个列设置Caption
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (columnCaptionMap.TryGetValue(column.ColumnName, out string caption))
                    {
                        column.ColumnName = caption;
                    }
                    // 如果没有找到映射，保持原列名
                }
            }

        }
    }
}
