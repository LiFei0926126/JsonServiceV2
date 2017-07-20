using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using Readearth.Data;
using System.Data;

namespace JsonService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“Typhoon”。
    public class TyphoonJsonService : ITyphoonJsonService
    {
        public Stream GetTyphoonWays()
        {
            WCFResult wcf = new WCFResult(false);
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
            return ServiceHelper.ConvertWCFResult2Stream(wcf);
        }
        public Stream GetTyphoons(string year)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                string str_Get_TF_Info = string.Format("SELECT DISTINCT TFBH,ENGNAME,CHNNAME FROM tcrealtime WHERE  CENTER='BABJ' and FcstType = 'BABJ' AND FCSTHOUR=0 AND year(DATETIME)={0} and engname<>'' ORDER BY DATETIME DESC", year);
                MySQLDatabase pMySQLDatabase = new MySQLDatabase();
                DataTable dt = pMySQLDatabase.GetDataset(str_Get_TF_Info).Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    while (true)
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
            return ServiceHelper.ConvertWCFResult2Stream(wcf);
        }
        public Stream GetTyphoonRealTimePoints(string tfbh)
        {
            WCFResult wcf = new WCFResult(false);
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
                    int iLevel = ServiceHelper.GetWindLevByWindSpeed(double.Parse(wind));
                    drNew["Level"] = iLevel;

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
            return ServiceHelper.ConvertWCFResult2Stream(wcf);
        }
        public Stream GetTyphoonForecastPoints(string tfbh, string way, string strQBTime)
        {
            WCFResult wcf = new WCFResult(false);
            try
            {
                DateTime dtQBTime = new DateTime();
                try
                {
                    dtQBTime = DateTime.ParseExact(strQBTime, "yyyyMMddHHmmss", null);
                }
                catch (Exception ex)
                {
                    throw new Exception("错误的时间字符串。");
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
                    string strLevel = ServiceHelper.GetTyphoonLevelByWindSpeed(double.Parse(wind));
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
            return ServiceHelper.ConvertWCFResult2Stream(wcf);
        }
    }
}
