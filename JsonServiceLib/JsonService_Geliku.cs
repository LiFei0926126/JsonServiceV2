using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Readearth.Data;
using QXJLHandleDatatable;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace JsonServiceLib
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“JsonService”。
    public class JsonService_Geliku : IJsonService_Geliku
    {
        #region Get
        public Stream GetAutoStationData_Geliku(string strQueryTime)
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


                string strSQL = string.Format("SELECT T2.DATETIME,T2.STATIONID,T1.STATIONNAME,T1.LON,T1.LAT,T1.TEMPERATURE,T1.RAINHOUR,T1.WINDSPEED,T1.WINDDIRECTION FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA]  AS T1 , (SELECT MAX(DATETIME) AS DATETIME,STATIONID FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE DATETIME >'{0}' AND DATETIME <='{1}' AND (DATEPART(MI,DATETIME)/5)%2=0  GROUP BY STATIONID) AS T2 WHERE T1.STATIONID=T2.STATIONID AND T1.DATETIME =T2.DATETIME ORDER BY T2.STATIONID;", dtQuery.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));
                //string strSQL = string.Format("SELECT DATETIME,STATIONID,STATIONNAME,LON,LAT,TEMPERATURE,RAINHOUR,WINDSPEED,WINDDIRECTION FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE DATETIME >'{0}' and DATETIME <='{1}' AND (DATEPART(MI,DATETIME)/5)%2=0  ORDER BY STATIONID ,DATETIME DESC", dtQuery.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));
                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);
                wcf.Result = true;
                wcf.Data = dtData;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetAutoStationDataByDatetime_Geliku(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                Database db = new Database();
                string strSQL = string.Format("SELECT DATETIME,STATIONID,STATIONNAME,LON,LAT,TEMPERATURE,RAINHOUR,WINDSPEED,WINDDIRECTION FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE DATETIME >'{0}' and DATETIME <='{1}' AND (DATEPART(MI,DATETIME)/5)%2=0  ORDER BY STATIONID ,DATETIME DESC", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);

                #region CSV
                string FileName = string.Format("AutoStationData_{0}_{1}.csv", dtStart.ToString("yyyyMMddHHmmss"), dtEnd.ToString("yyyyMMddHHmmss"));
                
                if (!Directory.Exists(@"F:\ReadearthCode\JSONPub\DownloadData"))
                    Directory.CreateDirectory(@"F:\ReadearthCode\JSONPub\DownloadData");

                StreamWriter sw = new StreamWriter(string.Format(@"F:\ReadearthCode\JSONPub\DownloadData\{0}", FileName), false, Encoding.UTF8);
                StringBuilder sb = new StringBuilder();
                string[] data = new string[dtData.Columns.Count];

                for (int i = 0; i < dtData.Columns.Count; i++)
                {
                    data[i] = dtData.Columns[i].ColumnName;
                }
                sb.AppendLine(string.Join(",", data));

                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    for (int j = 0; j < dtData.Columns.Count; j++)
                    {
                        data[j] = dr[j].ToString();
                    }
                    sb.AppendLine(string.Join(",", data));
                }
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();

                string strUri = @"http://61.152.126.152/JsonService/DownloadData/" + FileName; 
                #endregion

                wcf.Result = true;
                wcf.Data = strUri;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetAutoStationDataByDatetime_5mi_SanWei(string strStartTime, string strEndTime ,string type)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                Database db = new Database();
                string strSQL = string.Format("SELECT DATETIME,STATIONID,STATIONNAME,LON,LAT,TEMPERATURE,RAINHOUR,WINDSPEED,WINDDIRECTION FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE DATETIME >'{0}' and DATETIME <='{1}' ORDER BY STATIONID ,DATETIME", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);
                DataTable dtData1 = handle_Datatable_all_to_Datatable_Leijia.Trancefor_DataTable_To_DataTable_By_Type(dtData,int.Parse(type));

                wcf.Result = true;
                wcf.Data = dtData1;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetAutoStationDataByDatetime_5mi_Geliku(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                Database db = new Database();
                string strSQL = string.Format("SELECT DATETIME,STATIONID,STATIONNAME,LON,LAT,TEMPERATURE,RAINHOUR,WINDSPEED,WINDDIRECTION FROM [CITYDISASTERFORECAST].[DBO].[T_AUTOSTATIONDATA] WHERE DATETIME >'{0}' and DATETIME <='{1}' ORDER BY STATIONID ,DATETIME", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);
                #region CSV
                string FileName = string.Format("AutoStationData_5mi_Geliku_{0}_{1}.csv", dtStart.ToString("yyyyMMddHHmmss"), dtEnd.ToString("yyyyMMddHHmmss"));

                if (!Directory.Exists(@"F:\ReadearthCode\JSONPub\DownloadData"))
                    Directory.CreateDirectory(@"F:\ReadearthCode\JSONPub\DownloadData");

                StreamWriter sw = new StreamWriter(string.Format(@"F:\ReadearthCode\JSONPub\DownloadData\{0}", FileName), false, Encoding.UTF8);
                StringBuilder sb = new StringBuilder();
                string[] data = new string[dtData.Columns.Count];

                for (int i = 0; i < dtData.Columns.Count; i++)
                {
                    data[i] = dtData.Columns[i].ColumnName;
                }
                sb.AppendLine(string.Join(",", data));

                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    for (int j = 0; j < dtData.Columns.Count; j++)
                    {
                        data[j] = dr[j].ToString();
                    }
                    sb.AppendLine(string.Join(",", data));
                }
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();

                string strUri = @"http://61.152.126.152/JsonService/DownloadData/" + FileName;
                #endregion
                wcf.Result = true;
                wcf.Data = strUri;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetCurrentTyphoon()
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                MySQLDatabase m_Database = new MySQLDatabase();

                //DateTime dtnow = DateTime.Parse("2016-08-22 00:00:00");
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
                        if (int.Parse(fhour) > 0)
                            strDateTime = DateTime.Parse(strDateTime).AddHours(int.Parse(fhour)).ToString("yyyy-MM-dd HH:mm:ss");
                        drNew["DateTime"] = strDateTime;


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

                    list_TFs.Add(TF_Info);
                }

                //string strData = JsonConvert.SerializeObject(list_TFs);
                wcf.Result = true;
                wcf.Data = list_TFs;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);

        }
        public Stream GetHistoryTyphoon(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                
                MySQLDatabase m_Database = new MySQLDatabase();
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                #region 初始化时间
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {

                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {

                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                //dtStart = DateTime.Parse("2016-08-22 00:00:00");
                //dtEnd = DateTime.Parse("2016-08-25 00:00:00");
                //DateTime dtnow = DateTime.Now;
                string str_Get_TF_Info = string.Format("SELECT DISTINCT TFBH,ENGNAME,CHNNAME FROM tcrealtime WHERE CENTER='BABJ' AND FCSTTYPE='BABJ' AND FCSTHOUR=0 AND DATETIME>'{0}' AND DATETIME<='{1}' ORDER BY DATETIME DESC", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
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
                        if (int.Parse(fhour) > 0)
                            strDateTime = DateTime.Parse(strDateTime).AddHours(int.Parse(fhour)).ToString("yyyy-MM-dd HH:mm:ss");
                        drNew["DateTime"] = strDateTime;


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
                    list_TFs.Add(TF_Info);
                }

                //string strData = JsonConvert.SerializeObject(list_TFs);
                wcf.Result = true;
                wcf.Data = list_TFs;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);

        }
        public Stream GetRadarData(string strStationCode,string strElevation, string strQueryTime)
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

                //Database db = new Database();
                string strSQL = string.Format(@"SELECT TOP 1 [PATH],[DATETIME]  FROM [CITYDISASTERFORECAST].[DBO].[T_CACHE1]  WHERE DATETIME >'{0}' AND DATETIME <='{1}' AND PATH LIKE '%\{2}\19\{3}\%'  ORDER BY DATETIME DESC", dtQuery.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"), strStationCode, strElevation);
                Database db = new Database();
                DataTable dt = db.GetDataTable(strSQL);
               
                
                string strUri = dt.Rows[0]["PATH"].ToString().Replace(@"R:\radprodMxdPublish\", "http://61.152.126.152/RadarData/").Replace("\\", "/");
                string strDate = dt.Rows[0]["DATETIME"].ToString();

                RadarImage ri = new RadarImage(strUri, DateTime.Parse(strDate), 116.393553071857, 127.370446928143, 35.4938734230543, 26.433067725099);

                wcf.Result = true;
                wcf.Data = ri;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        enum ThunderSource
        {
            LS,
            EN,
            ADTD
        }
        public Stream GetThunderData(string strSource, string strStartTime, string strEndTime)
        {
            
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region Init Paras
                DateTime dtStartTime = DateTime.Now;
                DateTime dtEndTime = DateTime.Now;
                try
                {
                    dtStartTime = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEndTime = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }

                ThunderSource ts = (ThunderSource)Enum.Parse(typeof(ThunderSource), strSource);
                #endregion
                                
                string strSQL = "";
                string strMsg = "";
                switch (ts)
                {
                    case ThunderSource.LS:
                        strSQL = string.Format("SELECT LON,LAT,DATETIME,PEAK_KA FROM CITYDISASTERFORECAST.DBO.T_LIGHTNING WHERE DATETIME BETWEEN '{0}' AND '{1}' ORDER BY DATETIME ASC", dtStartTime, dtEndTime);

                    strMsg = "LS数据源";
                        break;
                    case ThunderSource.EN:
                        strSQL = string.Format("SELECT LONGITUDE AS LON,LATITUDE AS LAT,DATEADD(HH,8,LIGHTNING_TIME) AS DATETIME,Amplitude/1000 as PEAK_KA FROM CITYDISASTERFORECAST.DBO.[T_LTGFLASHPORTIONS] WHERE DATEADD(HH,8,LIGHTNING_TIME) BETWEEN '{0}' AND '{1}'   and Stroke_Type ='0'  ORDER BY DATETIME ASC", dtStartTime, dtEndTime);

                    strMsg = "EN数据源";
                        break;
                    case ThunderSource.ADTD:
                        strSQL = string.Format("SELECT [Lon] AS LON,[Lat] AS LAT,[LightTime] AS DATETIME,[PEAK_KA] as PEAK_KA FROM CITYDISASTERFORECAST.DBO.[T_LightADTD] WHERE [LightTime] BETWEEN '{0}' AND '{1}' ORDER BY [LightTime] ASC", dtStartTime, dtEndTime);

                    strMsg = "ADTD数据源";
                        break;
                }
                //if (ConfigurationManager.AppSettings["LightningSource"].ToString() == "V")
                //{
                //    strSQL = string.Format("SELECT LON,LAT,DATETIME,PEAK_KA FROM CITYDISASTERFORECAST.DBO.T_LIGHTNING WHERE DATETIME BETWEEN '{0}' AND '{1}' ORDER BY DATETIME ASC", dtStartTime, dtEndTime);

                //    strMsg = "LS数据源";
                //}
                //else
                //{
                //    strSQL = string.Format("SELECT LONGITUDE AS LON,LATITUDE AS LAT,DATEADD(HH,8,LIGHTNING_TIME) AS DATETIME,Amplitude/1000 as PEAK_KA FROM CITYDISASTERFORECAST.DBO.[T_LTGFLASHPORTIONS] WHERE DATEADD(HH,8,LIGHTNING_TIME) BETWEEN '{0}' AND '{1}'   and Stroke_Type ='0'  ORDER BY DATETIME ASC", dtStartTime, dtEndTime);

                //    strMsg = "EN数据源";
                //}
                Database db = new Database();
                DataTable dt = db.GetDataTable(strSQL);
                wcf.Data = dt;
                wcf.Result = true;
                wcf.Message = strMsg;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
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
                {
                    wcf.Result = false;
                    wcf.Data = "未查询到当前有生效预警。";
                }
                   
                //return new MemoryStream(Encoding.Default.GetBytes(res));
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetWaterStationData_Geliku(string strQueryTime)
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
                string strSQL = string.Format("SELECT t1.STATIONID,t1.DATATIME,t1.STATIONNAME,t1.WATERDEPTH FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] t1,(SELECT STATIONID,max(DATATIME) as DATATIME FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] WHERE DATATIME >'{0}' AND DATATIME <='{1}' group by stationID) t2 where t1.STATIONID=t2.STATIONID and t1.DATATIME=t2.DATATIME ORDER BY STATIONID ,DATATIME DESC", dtQuery.AddMinutes(-60).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));


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
        public Stream GetWaterStationDataByDatetime_Geliku(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                Database db = new Database();
                string strSQL = string.Format("SELECT STATIONID,DATATIME,STATIONNAME,WATERDEPTH FROM [CITYDISASTERFORECAST].[DBO].[TBL_WATERSTATIONDATA] WHERE DATATIME >'{0}' AND DATATIME <='{1}' ORDER BY STATIONID ,DATATIME DESC", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);

                #region CSV
                string FileName = string.Format("WaterStationData_{0}_{1}.csv", dtStart.ToString("yyyyMMddHHmmss"), dtEnd.ToString("yyyyMMddHHmmss"));

                if (!Directory.Exists(@"F:\ReadearthCode\JSONPub\DownloadData"))
                    Directory.CreateDirectory(@"F:\ReadearthCode\JSONPub\DownloadData");

                StreamWriter sw = new StreamWriter(string.Format(@"F:\ReadearthCode\JSONPub\DownloadData\{0}", FileName), false, Encoding.UTF8);
                StringBuilder sb = new StringBuilder();
                string[] data = new string[dtData.Columns.Count];

                for (int i = 0; i < dtData.Columns.Count; i++)
                {
                    data[i] = dtData.Columns[i].ColumnName;
                }
                sb.AppendLine(string.Join(",", data));

                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    for (int j = 0; j < dtData.Columns.Count; j++)
                    {
                        data[j] = dr[j].ToString();
                    }
                    sb.AppendLine(string.Join(",", data));
                }
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();

                string strUri = @"http://61.152.126.152/JsonService/DownloadData/" + FileName;
                #endregion

                wcf.Result = true;
                wcf.Data = strUri;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetWaterOut_Geliku(string strQueryTime)
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

                string strSQL = string.Format("SELECT T2.DATETIME,T2.STATIONID,T1.STATIONNAME,T1.QUYU,T1.SHUILIPIAN,T1.LON,T1.LAT,T1.XX,T1.YY,T1.BLUEWATER,T1.HIGHWATER,T1.OUTWATER,T1.SAFETYWATER,T1.HELIU,T1.DATASOURCE  FROM [CITYDISASTERFORECAST].[DBO].[T_WATEROUT1] AS T1  ,  (SELECT MAX(DATETIME) AS DATETIME,STATIONID FROM [CITYDISASTERFORECAST].[DBO].[T_WATEROUT1] WHERE DATETIME>='{0}' AND DATETIME <='{1}' AND  (LON IS NOT NULL AND LAT IS NOT NULL) GROUP BY STATIONID) AS T2 WHERE T1.STATIONID=T2.STATIONID AND T1.DATETIME =T2.DATETIME ORDER BY T2.STATIONID;", dtQuery.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));
                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);
                wcf.Result = true;
                wcf.Data = dtData;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetWaterOutByDatetime_Geliku(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                Database db = new Database();
                string strSQL = string.Format("SELECT DATETIME,STATIONID,STATIONNAME,QUYU,SHUILIPIAN,LON,LAT,XX,YY,BLUEWATER,HIGHWATER,OUTWATER,SAFETYWATER,HELIU,DATASOURCE  FROM [CITYDISASTERFORECAST].[DBO].[T_WATEROUT1]  WHERE DATETIME>'{0}' AND DATETIME <='{1}' ORDER BY DATETIME DESC", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);

                #region CSV
                string FileName = string.Format("WaterOut_{0}_{1}.csv", dtStart.ToString("yyyyMMddHHmmss"), dtEnd.ToString("yyyyMMddHHmmss"));

                if (!Directory.Exists(@"F:\ReadearthCode\JSONPub\DownloadData"))
                    Directory.CreateDirectory(@"F:\ReadearthCode\JSONPub\DownloadData");

                StreamWriter sw = new StreamWriter(string.Format(@"F:\ReadearthCode\JSONPub\DownloadData\{0}", FileName), false, Encoding.UTF8);
                StringBuilder sb = new StringBuilder();
                string[] data = new string[dtData.Columns.Count];

                for (int i = 0; i < dtData.Columns.Count; i++)
                {
                    data[i] = dtData.Columns[i].ColumnName;
                }
                sb.AppendLine(string.Join(",", data));

                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    for (int j = 0; j < dtData.Columns.Count; j++)
                    {
                        data[j] = dr[j].ToString();
                    }
                    sb.AppendLine(string.Join(",", data));
                }
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
                //OperationContext context = OperationContext.Current;
                //System.ServiceModel.Dispatcher.EndpointDispatcher pEndpointDispatcher = context.EndpointDispatcher as System.ServiceModel.Dispatcher.EndpointDispatcher;
                //MessageProperties properties = context.IncomingMessageProperties;
                //RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                
                string strUri = @"http://61.152.126.152/JsonService/DownloadData/" + FileName;
                #endregion
               
                wcf.Result = true;
                wcf.Data = strUri;
            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        struct GridRainDataset
        {
            public DateTime InitialForecastDate;
            public string DataType;
            public ImageExtent ImageExtent;
            public List<GridRainData> Data;
        }
        struct GridRainDataset1
        {
            public DateTime InitialForecastDate;
            public string DataType;
            public ImageExtent ImageExtent;
            public GridRainData MaxData;
            public List<GridRainData> Data;
        }
        struct GridRainData
        {
            public DateTime ForecastDate;
            public string Uri;
            public string TFW;
            public string XML;
        }
        public Stream GetGridRainData_Geliku(string strType, string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                
                Database db = new Database();
                string strSQL = string.Format("SELECT [NAME],[FORECASTDATE],[PERIOD],[FOLDER]  FROM [CITYDISASTERFORECAST].[DBO].[T_GRIDRAIN]  WHERE TYPE='{0}' AND FORECASTDATE>='{1}' AND  FORECASTDATE<='{2}'  ORDER BY FORECASTDATE DESC,PERIOD ASC", strType, dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);

                List<GridRainDataset> pGridRainDatasets = new List<GridRainDataset>();
                string strLastForecastDate = "";
                GridRainDataset tmp_GridRainDataset=new GridRainDataset();
                List<GridRainData> tmp_GrinDatas = null;
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];
                    
                    string strName = dr["NAME"].ToString();
                    string strForecastDate = dr["FORECASTDATE"].ToString();
                    string strPeriod = dr["PERIOD"].ToString();
                    string strFolder = dr["FOLDER"].ToString();

                    if (string.IsNullOrEmpty(strLastForecastDate) || strLastForecastDate != strForecastDate)
                    {
                        #region 数据加入列表
                        if (tmp_GrinDatas != null)
                        {
                            tmp_GridRainDataset.Data = tmp_GrinDatas;
                            pGridRainDatasets.Add(tmp_GridRainDataset);
                        }
                        #endregion

                        #region 数据初始化
                        tmp_GrinDatas = new List<GridRainData>();
                        tmp_GridRainDataset = new GridRainDataset();
                        tmp_GridRainDataset.InitialForecastDate = DateTime.Parse(strForecastDate);
                        strLastForecastDate = strForecastDate;
                        switch (strType)
                        {
                            case "01":
                                tmp_GridRainDataset.ImageExtent = new ImageExtent(112.985, 120.005, 39.035, 23.015);
                                tmp_GridRainDataset.DataType = strType.Replace("01", "短临格点降水预报");
                                break;
                            case "02":
                                tmp_GridRainDataset.ImageExtent = new ImageExtent(112.975, 120.975, 39.025, 23.025);
                                tmp_GridRainDataset.DataType = strType.Replace("02", "短期格点降水预报");
                                break;
                            case "03":
                                tmp_GridRainDataset.ImageExtent = new ImageExtent(115.88801, 123.842095, 34.4735615, 27.5710335);
                                tmp_GridRainDataset.DataType = strType.Replace("03", "雷达数值融合预报");
                                break;
                        }
                        #endregion
                    }

                    GridRainData grd = new GridRainData();
                    if (strType == "03")
                        grd.ForecastDate = DateTime.Parse(strForecastDate).AddMinutes(int.Parse(strPeriod));
                    else
                        grd.ForecastDate = DateTime.Parse(strForecastDate).AddHours(int.Parse(strPeriod));
                    grd.Uri = string.Format("http://61.152.126.152/TiffData/{0}/{1}", strFolder.Replace(@"\","/"), strName);
                    grd.TFW = grd.Uri.Replace(".tif", ".tfw");
                    grd.XML = grd.Uri + ".aux.xml";
                    tmp_GrinDatas.Add(grd);
                }

                tmp_GridRainDataset.Data = tmp_GrinDatas;
                pGridRainDatasets.Add(tmp_GridRainDataset);

                wcf.Data = pGridRainDatasets;
                wcf.Result = true;

            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }

        public Stream GetQPFData_Geliku(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion


                Database db = new Database();
                string strSQL = string.Format("SELECT [NAME],[FORECASTDATE],[PERIOD],[FOLDER]  FROM [CITYDISASTERFORECAST].[DBO].[T_QPF]  WHERE FORECASTDATE>'{0}' AND  FORECASTDATE<'{1}'  ORDER BY FORECASTDATE DESC", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);

                List<GridRainDataset> pGridRainDatasets = new List<GridRainDataset>();
                string strLastForecastDate = "";
                GridRainDataset tmp_GridRainDataset = new GridRainDataset();
                List<GridRainData> tmp_GrinDatas = null;
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];

                    string strName = dr["NAME"].ToString();
                    string strForecastDate = dr["FORECASTDATE"].ToString();
                    string strPeriod = dr["PERIOD"].ToString();
                    string strFolder = dr["FOLDER"].ToString();

                    if (string.IsNullOrEmpty(strLastForecastDate) || strLastForecastDate != strForecastDate)
                    {
                        #region 数据加入列表
                        if (tmp_GrinDatas != null)
                        {
                            tmp_GridRainDataset.Data = tmp_GrinDatas;
                            pGridRainDatasets.Add(tmp_GridRainDataset);
                        }
                        #endregion

                        #region 数据初始化
                        tmp_GrinDatas = new List<GridRainData>();
                        tmp_GridRainDataset = new GridRainDataset();
                        tmp_GridRainDataset.InitialForecastDate = DateTime.Parse(strForecastDate);
                        strLastForecastDate = strForecastDate;

                        tmp_GridRainDataset.ImageExtent = new ImageExtent(119.877, 124.207, 33.012, 28.682);
                        tmp_GridRainDataset.DataType = "QPF";
                        #endregion
                    }

                    GridRainData grd = new GridRainData();

                    grd.ForecastDate = DateTime.Parse(strForecastDate).AddMinutes(int.Parse(strPeriod));
                    grd.Uri = string.Format("http://61.152.126.152/TiffData/QPF/{0}/{1}", strFolder.Replace(@"\", "/"), strName);
                    grd.TFW = grd.Uri.Replace(".tif", ".tfw");
                    grd.XML = grd.Uri + ".aux.xml";
                    tmp_GrinDatas.Add(grd);
                }

                tmp_GridRainDataset.Data = tmp_GrinDatas;
                pGridRainDatasets.Add(tmp_GridRainDataset);

                wcf.Data = pGridRainDatasets;
                wcf.Result = true;

            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetUrbanFloodData_Geliku(string strType, string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                string strFilter = strType == "01" ? "%_02401" : "%07203";

                Database db = new Database();
                string strSQL = string.Format("SELECT [DATA_START_TIME] AS FORECASTDATE,[FILES_RESULT]  FROM [CITYDISASTERFORECAST].[DBO].[T_MODELSERVERINFO] WHERE [FILE_NAME] LIKE '{0}' AND [DATA_START_TIME] >='{1}' AND [DATA_START_TIME] <='{2}'  ORDER BY FORECASTDATE DESC", strFilter, dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);

                List<GridRainDataset1> pGridRainDatasets = new List<GridRainDataset1>();

                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    DataRow dr = dtData.Rows[i];

                    string strForecastDate = dr["FORECASTDATE"].ToString();
                    string[] strFiles = dr["FILES_RESULT"].ToString().TrimEnd(';').Split(';');

                    GridRainDataset1 tmp_GridRainDataset = new GridRainDataset1();
                    tmp_GridRainDataset.InitialForecastDate = DateTime.Parse(strForecastDate);
                    tmp_GridRainDataset.ImageExtent = new ImageExtent(121.342599, 121.643849, 31.378841, 31.120391);
                    tmp_GridRainDataset.DataType = "UrbanFlood";
                    List<GridRainData> tmp_GridRainData = new List<GridRainData>();
                    for (int j = 0; j < strFiles.Length; j++)
                    {
                        GridRainData grd = new GridRainData();
                        FileInfo fi = new FileInfo(strFiles[j]);
                        string[] tmps=fi.Name.Split(new char[]{'_','.'});
                        if (tmps[3].ToLower() == "max")
                        {
                            grd.ForecastDate = tmp_GridRainDataset.InitialForecastDate;
                            grd.Uri = "http://61.152.126.152/RadarData/" + strFiles[j].Replace(@"\", "/");
                            grd.TFW = grd.Uri.Replace(".tif", ".tfw");
                            grd.XML = grd.Uri+".aux.xml";

                            tmp_GridRainDataset.MaxData = grd;
                        }
                        else
                        {
                            grd.ForecastDate = tmp_GridRainDataset.InitialForecastDate.AddHours(int.Parse(tmps[3]));
                            grd.Uri = "http://61.152.126.152/RadarData/" + strFiles[j].Replace(@"\", "/");
                            grd.TFW = grd.Uri.Replace(".tif", ".tfw");
                            grd.XML = grd.Uri + ".aux.xml";
                            tmp_GridRainData.Add(grd);
                        }
                    }

                    tmp_GridRainDataset.Data = tmp_GridRainData;

                    pGridRainDatasets.Add(tmp_GridRainDataset);
                }
                wcf.Data = pGridRainDatasets;
                wcf.Result = true;

            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }


        public Stream GetYPWaterStation(string strQueryTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                DateTime dtQuery = DateTime.Now;

                #region 序列化时间
                try
                {
                    dtQuery = DateTime.ParseExact(strQueryTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                #endregion


                string strSQL = string.Format("SELECT T2.DATETIME,T2.ID,T1.LON,T1.LAT,T1.Value FROM [CITYDISASTERFORECAST].[DBO].[T_SWZZ_WaterData]  AS T1 , (SELECT MAX(DATETIME) AS DATETIME,ID FROM [CITYDISASTERFORECAST].[DBO].[T_SWZZ_WaterData] WHERE DATETIME >'{0}' AND DATETIME <='{1}'  GROUP BY ID) AS T2 WHERE T1.ID=T2.ID AND T1.DATETIME =T2.DATETIME ORDER BY T2.ID;", dtQuery.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm:ss"), dtQuery.ToString("yyyy-MM-dd HH:mm:ss"));
                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);
                wcf.Result = true;
                wcf.Data = dtData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetYPWaterStationByTime(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                #endregion


                string strSQL = string.Format("SELECT ID,[DATETIME] ,[LON] ,[LAT] ,[VALUE]  FROM [CITYDISASTERFORECAST].[DBO].[T_SWZZ_WATERDATA]  WHERE DATETIME >'{0}' AND DATETIME <='{1}' ORDER BY ID,DATETIME DESC;", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                Database db = new Database();
                DataTable dtData = db.GetDataTable(strSQL);
                wcf.Result = true;
                wcf.Data = dtData;
            }
            catch (Exception ex)
            {
                wcf.Result = false;
                wcf.Message = ex.Message;
                //string res = JsonConvert.SerializeObject(ex);
                //return new MemoryStream(Encoding.UTF8.GetBytes(res));
            }
            return ConvertWCFResult2Stream(wcf);
        }


        public Stream GetDisasterDetailData_Geliku(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion
                                
                Database db = new Database();
                string strSQL = string.Format("SELECT DATETIME_DISASTER,[LONTITUDE],[LATITUDE],[CASE_ADDR],[CASE_DESC],[ACCEPTER],[CODE_DISASTER],[DISTRICT]  FROM [SDE].[dbo].[ALARMDETAIL2013]  WHERE DATETIME_DISASTER>='{0}' AND  DATETIME_DISASTER<='{1}' ORDER BY [DATETIME_DISASTER] DESC", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);
                
                wcf.Data = dtData;
                wcf.Result = true;

            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        public Stream GetRealDisasterDetailData_Geliku(string strStartTime, string strEndTime)
        {
            WCFResult1 wcf = new WCFResult1(false);
            try
            {
                #region 序列化时间
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
                try
                {
                    dtStart = DateTime.ParseExact(strStartTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                try
                {
                    dtEnd = DateTime.ParseExact(strEndTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    wcf.Message = ex.Message;
                    return ConvertWCFResult2Stream(wcf);
                }
                #endregion

                Database db = new Database();
                string strSQL = string.Format("SELECT DATETIME_DISASTER,[LONTITUDE],[LATITUDE],[CASE_ADDR],[CASE_DESC],[ACCEPTER],[CODE_DISASTER],[DISTRICT]  FROM [CityDisasterForecast].[DBO].[ALARMDETAIL2013]  WHERE DATETIME_DISASTER>='{0}' AND  DATETIME_DISASTER<='{1}' ORDER BY [DATETIME_DISASTER] DESC", dtStart.ToString("yyyy-MM-dd HH:mm:ss"), dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
                DataTable dtData = db.GetDataTable(strSQL);

                wcf.Data = dtData;
                wcf.Result = true;

            }
            catch (Exception ex)
            {
                wcf.Message = ex.Message;
            }
            return ConvertWCFResult2Stream(wcf);
        }
        #endregion


        #region Public
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
        private string GetLevelByWind(string wind)
        {
            double windSpeed = double.Parse(wind);
            if (windSpeed <= 0)
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
        #endregion

        #region Private
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
                DataSet dtAllWarnContent = m_Database.GetDataset(strWarningTextSql);
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
        #endregion
    }
}
