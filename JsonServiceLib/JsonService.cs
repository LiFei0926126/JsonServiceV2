using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Data;
using Newtonsoft.Json;
using System.Net;
using Readearth.Data;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Xml;
using DotSpatial.Topology;
using System.Configuration;
using Projection;
using System.Data.Odbc;
using DotSpatial.Topology;
using DotSpatial.Data;

namespace JsonServiceLib
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“JsonService”。
    public class JsonService : IJsonService
    {
        #region IRadarService 成员
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strStart"></param>
        /// <param name="strEnd"></param>
        /// <returns></returns>
        ///<exception cref="ArgumentNullException">时间参数为空。</exception>
        ///<exception cref="ArgumentException">时间参数顺序错误。</exception>
        public Stream GetRadarStr(string strStart, string strEnd)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                //strStart = "20160620040000";
                //strEnd = "20160620060000";

                Database m_Database = new Database();
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                #region 序列化时间
                try
                {
                    dtStart = DateTime.ParseExact(strStart, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "dtEnd", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;

                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                #endregion

                DataTable dtData = new DataTable();
                //string strSQL1 = string.Format("SELECT * FROM T_APPRadarImage WHERE Layers='01' AND (FORECASTDATE BETWEEN '{0}' AND '{1}')", dtStart, dtEnd);
                //string strSQL2 = string.Format("SELECT * FROM T_APPRadarImage WHERE Layers='02' AND (FORECASTDATE BETWEEN '{0}' AND '{1}')", dtStart, dtEnd);
                string strSQL3 = string.Format("SELECT * FROM T_APPRainImage WHERE FORECASTDATE BETWEEN '{0}' AND '{1}'", dtStart, dtEnd);
                try
                {
                    dtData = m_Database.GetDataTable(strSQL3);

                    //dtData = m_Database.GetDataTable(strSQL1);
                    //if (dtData.Rows.Count == 0)
                    //    dtData = m_Database.GetDataTable(strSQL2);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                List<RadarImage> pRadarImages = new List<RadarImage>();

                string strUriLocal = "http://61.152.126.152/SLPC/Product/APPRadarImage/";
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];

                    string strTime = dr["Name"].ToString().Split('.')[0];
                                          
                    RadarImage pRadarImage = new RadarImage(
                       strUriLocal + dr["Folder"].ToString().Replace("Z:/APPRadarImage/", "") + '/' + dr["Name"].ToString()
                       ,DateTime.ParseExact(strTime, "yyyyMMddHHmm", null)
                       , 119.557
                       , 124.197
                       , 33.3
                       , 28.66);
                    ///20170626,为了应对客户提出的偏移问题，做了人工的偏移修正。
                     //,119.562
                     //,124.192
                     //,33.322
                     //,28.682);
                    pRadarImages.Add(pRadarImage);
                }
                string strData = JsonConvert.SerializeObject(pRadarImages);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
            //return sss1;
        }

        public Stream VerifyUser(string userName, string passWord)
        {
            WCFResult wcf = new WCFResult(false) ;
            try
            {
                if (userName.ToLower() != userName)
                {
                    throw new Exception("请输入正确的用户名。");
                }

                if (string.IsNullOrEmpty(userName))
                {
                    wcf = new WCFResult(false, "用户名不能为空。");
                    //return new MemoryStream(Encoding.UTF8.GetBytes("用户名不能为空。"));
                }
                if (string.IsNullOrEmpty(passWord))
                    wcf = new WCFResult(false, "密码不能为空。");
                // return new MemoryStream(Encoding.UTF8.GetBytes("密码不能为空。"));

                Database db = new Database();
                string strSQL = string.Format("SELECT * FROM T_User where userName ='{0}'", userName);
                DataTable dtUser = db.GetDataTable(strSQL);
                if (dtUser.Rows.Count == 0)
                    wcf = new WCFResult(false, "用户名错误。");
                else if (dtUser.Rows[0]["SN"].ToString().Trim() == passWord)
                {
                    wcf = new WCFResult(true, "登陆成功。");
                    Dictionary<string, string> user_Alias = new Dictionary<string, string>();
                    user_Alias.Add("userName", userName);
                    user_Alias.Add("AliasName", dtUser.Rows[0]["Alias"].ToString());
                    user_Alias.Add("Tag", dtUser.Rows[0]["Tag"].ToString().Replace('_',','));
                    if (File.Exists(@"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\APPUser\" + userName + ".png"))
                        user_Alias.Add("Image", "http://61.152.126.152/SLPC/Product/APPUser/" + userName + ".png");
                    else if(dtUser.Rows[0]["EMail"].ToString()=="yp")
                        user_Alias.Add("Image", "http://61.152.126.152/SLPC/Product/APPUser/yangpu.png");
                    else if (dtUser.Rows[0]["EMail"].ToString() == "sj")
                        user_Alias.Add("Image", "http://61.152.126.152/SLPC/Product/APPUser/songjiang.png");
                    string strData = JsonConvert.SerializeObject(user_Alias);
                    wcf.Data = strData;
                }
                else
                    wcf = new WCFResult(false, "密码错误。");
                //Alias
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetRiskAlarmUsernames()
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database db = new Database();
                string strSQL = string.Format("SELECT  UserName,Alias FROM [APP_Risk_Data].[dbo].[T_User]");
                DataTable dtUserObjectName = db.GetDataTable(strSQL);
                if (dtUserObjectName.Rows.Count == 0)
                    wcf = new WCFResult(false, "错误的用户名。");
                else
                {


                    string strData = JsonConvert.SerializeObject(dtUserObjectName);
                    wcf.Result = true;
                    wcf.Data = strData;
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetRiskAlarmByUsername(string userName)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Dictionary<string, string> dicLevel = new Dictionary<string, string>();
                dicLevel.Add("1", "红色");
                dicLevel.Add("2", "橙色");
                dicLevel.Add("3", "黄色");
                dicLevel.Add("4", "蓝色");

                Database db = new Database();
                string planPath = @"F:\configFile\Plan\应急预案";

                string strSQL = string.Format("SELECT Alias  FROM [APP_Risk_Data].[dbo].[T_User] where userName='{0}'", userName);
                DataTable dtUserObjectName = db.GetDataTable(strSQL);
                if (dtUserObjectName.Rows.Count == 0)                
                    wcf = new WCFResult(false, "错误的用户名。");                
                else
                {
                    string strObjectName = dtUserObjectName.Rows[0][0].ToString();
                    Alarm pAlarm = new Alarm();
                    pAlarm.Name = strObjectName;
                    pAlarm.LEVEL = "10";

                    //if (userName == "ypq")
                    //{
                    //    string ss = "SELECT T1.[ForecastTime],T1.[ObjectName],T1.[State],T1.[StateDM],T1.[Level],T1.[Details]  FROM [CityDisasterForecast1].[dbo].[V_PSQKAlarm] as T1,(SELECT max([ForecastTime]) as [ForecastTime],[ObjectName]  FROM [CityDisasterForecast1].[dbo].[V_PSQKAlarm]  group by objectName)  as T2  WHERE T1.[ForecastTime]=T2.[ForecastTime] AND T1.[ObjectName] =T2.[ObjectName] ORDER BY T2.[ForecastTime]";
                    //    DataTable dtSubAlarms = db.GetDataTable(ss);
                    //    List<Alarm> pSubAlarms = new List<Alarm>();
                    //    for (int i = 0; i < dtSubAlarms.Rows.Count; i++)
                    //    {
                    //        DataRow drSubAlarm = dtSubAlarms.Rows[i];

                    //        Alarm pSubAlarm = new Alarm();
                    //        pSubAlarm.Name = drSubAlarm["ObjectName"].ToString();
                    //        pSubAlarm.FORECASTDATE = DateTime.Parse(drSubAlarm["ForecastTime"].ToString());
                    //        pSubAlarm.OPERATION = drSubAlarm["State"].ToString();
                    //        pSubAlarm.LEVEL = drSubAlarm["Level"].ToString();
                    //        if (int.Parse(pAlarm.LEVEL) > int.Parse(pSubAlarm.LEVEL) && pSubAlarm.OPERATION != "解除")
                    //        {
                    //            pAlarm.FORECASTDATE = pSubAlarm.FORECASTDATE;
                    //            pAlarm.LEVEL = pSubAlarm.LEVEL;
                    //            pAlarm.OPERATION = pSubAlarm.OPERATION;
                    //            pAlarm.CONTENT = pSubAlarm.CONTENT;
                    //        }
                    //        pSubAlarm.LEVEL = dicLevel[drSubAlarm["Level"].ToString()];
                    //        pSubAlarm.CONTENT = drSubAlarm["Details"].ToString();


                    //        pSubAlarms.Add(pSubAlarm);
                    //    }
                    //    if (pAlarm.LEVEL == "10")
                    //    {
                    //        pAlarm.FORECASTDATE = DateTime.Now;
                    //        pAlarm.LEVEL = "4";
                    //        pAlarm.OPERATION ="解除";
                    //        pAlarm.CONTENT = "";
                    //    }

                    //    pAlarm.SubAlarms = pSubAlarms.ToArray();
                    //}
                    //else
                    //{
                        strSQL = string.Format("SELECT top 1  ForecastTime, ObjectName , Type, State, [Level], Details  FROM [CityDisasterForecast].[dbo].[V_Alarm]  where ObjectName='{0}' order by ForecastTime desc", strObjectName);
                        DataTable dtAlarm = db.GetDataTable(strSQL);

                        pAlarm.FORECASTDATE = DateTime.Parse(dtAlarm.Rows[0]["ForecastTime"].ToString());
                        pAlarm.LEVEL = dtAlarm.Rows[0]["Level"].ToString();
                        pAlarm.OPERATION = dtAlarm.Rows[0]["State"].ToString();
                        pAlarm.CONTENT = dtAlarm.Rows[0]["Details"].ToString();
                        pAlarm.TYPE = dtAlarm.Rows[0]["Type"].ToString();

                        if (pAlarm.OPERATION != "解除")
                        {
                            if (userName == "hlg")
                                planPath += @"\雷电用户\欢乐谷";
                            else if (userName == "xjwc")
                                planPath += @"\社区\新江湾社区";
                            else if (userName == "wjc")
                                planPath += @"\社区\五角场";
                            else
                                planPath += @"\社区\松江区";

                            if (userName.ToLower() != "egd" && userName.ToLower() != "hgq")
                            {
                                if (pAlarm.LEVEL == "1")
                                    planPath += @"\一级响应行动.txt";
                                if (pAlarm.LEVEL == "2")
                                    planPath += @"\二级响应行动.txt";
                                if (pAlarm.LEVEL == "3")
                                    planPath += @"\三级响应行动.txt";
                                if (pAlarm.LEVEL == "4")
                                    planPath += @"\四级响应行动.txt";

                                StreamReader sr = new StreamReader(planPath, Encoding.Default);
                                string strLine = sr.ReadToEnd();
                                sr.Close();

                                pAlarm.GUIDE = strLine;
                            }
                            else
                                pAlarm.GUIDE = "";
                        }
                        else
                            pAlarm.GUIDE = "";
                    //}
                    
                    pAlarm.LEVEL = dicLevel[pAlarm.LEVEL];
                    
                    string strData = JsonConvert.SerializeObject(pAlarm);
                    wcf.Result = true;
                    wcf.Data = strData;
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetRiskAlarmByUsername_V2(string userName)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Dictionary<string, string> dicLevel = new Dictionary<string, string>();
                dicLevel.Add("1", "红色");
                dicLevel.Add("2", "橙色");
                dicLevel.Add("3", "黄色");
                dicLevel.Add("4", "蓝色");

                Database db = new Database();
                string planPath = @"F:\configFile\Plan\应急预案";

                string strSQL = string.Format("SELECT Alias  FROM [APP_Risk_Data].[dbo].[T_User] where userName='{0}'", userName);
                DataTable dtUserObjectName = db.GetDataTable(strSQL);
                if (dtUserObjectName.Rows.Count == 0)
                    wcf = new WCFResult(false, "错误的用户名。");
                else
                {
                    string strObjectName = dtUserObjectName.Rows[0][0].ToString();
                    Alarm1 pAlarm = new Alarm1();
                    pAlarm.Name = strObjectName;
                    pAlarm.LEVEL = "10";
                    if (userName == "ypq")
                    {
                        string ss = "SELECT T1.[ForecastTime],T1.[ObjectName],T1.[State],T1.[StateDM],T1.[Level],T1.[Details]  FROM [CityDisasterForecast1].[dbo].[V_PSQKAlarm] as T1,(SELECT max([ForecastTime]) as [ForecastTime],[ObjectName]  FROM [CityDisasterForecast1].[dbo].[V_PSQKAlarm]  group by objectName)  as T2  WHERE T1.[ForecastTime]=T2.[ForecastTime] AND T1.[ObjectName] =T2.[ObjectName] ORDER BY T2.[ForecastTime]";
                        DataTable dtSubAlarms = db.GetDataTable(ss);
                        List<Alarm> pSubAlarms = new List<Alarm>();
                        DateTime dtAlarm = DateTime.Now.AddYears(-1) ;
                        string strGuid = "";
                        int maxLevel = 10;
                        for (int i = 0; i < dtSubAlarms.Rows.Count; i++)
                        {
                            DataRow drSubAlarm = dtSubAlarms.Rows[i];

                            Alarm1 pSubAlarm = new Alarm1();
                            pSubAlarm.Name = drSubAlarm["ObjectName"].ToString();
                            pSubAlarm.FORECASTDATE = DateTime.Parse(drSubAlarm["ForecastTime"].ToString());
                            pSubAlarm.OPERATION = drSubAlarm["State"].ToString();
                            pSubAlarm.LEVEL = drSubAlarm["Level"].ToString();
                            pSubAlarm.TYPE = "暴雨内涝风险预警";
                            if (int.Parse(pAlarm.LEVEL) > int.Parse(pSubAlarm.LEVEL) && pSubAlarm.OPERATION != "解除")
                            {
                                pAlarm.FORECASTDATE = pSubAlarm.FORECASTDATE;
                                pAlarm.LEVEL = pSubAlarm.LEVEL;
                                pAlarm.OPERATION = pSubAlarm.OPERATION;
                                pAlarm.CONTENT = pSubAlarm.CONTENT;
                                pAlarm.TYPE = pSubAlarm.TYPE;

                            }
                            if (maxLevel > int.Parse(pSubAlarm.LEVEL))
                                maxLevel = int.Parse(pSubAlarm.LEVEL);

                            if (pSubAlarm.FORECASTDATE > dtAlarm)
                            {
                                dtAlarm = pSubAlarm.FORECASTDATE;
                            }
                            pSubAlarm.LEVEL = dicLevel[drSubAlarm["Level"].ToString()];
                            pSubAlarm.CONTENT = drSubAlarm["Details"].ToString();

                            strGuid += string.Format("{0}:{1}", pSubAlarm.Name, pSubAlarm.LEVEL) + Environment.NewLine;
                            pSubAlarms.Add(pSubAlarm);
                        }
                        if (pAlarm.LEVEL == "10")
                        {
                            pAlarm.FORECASTDATE = dtAlarm;
                            pAlarm.TYPE = "暴雨内涝风险预警";
                            pAlarm.LEVEL = maxLevel.ToString();
                            pAlarm.OPERATION ="解除";
                            pAlarm.CONTENT = "";
                        }
                        for (int i = 0; i < pSubAlarms.Count; i++)
                        {
                            
                        }

                        pAlarm.SubAlarms = pSubAlarms.ToArray();
                        pAlarm.GUIDE = strGuid;
                    }
                    else
                    {
                    strSQL = string.Format("SELECT top 1  ForecastTime, ObjectName , Type, State, [Level], Details  FROM [CityDisasterForecast].[dbo].[V_Alarm]  where ObjectName='{0}' order by ForecastTime desc", strObjectName);
                    DataTable dtAlarm = db.GetDataTable(strSQL);

                    pAlarm.FORECASTDATE = DateTime.Parse(dtAlarm.Rows[0]["ForecastTime"].ToString());
                    pAlarm.LEVEL = dtAlarm.Rows[0]["Level"].ToString();
                    pAlarm.OPERATION = dtAlarm.Rows[0]["State"].ToString();
                    pAlarm.CONTENT = dtAlarm.Rows[0]["Details"].ToString();
                    pAlarm.TYPE = dtAlarm.Rows[0]["Type"].ToString();

                    if (pAlarm.OPERATION != "解除")
                    {
                        if (userName == "hlg")
                            planPath += @"\雷电用户\欢乐谷";
                        else if (userName == "xjwc")
                            planPath += @"\社区\新江湾社区";
                        else if (userName == "wjc")
                            planPath += @"\社区\五角场";
                        else
                            planPath += @"\社区\松江区";

                        if (userName.ToLower() != "egd" && userName.ToLower() != "hgq")
                        {
                            if (pAlarm.LEVEL == "1")
                                planPath += @"\一级响应行动.txt";
                            if (pAlarm.LEVEL == "2")
                                planPath += @"\二级响应行动.txt";
                            if (pAlarm.LEVEL == "3")
                                planPath += @"\三级响应行动.txt";
                            if (pAlarm.LEVEL == "4")
                                planPath += @"\四级响应行动.txt";

                            StreamReader sr = new StreamReader(planPath, Encoding.Default);
                            string strLine = sr.ReadToEnd();
                            sr.Close();

                            pAlarm.GUIDE = strLine;
                        }
                        else
                            pAlarm.GUIDE = "";
                    }
                    else
                        pAlarm.GUIDE = "";
                    }

                    pAlarm.LEVEL = dicLevel[pAlarm.LEVEL];

                    string strData = JsonConvert.SerializeObject(pAlarm);
                    wcf.Result = true;
                    wcf.Data = strData;
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream GetPSQKForecast(string startTime, string endTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region DateTime
                DateTime startDateTime = default(DateTime);
                DateTime endDateTime = default(DateTime);
                try
                {
                    startDateTime = DateTime.ParseExact(startTime, "yyyyMMddHHmmss", null);
                    endDateTime = DateTime.ParseExact(endTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex2)
                {
                    ArgumentException ex = new ArgumentException("时间参数错误。", "dtEnd", ex2);
                    string text = string.Concat(new string[]
					{
						ex.Message,
						Environment.NewLine,
						"调试信息：",
						ex.StackTrace,
						Environment.NewLine,
						ex.InnerException.Message,
						Environment.NewLine,
						ex.InnerException.StackTrace
					});
                    throw ex;
                }
                #endregion

                Dictionary<string, string> dicLevel = new Dictionary<string, string>();
                dicLevel.Add("1", "红色");
                dicLevel.Add("2", "橙色");
                dicLevel.Add("3", "黄色");
                dicLevel.Add("4", "蓝色");

                string ss = string.Format("SELECT T1.[ForecastTime],T1.[ObjectName],T1.[Level]  FROM [CityDisasterForecast1].[dbo].[T_PSQKForecast] as T1,(SELECT max([ForecastTime]) as [ForecastTime],[ObjectName]  FROM [CityDisasterForecast1].[dbo].[T_PSQKForecast]  where [ForecastTime] >'{0}' and [ForecastTime]  <'{1}' group by objectName)  as T2  WHERE T1.[ForecastTime]=T2.[ForecastTime] AND T1.[ObjectName] =T2.[ObjectName] ORDER BY T2.[ForecastTime]", startDateTime.ToString("yyyy-MM-dd HH:mm:ss"), endDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtSubAlarms = new Database().GetDataTable(ss);
                List<Alarm> pSubAlarms = new List<Alarm>();
                for (int i = 0; i < dtSubAlarms.Rows.Count; i++)
                {
                    Alarm pAlarm = new Alarm();
                    DataRow dr = dtSubAlarms.Rows[i];
                    pAlarm.Name = dr["ObjectName"].ToString();
                    pAlarm.FORECASTDATE = DateTime.Parse(dr["ForecastTime"].ToString());
                    pAlarm.LEVEL = dicLevel[dr["Level"].ToString()];
                    pAlarm.TYPE = "暴雨内涝风险预报（人工修正）";
                    pSubAlarms.Add(pAlarm);
                }

                wcf.Data = pSubAlarms;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetPSQKForecastModel(string startTime, string endTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region DateTime
                DateTime startDateTime = default(DateTime);
                DateTime endDateTime = default(DateTime);
                try
                {
                    startDateTime = DateTime.ParseExact(startTime, "yyyyMMddHHmmss", null);
                    endDateTime = DateTime.ParseExact(endTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex2)
                {
                    ArgumentException ex = new ArgumentException("时间参数错误。", "dtEnd", ex2);
                    string text = string.Concat(new string[]
					{
						ex.Message,
						Environment.NewLine,
						"调试信息：",
						ex.StackTrace,
						Environment.NewLine,
						ex.InnerException.Message,
						Environment.NewLine,
						ex.InnerException.StackTrace
					});
                    throw ex;
                }
                #endregion
                #region dic_AreaID
                Dictionary<string, string> dic_AreaID = new Dictionary<string, string>();
                dic_AreaID.Add("3", "二军大");
                dic_AreaID.Add("4", "复旦大学");
                dic_AreaID.Add("5", "同济大学");
                dic_AreaID.Add("8", "森林公园");
                dic_AreaID.Add("24", "民星北块");
                dic_AreaID.Add("29", "大柏树");
                dic_AreaID.Add("31", "周塘浜");
                dic_AreaID.Add("32", "松潘");
                dic_AreaID.Add("33", "周家嘴");
                dic_AreaID.Add("42", "Yn7");
                dic_AreaID.Add("85", "嫩江");
                dic_AreaID.Add("86", "营口");
                dic_AreaID.Add("123", "大定海");
                dic_AreaID.Add("124", "大武川");
                dic_AreaID.Add("135", "民星（南块）");
                dic_AreaID.Add("158", "国和");
                dic_AreaID.Add("159", "五角场");
                dic_AreaID.Add("160", "长白");
                dic_AreaID.Add("161", "控江");
                dic_AreaID.Add("170", "江湾新城");
                dic_AreaID.Add("175", "四平");
                dic_AreaID.Add("176", "鞍山");
                dic_AreaID.Add("177", "凤城");
                dic_AreaID.Add("178", "昆明");
                dic_AreaID.Add("180", "大连");
                dic_AreaID.Add("181", "惠民");
                dic_AreaID.Add("283", "复兴岛"); 
                #endregion

                Dictionary<string, string> dicLevel = new Dictionary<string, string>();
                dicLevel.Add("1", "红色");
                dicLevel.Add("2", "橙色");
                dicLevel.Add("3", "黄色");
                dicLevel.Add("4", "蓝色");

                string ss = string.Format("SELECT T1.[DATA_START_TIME],T1.[AREA_ID],[AREA_COUNT],[AREA_FILTER_COUNT] FROM [CITYDISASTERFORECAST].[DBO].[T_AREASTATISTICS_YP] AS T1, (SELECT MAX(DATA_START_TIME) AS DATA_START_TIME,AREA_ID FROM [CITYDISASTERFORECAST].[DBO].[T_AREASTATISTICS_YP]  WHERE DATA_START_TIME>'{0}' AND DATA_START_TIME<'{1}' GROUP BY  AREA_ID) AS T2 WHERE T1.[DATA_START_TIME]=T2.[DATA_START_TIME] AND T1.[AREA_ID] =T2.[AREA_ID] ORDER BY T2.[DATA_START_TIME]", startDateTime.ToString("yyyy-MM-dd HH:mm:ss"), endDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtSubAlarms = new Database().GetDataTable(ss);
                List<Alarm> pSubAlarms = new List<Alarm>();
                for (int i = 0; i < dtSubAlarms.Rows.Count; i++)
                {
                    Alarm pAlarm = new Alarm();
                    DataRow dr = dtSubAlarms.Rows[i];
                    pAlarm.Name = dic_AreaID[dr["AREA_ID"].ToString()];
                    pAlarm.FORECASTDATE = DateTime.Parse(dr["DATA_START_TIME"].ToString());

                    float AREA_COUNT=float.Parse(dr["AREA_COUNT"].ToString());
                    float AREA_FILTER_COUNT = float.Parse(dr["AREA_FILTER_COUNT"].ToString());
                    float percent = AREA_FILTER_COUNT * 100 / AREA_COUNT;
                    if (percent > 5)
                        pAlarm.LEVEL = dicLevel["1"];
                    else if (percent > 2.5)
                        pAlarm.LEVEL = dicLevel["2"];
                    else if (percent > 1.25)
                        pAlarm.LEVEL = dicLevel["3"];
                    else if (percent > 0)
                        pAlarm.LEVEL = dicLevel["4"];
                    else
                        pAlarm.LEVEL = "无预报等级";

                    pAlarm.TYPE = "暴雨内涝风险预报（水动力模型）";
                    pSubAlarms.Add(pAlarm);
                }

                wcf.Data = pSubAlarms;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream GetPotentialPoints()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DataTable dtData = new DataTable();
                #region CreateTable
                
                DataColumn dc = new DataColumn("Name", typeof(string));
                dtData.Columns.Add(dc);
                dc = new DataColumn("Type", typeof(string));
                dtData.Columns.Add(dc); 
                dc = new DataColumn("LON", typeof(double));
                dtData.Columns.Add(dc);
                dc = new DataColumn("LAT", typeof(double));
                dtData.Columns.Add(dc);
                #endregion

                #region Fill Table
                StreamReader sr = new StreamReader(@"F:\ReadearthCode\JSONService\JsonService\geometry\points.csv",Encoding.Default);

                string[] key = sr.ReadLine().Split(',');
                string strLine = "";
                while ((strLine = sr.ReadLine()) != null)
                {
                    DataRow drNew = dtData.NewRow();
                    string[] data = strLine.Split(',');
                    for (int i = 0; i < key.Length; i++)
                    {
                        drNew[key[i]] = data[i];
                    }
                    dtData.Rows.Add(drNew);
                }

                sr.Close();
                #endregion

                //string strData = JsonConvert.SerializeObject(dtData);
                wcf.Result = true;
                wcf.Data = dtData;

            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        struct structData
        {
            public DataTable RainHour
            {
                get;
                set;            
            }
            public DataTable Tempreture
            {
                get;
                set;
            }
            public DataTable Wind
            {
                get;
                set;
            }
        }
        public Stream GetAutoStationData1(string userName, string strEnd)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                #region 序列化时间
                DateTime dtEnd = new DateTime();
                try
                {
                    dtEnd = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "dtEnd", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                #endregion

                Database db = new Database();
                string stationID = db.GetDataTable("SELECT * FROM T_USER WHERE USERNAME ='" + userName + "'").Rows[0]["AutoStationID"].ToString();

                string strSQL = string.Format("SELECT TOP 6 DATETIME,RAINHOUR FROM CityDisasterForecast.dbo.T_AUTOSTATIONDATA WHERE DATETIME <='{0}' AND STATIONID='{1}' AND SUBSTRING(CONVERT(VARCHAR(100),DATETIME,120),15,5)='00:00' ORDER BY DATETIME DESC;SELECT TOP 1 DATETIME,RAINHOUR FROM CityDisasterForecast.dbo.T_AUTOSTATIONDATA WHERE DATETIME <='{0}' AND STATIONID='{1}' ORDER BY DATETIME DESC;SELECT TOP 24 DATETIME ,WINDDIRECTION,WINDSPEED   FROM CityDisasterForecast.dbo.T_AUTOSTATIONDATA WHERE DATETIME <='{0}' AND STATIONID ='{1}' ORDER BY DATETIME DESC;SELECT TOP 24 DATETIME ,TEMPERATURE FROM CityDisasterForecast.dbo.T_AUTOSTATIONDATA WHERE  DATETIME <='{0}' AND STATIONID ='{1}' AND SUBSTRING(CONVERT(VARCHAR(100),DATETIME,120),15,5)='00:00'ORDER BY DATETIME DESC", dtEnd, stationID);
                System.Data.DataSet ds = db.GetDataset(strSQL);

                //strSQL = string.Format("SELECT TOP 24 DATETIME ,WINDDIRECTION,WINDSPEED   FROM CityDisasterForecast.dbo.T_AUTOSTATIONDATA WHERE DATETIME <='{0}' AND STATIONID ={1} ORDER BY DATETIME DESC", dtEnd, stationID);
                //DataTable dtWind = db.GetDataTable(strSQL);

                //strSQL = string.Format("SELECT TOP 24 DATETIME ,TEMPERATURE FROM CityDisasterForecast.dbo.T_AUTOSTATIONDATA WHERE  DATETIME <='{0}' AND STATIONID ={1} AND SUBSTRING(CONVERT(VARCHAR(100),DATETIME,120),15,5)='00:00'ORDER BY DATETIME DESC", dtEnd, stationID);
                //DataTable dtTemp = db.GetDataTable(strSQL);

                DataTable dtRain = ds.Tables[1];
                DateTime dttmp = DateTime.Parse(dtRain.Rows[0][0].ToString());
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    DataRow drNew = dtRain.NewRow();
                    drNew["DATETIME"] = ds.Tables[0].Rows[i]["DATETIME"];
                    drNew["RAINHOUR"] = ds.Tables[0].Rows[i]["RAINHOUR"];
                    if (dttmp != DateTime.Parse(drNew["DATETIME"].ToString()))
                        dtRain.Rows.Add(drNew);
                }
                DataTable dtWind = ds.Tables[2];
                DataTable dtTemp = ds.Tables[3];

                structData structD = new structData();
                structD.RainHour = dtRain;
                structD.Tempreture =dtTemp;
                structD.Wind = dtWind;

                string strData = JsonConvert.SerializeObject(structD);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf); 
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream GetAutoStationData(string userName, string strEnd)
        {
            WCFResult wCFResult = new WCFResult(false);
            try
            {
                DateTime dateTime = default(DateTime);
                try
                {
                    dateTime = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex2)
                {
                    ArgumentException ex = new ArgumentException("时间参数错误。", "dtEnd", ex2);
                    string text = string.Concat(new string[]
					{
						ex.Message,
						Environment.NewLine,
						"调试信息：",
						ex.StackTrace,
						Environment.NewLine,
						ex.InnerException.Message,
						Environment.NewLine,
						ex.InnerException.StackTrace
					});
                    throw ex;
                }
                Database database = new Database();
                string arg = database.GetDataTable("SELECT * FROM T_USER WHERE USERNAME ='" + userName + "'").Rows[0]["AutoStationID"].ToString();
                DataTable dataTable = database.GetDataTable(string.Format("SELECT *  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE STATIONID ='{0}' AND DATETIME <='{1}' AND DATETIME >='{2}' ORDER BY DATETIME DESC", arg, dateTime.ToString("yyyy-MM-dd HH:mm:ss"), dateTime.AddDays(-1.0).ToString("yyyy-MM-dd HH:mm:ss")));
                int num = 0;
                while (num + 5 <= dateTime.Minute)
                {
                    num += 5;
                }
                DateTime dateTime2 = DateTime.Parse(dateTime.ToString("yyyy-MM-dd HH:00:00"));
                DateTime dateTime3 = dateTime2.AddMinutes((double)num);
                DataTable dataTable2 = dataTable.Clone();
                DataView dataView = new DataView(dataTable);
                SHFL.ForStationData forStationData = new SHFL.ForStationData();
                for (int i = 0; i < 72; i++)
                {
                    if (i < 18)
                    {
                        dataView.RowFilter = "DateTime='" + dateTime2.AddHours((double)(-1 * i - 6)).ToString() + "'";
                        if (dataView.ToTable().Rows.Count != 0)
                        {
                            dataTable2.Rows.Add(dataView.ToTable().Rows[0].ItemArray);
                        }
                        else
                        {
                            DataRow dataRow = dataTable2.NewRow();
                            dataRow["DateTime"] = dateTime2.AddHours((double)(-1 * i - 6));
                            dataRow["Temperature"] = forStationData.leastTime(dataTable, dateTime2.AddHours((double)(-1 * i - 6)));
                            dataTable2.Rows.Add(dataRow.ItemArray);
                        }
                    }
                    dataView.RowFilter = "DateTime='" + dateTime3.AddMinutes((double)(-5 * i)).ToString() + "'";
                    if (dataView.ToTable().Rows.Count != 0)
                    {
                        dataTable2.Rows.Add(dataView.ToTable().Rows[0].ItemArray);
                    }
                    else
                    {
                        DataRow dataRow = dataTable2.NewRow();
                        dataRow["DateTime"] = dateTime3.AddMinutes((double)(-5 * i));
                        dataRow["Temperature"] = forStationData.leastTime(dataTable, dateTime3.AddMinutes((double)(-5 * i)));
                        dataTable2.Rows.Add(dataRow.ItemArray);
                    }
                }
                dataTable2.DefaultView.Sort = "Datetime asc";
                dataTable2 = dataTable2.DefaultView.ToTable();
                string data = JsonConvert.SerializeObject(dataTable2);
                wCFResult.Result = true;
                wCFResult.Data = data;
            }
            catch (Exception ex2)
            {
                wCFResult.Message = ex2.Message;
            }
            string s = JsonConvert.SerializeObject(wCFResult);
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }


        public enum FileType
        {
            File02401 = 02401,
            File07203 = 07203,
            File36010 = 36010
        }
        //strDatetime:yyyyMMddHHmmss
        public Stream GetAutoStationModelData(string strQueryTime, string strType)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime queryTime = default(DateTime);

                #region 时间参数
                try
                {
                    queryTime = DateTime.ParseExact(strQueryTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex2)
                {
                    ArgumentException ex = new ArgumentException("时间参数错误。", "dtEnd", ex2);
                    string text = string.Concat(new string[]
					{
						ex.Message,
						Environment.NewLine,
						"调试信息：",
						ex.StackTrace,
						Environment.NewLine,
						ex.InnerException.Message,
						Environment.NewLine,
						ex.InnerException.StackTrace
					});
                    throw ex;
                } 
                #endregion

                if (!(new string[] { "02401", "07203", "36010" }).Contains(strType))
                    throw new Exception("文件类型参数错误。");
                Database database = new Database();
                string strSQL = string.Format("SELECT max(ForecastTime)  FROM CityDisasterForecast.dbo.T_AutoStationModelData where forecastTime <='{0}' and griddatatype='{1}'", queryTime, strType);

                string dtTime = database.GetFirstValue(strSQL);//SELECT ForecastTime, Period, GridDataType, StationID, StationName, Lon, Lat, RainHo         T_AutoStationModelData
                strSQL = string.Format("SELECT ForecastTime,Period,GridDataType,StationID,StationName,Lon,Lat,RainHour FROM CityDisasterForecast.dbo.T_AutoStationModelData where forecastTime ='{0}' and griddatatype='{1}' and stationid in ('58361','58362','58365','58366','58367','58369','58370','58460','58461','58462','58463','99102','99603','99641','99671','A4077');", dtTime, strType);
                DataTable dtData = database.GetDataTable(strSQL);
                wcf.Result = true;
                wcf.Data = dtData;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream GetDataByID(string stationID,string WaterStationID)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database db = new Database();

                DataTable dtData1 = db.GetDataTable(string.Format("SELECT top 1 Datetime,StationName,Temperature,RainHour,RH,WindDirection,WindSpeed,Visible  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE STATIONID ='{0}' ORDER BY DATETIME DESC", stationID));
                DataTable dtData2 = db.GetDataTable(string.Format("SELECT top 1 StationName,DataTime,WaterDepth,Status FROM [CITYDISASTERFORECAST].[DBO].[Tbl_WaterStationData] WHERE STATIONID ='{0}' ORDER BY DataTime DESC", WaterStationID));
                if (dtData1.Rows.Count == 0)
                {
                    wcf.Message = "自动站点号有误。";
                    dtData1.Rows.Add(new object[1]);
                }
                if (dtData2.Rows.Count == 0)
                {
                    wcf.Message = wcf.Message == "" ? "积水站点号有误。" : "自动站点号有误,积水站点号有误。";
                    dtData2.Rows.Add(new object());
                }
                string time = dtData1.Rows[0]["Datetime"].ToString();
                dtData1.Columns.Remove("Datetime");
                dtData1.Columns.Add("Datetime", typeof(string));
                dtData1.Columns.Add("WaterStationName", typeof(string));
                dtData1.Columns.Add("WaterDataTime", typeof(string));
                dtData1.Columns.Add("WaterDepth", typeof(string));
                dtData1.Columns.Add("Status", typeof(string));

                dtData1.Rows[0]["Datetime"] = time;
                dtData1.Rows[0]["WaterStationName"] = dtData2.Rows[0]["StationName"];
                dtData1.Rows[0]["WaterDataTime"] = dtData2.Rows[0]["DataTime"].ToString();
                dtData1.Rows[0]["WaterDepth"] = dtData2.Rows[0]["WaterDepth"];
                dtData1.Rows[0]["Status"] = dtData2.Rows[0]["Status"];
                string strData = JsonConvert.SerializeObject(dtData1);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream Get5DayForecast()
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\STYB\"+DateTime.Now.ToString("yyyy")+"\\"+DateTime.Now.ToString("yyyyMMdd");
                string[] files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    path = path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\STYB\" + DateTime.Now.AddDays(-1).ToString("yyyy") + "\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    files = Directory.GetFiles(path);
                }
                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[]{'_','.'})[2];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }
                //selectFile = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\STYB\2016\20160531\smc_ybm10_201605311500.txt";
                DataTable dtRes = new DataTable();
                dtRes.Columns.Add(new DataColumn("Time",typeof(string)));
                dtRes.Columns.Add(new DataColumn("Day", typeof(string)));
                dtRes.Columns.Add(new DataColumn("Night", typeof(string)));
                dtRes.Columns.Add(new DataColumn("LowTmp", typeof(string)));
                dtRes.Columns.Add(new DataColumn("HighTmp", typeof(string)));
                dtRes.Columns.Add(new DataColumn("Wind", typeof(string)));
                dtRes.Columns.Add(new DataColumn("WindLev", typeof(string)));

                StreamReader sr = new StreamReader(selectFile,Encoding.Default);
                string strLine = sr.ReadLine();
                int count = 0;
                while ((strLine = sr.ReadLine()) != null)
                {
                    string Nowtime = DateTime.Now.ToShortDateString().Replace(@"/","-");
                    string[] datas = Regex.Split(strLine, "\\s+");
                    if (datas[0] == Nowtime)
                        break;
                }
                while (strLine != null)
                {
                    string[] datas = Regex.Split(strLine, "\\s+");
                    DataRow drNew = dtRes.NewRow();
                    for (int i = 0; i < dtRes.Columns.Count; i++)
                    {
                        if (dtRes.Columns[i].ColumnName == "Day" || dtRes.Columns[i].ColumnName == "Night")
                        {
                            drNew[i] = GetWeather(datas[i]);
                        }
                        else
                            drNew[i] = datas[i];
                    }
                    dtRes.Rows.Add(drNew);
                    dtRes.AcceptChanges();

                    count++;
                    //if (count >= 5)
                    //    break;
                    strLine=sr.ReadLine();
                }
                
                string strData = JsonConvert.SerializeObject(dtRes);

                wcf.Result = true;
                wcf.Data = strData;
                //return new MemoryStream(Encoding.Default.GetBytes(res));
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        string GetWeather(string contect)
        {	
            if (contect.Contains("暴雪"))
                return "暴雪";
            else if (contect.Contains("大雪"))
                return "大雪";
            else if (contect.Contains("中雪"))
                return "中雪";
            else if (contect.Contains("小雪"))
                return "小雪";
            else if (contect.Contains("阵雪"))
                return "阵雪";
            else if (contect.Contains("冻雨"))
                return "冻雨";
            else if (contect.Contains("雨夹雪"))
                return "雨夹雪";
            else if (contect.Contains("特大暴雨"))
                return "特大暴雨";
            else if (contect.Contains("大暴雨"))
                return "大暴雨";
            else if (contect.Contains("雷阵雨")||contect.Contains("雷雨"))
                return "雷阵雨";
            else if (contect.Contains("暴雨"))
                return "暴雨";
            else if (contect.Contains("大雨"))
                return "大雨";
            else if (contect.Contains("阵雨"))
                return "阵雨";
            else if (contect.Contains("中雨"))
                return "中雨";
            else if (contect.Contains("小雨"))
                return "小雨";
             else if (contect.Contains("冰雹"))
                return "冰雹";
             else if (contect.Contains("阴"))
                return "阴";
             else if (contect.Contains("多云"))
                return "多云";
            else
                return "晴";

        }
        public Stream Get5DayForecast_Weixin()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\STYB\" + DateTime.Now.ToString("yyyy") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                string[] files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    path = path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\STYB\" + DateTime.Now.AddDays(-1).ToString("yyyy") + "\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    files = Directory.GetFiles(path);
                }
                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[2];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }
                //selectFile = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\STYB\2016\20160531\smc_ybm10_201605311500.txt";
                DataTable dtRes = new DataTable();
                dtRes.Columns.Add(new DataColumn("Time", typeof(string)));
                dtRes.Columns.Add(new DataColumn("Day", typeof(string)));
                dtRes.Columns.Add(new DataColumn("Night", typeof(string)));
                dtRes.Columns.Add(new DataColumn("LowTmp", typeof(string)));
                dtRes.Columns.Add(new DataColumn("HighTmp", typeof(string)));
                dtRes.Columns.Add(new DataColumn("Wind", typeof(string)));
                dtRes.Columns.Add(new DataColumn("WindLev", typeof(string)));

                StreamReader sr = new StreamReader(selectFile, Encoding.Default);
                string strLine = sr.ReadLine();
                int count = 0;
                while ((strLine = sr.ReadLine()) != null)
                {
                    //string Nowtime = DateTime.Now.AddDays(1).ToShortDateString().ToString().Replace(@"/", "-");
                    string Nowtime = DateTime.Now.ToShortDateString().ToString().Replace(@"/", "-");
                    string[] datas = Regex.Split(strLine, "\\s+");
                    if (datas[0] == Nowtime)
                        break;
                }
                while (strLine != null)
                {
                    string[] datas = Regex.Split(strLine, "\\s+");
                    DataRow drNew = dtRes.NewRow();
                    for (int i = 0; i < dtRes.Columns.Count; i++)
                    {
                        if (dtRes.Columns[i].ColumnName == "Day" || dtRes.Columns[i].ColumnName == "Night")
                        {
                            drNew[i] = GetWeather(datas[i]);
                        }
                        else
                            drNew[i] = datas[i];
                    }
                    dtRes.Rows.Add(drNew);
                    dtRes.AcceptChanges();

                    count++;
                    //if (count >= 5)
                    //    break;
                    strLine = sr.ReadLine();
                }

                string strData = JsonConvert.SerializeObject(dtRes);

                wcf.Result = true;
                wcf.Data = dtRes;
                //return new MemoryStream(Encoding.Default.GetBytes(res));
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetWeatherWarnning()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {

                DataTable dtData = GetWaringData();
                if (dtData != null)
                {
                    wcf.Result = true;
                    wcf.Data = dtData;
                }
                else
                    throw new Exception("未查询到当前有生效预警。");
                //return new MemoryStream(Encoding.Default.GetBytes(res));
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream GetWordText(string obejctName, string starttime, string endtime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                #region 序列化参数
                try
                {
                    dtStart = DateTime.ParseExact(starttime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                try
                {
                    dtEnd = DateTime.ParseExact(endtime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                #endregion

                Database db = new Database();
                string strSQL = string.Format("select AutoStationID from [APP_Risk_Data].[dbo].[T_User] where Alias ='{0}'",obejctName);
                DataTable dtID = db.GetDataTable(strSQL);
                if (dtID.Rows.Count > 0)
                {
                    string strAutoStationID = dtID.Rows[0][0].ToString();
                    strSQL = "SELECT SUM([RAINHOUR]),STATIONNAME  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE STATIONID=@stationid AND  DATETIME >@starttime AND DATETIME<@endtime AND DATEPART(MI,DATETIME) =0 GROUP BY STATIONNAME;  SELECT [RAINHOUR]  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE STATIONID=@stationid AND  DATETIME =@starttime;  SELECT [RAINHOUR]  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE STATIONID=@stationid AND  DATETIME =@endtime;SELECT TOP 1 ([RAINHOUR]),DATETIME  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA]  WHERE STATIONID=@stationid AND  DATETIME >@starttime AND DATETIME<@endtime AND DATEPART(MI,DATETIME) =0 ORDER BY RAINHOUR DESC;";


                    System.Data.SqlClient.SqlParameter[] paras = new System.Data.SqlClient.SqlParameter[3];

                    paras[0] = new System.Data.SqlClient.SqlParameter("@stationid", strAutoStationID);
                    paras[1] = new System.Data.SqlClient.SqlParameter("@starttime", dtStart);
                    paras[2] = new System.Data.SqlClient.SqlParameter("@endtime", dtEnd);

                    System.Data.DataSet ds = db.GetDataset(strSQL, paras);

                    string sumRain = ds.Tables[0].Rows.Count > 0 ? ds.Tables[0].Rows[0][0].ToString() : "0";
                    string stationName = ds.Tables[0].Rows.Count > 0 ? ds.Tables[0].Rows[0][1].ToString() : "[error]";

                    string startRain = ds.Tables[1].Rows.Count > 0 ? ds.Tables[1].Rows[0][0].ToString() : "0";
                    string endRain = ds.Tables[2].Rows.Count > 0 ? ds.Tables[2].Rows[0][0].ToString() : "0";

                    string maxHourRain = ds.Tables[3].Rows.Count > 0 ? ds.Tables[3].Rows[0][0].ToString() : "0";
                    DateTime maxHourRainTime = ds.Tables[3].Rows.Count > 0 ? DateTime.Parse(ds.Tables[3].Rows[0][1].ToString()) : DateTime.MinValue;
                    string strDate = "";
                    if (maxHourRainTime.Hour == 0)
                        strDate = string.Format("{0}到{1}", maxHourRainTime.AddHours(-1).ToString("MM月dd日HH时"), maxHourRainTime.ToString("MM月dd日HH时"));
                    else
                        strDate = string.Format("{0}到{1}", maxHourRainTime.AddHours(-1).ToString("MM月dd日HH时"), maxHourRainTime.ToString("HH时"));

                    double totalRain = double.NaN;
                    if (dtStart.Minute == 0)
                    {
                        totalRain = double.Parse(sumRain) + double.Parse(endRain);
                    }
                    else
                    {
                        totalRain = double.Parse(sumRain) + double.Parse(endRain) - double.Parse(startRain);
                    }
                    string re = string.Format("{0}{1}自动站累计降水{2}mm，最大小时降水为{3}mm/h，出现时间在{4}。"
                        , obejctName
                        , stationName
                        , totalRain
                        , maxHourRain
                        , strDate
                        );
                    wcf.Result = true;
                    wcf.Data = re;
                }
                else
                    wcf.Message = "街道名称未查询到自动站ID";
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            } 
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetWordText1(string obejctName, string starttime, string endtime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                
                #region 序列化参数
                try
                {
                    dtStart = DateTime.ParseExact(starttime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                try
                {
                    dtEnd = DateTime.ParseExact(endtime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                #endregion
                ThunderObject to=null;
                //obejctName = "上海第二工业大学";
                if (obejctName == "上海第二工业大学")
                    to = new ThunderObject("egd");
                if (obejctName == "上海化学工业区")
                    to = new ThunderObject("hgq"); 
                if (obejctName == "欢乐谷")
                    to = new ThunderObject("hlg");


                string strSQL = "";
                if (ConfigurationManager.AppSettings["LightningSource"].ToString() == "V")
                {
                    strSQL = string.Format("SELECT DATETIME,LON,LAT,(LON-{6})*(LON-{6})+(LAT-{7})*(LAT-{7}) AS DIS  FROM CITYDISASTERFORECAST.DBO.T_LIGHTNING WHERE (DATETIME >='{0}'AND DATETIME<'{1}') AND (LON BETWEEN {2} AND {3}) AND (LAT  BETWEEN {4} AND {5}) order by dis", dtStart,dtEnd, to.XMin, to.XMax, to.YMin, to.YMax, to.Lon, to.Lat);
                }
                else
                {
                    strSQL = string.Format("SELECT  LIGHTNING_TIME AS DATETIME,LONGITUDE AS LON,LATITUDE AS LAT,(LONGITUDE-{5})*(LONGITUDE-{5})+(LATITUDE-{6})*(LATITUDE-{6}) AS DIS FROM CITYDISASTERFORECAST.DBO.T_LTGFLASHPORTIONS WHERE (LIGHTNING_TIME >'{0}' and LIGHTNING_TIME <'{7}' )AND STROKE_TYPE=0 AND (LONGITUDE BETWEEN {1} AND {2}) AND (LATITUDE  BETWEEN {3} AND {4}) order by dis", dtStart, to.XMin, to.XMax, to.YMin, to.YMax, to.Lon, to.Lat,dtEnd);

                }
                DataTable dtThunder = new Database().GetDataTable(strSQL);
                int count10 = 0, count20 = 0, count30 = 0;
                for (int i = 0; i < dtThunder.Rows.Count; i++)
                {
                    DataRow dr = dtThunder.Rows[i];
                    Coordinate cc = new Coordinate(double.Parse(dr["LON"].ToString()), double.Parse(dr["LAT"].ToString()));

                    LamProjection lpj = new LamProjection(new VectorPointXY(121, 32), 30, 60, GeoRefEllipsoid.WGS_84);
                    VectorPointXY xy0 = lpj.GeoToProj(new VectorPointXY(to.Lon, to.Lat));
                    VectorPointXY xy = lpj.GeoToProj(new VectorPointXY(cc.X, cc.Y));

                    double dis = Math.Round(0.001 * Math.Sqrt((xy.X - xy0.X) * (xy.X - xy0.X) + (xy.Y - xy0.Y) * (xy.Y - xy0.Y)), 2);
                    if (dis <= 10)
                        count10++;
                    else if (dis <= 20) 
                        count20++;
                    else if (dis <= 30) 
                        count30++;
                    else
                        continue;

                }
                ThunderTextObject tto = new ThunderTextObject();
                tto.COUNT10 = count10;
                tto.COUNT20 = count20;
                tto.COUNT30 = count30;

                if(count10>0&&count20>0&&count30>0)
                    tto.Desc = "不同预警圈内都有闪电发生";
                else if (count10 > 0 && count20 > 0)
                    tto.Desc = "I、II级预警圈内都有闪电发生";
                else if (count10 > 0 && count30 > 0)
                    tto.Desc = "I、III级预警圈内都有闪电发生";
                else if (count30 > 0 && count20 > 0)
                    tto.Desc = "II、III级预警圈内都有闪电发生";
                else if (count10 > 0)
                    tto.Desc = "I级预警圈内有闪电发生";
                else if (count20 > 0)
                    tto.Desc = "II级预警圈内有闪电发生";
                else if (count30 > 0)
                    tto.Desc = "III级预警圈内有闪电发生";

                wcf.Data = tto;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        DataTable GetWaringData()
        {
            Database m_Database = new Database();
            //返回到前台的预警信息文本
            //查询预警的种类
            string strWarnTypeSQL = "select DM from [CITYDISASTERFORECAST].[DBO].D_YJXHMessage_Type";
            DataTable dtType = m_Database.GetDataTable(strWarnTypeSQL);
            List<string> listTypeNo = new List<string>();

            if (dtType.Rows.Count > 0)
            {
                for (int i = 0; i < dtType.Rows.Count; i++)
                {
                    listTypeNo.Add(dtType.Rows[i]["DM"].ToString());
                }
            }
            else
            {
                for (int j = 0; j < 15; j++)
                {
                    listTypeNo.Add((j + 1).ToString());
                }
            }
            string strWarningTextSql = "";
            for (int m = 0; m < listTypeNo.Count; m++)
            {
                //SELECT TOP 1 FORECASTDATE ,TYPE,OPERATION,LEVEL,CONTENT,GUIDE 

                strWarningTextSql += "SELECT TOP 1 起报时间 AS FORECASTDATE,操作 AS OPERATION ,操作代码 AS OPERATION_CODE,级别 AS LEVEL,级别代码 AS LEVEL_CODE,类型 AS  TYPE,类型代码 AS  TYPE_CODE,正文 AS CONTENT,防御指引 AS GUIDE FROM [CITYDISASTERFORECAST].[DBO].V_YJXHMessage  where 类型代码=" + m.ToString() + " order by 起报时间 desc;";
            }
            if (strWarningTextSql != "")
            {
                System.Data.DataSet dtAllWarnContent = m_Database.GetDataset(strWarningTextSql);
                DataTable dtCurWar;
                if (dtAllWarnContent.Tables.Count > 0)
                {
                    string strForeTime = "";

                    for (int k = 1; k < dtAllWarnContent.Tables.Count; k++)
                    {
                        dtCurWar = dtAllWarnContent.Tables[k];
                        if (dtCurWar.Rows.Count > 0)
                        {
                            if (dtCurWar.Rows[0]["OPERATION_CODE"].ToString() != "3")
                            {
                                dtAllWarnContent.Tables[0].Rows.Add(dtCurWar.Rows[0].ItemArray);
                            }
                            //如果状态为解除，根据距离当前时间是否有1小时来来确定是否显示
                            else
                            {
                                strForeTime = dtCurWar.Rows[0]["FORECASTDATE"].ToString();
                                TimeSpan timeSpan = new TimeSpan((DateTime.Parse(strForeTime) - DateTime.Now).Days, (DateTime.Parse(strForeTime) - DateTime.Now).Hours, (DateTime.Parse(strForeTime) - DateTime.Now).Minutes, (DateTime.Parse(strForeTime) - DateTime.Now).Seconds);
                                if (Math.Abs(timeSpan.TotalHours) < 1)
                                {
                                    dtAllWarnContent.Tables[0].Rows.Add(dtCurWar.Rows[0].ItemArray);
                                }
                            }
                        }
                    }
                }

                return dtAllWarnContent.Tables[0];
            }
            return null;
        }
        public Stream GetRiskWarnning(string CommunityID)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {

                string strSQL = string.Format("SELECT ID,NAME,DISTRICT,LON,LAT FROM [APP_RISK_DATA].[DBO].[T_STREETLOCATION] WHERE ID={0}", CommunityID);
                Database db = new Database();

                DataTable dtCommunity = db.GetDataTable(strSQL);
                if (dtCommunity.Rows.Count == 0)
                    throw new Exception("无效的社区编号");
                Community c = new Community();
                c.ID = dtCommunity.Rows[0]["ID"].ToString();
                c.Name = dtCommunity.Rows[0]["NAME"].ToString();
                c.District_Name = dtCommunity.Rows[0]["DISTRICT"].ToString();
                
                strSQL = "SELECT TOP 1 [ID],[FORECASTTIME],[OBJECTNAME],[TYPE],[STATE],[LEVEL],[DETAILS]  FROM [CITYDISASTERFORECAST].[DBO].[T_ALARM]  WHERE OBJECTNAME ='"+c.Name+"'  ORDER BY FORECASTTIME DESC";
                DataTable dtData = db.GetDataTable(strSQL);
                wcf.Result = true;
                wcf.Data = dtData;
                //return new MemoryStream(Encoding.Default.GetBytes(res));
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetZXSForecast()
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\ZXSYB\" + DateTime.Now.ToString("yyyy") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                string[] files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    path =  @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\ZXSYB\" + DateTime.Now.AddDays(-1).ToString("yyyy") + "\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    files = Directory.GetFiles(path);
                }
                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[2];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }

                DataTable dtRes = new DataTable();
                dtRes.Columns.Add(new DataColumn("Time", typeof(string)));
                dtRes.Columns.Add(new DataColumn("Weather", typeof(string)));
                dtRes.Columns.Add(new DataColumn("LowTmp", typeof(string)));
                dtRes.Columns.Add(new DataColumn("HighTmp", typeof(string)));
                dtRes.Columns.Add(new DataColumn("Wind", typeof(string)));
                dtRes.Columns.Add(new DataColumn("WindLev", typeof(string)));

                StreamReader sr = new StreamReader(selectFile, Encoding.Default);
                string strLine;
                while ((strLine = sr.ReadLine()) != null)
                {
                    string[] datas = strLine.Split('|');//"|");
                    DataRow drNew = dtRes.NewRow();
                    drNew[0] = datas[0];
                    drNew[1] =  GetWeather(datas[1]);
                    //drNew[1] = datas[1] == "晴天" ? "晴" : datas[1];
                    drNew[2] = datas[13];
                    drNew[3] = datas[10];
                    drNew[4] = datas[7];
                    drNew[5] = datas[8];

                    dtRes.Rows.Add(drNew);
                    dtRes.AcceptChanges();
                }
                string strData = JsonConvert.SerializeObject(dtRes);

                wcf.Result = true;
                wcf.Data = strData;
                //return new MemoryStream(Encoding.Default.GetBytes(strData));
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetAirQuality()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime dtQuery = DateTime.Now;
                if (DateTime.Now.Hour < 17)
                    dtQuery = dtQuery.AddDays(-1);
                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\AirQuality\" + dtQuery.ToString("yyyy") + "\\" + dtQuery.ToString("yyyyMMdd");
                string[] files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\AirQuality\" + dtQuery.AddDays(-1).ToString("yyyy") + "\\" + dtQuery.AddDays(-1).ToString("yyyyMMdd");
                    files = Directory.GetFiles(path);
                }
                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[2];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }

                //selectFile = @"C:\Users\Jinxh\Desktop\JsonService\JsonService\JsonService\TestFile\20160607\AQI_SH_201606071700.txt";
                StreamReader sr = new StreamReader(selectFile, Encoding.Default);
                
                string strLine = sr.ReadLine().Trim() ;
                AQI pAQI = new AQI();
                pAQI.Title = strLine;

                strLine = sr.ReadLine().Replace("发布", "").Replace("\t", " ").Trim(new char[] { '(', '）', ' ' });
                pAQI.PublisDate = strLine;
                sr.ReadLine();
                for (int i = 0; i < 5; i++)
                {
                    strLine = sr.ReadLine().Replace("\t", " ");

                    AQIData pAQIData = new AQIData();
                    string[] datas = Regex.Split(strLine, "\\s+");//"|");
                    pAQIData.Period = datas[0];
                    pAQIData.AQI = datas[1];
                    pAQIData.Level = datas[2];
                    pAQIData.Pripoll = datas[3];

                    pAQI.AQIDatas.Add(pAQIData);
                }
                
                //string strData = JsonConvert.SerializeObject(dtRes);

                wcf.Result = true;
                wcf.Data = pAQI;
                //return new MemoryStream(Encoding.Default.GetBytes(strData));
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        
        public Stream PostTouTiao(Stream strImage)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                string rootPath = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\APPTouTiaoImage\";
                StreamReader sr = new StreamReader(strImage);
                //StreamReader sr = new StreamReader(@"f:\test.log");
                string strJson = sr.ReadToEnd();
                sr.Close();
                //Directory.CreateDirectory(@"F:\ReadearthCode\JSONPub\Log");
                //StreamWriter sw = new StreamWriter(@"F:\ReadearthCode\JSONPub\Log\tets.log");
                //sw.Write(strJson);
                //sw.Close();
                
                News pTouTiao = JsonConvert.DeserializeObject<News>(strJson);
                if (pTouTiao.Title == "")
                    throw new Exception("头条标题不能为空。请检查。");
                //if (pTouTiao.SimpleDescription == "")
                //    throw new Exception("头条简要描述不能为空。请检查。");
                if (pTouTiao.Uri == "")
                    throw new Exception("头条链接不能为空。请检查。");
                


                if (pTouTiao.Base64Str != "")
                {
                    byte[] tmp = Convert.FromBase64String(pTouTiao.Base64Str.Replace("%2B", "+"));
                    MemoryStream ms = new MemoryStream(tmp);
                    ms.Write(tmp, 0, tmp.Length);
                    string relativePath = Guid.NewGuid().ToString("N") + ".jpg";
                    Image pImage = Image.FromStream(ms);
                    pImage.Save(rootPath + relativePath);
                    pTouTiao.ImageUrl = relativePath.Replace("\\", "/");
                }
                //pTouTiao.StrTime = DateTime.Now.ToString();

                string strInsert = string.Format("INSERT INTO [APP_RISK_DATA].[DBO].[T_TOUTIAO] (TITLE,SIMPLEDESCRIPTION,IMAGERELATIVEPATH,URI,PUBLISHDATETIME) VALUES ('{0}','{1}','{2}','{3}','{4}')", pTouTiao.Title, pTouTiao.SimpleDescription, pTouTiao.ImageUrl, pTouTiao.Uri, pTouTiao.StrTime);

                pDatabase.Execute(strInsert);

                wcf.Result = true;
                wcf.Message = "头条上传成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream PostTouTiao_Update(Stream Update)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                string rootPath = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\APPTouTiaoImage\";
                StreamReader sr = new StreamReader(Update);
                string strJson = sr.ReadToEnd();
                sr.Close();


                pDatabase.Execute(strJson);

                wcf.Result = true;
                wcf.Message = "头条修改成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream PostQixiangkepu(Stream strImage)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                string rootPath = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\APPTouTiaoImage\";
                StreamReader sr = new StreamReader(strImage);
                //StreamReader sr = new StreamReader(@"f:\test.log");
                string strJson = sr.ReadToEnd();
                sr.Close();
                //Directory.CreateDirectory(@"F:\ReadearthCode\JSONPub\Log");
                //StreamWriter sw = new StreamWriter(@"F:\ReadearthCode\JSONPub\Log\tets.log");
                //sw.Write(strJson);
                //sw.Close();

                News pTouTiao = JsonConvert.DeserializeObject<News>(strJson);
                if (pTouTiao.Title == "")
                    throw new Exception("头条标题不能为空。请检查。");
                //if (pTouTiao.SimpleDescription == "")
                //    throw new Exception("头条简要描述不能为空。请检查。");
                if (pTouTiao.Uri == "")
                    throw new Exception("头条链接不能为空。请检查。");



                if (pTouTiao.Base64Str != "")
                {
                    byte[] tmp = Convert.FromBase64String(pTouTiao.Base64Str.Replace("%2B", "+"));
                    MemoryStream ms = new MemoryStream(tmp);
                    ms.Write(tmp, 0, tmp.Length);
                    string relativePath = Guid.NewGuid().ToString("N") + ".jpg";
                    Image pImage = Image.FromStream(ms);
                    pImage.Save(rootPath + relativePath);
                    pTouTiao.ImageUrl = relativePath.Replace("\\", "/");
                }
                //pTouTiao.StrTime = DateTime.Now.ToString();

                string strInsert = string.Format("INSERT INTO [APP_RISK_DATA].[DBO].[T_QXKP] (TITLE,SIMPLEDESCRIPTION,IMAGERELATIVEPATH,URI,PUBLISHDATETIME) VALUES ('{0}','{1}','{2}','{3}','{4}')", pTouTiao.Title, pTouTiao.SimpleDescription, pTouTiao.ImageUrl, pTouTiao.Uri, pTouTiao.StrTime);

                pDatabase.Execute(strInsert);

                wcf.Result = true;
                wcf.Message = "气象科普上传成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream PostQixiangkepu_Update(Stream Update)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                string rootPath = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\APPTouTiaoImage\";
                StreamReader sr = new StreamReader(Update);
                string strJson = sr.ReadToEnd();
                sr.Close();


                pDatabase.Execute(strJson);

                wcf.Result = true;
                wcf.Message = "气象科普修改成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public AQI Test()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime dtQuery = DateTime.Now;
                if (DateTime.Now.Hour < 17)
                    dtQuery = dtQuery.AddDays(-1);
                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\AirQuality\" + dtQuery.ToString("yyyy") + "\\" + dtQuery.ToString("yyyyMMdd");
                string[] files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\AirQuality\" + dtQuery.AddDays(-1).ToString("yyyy") + "\\" + dtQuery.AddDays(-1).ToString("yyyyMMdd");
                    files = Directory.GetFiles(path);
                }
                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[2];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }

                //selectFile = @"C:\Users\Jinxh\Desktop\JsonService\JsonService\JsonService\TestFile\20160607\AQI_SH_201606071700.txt";
                StreamReader sr = new StreamReader(selectFile, Encoding.Default);

                string strLine = sr.ReadLine().Trim();
                AQI pAQI = new AQI();
                pAQI.Title = strLine;

                strLine = sr.ReadLine().Replace("发布", "").Replace("\t", " ").Trim(new char[] { '(', '）', ' ' });
                pAQI.PublisDate = strLine;
                sr.ReadLine();
                for (int i = 0; i < 5; i++)
                {
                    strLine = sr.ReadLine().Replace("\t", " ");

                    AQIData pAQIData = new AQIData();
                    string[] datas = Regex.Split(strLine, "\\s+");//"|");
                    pAQIData.Period = datas[0];
                    pAQIData.AQI = datas[1];
                    pAQIData.Level = datas[2];
                    pAQIData.Pripoll = datas[3];

                    pAQI.AQIDatas.Add(pAQIData);
                }

                //string strData = JsonConvert.SerializeObject(dtRes);

                wcf.Result = true;
                wcf.Data = pAQI;
                return pAQI;
                //return new MemoryStream(Encoding.Default.GetBytes(strData));
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            return null;
            //string res = JsonConvert.SerializeObject(wcf);
            //return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream PostAPPAlive(Stream data)
        {
            WCFResult wcf = new WCFResult(false); 
            try
            {
                Database pDatabase = new Database();
                StreamReader sr = new StreamReader(data);
                string strData= sr.ReadToEnd();
                sr.Close();

                AppAlive pAppAlive = JsonConvert.DeserializeObject<AppAlive>(strData);
                string strSQL = string.Format("INSERT INTO T_UserAlive (GUID,Datetime,Version,UserName,Platform) VALUES('{0}','{1}','{2}','{3}','{4}')"
                    , pAppAlive.GUID
                    , pAppAlive.Datetime
                    , pAppAlive.Version
                    , pAppAlive.UserName
                    , pAppAlive.Platform
                    );

               int res = pDatabase.Execute(strSQL);
               if (res > 0)
               {
                   wcf.Result = true;
                   wcf.Message = "上传成功！";
               }
               else
                   wcf.Message = "上传失败！";


            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string ress = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(ress));
        }

       
        public Stream GetTouTiao(string strStart, string strEnd)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                #region 序列化参数
                try
                {
                    dtStart = DateTime.ParseExact(strStart, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                #endregion
                string strSQL = string.Format("SELECT * FROM T_TOUTIAO WHERE (PUBLISHDATETIME BETWEEN '{0}' AND '{1}') ORDER BY PUBLISHDATETIME DESC", dtStart, dtEnd);

                DataTable dtData = pDatabase.GetDataTable(strSQL);
                List<News> listDisas = new List<News>();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    News pTouTiao = new News();
                    pTouTiao.Title = dr["Title"].ToString();
                    pTouTiao.ImageUrl = "http://61.152.126.152/SLPC/Product/APPTouTiaoImage/" + dr["ImageRelativePath"].ToString();
                    pTouTiao.SimpleDescription = dr["SimpleDescription"].ToString();
                    pTouTiao.Uri = dr["Uri"].ToString();
                    pTouTiao.StrTime = dr["PublishDateTime"].ToString();
                    listDisas.Add(pTouTiao);
                }
                string strData = JsonConvert.SerializeObject(listDisas);
                wcf.Result = true;
                wcf.Data = strData;
                //wcf.Message = "灾情上传成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetQXKP(string strStart, string strEnd)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                #region 序列化参数
                try
                {
                    dtStart = DateTime.ParseExact(strStart, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                #endregion
                string strSQL = string.Format("SELECT * FROM T_QXKP WHERE (PUBLISHDATETIME BETWEEN '{0}' AND '{1}') ORDER BY PUBLISHDATETIME DESC", dtStart, dtEnd);

                DataTable dtData = pDatabase.GetDataTable(strSQL);
                List<News> listDisas = new List<News>();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    News pTouTiao = new News();
                    pTouTiao.Title = dr["Title"].ToString();
                    pTouTiao.ImageUrl = "http://61.152.126.152/SLPC/Product/APPTouTiaoImage/" + dr["ImageRelativePath"].ToString();
                    pTouTiao.SimpleDescription = dr["SimpleDescription"].ToString();
                    pTouTiao.Uri = dr["Uri"].ToString();
                    pTouTiao.StrTime = dr["PublishDateTime"].ToString();
                    listDisas.Add(pTouTiao);
                }
                string strData = JsonConvert.SerializeObject(listDisas);
                wcf.Result = true;
                wcf.Data = strData;
                //wcf.Message = "灾情上传成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetYXYB(string strStart, string strEnd)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                #region 序列化参数
                try
                {
                    dtStart = DateTime.ParseExact(strStart, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                #endregion
                string strSQL = string.Format("SELECT * FROM T_YXYB WHERE (PUBLISHDATETIME BETWEEN '{0}' AND '{1}') ORDER BY PUBLISHDATETIME DESC", dtStart, dtEnd);

                DataTable dtData = pDatabase.GetDataTable(strSQL);
                List<News> listDisas = new List<News>();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    News pTouTiao = new News();
                    pTouTiao.Title = dr["Title"].ToString();
                    pTouTiao.ImageUrl = "http://61.152.126.152/SLPC/Product/YXYB/" + dr["ImageRelativePath"].ToString();
                    pTouTiao.SimpleDescription = dr["SimpleDescription"].ToString();
                    pTouTiao.Uri = "http://61.152.126.152/SLPC/Product/YXYB/" + dr["Uri"].ToString();
                    pTouTiao.StrTime = dr["PublishDateTime"].ToString();
                    listDisas.Add(pTouTiao);
                }
                string strData = JsonConvert.SerializeObject(listDisas);
                wcf.Result = true;
                wcf.Data = strData;
                //wcf.Message = "灾情上传成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetNumofTouTiao(string strTime, string number,string updown)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                #region 序列化参数
                DateTime dtTime = new DateTime();
                int dtNum;
                string UpOrDown="";
                try
                {
                    dtTime = DateTime.ParseExact(strTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                try
                {
                    dtNum = int.Parse(number);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("经度参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                try
                {
                    UpOrDown = string.Compare("up", updown, true) == 0 ? ">" : "<";
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                #endregion

                Database pDatabase = new Database();
                string strSQL = string.Format("SELECT TOP({0}) * FROM T_TOUTIAO WHERE PUBLISHDATETIME {1}'{2}' ORDER BY PUBLISHDATETIME DESC", dtNum, UpOrDown, dtTime);
                DataTable dtData = pDatabase.GetDataTable(strSQL);
                List<News> listDisas = new List<News>();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    News pTouTiao = new News();
                    pTouTiao.Title = dr["Title"].ToString();
                    pTouTiao.ImageUrl = "http://61.152.126.152/SLPC/Product/APPTouTiaoImage/" + dr["ImageRelativePath"].ToString();
                    pTouTiao.SimpleDescription = dr["SimpleDescription"].ToString();
                    pTouTiao.Uri = dr["Uri"].ToString();
                    pTouTiao.StrTime = dr["PublishDateTime"].ToString();
                    listDisas.Add(pTouTiao);
                }
                string strData = JsonConvert.SerializeObject(listDisas);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetNumofQixiangkepu(string strTime, string number, string updown)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                #region 序列化参数
                DateTime dtTime = new DateTime();
                int dtNum;
                string UpOrDown = "";
                try
                {
                    dtTime = DateTime.ParseExact(strTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                try
                {
                    dtNum = int.Parse(number);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("经度参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                try
                {
                    UpOrDown = string.Compare("up", updown, true) == 0 ? ">" : "<";
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                #endregion

                Database pDatabase = new Database();
                string strSQL = string.Format("SELECT TOP({0}) * FROM T_QXKP WHERE PUBLISHDATETIME {1}'{2}' ORDER BY PUBLISHDATETIME DESC", dtNum, UpOrDown, dtTime);
                DataTable dtData = pDatabase.GetDataTable(strSQL);
                List<News> listDisas = new List<News>();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    News pTouTiao = new News();
                    pTouTiao.Title = dr["Title"].ToString();
                    pTouTiao.ImageUrl = "http://61.152.126.152/SLPC/Product/APPTouTiaoImage/" + dr["ImageRelativePath"].ToString();
                    pTouTiao.SimpleDescription = dr["SimpleDescription"].ToString();
                    pTouTiao.Uri = dr["Uri"].ToString();
                    pTouTiao.StrTime = dr["PublishDateTime"].ToString();
                    listDisas.Add(pTouTiao);
                }
                string strData = JsonConvert.SerializeObject(listDisas);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetNumofYingxiangyubao(string strTime, string number, string updown)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                #region 序列化参数
                DateTime dtTime = new DateTime();
                int dtNum;
                string UpOrDown = "";
                try
                {
                    dtTime = DateTime.ParseExact(strTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                try
                {
                    dtNum = int.Parse(number);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("经度参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                try
                {
                    UpOrDown = string.Compare("up", updown, true) == 0 ? ">" : "<";
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                #endregion

                Database pDatabase = new Database();
                string strSQL = string.Format("SELECT TOP({0}) * FROM T_YXYB WHERE PUBLISHDATETIME {1}'{2}' ORDER BY PUBLISHDATETIME DESC", dtNum, UpOrDown, dtTime);
                DataTable dtData = pDatabase.GetDataTable(strSQL);
                List<News> listDisas = new List<News>();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    News pTouTiao = new News();
                    pTouTiao.Title = dr["Title"].ToString();
                    pTouTiao.ImageUrl = "http://61.152.126.152/SLPC/Product/YXYB/" + dr["ImageRelativePath"].ToString();
                    pTouTiao.SimpleDescription = dr["SimpleDescription"].ToString();
                    pTouTiao.Uri = "http://61.152.126.152/SLPC/Product/YXYB/" + dr["Uri"].ToString();
                    pTouTiao.StrTime = dr["PublishDateTime"].ToString();
                    listDisas.Add(pTouTiao);
                }
                string strData = JsonConvert.SerializeObject(listDisas);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream PostDisaster(Stream strImage)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                string rootPath = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\APPDisasterImage\";
                StreamReader sr = new StreamReader(strImage);
                string strJson = sr.ReadToEnd();
                sr.Close();

                //strJson = "{\"Base64Str\":\"\",\"Description\":\"gj\",\"Lat\":31.103822591538233,\"Lon\":121.22193887829779,\"OccurTime\":\"2016-04-07 11:40:27\",\"Tag\":\"hlg\",\"Type\":\"01\",\"UpLoadTime\":\"2016-04-07 11:40:27\"}";
                UpLoadDisaster pUpLoadDisaster = JsonConvert.DeserializeObject<UpLoadDisaster>(strJson);

                byte[] tmp = Convert.FromBase64String(pUpLoadDisaster.Base64Str);
                MemoryStream ms = new MemoryStream(tmp);
                ms.Write(tmp, 0, tmp.Length);
                string relativePath = pUpLoadDisaster.Type + "\\" + Guid.NewGuid().ToString("N") + ".jpg";
                Image pImage = Image.FromStream(ms);
                pImage.Save(rootPath + relativePath);

                pUpLoadDisaster.ImageUrl = relativePath.Replace("\\", "/");

                string strInsert = string.Format("INSERT INTO [APP_Risk_Data].[dbo].[T_UploadDisaster] (Occurtime,Type,UpTime,Lon,Lat,Description,ImageRelativePath,DamageDes,Tag)     VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')", pUpLoadDisaster.OccurTime.ToString("yyyy-MM-dd HH:mm:ss"), pUpLoadDisaster.Type, pUpLoadDisaster.UpLoadTime.ToString("yyyy-MM-dd HH:mm:ss"), pUpLoadDisaster.Lon, pUpLoadDisaster.Lat, pUpLoadDisaster.Description, pUpLoadDisaster.ImageUrl, pUpLoadDisaster.DamageDes,pUpLoadDisaster.Tag);

                pDatabase.Execute(strInsert);
                wcf.Result = true;
                wcf.Message = "灾情上传成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
            //return ms;
        }

        public Stream PostWeatherRemind(Stream strWeatherRemind)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                StreamReader sr = new StreamReader(strWeatherRemind);
                string strJson = sr.ReadToEnd();
                sr.Close();

                WeatherRemind pWeatherRemind = JsonConvert.DeserializeObject<WeatherRemind>(strJson);
                if (pWeatherRemind.Content == "")
                    throw new Exception("天气提醒正文不能为空，请检查。");
                if (pWeatherRemind.Period <=0)
                    throw new Exception("天气提醒时效不能小于等于零，请检查。");

                //string strSQL = string.Format("select * from [APP_Risk_Data].[dbo].[T_WeatherRemind] where [Content]='{0}' and PublishTime>='{1}'", pWeatherRemind.Content,DateTime.Now.AddMinutes(-15));
                //DataTable dtTmp = pDatabase.GetDataTable(strSQL);
                //if(dtTmp.Rows.Count>0)
                //    throw   new Exception("15分钟之内已经录入相同内容的数据，请不要重复录入。");
                //string strInsert = string.Format("INSERT INTO [APP_Risk_Data].[dbo].[T_WeatherRemind] ([Content], PublishTime,PERIOD)     VALUES ('{0}','{1}','{2}')", pWeatherRemind.Content, pWeatherRemind.StrPublishTime, pWeatherRemind.Period);

                //pDatabase.Execute(strInsert);
                pDatabase.Execute(pWeatherRemind.ToString());

                wcf.Result = true;
                wcf.Message = "提醒上传成功！";
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
            //return ms;
        }

        public Stream GetWeatherRemind()
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                string strSQL = string.Format("SELECT top 1 ID, CONTENT, PUBLISHTIME,PERIOD FROM T_WEATHERREMIND ORDER BY PUBLISHTIME DESC");
                DataTable dtRes = pDatabase.GetDataTable(strSQL);

                if (dtRes.Rows.Count == 0)
                    wcf.Message = "未查询到数据。";
                else
                {
                    DataRow dr = dtRes.Rows[0];
                    int period = int.Parse(dr["Period"].ToString());
                    DateTime dtPublish = DateTime.Parse(dr["PUBLISHTIME"].ToString());
                    if (dtPublish.AddMinutes(period) > DateTime.Now)
                    {

                        WeatherRemind pWeatherRemind = new WeatherRemind();
                        pWeatherRemind.Content = dtRes.Rows[0]["CONTENT"].ToString();
                        pWeatherRemind.StrPublishTime = dtRes.Rows[0]["PUBLISHTIME"].ToString();
                        pWeatherRemind.Period = period;

                        string strData = JsonConvert.SerializeObject(pWeatherRemind);

                        wcf.Result = true;
                        wcf.Data = strData;
                    }
                    else
                    {
                        wcf.Message = "最新提醒已经超出时效。";
                    }
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        
        }
        public Stream GetWeatherRemindByUserName(string userName)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database pDatabase = new Database();
                string strSQL = string.Format("SELECT top 1 ID, CONTENT, PUBLISHTIME,PERIOD FROM T_WEATHERREMIND where username ='{0}' ORDER BY PUBLISHTIME DESC", userName);
                DataTable dtRes = pDatabase.GetDataTable(strSQL);

                if (dtRes.Rows.Count == 0)
                    wcf.Message = "未查询到数据。";
                else
                {
                    DataRow dr = dtRes.Rows[0];
                    int period = int.Parse(dr["Period"].ToString());
                    DateTime dtPublish = DateTime.Parse(dr["PUBLISHTIME"].ToString());
                    if (dtPublish.AddMinutes(period) > DateTime.Now)
                    {

                        WeatherRemind pWeatherRemind = new WeatherRemind();
                        pWeatherRemind.Content = dtRes.Rows[0]["CONTENT"].ToString();
                        pWeatherRemind.StrPublishTime = dtRes.Rows[0]["PUBLISHTIME"].ToString();
                        pWeatherRemind.Period = period;
                        pWeatherRemind.Users = new string[] { userName };
                        string strData = JsonConvert.SerializeObject(pWeatherRemind);

                        wcf.Result = true;
                        wcf.Data = strData;
                    }
                    else
                    {
                        wcf.Message = "最新提醒已经超出时效。";
                    }
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));

        }
        public Stream GetWeatherRemindByTime(string startTime,string endTime)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                List<WeatherRemind> listData = new List<WeatherRemind>();
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();

                #region 序列化参数
                try
                {
                    dtStart = DateTime.ParseExact(startTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                try
                {
                    dtEnd = DateTime.ParseExact(endTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                #endregion
                Database pDatabase = new Database();
                string strSQL1 = "SELECT [UserName],[Alias] FROM [APP_Risk_Data].[dbo].[T_User]";
                DataTable dtDic = pDatabase.GetDataTable(strSQL1);
                Dictionary<string, string> dic_User = new Dictionary<string, string>();
                for (int m = 0; m < dtDic.Rows.Count; m++)
                {
                    DataRow dr = dtDic.Rows[m];
                    dic_User.Add(dr["UserName"].ToString(), dr["Alias"].ToString());
                }


                string strSQL = string.Format("SELECT ID, CONTENT, PUBLISHTIME,PERIOD,USERNAME FROM T_WEATHERREMIND WHERE PUBLISHTIME >='{0}' and PUBLISHTIME<='{1}' ORDER BY PUBLISHTIME DESC", dtStart,dtEnd);
                DataTable dtRes = pDatabase.GetDataTable(strSQL);

                if (dtRes.Rows.Count == 0)
                    wcf.Message = "未查询到数据。";
                else
                {
                    for (int i = 0; i < dtRes.Rows.Count; i++)
                    {
                        DataRow dr = dtRes.Rows[i];
                        int period = int.Parse(dr["PERIOD"].ToString());
                        DateTime dtPublish = DateTime.Parse(dr["PUBLISHTIME"].ToString());
                        string userName = dr["USERNAME"].ToString();

                        WeatherRemind pWeatherRemind = new WeatherRemind();
                        pWeatherRemind.Content = dr["CONTENT"].ToString();
                        pWeatherRemind.StrPublishTime = dtPublish.ToString();
                        pWeatherRemind.Period = period;
                        if (dic_User.Keys.Contains(userName))
                            pWeatherRemind.Users = new string[] { dic_User[userName] };
                        else
                            pWeatherRemind.Users = new string[] {"" };
                        listData.Add(pWeatherRemind);
                    }
                    string strData = JsonConvert.SerializeObject(listData);

                    wcf.Result = true;
                    wcf.Data = strData;
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));

        }
        public Stream GetThunderRemind(string tag)
        {

            WCFResult wcf = new WCFResult(false);
            try
            {
                //Dictionary<string, string[]> stationInfos = new Dictionary<string, string[]>();
                string[] TAGs = ConfigurationManager.AppSettings["stations"].ToString().Split(',');

                if (!TAGs.Contains(tag))
                {
                    throw new Exception("错误的用户。");
                }

                //for (int i = 0; i < TAGs.Length; i++)
                //{
                string[] stationInfo = ConfigurationManager.AppSettings[tag].ToString().Split(',');
                //stationInfos.Add(TAGs[i],stationInfo);
                //}


                //string stationTag =  stationInfo[0];
                string stationName = stationInfo[0];
                string stationLong = stationInfo[1];
                string stationLat = stationInfo[2];
                string stationDM = stationInfo[3];

                string strSelect = string.Format("SELECT TOP 1 NAME,FOLDER FROM CITYDISASTERFORECAST.DBO.[T_WRNZONES] WHERE STATION='{0}' ORDER BY FORECASTDATE DESC;", stationDM);
                DataTable dtPath = new Database().GetDataTable(strSelect);


                DateTime dtS = DateTime.Now;
                //DateTime dtS = DateTime.Parse("2017-07-18 14:42:00");

                ThunderRemind pThunderRemind = new ThunderRemind();
                //if (dtPath.Rows.Count > 0)
                if (false)
                {
                    #region 解析警戒区 [Polygon geom]
                    Diamond9 d9 = new Diamond9(string.Format(@"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\{0}\{1}", dtPath.Rows[0][1].ToString().Replace("/", "\\"), dtPath.Rows[0][0].ToString()));
                    FeatureSet fs = new FeatureSet(FeatureType.Polygon);

                    fs.DataTable.Columns.Add(new DataColumn("ID", typeof(int)));
                    fs.DataTable.Columns.Add(new DataColumn("Text", typeof(string)));

                    Polygon geom = new DotSpatial.Topology.Polygon(d9.Red);
                    IFeature feature = fs.AddFeature(geom);
                    feature.DataRow.BeginEdit();
                    feature.DataRow["ID"] = 1;
                    feature.DataRow["Text"] = "Red";
                    feature.DataRow.EndEdit();
                    #endregion

                    #region 查询闪电
                    string strSQL = "";
                    //strSQL = "SELECT  DATETIME,LON,LAT,PEAK_KA,(LON-121.208)*(LON-121.208)+(LAT-31.099)*(LAT-31.099) AS DIS FROM CityDisasterForecast.dbo.T_LIGHTNING WHERE (DATETIME >'2016-06-30 22:00:00' and DATETIME <'2016-07-30 22:00:00') and (lon between 121.1575 and 121.2625) and (lat  between 31.054 and 31.144) order by dis desc";
                    if (ConfigurationManager.AppSettings["LightningSource"].ToString() == "V")
                    {
                        strSQL = string.Format("SELECT DATETIME,LON,LAT,(LON-{5})*(LON-{5})+(LAT-{6})*(LAT-{6}) AS DIS  FROM CITYDISASTERFORECAST.DBO.T_LIGHTNING WHERE DATETIME >'{0}'AND (LON BETWEEN {1} AND {2}) AND (LAT  BETWEEN {3} AND {4}) order by dis", DateTime.Now.AddMinutes(-30), geom.Envelope.Minimum.X, geom.Envelope.Maximum.X, geom.Envelope.Minimum.Y, geom.Envelope.Maximum.Y, stationLong, stationLat);

                        //strSQL = string.Format("SELECT DATETIME,LON,LAT,(LON-{5})*(LON-{5})+(LAT-{6})*(LAT-{6}) AS DIS  FROM CITYDISASTERFORECAST.DBO.T_LIGHTNING WHERE (DATETIME >'{0}' and datetime <'2016-07-30 00:00:00')AND (LON BETWEEN {1} AND {2}) AND (LAT  BETWEEN {3} AND {4}) order by dis", "2016-06-30 00:00:00", geom.Envelope.Minimum.X, geom.Envelope.Maximum.X, geom.Envelope.Minimum.Y, geom.Envelope.Maximum.Y, stationLong, stationLat);

                        pThunderRemind.DataSource = "Vasaila";
                    }
                    else
                    {
                        strSQL = string.Format("SELECT  LIGHTNING_TIME AS DATETIME,LONGITUDE AS LON,LATITUDE AS LAT,(LONGITUDE-{5})*(LONGITUDE-{5})+(LATITUDE-{6})*(LATITUDE-{6}) AS DIS FROM CITYDISASTERFORECAST.DBO.T_LTGFLASHPORTIONS WHERE LIGHTNING_TIME >'{0}' AND STROKE_TYPE=0 AND (LONGITUDE BETWEEN {1} AND {2}) AND (LATITUDE  BETWEEN {3} AND {4}) order by dis", DateTime.Now.AddMinutes(-30), geom.Envelope.Minimum.X, geom.Envelope.Maximum.X, geom.Envelope.Minimum.Y, geom.Envelope.Maximum.Y, stationLong, stationLat);

                        pThunderRemind.DataSource = "WeatherBug";
                    }
                    DataTable dtThunder = new Database().GetDataTable(strSQL);
                    #endregion

                    double dis = double.NaN;
                    #region 遍历闪电，计算最近距离
                    bool isCalWRNZones = false;
                    for (int i = 0; i < dtThunder.Rows.Count; i++)
                    {
                        DataRow dr = dtThunder.Rows[i];
                        Coordinate cc = new Coordinate(double.Parse(dr["LON"].ToString()), double.Parse(dr["LAT"].ToString()));
                        var pt = new Feature(cc);
                        if (feature.Contains(pt))
                        {
                            LamProjection lpj = new LamProjection(new VectorPointXY(121, 32), 30, 60, GeoRefEllipsoid.WGS_84);
                            VectorPointXY xy0 = lpj.GeoToProj(new VectorPointXY(double.Parse(stationLong), double.Parse(stationLat)));
                            VectorPointXY xy = lpj.GeoToProj(new VectorPointXY(cc.X, cc.Y));

                            dis = Math.Round(0.001 * Math.Sqrt((xy.X - xy0.X) * (xy.X - xy0.X) + (xy.Y - xy0.Y) * (xy.Y - xy0.Y)), 2);
                            DateTime dtTd = DateTime.Parse(dr["DATETIME"].ToString());
                            pThunderRemind.Message = string.Format("{0}发生在距离{1}地区{2}Km的闪电活动，可能将影响本地。。", dtTd.ToString("HH时mm分"), stationName, dis.ToString());
                            //pThunderRemind.Message = string.Format("30分钟内，距离{0}最近的闪电距离为{1}千米。", stationName, dis.ToString());
                            isCalWRNZones = true;
                            break;
                        }

                    }
                    if (!isCalWRNZones)
                        pThunderRemind.Message = string.Format("当前无闪电影响{0}地区。", stationName, dis.ToString());
                    #endregion


                }
                else
                {
                    #region 无警戒区

                    string strSQL = "";
                    if (ConfigurationManager.AppSettings["LightningSource"].ToString() == "V")
                    {
                        strSQL = string.Format("SELECT TOP 1 DATETIME,LON,LAT,PEAK_KA,(LON-{0})*(LON-{0})+(LAT-{1})*(LAT-{1}) AS DIS  FROM CityDisasterForecast.dbo.T_LIGHTNING WHERE DATETIME >'{2}' and DATETIME <'{3}'  ORDER BY DIS", stationLong, stationLat, dtS.AddMinutes(-30), dtS);
                        //strSQL = string.Format("SELECT TOP 1 DATETIME,LON,LAT,PEAK_KA,(LON-{0})*(LON-{0})+(LAT-{1})*(LAT-{1}) AS DIS  FROM CityDisasterForecast.dbo.T_LIGHTNING WHERE DATETIME >'{2}' AND DATETIME <'{3}' ORDER BY DIS", stationLong, stationLat, DateTime.Parse("2016-07-05 17:30:00"), DateTime.Parse("2016-07-05 18:30:00"));
                        pThunderRemind.DataSource = "Vasaila";
                    }
                    else
                    {
                        strSQL = string.Format("SELECT TOP 1 LIGHTNING_TIME,LONGITUDE AS LON,LATITUDE AS LAT,AMPLITUDE,(LONGITUDE-{0})*(LONGITUDE-{0})+(LATITUDE-{1})*(LATITUDE-{1}) AS DIS  FROM CityDisasterForecast.dbo.T_LTGFLASHPORTIONS WHERE LIGHTNING_TIME >'{2}'AND LIGHTNING_TIME <'{3}'AND STROKE_TYPE=0 ORDER BY DIS", stationLong, stationLat, dtS.AddHours(-8).AddMinutes(-30), dtS.AddHours(-8));

                        //DateTime dtTest = DateTime.Parse("2016-06-20 06:00:00").AddHours(-8);
                        //strSQL = string.Format("SELECT TOP 1 LIGHTNING_TIME,LONGITUDE AS LON,LATITUDE AS LAT,AMPLITUDE,(LONGITUDE-{0})*(LONGITUDE-{0})+(LATITUDE-{1})*(LATITUDE-{1}) AS DIS  FROM CityDisasterForecast.dbo.T_LTGFLASHPORTIONS WHERE LIGHTNING_TIME >='{2}' and LIGHTNING_TIME <='{3}'AND STROKE_TYPE=0 ORDER BY DIS", stationLong, stationLat, dtTest.AddMinutes(-30), dtTest);
                        pThunderRemind.DataSource = "WeatherBug";
                    }
                    DataTable dtData = new Database().GetDataTable(strSQL);
                    double dis = double.NaN;
                    string strMessage = "";
                    if (dtData.Rows.Count != 0)
                    //throw new Exception ("30分钟内未查询到闪电。");
                    {
                        LamProjection lpj = new LamProjection(new VectorPointXY(121, 32), 30, 60, GeoRefEllipsoid.WGS_84);
                        VectorPointXY xy0 = lpj.GeoToProj(new VectorPointXY(double.Parse(stationLong), double.Parse(stationLat)));
                        VectorPointXY xy = lpj.GeoToProj(new VectorPointXY(double.Parse(dtData.Rows[0]["LON"].ToString()), double.Parse(dtData.Rows[0]["LAT"].ToString())));

                        dis = Math.Round(0.001 * Math.Sqrt((xy.X - xy0.X) * (xy.X - xy0.X) + (xy.Y - xy0.Y) * (xy.Y - xy0.Y)), 2);
                    }
                    else
                        dis = double.NaN;
                    if (dis <= 50)
                        strMessage = string.Format("30分钟内，距离{0}最近的闪电距离为{1}千米。", stationName, dis.ToString());
                    else
                        strMessage = string.Format("30分钟内，{0}50公里内无闪电发生。", stationName, dis.ToString());

                    //strMessage = string.Format("30分钟内，距离{0}最近的闪电距离为9.4千米。", stationName);

                    pThunderRemind.Message = strMessage;
                    #endregion
                }
                pThunderRemind.PublishTime = DateTime.Now.ToString();
                pThunderRemind.InvalidTime = DateTime.Now.AddMinutes(5).ToString();

                string strData = JsonConvert.SerializeObject(pThunderRemind);

                wcf.Result = true;
                wcf.Data = strData;
                TimeSpan tsss = dtS - DateTime.Now;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));

        }
        public Stream GetDisaster(string userName,string strStart, string strEnd)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database m_Database = new Database();
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                #region 序列化时间
                try
                {
                    dtStart = DateTime.ParseExact(strStart, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "dtEnd", ex);

                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;

                    //return message;
                    throw aex;
                    //return new MemoryStream(Encoding.UTF8.GetBytes(message));
                }
                #endregion

                DataTable dtDic = m_Database.GetDataTable("select * from D_UPLOADDISASTER_Type");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                for (int i = 0; i < dtDic.Rows.Count; i++)
                {
                    string key = dtDic.Rows[i]["DM"].ToString();
                    string value = dtDic.Rows[i]["MC"].ToString();
                    dic.Add(key, value);
                }

                string strSQL = string.Format("SELECT * FROM T_UPLOADDISASTER WHERE (OCCURTIME BETWEEN '{0}' AND '{1}') and Tag='{2}'", dtStart, dtEnd,userName);

                DataTable dtData = m_Database.GetDataTable(strSQL);
                List<UpLoadDisaster> listDisas = new List<UpLoadDisaster>();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    UpLoadDisaster pUpLoadDisaster = new UpLoadDisaster();
                    pUpLoadDisaster.Lon = double.Parse(dr["Lon"].ToString());
                    pUpLoadDisaster.Lat = double.Parse(dr["Lat"].ToString());

                    pUpLoadDisaster.OccurTime = DateTime.Parse(dr["OccurTime"].ToString());
                    pUpLoadDisaster.UpLoadTime = DateTime.Parse(dr["UpTime"].ToString());

                    string strType = dr["Type"].ToString();
                    pUpLoadDisaster.Type = strType;

                    pUpLoadDisaster.Type_zhCN = dic[strType];
                    pUpLoadDisaster.Description = dr["Description"].ToString();
                    pUpLoadDisaster.ImageUrl = "http://61.152.126.152/SLPC/Product/APPDisasterImage/" + dr["ImageRelativePath"].ToString();
                    pUpLoadDisaster.DamageDes = dr["DamageDes"].ToString();

                    listDisas.Add(pUpLoadDisaster);
                }
                string strData = JsonConvert.SerializeObject(listDisas);

                wcf.Result = true;
                wcf.Data = strData;
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }

            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetAvailableCommunityList()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                StreamReader srList = new StreamReader(@"F:\ReadearthCode\JSONService\JsonService\geometry\list.csv", Encoding.Default);
                string strLine = "";
                while ((strLine = srList.ReadLine()) != null)
                {
                    string[] ss = strLine.Split(new char[] { ',' });
                    dic.Add(ss[0], ss[1]);
                }
                srList.Close();
                srList.Dispose();

                string[] key = dic.Keys.ToArray();
                wcf.Data = key;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetGeometry(string userName)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                StreamReader srList = new StreamReader(@"F:\ReadearthCode\JSONService\JsonService\geometry\list.csv",Encoding.Default);
                string strLine = "";
                while ((strLine = srList.ReadLine()) != null)
                {
                    string[] ss = strLine.Split(new char[] { ',' });
                    dic.Add(ss[0], ss[1]);
                }
                srList.Close();
                srList.Dispose();

                Database db = new Database();
                string strSQL = string.Format("SELECT ALIAS FROM T_User where userName ='{0}'", userName);
                DataTable dtUser = db.GetDataTable(strSQL);

                StreamReader sr = new StreamReader(new System.IO.FileStream(@"F:\ReadearthCode\JSONService\JsonService\geometry\" + dic[dtUser.Rows[0][0].ToString()], FileMode.Open, FileAccess.Read));
                
                string strJson = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();

                wcf.Result = true;
                wcf.Data = strJson;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public class Geometry 
        {
            public string FID { get; set; }
            public List<List<double[]>> Points { get; set; }
            public Extent Extent { get; set; }
            public List<Geometry1> SubGeometries { get; set; }

        }
        public class Geometry1
        {
            public string FID { get; set; }
            public List<List<double[]>> Points { get; set; }
            public Extent Extent { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }
        public class Extent
        {
            public double left, right, top, bottom;        
        }
        public Stream GetGeometry_V2(string userName)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                StreamReader srList = new StreamReader(@"F:\ReadearthCode\JSONService\JsonService\geometry\list.csv", Encoding.Default);
                string strLine = "";
                while ((strLine = srList.ReadLine()) != null)
                {
                    string[] ss = strLine.Split(new char[] { ',' });
                    dic.Add(ss[0], ss[1]);
                }
                srList.Close();
                srList.Dispose();

                Database db = new Database();
                string strSQL = string.Format("SELECT ALIAS FROM T_User where userName ='{0}'", userName);
                DataTable dtUser = db.GetDataTable(strSQL);

                StreamReader sr = new StreamReader(new System.IO.FileStream(@"F:\ReadearthCode\JSONService\JsonService\geometry\" + dic[dtUser.Rows[0][0].ToString()], FileMode.Open, FileAccess.Read));

                string strJson = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();

                Geometry pGeo = JsonConvert.DeserializeObject<Geometry>(strJson);

                if (userName == "ypq")
                {
                    StreamReader srTmp = new StreamReader(new System.IO.FileStream(@"F:\ReadearthCode\JSONService\JsonService\geometry\PSQK_YP.txt", FileMode.Open, FileAccess.Read));

                    string strTmp = srTmp.ReadToEnd();
                    sr.Close();
                    sr.Dispose();
                    List<Geometry1> pGeos = JsonConvert.DeserializeObject<List<Geometry1>>(strTmp);
                    pGeo.SubGeometries = pGeos;
                }
                //List<Geometry> listGeos = new List<Geometry>();
                //listGeos.Add(pGeo);
                wcf.Result = true;
                //wcf.Data = listGeos;
                wcf.Data = JsonConvert.SerializeObject(pGeo);
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetGeometry_PSQK_YP()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                StreamReader sr = new StreamReader(new System.IO.FileStream(@"F:\ReadearthCode\JSONService\JsonService\geometry\PSQK_YP.txt", FileMode.Open, FileAccess.Read));

                string strJson = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();

                wcf.Result = true;
                wcf.Data = strJson;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetRTAutoStationData(string userName)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                DataTable dtRes = new DataTable();
                dtRes.Columns.Add(new DataColumn("Time", typeof(string)));//时间
                dtRes.Columns.Add(new DataColumn("Weather", typeof(string)));//天气现象
                dtRes.Columns.Add(new DataColumn("Tmp", typeof(string)));//温度
                dtRes.Columns.Add(new DataColumn("Wind", typeof(string)));//风向
                dtRes.Columns.Add(new DataColumn("WindLev", typeof(string)));//风速
                dtRes.Columns.Add(new DataColumn("Rain", typeof(string)));//雨量
                DataRow drNew = dtRes.NewRow();

                #region 雨量
                Database db = new Database();

                string stationID = db.GetDataTable("SELECT * FROM T_USER WHERE USERNAME ='" + userName + "'").Rows[0]["AutoStationID"].ToString();

                DataTable dtData = db.GetDataTable(string.Format("SELECT top (1) *  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE STATIONID ='{0}' ORDER BY  DATETIME DESC", stationID));
                //string strData = JsonConvert.SerializeObject(dtData);

                drNew["Time"] = dtData.Rows[0]["Datetime"].ToString();
                drNew["Tmp"] = dtData.Rows[0]["Temperature"].ToString();
                drNew["WindLev"] =GetWindLev(double.Parse( dtData.Rows[0]["WindSpeedGust"].ToString()));
                drNew["Rain"] = dtData.Rows[0]["RainHour"].ToString();
                #endregion

                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\ZXSYB\" + DateTime.Now.ToString("yyyy") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                string[] files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    path = path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\ZXSYB\" + DateTime.Now.AddDays(-1).ToString("yyyy") + "\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    files = Directory.GetFiles(path);
                }
                string selectFile = "";


                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[2];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }

                //smc_shdljxhtqyb_201603311300.txt
                string strTime = new FileInfo(selectFile).Name.Split(new char[] { '_', '.' })[2];
                DateTime dtFore = DateTime.ParseExact(strTime, "yyyyMMddHHmm", null);
                TimeSpan ts = DateTime.Now - dtFore;
               
                StreamReader sr = new StreamReader(selectFile, Encoding.Default);
                string strLine;
                for (int i = 0; i < ts.Hours; i++)
                {
                    strLine = sr.ReadLine();
                }
                while ((strLine = sr.ReadLine()) != null)
                {
                    string[] datas = strLine.Split('|');//"|");

                    drNew["Weather"] = datas[1];
                    drNew["Wind"] = datas[7];

                    dtRes.Rows.Add(drNew);
                    dtRes.AcceptChanges();
                    break;
                }

                wcf.Result = true;
                string strData = JsonConvert.SerializeObject(dtRes);
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream  GetLightning(string strStart, string strEnd)
        {

            //strStart = "20160620040000";
            //strEnd = "20160620060000";
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database m_Database = new Database();
                #region 序列化时间
                DateTime dtStartTime = new DateTime();
                DateTime dtEndTime = new DateTime();
                try
                {
                    dtStartTime = DateTime.ParseExact(strStart, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                try
                {
                    dtEndTime = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }

                double interval = (dtEndTime - dtStartTime).TotalSeconds / 20;

                #endregion

                #region 李飞 2016-03-28 09:00:00
                //闪电字典
                List<Lightning[]> list_Lighting = new List<Lightning[]>();

                #region 初始化分段闪电列表
                List<Lightning> list = new List<Lightning>();
                #endregion

                #region 闪电分段计算
                string selectSQL = "";
                string strMsg = "";
                if (ConfigurationManager.AppSettings["LightningSource"].ToString() == "V")
                {
                    selectSQL = string.Format("SELECT LON,LAT,DATETIME FROM CITYDISASTERFORECAST.DBO.T_LIGHTNING WHERE DATETIME BETWEEN '{0}' AND '{1}' ORDER BY DATETIME ASC", dtEndTime.AddHours(-2), dtEndTime);

                    strMsg = "Vaisala数据源";
                }
                else
                {
                    selectSQL = string.Format("SELECT LONGITUDE AS LON,LATITUDE AS LAT,DATEADD(HH,8,LIGHTNING_TIME) AS DATETIME FROM CITYDISASTERFORECAST.DBO.[T_LTGFLASHPORTIONS] WHERE DATEADD(HH,8,LIGHTNING_TIME) BETWEEN '{0}' AND '{1}'  and Stroke_Type ='0' ORDER BY DATETIME ASC", dtEndTime.AddHours(-2), dtEndTime);

                    strMsg = "WeatherBug数据源";
                }
                DataTable dtData = m_Database.GetDataTable(selectSQL);
                if (dtData.Rows.Count == 0)
                    wcf.Message = "当前时间段，未查询到闪电数据。";
                else
                {
                    for (int mm = 0; mm < 20; mm++)
                    {
                        #region 计算分段时间节点
                        DateTime dtTime_tmp1 = dtStartTime.AddSeconds(interval*mm);
                        DateTime dtTime_tmp2 = dtStartTime.AddSeconds(interval * (mm+1));
                        #endregion
                        for (int i = 0; i < dtData.Rows.Count; i++)
                        {
                            DataRow dr = dtData.Rows[i];
                            double dLon = double.Parse(dr["Lon"].ToString());
                            double dLat = double.Parse(dr["Lat"].ToString());
                            DateTime tDatetime = DateTime.Parse(dr["Datetime"].ToString());
                            Lightning sLightning = new Lightning(dLon, dLat, tDatetime);
                            if (sLightning.Datetime >= dtTime_tmp1 && sLightning.Datetime <= dtTime_tmp2)
                                list.Add(sLightning);
                        }

                        Lightning[] tmp1 = new Lightning[list.Count];
                        list.CopyTo(tmp1);
                        list_Lighting.Add(tmp1);
                        list.Clear();
                    }
                }
                #endregion 
                #endregion

                string strData = JsonConvert.SerializeObject(list_Lighting);
                wcf.Result = true;
                wcf.Message = strMsg;
                wcf.Data = strData;
            }
            catch(Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
            //return res;
        }

        public Stream Get_2H_Lightning(string strEnd)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                //strEnd = "20160620060000";
                #region 序列化时间
                DateTime dtEndTime = new DateTime();
                try
                {
                    dtEndTime = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }

                #endregion
                
                #region 李飞 2016-03-28 09:00:00
                #region 计算分段时间节点

                Database m_Database = new Database();
                //DateTime dtTime_tmp = dtEndTime.AddHours(-2).AddMinutes(6);
                #endregion

                //闪电字典
                List<Lightning[]> list_Lighting = new List<Lightning[]>();

                

                #region 闪电分段计算
                string selectSQL = "";
                string strMsg = "";
                if (ConfigurationManager.AppSettings["LightningSource"].ToString() == "V")
                {
                    selectSQL = string.Format("SELECT LON,LAT,DATETIME FROM CITYDISASTERFORECAST.DBO.T_LIGHTNING WHERE DATETIME BETWEEN '{0}' AND '{1}' ORDER BY DATETIME ASC", dtEndTime.AddHours(-2), dtEndTime);

                    strMsg = "Vaisala数据源";
                }
                else
                {
                    selectSQL = string.Format("SELECT LONGITUDE AS LON,LATITUDE AS LAT,DATEADD(HH,8,LIGHTNING_TIME) AS DATETIME FROM CITYDISASTERFORECAST.DBO.[T_LTGFLASHPORTIONS] WHERE DATEADD(HH,8,LIGHTNING_TIME) BETWEEN '{0}' AND '{1}'   and Stroke_Type ='0'  ORDER BY DATETIME ASC", dtEndTime.AddHours(-2), dtEndTime);

                    strMsg = "WeatherBug数据源";
                }
                DataTable dtData = m_Database.GetDataTable(selectSQL);
                if (dtData.Rows.Count == 0)
                    wcf.Message = "当前时间段，未查询到闪电数据。";
                else
                {
                    for (int i = 0; i < 20; i++)
                    {
                        #region 初始化分段闪电列表
                        List<Lightning> list = new List<Lightning>();
                        #endregion

                        DateTime dts = dtEndTime.AddHours(-2).AddMinutes(6*i);
                        DataRow[] dtSelected = dtData.Select(string.Format("DATETIME >'{0}' AND DATETIME <'{1}'", dts, dts.AddMinutes(6)));
                        for (int j = 0; j < dtSelected.Length; j++)
                        {
                            DataRow dr = dtSelected[j];
                            double dLon = double.Parse(dr["Lon"].ToString());
                            double dLat = double.Parse(dr["Lat"].ToString());
                            DateTime tDatetime = DateTime.Parse(dr["Datetime"].ToString());
                            Lightning sLightning = new Lightning(dLon, dLat, tDatetime);
                            list.Add(sLightning);
                        }
                        Lightning[] tmp = new Lightning[list.Count];
                        list.CopyTo(tmp);
                        list_Lighting.Add(tmp);
                        list.Clear();

                    }
                }
                #endregion
                #endregion

                string strData = JsonConvert.SerializeObject(list_Lighting);
                wcf.Result = true;
                wcf.Message = strMsg;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
            //return res;
        }
        public Stream Get_1H_Lightning(string EndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                //EndTime = "20160620060000";
                #region 序列化时间
                DateTime dtEndTime = new DateTime();
                DateTime dtStartTime = new DateTime();
                try
                {
                    dtEndTime = DateTime.ParseExact(EndTime, "yyyyMMddHHmmss", null);
                    dtStartTime = dtEndTime.AddHours(-1);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }

                #endregion

                #region 李飞 2016-03-28 09:00:00
                #region 计算分段时间节点

                Database m_Database = new Database();
                
                #endregion

                #region 初始化分段闪电列表
                List<Lightning> list = new List<Lightning>();



                string selectSQL = string.Format("SELECT DATETIME,LON,LAT FROM CITYDISASTERFORECAST.DBO.T_LIGHTNING WHERE (LON BETWEEN '120.5' AND '122.25') AND (LAT BETWEEN '30.5' AND '32.25') AND  DATETIME BETWEEN '{0}' AND '{1}' ORDER BY DATETIME ASC", dtStartTime, dtEndTime);
                DataTable dtData = m_Database.GetDataTable(selectSQL);
                if (dtData.Rows.Count == 0)
                    wcf.Message = "当前时间段，未查询到闪电数据。";
                else
                {
                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        DataRow dr = dtData.Rows[i];
                        double dLon = double.Parse(dr["Lon"].ToString());
                        double dLat = double.Parse(dr["Lat"].ToString());
                        DateTime tDatetime = DateTime.Parse(dr["Datetime"].ToString());
                        Lightning sLightning = new Lightning(dLon, dLat, tDatetime);
                        
                        list.Add(sLightning);
                    }                   
                }
                #endregion
                #endregion

                //string strData = JsonConvert.SerializeObject(list);
                wcf.Result = true;
                wcf.Data = list;
                //wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
            //return res;
        }
        public Stream GetQPF(string lon, string lat)
        {
            string ctrFile = "";
            WCFResult wcf = new WCFResult(false);
            try
            {
                DateTime dt1111 = DateTime.Now;
                #region 序列化经纬度
                double dLon = double.NaN;
                try
                {
                    dLon = double.Parse(lon);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("经度参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                double dLat = double.NaN;
                try
                {
                    dLat = double.Parse(lat);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("纬度参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                #endregion
                string strPath = @"F:\QPF_Files\";
                //string strPath = "http://61.152.126.152/QPF/Nowcast/TREC/QPF_D4";
                DateTime dtNow = DateTime.Now;
                //DateTime dtNow_UTC = dtNow.AddHours(-8);
                //DateTime dtTmp = new DateTime();
                string pattern = dtNow.ToString("yyyyMMddHH") + "*.000";
                //pattern = "201702080929.000";
                string[] files = System.IO.Directory.GetFiles(strPath, pattern);
                if (files.Length == 0)
                {
                    pattern = dtNow.AddHours(-1).ToString("yyyyMMddHH") + "*.000";
                    files = System.IO.Directory.GetFiles(strPath, pattern);
                }

                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);
                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[0];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("DateTime",typeof(string)));
                dt.Columns.Add(new DataColumn("QPF",typeof(double)));
                for (int i = 0; i <=120; i+=6)
                {
                    string d4File = strPath + "\\" + dtForecast.ToString("yyyyMMddHHmm") + "." + i.ToString("000");
                    ctrFile = d4File;
                    SHFL.Diamond4 pDiamond4 = new SHFL.Diamond4(d4File);
                    string message=pDiamond4.ReadData(dLon, dLat);

                    if (Regex.Split(message, "-")[0] != "")
                    {
                        wcf.Message = Regex.Split(message, "-")[0];
                    }
                    else
                    {
                        double qpf = double.Parse(Regex.Split(message, "-")[1]);
                        DataRow drNew = dt.NewRow();
                        drNew["DateTime"] = dtForecast.AddMinutes(i).ToString("yyyy/MM/dd HH:mm:ss");
                        drNew["QPF"] = qpf;
                        dt.Rows.Add(drNew);
                        dt.AcceptChanges();
                    }
                }

                string strData = JsonConvert.SerializeObject(dt);
                wcf.Result = true;
                wcf.Data = strData;

                TimeSpan ts1 = DateTime.Now - dt1111;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetQPFByUserName(string userName)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Database db = new Database();
                string strSQL = string.Format("SELECT Alias  FROM [APP_Risk_Data].[dbo].[T_User] where userName='{0}'", userName);
                DataTable dtUserObjectName = db.GetDataTable(strSQL);
                if (dtUserObjectName.Rows.Count == 0)
                    throw new Exception("错误的用户名。。");
                else
                {
                    string strAlias = dtUserObjectName.Rows[0][0].ToString();

                    string strForecastTime = string.Format("SELECT TOP 1 [jdName],[ForecastTimeBJ],[Period],[QPFValue]  FROM [CityDisasterForecast].[dbo].[T_QPF_JDDATA]  where jdname='{0}'  order by [ForecastTimeBJ] desc ", strAlias);

                    DateTime dtTime = DateTime.Parse(db.GetDataTable(strForecastTime).Rows[0]["ForecastTimeBJ"].ToString());
                    if (dtTime > DateTime.Now.AddMinutes(-60))
                    {
                        strSQL = string.Format("SELECT [jdName],[ForecastTimeBJ],[Period],[QPFValue]  FROM [CityDisasterForecast].[dbo].[T_QPF_JDDATA]  where jdname='{0}' and ForecastTimeBJ='{1}'  order by [ForecastTimeBJ] desc ,period asc", strAlias, dtTime);

                        DataTable dtQPF = db.GetDataTable(strSQL);

                        DataTable dt = new DataTable();
                        dt.Columns.Add(new DataColumn("DateTime", typeof(string)));
                        dt.Columns.Add(new DataColumn("QPF", typeof(double)));

                        for (int i = 0; i < dtQPF.Rows.Count; i++)
                        {
                            DataRow drNew = dt.NewRow();
                            drNew["DateTime"] = dtTime.AddMinutes(int.Parse(dtQPF.Rows[i]["Period"].ToString())).ToString("yyyy/MM/dd HH:mm:ss");
                            drNew["QPF"] = double.Parse(dtQPF.Rows[i]["QPFValue"].ToString());

                            dt.Rows.Add(drNew);
                            dt.AcceptChanges();
                        }
                        string strData = JsonConvert.SerializeObject(dt);
                        wcf.Result = true;
                        wcf.Data = strData;
                    }
                    else
                        throw new Exception("无数据。");
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetDisasterHistory(string strStart, string strEnd)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                #region 序列化时间
                DateTime dtStartTime = new DateTime();
                DateTime dtEndTime = new DateTime();
                try
                {
                    dtStartTime = DateTime.ParseExact(strStart, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                try
                {
                    dtEndTime = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strStart", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                #endregion

                Database m_Database = new Database();
                string selectSQL = string.Format("SELECT CONVERT(VARCHAR(20),Alarm_DateTime,120),Alarm_Name,Alarm_Telphone,Alarm_Address,Longitude,Latitude,Disaster_Adrress,Disaster_Description,Disaster_Accepter,Disaster_Code,CONVERT(VARCHAR(20),Disaster_Datetime,120),CONVERT(VARCHAR(20),DB_Datetime,120),Disaster_District FROM CITYDISASTERFORECAST.DBO.T_DisasterHistory WHERE Alarm_DateTime BETWEEN '{0}' AND '{1}' ORDER BY Alarm_DateTime DESC", dtStartTime, dtEndTime);
                DataTable dtData = m_Database.GetDataTable(selectSQL);
                dtData.Columns["Column1"].ColumnName = "Alarm_DateTime";
                dtData.Columns["Column2"].ColumnName = "Disaster_Datetime";
                dtData.Columns["Column3"].ColumnName = "DB_Datetime";
                if (dtData.Rows.Count == 0)
                    wcf.Message = "当前时间段，未查询到灾害数据。";
                string strData = JsonConvert.SerializeObject(dtData);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch(Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream GetMinDisaster(string strEnd, string AddTimes)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                #region 序列化时间
                DateTime dtEndTime = new DateTime();
                try
                {
                    dtEndTime = DateTime.ParseExact(strEnd, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("时间参数错误。", "strEnd", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                double AddMinuter;
                try
                {
                    AddMinuter = double.Parse(AddTimes);
                }
                catch (Exception ex)
                {
                    ArgumentException aex = new ArgumentException("分钟参数错误。", "AddMinuter", ex);
                    string message = aex.Message + Environment.NewLine
                        + "调试信息：" + aex.StackTrace + Environment.NewLine
                        + aex.InnerException.Message + Environment.NewLine
                        + aex.InnerException.StackTrace;
                    throw aex;
                }
                DateTime dtStartTime = dtEndTime.AddMinutes(-AddMinuter);
                #endregion

                Database m_Database = new Database();
                string selectSQL = string.Format("SELECT CONVERT(VARCHAR(20),DateTime,120),Lon,Lat,PEAK_KA,District,SpecialRegion,C_100m,C_500m,C_1000m FROM CITYDISASTERFORECAST.DBO.T_Lightning WHERE DateTime BETWEEN '{0}' AND '{1}' ORDER BY DateTime DESC", dtStartTime, dtEndTime);
                DataTable dtData = m_Database.GetDataTable(selectSQL);
                dtData.Columns["Column1"].ColumnName = "DateTime";
                if (dtData.Rows.Count == 0)
                    wcf.Message = "当前时间段，未查询到闪电数据。";
                string strData = JsonConvert.SerializeObject(dtData);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetHealthyMeteorological()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                WebClient wc = new WebClient();
                Stream s = wc.OpenRead("http://222.66.83.21:808/ScreenDisplay/HealthWeather2/webservice/Publish.asmx/GetCrows?authCode=shjkqxyb");
                StreamReader sr = new StreamReader(s);
                XmlTextReader xmltr = new XmlTextReader(sr);

                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(xmltr);

                DataTable dt = ds.Tables[0];
                DataView dv = dt.DefaultView;
                dv.Sort = "Crow";
                dt = dv.ToTable();

                List<HealthyMeteorological> data = new List<HealthyMeteorological>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    
                    string strCrow = dr["Crow"].ToString();

                    Detail d = new Detail();
                    d.Date = dr["Date"].ToString();
                    d.WarningLevel = dr["WarningLevel"].ToString();
                    d.WarningDesc = dr["WarningDesc"].ToString();
                    d.Influ = dr["Influ"].ToString();
                    d.Wat_guide = dr["Wat_guide"].ToString();
                    d.Guide = dr["Guide"].ToString();


                    HealthyMeteorological hm = null;
                    for (int j = 0; j < data.Count; j++)
                    {
                        if (data[j].Crow == strCrow)
                        {
                            hm = data[j];
                            hm.Deatails.Add(d);
                            break;
                        }
                    }
                    if (hm == null)
                    {
                        hm = new HealthyMeteorological();

                        hm.Crow = strCrow;
                        hm.Deatails.Add(d);

                        data.Add(hm);
                    }
                   
                   
                }

                //string strData = JsonConvert.SerializeObject(data);
                //wcf.Data = strData;
                wcf.Data = data;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetEditionURL()
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                Dictionary<string, string> EditionURL = new Dictionary<string, string>();
                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\EditionURL\app.txt";

                StreamReader sr = new StreamReader(path);
                string EditionName = sr.ReadLine();
                sr.Close();

                EditionURL.Add("Edition", EditionName);
                EditionURL.Add("URL", "http://61.152.126.152/SLPC/EditionURL/app.apk");
                string strData = JsonConvert.SerializeObject(EditionURL);
                wcf.Data = strData;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetCurrentWeatherReport()
        { 
        WCFResult wcf = new WCFResult(false);
            try
            {
                //F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\QXBG\2016\20160531
                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\QXBG\" + DateTime.Now.ToString("yyyy") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                string[] files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    path = path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\QXBG\" + DateTime.Now.AddDays(-1).ToString("yyyy") + "\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    files = Directory.GetFiles(path);
                }
                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[2];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        dtForecast = dtTmp;
                        selectFile = files[i];
                    }
                }

                wcf.Data = dtForecast.ToString("yyyy年MM月dd日HH时")+"："+GetMessageText(selectFile);
               wcf.Result = true;

            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetCurrentTypgoon()
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                MySQLDatabase m_Database = new MySQLDatabase();

                DateTime dtnow = DateTime.Now;
                string str_Get_TF_Info = string.Format("SELECT DISTINCT TFBH,ENGNAME,CHNNAME FROM tcrealtime WHERE CENTER='BABJ' AND FCSTTYPE='BABJ' AND FCSTHOUR=0 AND DATETIME>'{0} 16:00:00'  ORDER BY DATETIME DESC", dtnow.AddDays(-1).ToString("yyyy-MM-dd"));
                DataTable dtTFBH = m_Database.GetDataset(str_Get_TF_Info).Tables[0];

                List<DataTable> list_TFs = new List<DataTable>();
                for (int i = 0; i < dtTFBH.Rows.Count; i++)
                {
                    #region 实况
                    DataTable TF_Info = new DataTable();
                    TF_Info.Columns.Add(new DataColumn("TFBH", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("ENName", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("CHName", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("DateTime", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Period", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Lon", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Lat", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Pressure", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Wind", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Level", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("WR7", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("WR8", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("WR10", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("WR12", typeof(string)));
                    DataRow dr = dtTFBH.Rows[i];

                    string strTFBH = dr["tfbh"].ToString();
                    string strCNName = dr["engname"].ToString();
                    string strZHName = Encoding.Default.GetString(Encoding.GetEncoding("latin1").GetBytes(dr["chnname"].ToString()));

                    string strMYSQL = string.Format("SELECT DISTINCT YEAR,MON,DAY,HOUR,FHOUR,LAT,LON,PRESSURE,WIND,xx1,xx2,xx3,xx4 FROM tctracks_ts WHERE NNO ='{0}' AND WAY='BABJ' AND FHOUR=0 ORDER BY YEAR ASC,MON ASC,DAY ASC,HOUR ASC", strTFBH);
                    DataTable dtTcTrack = m_Database.GetDataset(strMYSQL).Tables[0];
                    DateTime dtLastTime=new DateTime();
                    for (int j = 0; j < dtTcTrack.Rows.Count; j++)
                    {
                        DataRow drNew = TF_Info.NewRow();

                        drNew["TFBH"] = strTFBH;
                        drNew["ENName"] = strCNName;
                        drNew["CHName"] = strZHName;
                        string strDateTime = string.Format("{0}-{1}-{2} {3}:00:00",
                            dtTcTrack.Rows[j]["YEAR"].ToString(),
                            dtTcTrack.Rows[j]["MON"].ToString(),
                            dtTcTrack.Rows[j]["DAY"].ToString(),
                            dtTcTrack.Rows[j]["HOUR"].ToString());
                        if(j==dtTcTrack.Rows.Count-1)
                            dtLastTime=DateTime.Parse(strDateTime);

                        string fhour = dtTcTrack.Rows[j]["FHOUR"].ToString();
                        drNew["Period"] = fhour;
                        if (int.Parse(fhour) > 0)
                            strDateTime = DateTime.Parse(strDateTime).AddHours(int.Parse(fhour)).ToString("yyyy-MM-dd HH:mm:ss");
                        drNew["DateTime"] = strDateTime;

                        drNew["Lon"] = dtTcTrack.Rows[j]["LON"].ToString();
                        drNew["Lat"] = dtTcTrack.Rows[j]["LAT"].ToString();
                        drNew["Pressure"] = dtTcTrack.Rows[j]["PRESSURE"].ToString();
                        string wind=dtTcTrack.Rows[j]["WIND"].ToString();
                        drNew["Wind"] = wind;
                        string strLevel = GetLevelByWind(wind);
                        drNew["Level"] = strLevel;
                        
                        int wr7 = int.Parse(dtTcTrack.Rows[j]["xx1"].ToString());
                        int wr10 = int.Parse(dtTcTrack.Rows[j]["xx2"].ToString());

                        drNew["WR7"] = wr7==0?"9999":wr7.ToString();
                        drNew["WR8"] = "9999";// wr8 == 0 ? "9999" : wr8.ToString();
                        drNew["WR10"] = wr10 == 0 ? "9999" : wr10.ToString();
                        drNew["WR12"] = "9999";//wr12 == 0 ? "9999" : wr12.ToString();

                        TF_Info.Rows.Add(drNew);
                        TF_Info.AcceptChanges();
                    }
                    #endregion
                    DataTable dtTcForecast = new DataTable();
                    for (int ii = dtTcTrack.Rows.Count - 1; ii >= 0; ii--)
                    {
                        string strDateTime = string.Format("{0}-{1}-{2} {3}:00:00",
                          dtTcTrack.Rows[ii]["YEAR"].ToString(),
                          dtTcTrack.Rows[ii]["MON"].ToString(),
                          dtTcTrack.Rows[ii]["DAY"].ToString(),
                          dtTcTrack.Rows[ii]["HOUR"].ToString());

                        dtLastTime = DateTime.Parse(strDateTime);
                        
                        strMYSQL = string.Format("SELECT DISTINCT YEAR,MON,DAY,HOUR,FHOUR,LAT,LON,PRESSURE,WIND,xx1,xx2,xx3,xx4 FROM tctracks_ts WHERE YEAR ={0} AND mon={1} and Day={2} and hour={3} and WAY='BABJ' AND FHOUR>0 AND NNO='{4}' ORDER BY FHOUR ASC", dtLastTime.Year, dtLastTime.Month, dtLastTime.Day, dtLastTime.Hour, strTFBH);

                        dtTcForecast = m_Database.GetDataset(strMYSQL).Tables[0];
                        if (dtTcForecast.Rows.Count > 0)
                            break;
                        else
                            TF_Info.Rows.RemoveAt(ii);
                    }

                    for (int j = 0; j < dtTcForecast.Rows.Count; j++)
                    {
                        DataRow drNew = TF_Info.NewRow();

                        drNew["TFBH"] = strTFBH;
                        drNew["ENName"] = strCNName;
                        drNew["CHName"] = strZHName;
                        string strDateTime = string.Format("{0}-{1}-{2} {3}:00:00",
                            dtTcForecast.Rows[j]["YEAR"].ToString(),
                            dtTcForecast.Rows[j]["MON"].ToString(),
                            dtTcForecast.Rows[j]["DAY"].ToString(),
                            dtTcForecast.Rows[j]["HOUR"].ToString());

                        string fhour = dtTcForecast.Rows[j]["FHOUR"].ToString();
                        drNew["Period"] = fhour;
                        if (int.Parse(fhour) > 0)
                            strDateTime = DateTime.Parse(strDateTime).AddHours(int.Parse(fhour)).ToString("yyyy-MM-dd HH:mm:ss");
                        drNew["DateTime"] = strDateTime;
                        
                        drNew["Lon"] = dtTcForecast.Rows[j]["LON"].ToString();
                        drNew["Lat"] = dtTcForecast.Rows[j]["LAT"].ToString();
                        drNew["Pressure"] = dtTcForecast.Rows[j]["PRESSURE"].ToString();
                        string wind = dtTcForecast.Rows[j]["WIND"].ToString();
                        drNew["Wind"] = wind;
                        string strLevel = GetLevelByWind(wind);
                        drNew["Level"] = strLevel;

                        int wr7 = int.Parse(dtTcForecast.Rows[j]["xx1"].ToString());
                        int wr10 = int.Parse(dtTcForecast.Rows[j]["xx2"].ToString());

                        drNew["WR7"] = wr7 == 0 ? "9999" : wr7.ToString();
                        drNew["WR8"] = "9999";//wr8 == 0 ? "9999" : wr8.ToString();
                        drNew["WR10"] = wr10 == 0 ? "9999" : wr10.ToString();
                        drNew["WR12"] = "9999";//wr12 == 0 ? "9999" : wr12.ToString();

                        TF_Info.Rows.Add(drNew);
                        TF_Info.AcceptChanges();
                    }
                    list_TFs.Add(TF_Info);
                }

                string strData = JsonConvert.SerializeObject(list_TFs);
                wcf.Result = true;
                wcf.Data = strData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        
        }
        public Stream GetCurrentTypgoonTest()
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                MySQLDatabase m_Database = new MySQLDatabase();

                DateTime dtnow = DateTime.Now;
                string str_Get_TF_Info = string.Format("SELECT DISTINCT TFBH,ENGNAME,CHNNAME FROM tcrealtime WHERE CENTER='BABJ' AND FCSTTYPE='BABJ' AND FCSTHOUR=0 AND DATETIME>'{0} 16:00:00'  ORDER BY DATETIME DESC", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
                DataTable dtTFBH = m_Database.GetDataset(str_Get_TF_Info).Tables[0];

                List<DataTable> list_TFs = new List<DataTable>();
                for (int i = 0; i < dtTFBH.Rows.Count; i++)
                {
                    #region 单台风
                    #region DataTable
                    DataTable TF_Info = new DataTable();
                    TF_Info.Columns.Add(new DataColumn("TFBH", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("ENName", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("CHName", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("DateTime", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Period", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("ForecastTime", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Lon", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Lat", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Pressure", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Wind", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("Level", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("WR7", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("WR8", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("WR10", typeof(string)));
                    TF_Info.Columns.Add(new DataColumn("WR12", typeof(string)));
                    #endregion

                    #region 实况
                    DataRow dr = dtTFBH.Rows[i];

                    string strTFBH = dr["tfbh"].ToString();
                    string strCNName = dr["engname"].ToString();
                    string strZHName = Encoding.Default.GetString(Encoding.GetEncoding("latin1").GetBytes(dr["chnname"].ToString()));

                    string strMYSQL = string.Format("SELECT DISTINCT YEAR,MON,DAY,HOUR,FHOUR,LAT,LON,PRESSURE,WIND,xx1,xx2,xx3,xx4 FROM tctracks_ts WHERE NNO ='{0}' AND WAY='BABJ' AND FHOUR=0 ORDER BY YEAR ASC,MON ASC,DAY ASC,HOUR ASC", strTFBH);
                    DataTable dtTcTrack = m_Database.GetDataset(strMYSQL).Tables[0];
                    DateTime dtLastTime = new DateTime();
                    for (int j = 0; j < dtTcTrack.Rows.Count; j++)
                    {
                        DataRow drNew = TF_Info.NewRow();

                        drNew["TFBH"] = strTFBH;
                        drNew["ENName"] = strCNName;
                        drNew["CHName"] = strZHName;
                        string strDateTime = string.Format("{0}-{1}-{2} {3}:00:00",
                            dtTcTrack.Rows[j]["YEAR"].ToString(),
                            dtTcTrack.Rows[j]["MON"].ToString(),
                            dtTcTrack.Rows[j]["DAY"].ToString(),
                            dtTcTrack.Rows[j]["HOUR"].ToString());
                        if (j == dtTcTrack.Rows.Count - 1)
                            dtLastTime = DateTime.Parse(strDateTime);




                        string fhour = dtTcTrack.Rows[j]["FHOUR"].ToString();
                        drNew["Period"] = fhour;
                        //if (int.Parse(fhour) > 0)
                        //    strDateTime = DateTime.Parse(strDateTime).AddHours(int.Parse(fhour)).ToString("yyyy-MM-dd HH:mm:ss");
                        drNew["DateTime"] = strDateTime;

                        drNew["ForecastTime"] = (DateTime.Parse(strDateTime).AddHours(int.Parse(fhour))).ToString();
                        drNew["Lon"] = dtTcTrack.Rows[j]["LON"].ToString();
                        drNew["Lat"] = dtTcTrack.Rows[j]["LAT"].ToString();
                        drNew["Pressure"] = dtTcTrack.Rows[j]["PRESSURE"].ToString();
                        string wind = dtTcTrack.Rows[j]["WIND"].ToString();
                        drNew["Wind"] = wind;
                        string strLevel = GetLevelByWind(wind);
                        drNew["Level"] = strLevel;

                        int wr7 = int.Parse(dtTcTrack.Rows[j]["xx1"].ToString());
                        //int wr8 = int.Parse(dtTcTrack.Rows[j]["xx2"].ToString());
                        int wr10 = int.Parse(dtTcTrack.Rows[j]["xx2"].ToString());
                        // int wr12 = int.Parse(dtTcTrack.Rows[j]["xx4"].ToString());

                        drNew["WR7"] = wr7 == 0 ? "9999" : wr7.ToString();
                        drNew["WR8"] = "9999";// wr8 == 0 ? "9999" : wr8.ToString();
                        drNew["WR10"] = wr10 == 0 ? "9999" : wr10.ToString();
                        drNew["WR12"] = "9999";//wr12 == 0 ? "9999" : wr12.ToString();

                        TF_Info.Rows.Add(drNew);
                        TF_Info.AcceptChanges();
                    }
                    #endregion

                    if ((DateTime.Now - dtLastTime).TotalHours < 12)
                    {
                        #region 预报
                        DataTable dtTcForecast = new DataTable();
                        for (int ii = dtTcTrack.Rows.Count - 1; ii >= 0; ii--)
                        {
                            string strDateTime = string.Format("{0}-{1}-{2} {3}:00:00",
                              dtTcTrack.Rows[ii]["YEAR"].ToString(),
                              dtTcTrack.Rows[ii]["MON"].ToString(),
                              dtTcTrack.Rows[ii]["DAY"].ToString(),
                              dtTcTrack.Rows[ii]["HOUR"].ToString());

                            dtLastTime = DateTime.Parse(strDateTime);
                            strMYSQL = string.Format("SELECT DISTINCT YEAR,MON,DAY,HOUR,FHOUR,LAT,LON,PRESSURE,WIND,xx1,xx2,xx3,xx4 FROM tctracks_ts WHERE YEAR ={0} AND mon={1} and Day={2} and hour={3} and WAY='BABJ' AND FHOUR>0 AND NNO='{4}' order by FHOUR asc", dtLastTime.Year, dtLastTime.Month, dtLastTime.Day, dtLastTime.Hour, strTFBH);

                            dtTcForecast = m_Database.GetDataset(strMYSQL).Tables[0];
                            if (dtTcForecast.Rows.Count > 0)
                                break;
                            //else
                            //    TF_Info.Rows.RemoveAt(ii);
                        }

                        //strMYSQL = string.Format("SELECT DISTINCT YEAR,MON,DAY,HOUR,FHOUR,LAT,LON,PRESSURE,WIND,xx1,xx2,xx3,xx4 FROM tctracks_ts WHERE YEAR ={0} AND mon={1} and Day={2} and hour={3} and WAY='BABJ' AND FHOUR>0", dtLastTime.Year, dtLastTime.Month, dtLastTime.Day, dtLastTime.Hour);                 

                        //DataTable dtTcForecast = m_Database.GetDataset(strMYSQL).Tables[0];

                        for (int j = 0; j < dtTcForecast.Rows.Count; j++)
                        {
                            DataRow drNew = TF_Info.NewRow();

                            drNew["TFBH"] = strTFBH;
                            drNew["ENName"] = strCNName;
                            drNew["CHName"] = strZHName;
                            string strDateTime = string.Format("{0}-{1}-{2} {3}:00:00",
                                dtTcForecast.Rows[j]["YEAR"].ToString(),
                                dtTcForecast.Rows[j]["MON"].ToString(),
                                dtTcForecast.Rows[j]["DAY"].ToString(),
                                dtTcForecast.Rows[j]["HOUR"].ToString());

                            string fhour = dtTcForecast.Rows[j]["FHOUR"].ToString();
                            drNew["Period"] = fhour;
                            //if (int.Parse(fhour) > 0)
                            //    strDateTime = DateTime.Parse(strDateTime).AddHours(int.Parse(fhour)).ToString("yyyy-MM-dd HH:mm:ss");
                            drNew["DateTime"] = strDateTime;
                            drNew["ForecastTime"] = (DateTime.Parse(strDateTime).AddHours(int.Parse(fhour))).ToString();
                            drNew["Lon"] = dtTcForecast.Rows[j]["LON"].ToString();
                            drNew["Lat"] = dtTcForecast.Rows[j]["LAT"].ToString();
                            drNew["Pressure"] = dtTcForecast.Rows[j]["PRESSURE"].ToString();
                            string wind = dtTcForecast.Rows[j]["WIND"].ToString();
                            drNew["Wind"] = wind;
                            string strLevel = GetLevelByWind(wind);
                            drNew["Level"] = strLevel;

                            int wr7 = int.Parse(dtTcForecast.Rows[j]["xx1"].ToString());
                            //int wr8 = int.Parse(dtTcForecast.Rows[j]["xx2"].ToString());
                            int wr10 = int.Parse(dtTcForecast.Rows[j]["xx2"].ToString());
                            //int wr12 = int.Parse(dtTcForecast.Rows[j]["xx4"].ToString());

                            drNew["WR7"] = wr7 == 0 ? "9999" : wr7.ToString();
                            drNew["WR8"] = "9999";//wr8 == 0 ? "9999" : wr8.ToString();
                            drNew["WR10"] = wr10 == 0 ? "9999" : wr10.ToString();
                            drNew["WR12"] = "9999";//wr12 == 0 ? "9999" : wr12.ToString();

                            TF_Info.Rows.Add(drNew);
                            TF_Info.AcceptChanges();
                        }
                        #endregion
                    }
                    list_TFs.Add(TF_Info);
                    if ((DateTime.Now - dtLastTime).TotalHours >=12)
                    {
                        list_TFs.Clear();
                        break;
                    } 
                    
                    
                    #endregion
                }

                string strData = JsonConvert.SerializeObject(list_TFs);
                if (strData == "[]")
                {
                    wcf.Result = false;
                    wcf.Message = "当前无台风。";
                }
                else
                {
                    wcf.Result = true;
                    wcf.Data = strData;
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));

        }
        public Stream GetWarnning(string JDName)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                string path = "";
                switch (JDName.ToLower())
                {
                    case "wjcjd":
                        path = @"F:\city\五角场街道";
                        break;
                    case "xjwcjd":
                        path = @"F:\city\新江湾城街道";
                        break;
                }
                string[] files = Directory.GetFiles(path);
                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    if (fileName.Contains("code"))
                        continue;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[0];
                    DateTime dtTmp = strDatetime.Length == 12 ? DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null) : DateTime.ParseExact(strDatetime, "yyyyMMddHHmmss", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }

                StreamReader sr = new StreamReader(selectFile, Encoding.Default);
                string var = sr.ReadToEnd();
                wcf.Result = true;
                wcf.Data = var;
                wcf.Message = "";

            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));

        }
        public Stream GetDistrictList()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string strSQL = "SELECT DM,MC AS NAME FROM CITYDISASTERFORECAST.DBO.D_DISASTER_DISTRICT WHERE DM >0 ORDER BY DM";
                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);
                List<District> data = new List<District>();
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];

                    District d = new District();
                    d.DM = dr["DM"].ToString();
                    d.Name = dr["NAME"].ToString();
                    data.Add(d);
                }
                wcf.Result = true;
                wcf.Data = data;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetCommunityListByDistrict(string District_DM)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string strSQL = "SELECT DM,MC AS NAME FROM CITYDISASTERFORECAST.DBO.D_DISASTER_DISTRICT WHERE DM ='" + District_DM + "'";
                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);
                if (dtData.Rows.Count == 0)
                    throw new Exception("输入的区县代码超出范围。");

                DataRow dr = dtData.Rows[0];

                District d = new District();
                d.DM = dr["DM"].ToString();
                d.Name = dr["NAME"].ToString();

                #region Communities
                strSQL = string.Format("SELECT ID,NAME,DISTRICT,LON,LAT FROM [APP_RISK_DATA].[DBO].[T_STREETLOCATION]");
                DataTable dtCommunity = new Database().GetDataTable(strSQL);

                List<Community> listCommunity = new List<Community>();
                for (int i = 0; i < dtCommunity.Rows.Count; i++)
                {
                    DataRow drCommunity = dtCommunity.Rows[i];
                    Community c = new Community();
                    c.ID = drCommunity["ID"].ToString();
                    c.Name = drCommunity["NAME"].ToString();
                    c.District_Name = drCommunity["DISTRICT"].ToString();

                    listCommunity.Add(c);
                } 
                #endregion

                List<Community> ress = new List<Community>();
                if (District_DM != "1")
                    ress = listCommunity.Where(p => p.District_Name == d.Name).ToList<Community>();
                else
                {
                    string[] districts = new string[] { "杨浦区", "虹口区", "长宁区", "普陀区", "静安区", "徐汇区", "黄浦区" };
                    ress = listCommunity.Where(p => districts.Contains(p.District_Name)).ToList<Community>();
                }
                wcf.Result = true;
                wcf.Data = ress;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetCommunityListByDistrictName(string District_Name)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region Communities
                string strSQL = string.Format("SELECT ID,NAME,DISTRICT,LON,LAT FROM [APP_RISK_DATA].[DBO].[T_STREETLOCATION]");
                DataTable dtCommunity = new Database().GetDataTable(strSQL);

                List<Community> listCommunity = new List<Community>();
                for (int i = 0; i < dtCommunity.Rows.Count; i++)
                {
                    DataRow drCommunity = dtCommunity.Rows[i];
                    Community c = new Community();
                    c.ID = drCommunity["ID"].ToString();
                    c.Name = drCommunity["NAME"].ToString();
                    c.District_Name = drCommunity["DISTRICT"].ToString();

                    listCommunity.Add(c);
                }
                #endregion
                List<Community> ress = new List<Community>();
                if (District_Name != "中心城区")
                    ress = listCommunity.Where(p => p.District_Name == District_Name).ToList<Community>();
                else
                {
                    string[] districts = new string[] { "杨浦区", "虹口区", "长宁区", "普陀区", "静安区", "徐汇区", "黄浦区" };
                    ress = listCommunity.Where(p => districts.Contains(District_Name)).ToList<Community>();

                }

                wcf.Result = true;
                wcf.Data = ress;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetCommunityByXY(string lon, string lat)
        { WCFResult1 wcf = new WCFResult1(false);
            try
            {
                TopologyOperator.Initial();
                TopologyOperator t = new TopologyOperator();
                string streetName = t.GetDistrics(new Coordinate(double.Parse(lon.Replace("_", ".")), double.Parse(lat.Replace("_", "."))));

                #region Communities
                string strSQL = string.Format("SELECT ID,NAME,DISTRICT,LON,LAT FROM [APP_RISK_DATA].[DBO].[T_STREETLOCATION]");
                DataTable dtCommunity = new Database().GetDataTable(strSQL);

                List<Community> listCommunity = new List<Community>();
                for (int i = 0; i < dtCommunity.Rows.Count; i++)
                {
                    DataRow drCommunity = dtCommunity.Rows[i];
                    Community c = new Community();
                    c.ID = drCommunity["ID"].ToString();
                    c.Name = drCommunity["NAME"].ToString();
                    c.District_Name = drCommunity["DISTRICT"].ToString();

                    listCommunity.Add(c);
                }
                #endregion
                List<Community> ress = listCommunity.Where(p => p.Name == streetName).ToList<Community>();
                if (ress.Count > 0)
                {
                    wcf.Result = true;
                    wcf.Data = ress[0];
                }
                else
                {
                    wcf.Result = false;
                    wcf.Message = "未查询到所给位置的街道信息。";
                }
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetIndex()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\MeteorologicalIndex\" + DateTime.Now.ToString("yyyy") + "\\" + DateTime.Now.ToString("yyyyMMdd");
                string[] files = Directory.GetFiles(path);
                if (files.Length == 0)
                {
                    path = path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\Product\MeteorologicalIndex\" + DateTime.Now.AddDays(-1).ToString("yyyy") + "\\" + DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
                    files = Directory.GetFiles(path);
                }
                string selectFile = "";
                DateTime dtForecast = new DateTime(2000, 1, 1);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo fi = new FileInfo(files[i]);

                    string fileName = fi.Name;
                    string strDatetime = fileName.Split(new char[] { '_', '.' })[1];
                    DateTime dtTmp = DateTime.ParseExact(strDatetime, "yyyyMMddHHmm", null);
                    if (dtTmp > dtForecast)
                    {
                        selectFile = files[i];
                        dtForecast = dtTmp;
                    }
                }
                
                Dictionary<string, MeteorologicalIndex> dicData = new Dictionary<string, MeteorologicalIndex>();

                string ss1 = "[体感指数,穿衣指数,中暑指数,洗晒指数,火险指数,日照指数]{4}";
                string ss2 = "[体感指数,穿衣指数,感冒指数,洗晒指数,火险指数,日照指数]{4}";


                string pattern = "";
                //"[体感指数,穿衣指数,中暑指数,洗晒指数,火险指数,日照指数]{4}";
                if (DateTime.Now.Month >= 6 && DateTime.Now.Month <= 9)
                    pattern = ss1;
                else
                    pattern = ss2;

                StreamReader sr = new StreamReader(selectFile, Encoding.Default);
                string strLine = sr.ReadLine();
                while ((strLine = sr.ReadLine()) != null)
                {
                    if (Regex.IsMatch(strLine, pattern))
                    {
                        
                        string[] strDatas = Regex.Split(strLine, "\\s+");

                        Description d = new Description();
                        d.Level = strDatas[3];
                        d.Comfort = strDatas[4];
                        d.Descrip = strDatas[5];
                        d.Suggestion = strDatas[6];

                        string pattern1 = "[早晨,上午,中午,下午,晚上]{2}";
                        Match m1 = Regex.Match(strDatas[1], pattern1);
                        d.Time = Regex.IsMatch(strDatas[1], pattern1) ? Regex.Match(strDatas[1], pattern1).ToString() : "";


                        Match m = Regex.Match(strDatas[1], pattern);
                        string key = m.ToString();

                        MeteorologicalIndex mi = new MeteorologicalIndex();
                        if (dicData.Keys.Contains(key))
                        {
                            mi = dicData[key];
                            mi.Descriptions.Add(d);
                        }
                        else
                        {
                            mi.Name = key;
                            mi.PublishTime = dtForecast.ToString("yyyy-MM-dd HH:mm:00");
                            mi.Descriptions.Add(d);

                            dicData.Add(key, mi);
                        }
                    }
                }
                wcf.Result = true;
                wcf.Data = dicData.Values.ToList();
                
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetCommunityAutoStationDataByCommunityID(string CommunityID)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string strSQL = string.Format("SELECT ID,NAME,DISTRICT,LON,LAT FROM [APP_RISK_DATA].[DBO].[T_STREETLOCATION] WHERE ID={0}", CommunityID);

                DataTable dtCommunity = new Database().GetDataTable(strSQL);
                if (dtCommunity.Rows.Count == 0)
                    throw new Exception("无效的社区编号");
                string lon = dtCommunity.Rows[0]["LON"].ToString();
                string lat = dtCommunity.Rows[0]["LAT"].ToString();

                return GetCommunityAutoStationData(lon, lat);
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetCommunityAutoStationData(string lon, string lat)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string strSQL =string.Format( "SELECT [STATIONID],[STATIONNAME],([LON]-{0})*([LON]-{0})+([LAT]-{1})*([LAT]-{1}) AS DIS  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONINFO] ORDER BY DIS",lon.Replace("_","."),lat.Replace("_","."));
                Database db=new Database();
                DataTable dtSta = db.GetDataTable(strSQL);
                CommunityAutoStationData asd = new CommunityAutoStationData();
                for (int i = 0; i < dtSta.Rows.Count; i++)
                {
                    DataRow dr = dtSta.Rows[i];

                    string stationID = dr["STATIONID"].ToString();
                    DataTable dtDataTmp = db.GetDataTable("SELECT TOP 1 [DATETIME],[TEMPERATURE],[RAINHOUR],[WINDDIRECTION],[WINDSPEED]  FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA]  WHERE STATIONID='" + stationID + "' ORDER BY DATETIME DESC");
                    if (dtDataTmp.Rows.Count > 0)
                    {
                        double Temperature = double.Parse(dtDataTmp.Rows[0]["Temperature"].ToString());
                        double RainHour = double.Parse(dtDataTmp.Rows[0]["RainHour"].ToString());
                        double WindDirection = double.Parse(dtDataTmp.Rows[0]["WindDirection"].ToString());
                        double WindSpeed = double.Parse(dtDataTmp.Rows[0]["WindSpeed"].ToString());

                        DateTime dtTime = DateTime.Parse(dtDataTmp.Rows[0]["DATETIME"].ToString());
                        //if (DateTime.Parse(asd.Datetime) < dtTime && dtTime > DateTime.Now.AddHours(-24))
                        //    asd.Datetime = dtTime.ToString("yyyy-MM-dd HH:mm:ss");
                        if (dtTime > DateTime.Now.AddHours(-24))
                        {
                            if (DateTime.Parse(asd.Datetime) < dtTime)
                                asd.Datetime = dtTime.ToString("yyyy-MM-dd HH:mm:ss");

                            if (double.IsNaN(asd.Tempreture))
                                if (Temperature != -9999.9 && Temperature != -1000.0)
                                    asd.Tempreture = Temperature;
                            if (double.IsNaN(asd.RainHour))
                                if (RainHour != -9999.9 && RainHour != -1000.0)
                                    asd.RainHour = RainHour;
                            if (double.IsNaN(asd.WindDir))
                                if (WindDirection != -9999.9 && WindDirection != -1000.0)
                                    asd.WindDir = WindDirection;
                            if (double.IsNaN(asd.WindSpd))
                                if (WindSpeed != -9999.9 && WindSpeed != -1000.0)
                                    asd.WindSpd = WindSpeed;

                            if (asd.IsDataComplete())
                                break;
                        }
                    }
                }
                wcf.Result = true;
                wcf.Data = asd;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetCommunityWaterStationData(string CommunityID)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string waterStationID = "";
                try
                {
                    waterStationID = ConfigurationManager.AppSettings[CommunityID].ToString();
                }
                catch
                {
                    throw new Exception("暂无积水站数据。");
                }
                string strSQL = string.Format("SELECT TOP 1 [DATATIME],[WATERDEPTH] FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] WHERE STATIONID ='{0}' ORDER BY DATATIME DESC", waterStationID);
                Database db = new Database();
                DataTable dtSta = db.GetDataTable(strSQL);
                
                wcf.Result = true;
                wcf.Data = dtSta;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetDistrictAutoStationData(string District_DM)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string strSQL = "SELECT DM,MC AS NAME FROM CITYDISASTERFORECAST.DBO.D_DISASTER_DISTRICT WHERE DM ='" + District_DM + "'";
                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);
                List<District> data = new List<District>();
                District d = new District();
                d.DM = dtData.Rows[0]["DM"].ToString();
                d.Name = dtData.Rows[0]["NAME"].ToString();

                string stationID = "";
                switch (d.Name)
                {
                    case "中心城区":
                    case "黄浦区":
                    case "静安区":
                    case "徐汇区":
                    case "普陀区":
                    case "杨浦区":
                    case "虹口区":
                    case "长宁区":
                        stationID = "58367";
                        break;
                    case "闵行区":
                        stationID = "58361";
                        break;
                    case "宝山区":
                        stationID = "58362";
                        break;
                    case "嘉定区":
                        stationID = "58365";
                        break;
                    case "崇明县":
                        stationID = "58366";
                        break;
                    case "徐家汇":
                        stationID = "58367";
                        break;
                    case "浦东新区":
                        stationID = "58370";
                        break;
                    case "金山区":
                        stationID = "58460";
                        break;
                    case "青浦区":
                        stationID = "58461";
                        break;
                    case "松江区":
                        stationID = "58462";
                        break;
                    case "奉贤区":
                        stationID = "58463";
                        break;
                }
                DistrictAutoStationData dasd = new DistrictAutoStationData();
                DataTable dtDataTmp = db.GetDataTable("SELECT TOP 1 [DATETIME],[TEMPERATURE],[RAINHOUR],[WINDDIRECTION],[WINDSPEED],[VISIBLE]   FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA]  WHERE STATIONID='" + stationID + "' ORDER BY DATETIME DESC");
                double Temperature = double.Parse(dtDataTmp.Rows[0]["Temperature"].ToString());
                double RainHour = double.Parse(dtDataTmp.Rows[0]["RainHour"].ToString());
                double WindDirection = double.Parse(dtDataTmp.Rows[0]["WindDirection"].ToString());
                double WindSpeed = double.Parse(dtDataTmp.Rows[0]["WindSpeed"].ToString());
                double Visible = double.Parse(dtDataTmp.Rows[0]["Visible"].ToString());

                string date = dtDataTmp.Rows[0]["DATETIME"].ToString();
                dasd.Datetime = date;

                if (double.IsNaN(dasd.Tempreture))
                    if (Temperature != -9999.9)
                        dasd.Tempreture = Temperature;
                if (double.IsNaN(dasd.RainHour))
                    if (RainHour != -9999.9)
                        dasd.RainHour = RainHour;
                if (double.IsNaN(dasd.WindDir))
                    if (RainHour != -9999.9)
                        dasd.WindDir = WindDirection;
                if (double.IsNaN(dasd.WindSpd))
                    if (WindSpeed != -9999.9)
                        dasd.WindSpd = WindSpeed;
                if (double.IsNaN(dasd.Visible))
                    if (Visible != -9999.9)
                        dasd.Visible = Visible;

                wcf.Result = true;
                wcf.Data = dasd;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }
        public Stream GetDistrictAutoStationDataByName(string District_Name)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                

                string stationID = "";
                switch (District_Name)
                {
                    case "中心城区":
                    case "黄浦区":
                    case "静安区":
                    case "徐汇区":
                    case "普陀区":
                    case "杨浦区":
                    case "虹口区":
                    case "长宁区":
                        stationID = "58367";
                        break;
                    case "闵行区":
                        stationID = "58361";
                        break;
                    case "宝山区":
                        stationID = "58362";
                        break;
                    case "嘉定区":
                        stationID = "58365";
                        break;
                    case "崇明县":
                        stationID = "58366";
                        break;
                    case "徐家汇":
                        stationID = "58367";
                        break;
                    case "浦东新区":
                        stationID = "58370";
                        break;
                    case "金山区":
                        stationID = "58460";
                        break;
                    case "青浦区":
                        stationID = "58461";
                        break;
                    case "松江区":
                        stationID = "58462";
                        break;
                    case "奉贤区":
                        stationID = "58463";
                        break;
                }
                DistrictAutoStationData dasd = new DistrictAutoStationData();
                Database db = new Database();
                DataTable dtDataTmp = db.GetDataTable("SELECT TOP 1 [DATETIME],[TEMPERATURE],[RAINHOUR],[WINDDIRECTION],[WINDSPEED],[VISIBLE]   FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA]  WHERE STATIONID='" + stationID + "' ORDER BY DATETIME DESC");
                double Temperature = double.Parse(dtDataTmp.Rows[0]["Temperature"].ToString());
                double RainHour = double.Parse(dtDataTmp.Rows[0]["RainHour"].ToString());
                double WindDirection = double.Parse(dtDataTmp.Rows[0]["WindDirection"].ToString());
                double WindSpeed = double.Parse(dtDataTmp.Rows[0]["WindSpeed"].ToString());
                double Visible = double.Parse(dtDataTmp.Rows[0]["Visible"].ToString());
                string date = dtDataTmp.Rows[0]["DATETIME"].ToString();
                dasd.Datetime = date;

                if (double.IsNaN(dasd.Tempreture))
                    if (Temperature != -9999.9)
                        dasd.Tempreture = Temperature;
                if (double.IsNaN(dasd.RainHour))
                    if (RainHour != -9999.9)
                        dasd.RainHour = RainHour;
                if (double.IsNaN(dasd.WindDir))
                    if (RainHour != -9999.9)
                        dasd.WindDir = WindDirection;
                if (double.IsNaN(dasd.WindSpd))
                    if (WindSpeed != -9999.9)
                        dasd.WindSpd = WindSpeed;
                if (double.IsNaN(dasd.Visible))
                    if (Visible != -9999.9)
                        dasd.Visible = Visible;

                wcf.Result = true;
                wcf.Data = dasd;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

        public Stream GetWaterStationData(string strQueryTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime dtQuery = DateTime.Now;
                try
                {
                    dtQuery = DateTime.ParseExact(strQueryTime, "yyyyMMddHHmmss", null);


                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }

                //string strSQL = string.Format("SELECT STATIONID,DATATIME,STATIONNAME,WATERDEPTH FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] WHERE DATATIME >'{0}' AND DATATIME <='{1}' ORDER BY STATIONID ,DATATIME DESC", dtQuery.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));
                
                string strSQL = string.Format("SELECT t1.STATIONID,t1.DATATIME,t1.STATIONNAME,t3.LON,t3.LAT,t1.WATERDEPTH FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] t1,(SELECT STATIONID,max(DATATIME) as DATATIME FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] WHERE DATATIME >'{0}' AND DATATIME <='{1}' group by stationID) t2,(SELECT StationID,Lon,Lat FROM [CityDisasterForecast].[dbo].[Tbl_WaterStation]) t3  where t1.STATIONID=t2.STATIONID and t1.DATATIME=t2.DATATIME and t1.STATIONID=t3.STATIONID ORDER BY STATIONID ,DATATIME DESC", dtQuery.AddHours(-4).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));


                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);

                if (dtData.Rows.Count == 0)
                {
                    throw new Exception("所给时间未查询到数据。");
                }
                wcf.Result = true;
                wcf.Data = dtData;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream Get_YP_WaterStationData(string strQueryTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime dtQuery = DateTime.Now;
                try
                {
                    dtQuery = DateTime.ParseExact(strQueryTime, "yyyyMMddHHmmss", null);


                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }

                //string strSQL = string.Format("SELECT STATIONID,DATATIME,STATIONNAME,WATERDEPTH FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] WHERE DATATIME >'{0}' AND DATATIME <='{1}' ORDER BY STATIONID ,DATATIME DESC", dtQuery.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));

                string strSQL = string.Format("SELECT t1.STATIONID,t1.DATATIME,t1.STATIONNAME,t3.LON,t3.LAT,t1.WATERDEPTH FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] t1,(SELECT STATIONID,max(DATATIME) as DATATIME FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] WHERE DATATIME >'{0}' AND DATATIME <='{1}' group by stationID) t2,(SELECT StationID,Lon,Lat FROM [CityDisasterForecast].[dbo].[Tbl_WaterStation] where stationid in ('74467053','82275990','82603654','82603694','95274837','95274854','PCLJ0004','PCLJ0010','PCLJ0029','PSJS0002','PSJS0004','PSJS0005','PSJS0006','PSJS0026')) t3  where t1.STATIONID=t2.STATIONID and t1.DATATIME=t2.DATATIME and t1.STATIONID=t3.STATIONID ORDER BY STATIONID ,DATATIME DESC", dtQuery.AddHours(-4).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));


                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);

                if (dtData.Rows.Count == 0)
                {
                    throw new Exception("所给时间未查询到数据。");
                }
                wcf.Result = true;
                wcf.Data = dtData;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        #endregion


        #region 三维
        public Stream GetTyphoonWays()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                //string strMYSQL = "select way,wayname,wayname_en,content from tctracks_explains where way in ('BABJ','JAWT','RKSL','VHHH')";
                string strMYSQL = "select way,wayname,wayname_en,content from tctracks_explains";
                MySQLDatabase pMySQLDatabase = new MySQLDatabase();
                DataTable dt = pMySQLDatabase.GetDataset(strMYSQL).Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    dr["wayname"] = Encoding.Default.GetString(Encoding.GetEncoding("latin1").GetBytes(dr["wayname"].ToString()));
                    dr["content"] = Encoding.Default.GetString(Encoding.GetEncoding("latin1").GetBytes(dr["content"].ToString()));
                }
                wcf.Data = dt;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetTyphoons(string year)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                string str_Get_TF_Info = string.Format("SELECT DISTINCT TFBH,ENGNAME,CHNNAME FROM tcrealtime WHERE  CENTER='BABJ' and FcstType = 'BABJ' AND FCSTHOUR=0 AND year(DATETIME)={0} and engname<>'' ORDER BY DATETIME DESC", year);
                MySQLDatabase pMySQLDatabase = new MySQLDatabase();
                DataTable dt = pMySQLDatabase.GetDataset(str_Get_TF_Info).Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    while(true)
                    {
                        try
                        {
                            if (int.Parse(dr["TFBH"].ToString()) > 0)
                                break;
                            else
                            {
                                dt.Rows.RemoveAt(i);
                                dr = dt.Rows[i];
                            }
                        }
                        catch (Exception ex)
                        {
                            dt.Rows.RemoveAt(i);
                            dr = dt.Rows[i];
                        }
                    }
                    dr["chnname"] = Encoding.Default.GetString(Encoding.GetEncoding("latin1").GetBytes(dr["chnname"].ToString()));
                }
                
                wcf.Data = dt;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetTyphoonRealTimePoints(string tfbh)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                MySQLDatabase m_Database = new MySQLDatabase();
                string str_Get_TF_Info = string.Format("SELECT DISTINCT TFBH,ENGNAME,CHNNAME FROM tcrealtime WHERE TFBH='{0}' AND FCSTTYPE='BABJ' order by TFBH desc", tfbh);
                DataTable dtTFBH = m_Database.GetDataset(str_Get_TF_Info).Tables[0];
                if (dtTFBH.Rows.Count == 0)
                    throw new Exception("错误的台风编号。");

                DataRow drTFBH = dtTFBH.Rows[0];
                string strCNName = drTFBH["engname"].ToString();
                string strZHName = Encoding.Default.GetString(Encoding.GetEncoding("latin1").GetBytes(drTFBH["chnname"].ToString()));

                DataTable TF_Info = new DataTable();
                TF_Info.Columns.Add(new DataColumn("TFBH", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("ENName", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("CHName", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Way", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("DateTime", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Period", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Lon", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Lat", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Pressure", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Wind", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Level", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("WR7", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("WR8", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("WR10", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("WR12", typeof(string)));

                string strMYSQL = string.Format("SELECT DISTINCT YEAR,MON,DAY,HOUR,FHOUR,LAT,LON,PRESSURE,WIND,xx1,xx2,xx3,xx4 FROM tctracks_ts WHERE NNO ='{0}' AND WAY='BABJ' AND FHOUR=0  ORDER BY YEAR ASC,MON ASC,DAY ASC,HOUR ASC", tfbh);
                DataTable dtTcTrack = m_Database.GetDataset(strMYSQL).Tables[0];
                for (int i = 0; i < dtTcTrack.Rows.Count; i++)
                {
                    DataRow drNew = TF_Info.NewRow();

                    drNew["TFBH"] = tfbh;
                    drNew["ENName"] = strCNName;
                    drNew["CHName"] = strZHName;
                    drNew["Way"] = "BABJ";
                    string strDateTime = string.Format("{0}-{1}-{2} {3}:00:00",
                        dtTcTrack.Rows[i]["YEAR"].ToString(),
                        dtTcTrack.Rows[i]["MON"].ToString(),
                        dtTcTrack.Rows[i]["DAY"].ToString(),
                        dtTcTrack.Rows[i]["HOUR"].ToString());

                    drNew["Period"] = 0;
                    drNew["DateTime"] = DateTime.Parse(strDateTime).ToString("yyyy-MM-dd HH:mm:ss");


                    drNew["Lon"] = dtTcTrack.Rows[i]["LON"].ToString();
                    drNew["Lat"] = dtTcTrack.Rows[i]["LAT"].ToString();
                    drNew["Pressure"] = dtTcTrack.Rows[i]["PRESSURE"].ToString();
                    string wind = dtTcTrack.Rows[i]["WIND"].ToString();
                    drNew["Wind"] = wind;
                    string strLevel = GetLevelByWind(wind);
                    drNew["Level"] = strLevel;

                    int wr7 = int.Parse(dtTcTrack.Rows[i]["xx1"].ToString());
                    //int wr8 = int.Parse(dtTcTrack.Rows[j]["xx2"].ToString());
                    int wr10 = int.Parse(dtTcTrack.Rows[i]["xx2"].ToString());
                    // int wr12 = int.Parse(dtTcTrack.Rows[j]["xx4"].ToString());

                    drNew["WR7"] = wr7 == 0 ? "9999" : wr7.ToString();
                    drNew["WR8"] = "9999";// wr8 == 0 ? "9999" : wr8.ToString();
                    drNew["WR10"] = wr10 == 0 ? "9999" : wr10.ToString();
                    drNew["WR12"] = "9999";//wr12 == 0 ? "9999" : wr12.ToString();

                    TF_Info.Rows.Add(drNew);
                    TF_Info.AcceptChanges();
                }
                wcf.Data = TF_Info;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetTyphoonForecastPoints(string tfbh, string way, string strQBTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime dtQBTime = new DateTime();
                try
                {
                    dtQBTime = DateTime.ParseExact(strQBTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    throw new Exception ("错误的时间字符串。");
                }

                MySQLDatabase m_Database = new MySQLDatabase();
                string str_Get_TF_Info = string.Format("SELECT DISTINCT TFBH,ENGNAME,CHNNAME FROM tcrealtime WHERE TFBH='{0}' AND FCSTTYPE='BABJ'", tfbh);
                DataTable dtTFBH = m_Database.GetDataset(str_Get_TF_Info).Tables[0];
                if (dtTFBH.Rows.Count == 0)
                    throw new Exception("错误的台风编号。");

                DataRow drTFBH = dtTFBH.Rows[0];
                string strCNName = drTFBH["engname"].ToString();
                string strZHName = Encoding.Default.GetString(Encoding.GetEncoding("latin1").GetBytes(drTFBH["chnname"].ToString()));

                DataTable TF_Info = new DataTable();
                TF_Info.Columns.Add(new DataColumn("TFBH", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("ENName", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("CHName", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Way", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("DateTime", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Period", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Lon", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Lat", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Pressure", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Wind", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("Level", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("WR7", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("WR8", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("WR10", typeof(string)));
                TF_Info.Columns.Add(new DataColumn("WR12", typeof(string)));

                string strMYSQL = string.Format("SELECT DISTINCT YEAR,MON,DAY,HOUR,FHOUR,LAT,LON,PRESSURE,WIND,xx1,xx2,xx3,xx4 FROM tctracks_ts WHERE NNO ='{0}' AND WAY='{1}' AND FHOUR>0 and year=year('{2}') and Mon=MONTH('{2}') and Day=Day('{2}') and Hour=Hour('{2}') ORDER BY FHOUR ASC", tfbh, way, dtQBTime);
                DataTable dtTcTrack = m_Database.GetDataset(strMYSQL).Tables[0];
                for (int i = 0; i < dtTcTrack.Rows.Count; i++)
                {
                    DataRow drNew = TF_Info.NewRow();

                    drNew["TFBH"] = tfbh;
                    drNew["ENName"] = strCNName;
                    drNew["CHName"] = strZHName;
                    drNew["Way"] = way.ToUpper();
                    string strDateTime = string.Format("{0}-{1}-{2} {3}:00:00",
                        dtTcTrack.Rows[i]["YEAR"].ToString(),
                        dtTcTrack.Rows[i]["MON"].ToString(),
                        dtTcTrack.Rows[i]["DAY"].ToString(),
                        dtTcTrack.Rows[i]["HOUR"].ToString());

                    drNew["Period"] = dtTcTrack.Rows[i]["FHOUR"].ToString();
                    drNew["DateTime"] = DateTime.Parse(strDateTime).ToString("yyyy-MM-dd HH:mm:ss");


                    drNew["Lon"] = dtTcTrack.Rows[i]["LON"].ToString();
                    drNew["Lat"] = dtTcTrack.Rows[i]["LAT"].ToString();
                    drNew["Pressure"] = dtTcTrack.Rows[i]["PRESSURE"].ToString();
                    string wind = dtTcTrack.Rows[i]["WIND"].ToString();
                    drNew["Wind"] = wind;
                    string strLevel = GetLevelByWind(wind);
                    drNew["Level"] = strLevel;

                    int wr7 = int.Parse(dtTcTrack.Rows[i]["xx1"].ToString());
                    //int wr8 = int.Parse(dtTcTrack.Rows[j]["xx2"].ToString());
                    int wr10 = int.Parse(dtTcTrack.Rows[i]["xx2"].ToString());
                    // int wr12 = int.Parse(dtTcTrack.Rows[j]["xx4"].ToString());

                    drNew["WR7"] = wr7 == 0 ? "9999" : wr7.ToString();
                    drNew["WR8"] = "9999";// wr8 == 0 ? "9999" : wr8.ToString();
                    drNew["WR10"] = wr10 == 0 ? "9999" : wr10.ToString();
                    drNew["WR12"] = "9999";//wr12 == 0 ? "9999" : wr12.ToString();

                    TF_Info.Rows.Add(drNew);
                    TF_Info.AcceptChanges();
                }
                wcf.Data = TF_Info;
                wcf.Result = true;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        #endregion

        /// <summary>
        /// 从文本文件内获取到短信内容
        /// </summary>
        /// <param name="txtPath">参数：文本文件路径</param>
        /// <returns>返回：短信内容</returns>
        private  string GetMessageText(string txtPath)
        {
            try
            {
                StreamReader sr = new StreamReader(txtPath, Encoding.Default);
                string strLine = "";
                while (!strLine.Contains("长江口区天气预报："))
                {
                    strLine = sr.ReadLine();
                }
                strLine = sr.ReadLine();
                List<string> list_sentences = new List<string>();
                //按句号拆分
                string[] sentences = strLine.Split('。');
                for (int i = 0; i < sentences.Length; i++)
                {
                    string[] commas = sentences[i].Replace("都", "").Split('，');

                    List<string> tmp = new List<string>();
                    for (int j = 0; j < commas.Length; j++)
                        if (!commas[j].Contains("长江口"))
                            tmp.Add(commas[j]);

                    sentences[i] = string.Join("，", tmp.ToArray());
                    if (!sentences[i].Contains("火险等级") && !sentences[i].Contains("湿度"))
                        list_sentences.Add(sentences[i]);
                }
                return string.Join("。", list_sentences.ToArray()).Trim();
            }
            catch
            {
                return "";
            }
        }
        private string GetLevelByWind(string wind)
        {
            double windSpeed = double.Parse(wind);
            if (windSpeed < 0)
                return "error";
            if (windSpeed < 10.8)
                return "低涡";
            if (windSpeed < 17)
                return "热带低压";
            if (windSpeed < 24)
                return "热带风暴";
            if (windSpeed < 32)
                return "强热带风暴";
            if (windSpeed < 41)
                return "台风";
            if (windSpeed < 51)
                return "强台风";

            return "超强台风";
        }

        private string GetWindLev(double windspeed)
        {
            if (windspeed <= 0.2)
                return "0";
            if (windspeed <= 1.5)
                return "1";
            if (windspeed <= 3.3)
                return "2";
            if (windspeed <= 5.4)
                return "3";
            if (windspeed <= 7.9)
                return "4";
            if (windspeed <= 10.7)
                return "5";
            if (windspeed <= 13.8)
                return "6";
            if (windspeed <= 17.1)
                return "7";
            if (windspeed <= 20.7)
                return "8";
            if (windspeed <= 24.4)
                return "9";
            if (windspeed <= 28.4)
                return "10";
            if (windspeed <= 32.6)
                return "11";

            return "12";
        }

        public void SendSMS(string tel, string msg)
        {
            string connectionStr = "Driver={IBM DB2 ODBC DRIVER};Database=MASDB;hostname=192.168.169.13;port=50110;protocol=TCPIP; uid=db2inst1; pwd=vw3yYnVZNN";
            int num = this.ExecuteNonQuery(connectionStr, string.Concat(new string[]
			{
				"insert into TBL_SMSENDTASK( CREATORID,SMSENDEDNUM,OPERATIONTYPE,SUBOPERATIONTYPE,SENDTYPE,ORGADDR,DESTADDR,SM_CONTENT,SENDTIME,NEEDSTATEREPORT,SERVICEID,FEETYPE,FEECODE,SMTYPE,MESSAGEID,DESTADDRTYPE,SUBTIME,TASKSTATUS,SENDLEVEL,SENDSTATE,TRYTIMES,COUNT) values ('0000',0,'api','0',1,'106573000281','",
				tel,
				"','",
				msg,
				"',current timestamp,1,'MSH0010103','01','0',0,'0',0,current timestamp,0,0,0,0,1)"
			}));
        }
        public int ExecuteNonQuery(string connectionStr, string strSQL)
        {
            OdbcConnection odbcConnection = new OdbcConnection(connectionStr);
            if (string.IsNullOrEmpty(connectionStr))
            {
                throw new Exception("无传入数据连接字串");
            }
            if (string.IsNullOrEmpty(strSQL))
            {
                throw new Exception("无传入SQL语句");
            }
            int result;
            try
            {
                odbcConnection.Open();
                OdbcCommand odbcCommand = new OdbcCommand(strSQL, odbcConnection);
                int num = odbcCommand.ExecuteNonQuery();
                result = num;
            }
            catch (OdbcException ex)
            {
                throw ex;
            }
            catch (Exception ex2)
            {
                throw ex2;
            }
            finally
            {
                odbcConnection.Close();
                odbcConnection.Dispose();
            }
            return result;
        }
        /// <summary>
        /// 将WCF结果转换为流。
        /// </summary>
        /// <param name="wcf"></param>
        /// <returns></returns>
        public Stream ConvertWCFResult2Stream(WCFResult1 wcf)
        {
            string res = JsonConvert.SerializeObject(wcf);
            return new MemoryStream(Encoding.UTF8.GetBytes(res));
        }

    }
}
