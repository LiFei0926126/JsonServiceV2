using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Configuration;

namespace JsonServiceLib
{
    public class AppAlive
    {
        public string GUID;
        public string Datetime;
        public string UserName;
        public string Platform;
        public string Version;
    }


    public class Alarm
    {
        public string Name{get;set;}
        public DateTime FORECASTDATE{get;set;}
        public string   TYPE{get;set;}
        public string   OPERATION{get;set;}
        public string   LEVEL{get;set;}
        public string   CONTENT{get;set;}
        public string   GUIDE{get;set;}
    }
    public class Alarm1 : Alarm
    {
        public Alarm[] SubAlarms;    
    }

    public class ThunderRemind
    {
        public string Type = "闪电提醒";
        public string Message;
        public string DataSource;
        public string PublishTime;
        public string InvalidTime;
    }
    public class WCFResult
    {
        #region 私有变量
        bool m_Result;
        string m_Message;
        string m_Data;
        #endregion

        #region 构造函数
        public WCFResult(bool result)
        {
            m_Result = result;
        }
        public WCFResult(bool result, string strMessage)
        {
            m_Result = result;
            m_Message = strMessage;
        }
        public WCFResult(bool result, string strMessage, string strData)
        {
            m_Result = result;
            m_Message = strMessage;
            m_Data = strData;
        }
        #endregion

        #region 属性
        public bool Result
        {
            get
            {
                return m_Result;
            }
            set
            {
                m_Result = value;
            }
        }
        public string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                m_Message = value;
            }
        }
        public string Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                m_Data = value;
            }
        }
        #endregion

    }
    public class WCFResult1
    {
        #region 私有变量
        bool m_Result;
        string m_Message;
        object m_Data;
        #endregion

        #region 构造函数
        public WCFResult1(bool result)
        {
            m_Result = result;
        }
        public WCFResult1(bool result, string strMessage)
        {
            m_Result = result;
            m_Message = strMessage;
        }
        public WCFResult1(bool result, string strMessage, object objData)
        {
            m_Result = result;
            m_Message = strMessage;
            m_Data = objData;
        }
        #endregion

        #region 属性
        public bool Result
        {
            get
            {
                return m_Result;
            }
            set
            {
                m_Result = value;
            }
        }
        public string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                m_Message = value;
            }
        }
        public object Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                m_Data = value;
            }
        }
        #endregion

    }
    /// <summary>
    /// <c>雷达图片属性</c>
    /// </summary>    
    public class RadarImage
    {
        #region 成员变量
        string _RadarImageUrl = string.Empty;
        double _TimeSecond = double.NaN;
        ImageExtent _ImageExtent;
        string _RadarImageTFW = string.Empty;
        string _RadarImageXML = string.Empty;
        #endregion

        #region 构造函数
        public RadarImage(string radarImageUrl, DateTime imageDateTime, double left, double right, double top, double bottom)
        {
            _RadarImageUrl = radarImageUrl;
            _TimeSecond = imageDateTime.ToUniversalTime().Subtract(DateTime.Parse("1970-1-1")).TotalSeconds;
            //(imageDateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            _ImageExtent = new ImageExtent(left, right, top, bottom);
        }
        #endregion

        #region 属性
        /// <summary>
        /// <c>图片URL</c>
        /// </summary>
        
        public Uri RadarImageUrl
        {
            get
            {
                return new Uri(_RadarImageUrl);
            }
        }
        public string RadarImageTFW
        {
            get
            {
                return _RadarImageUrl.Replace(".tif", ".tfw").Replace(".png", ".tfw");
            }
        }
        public string RadarImageXML
        {
            get
            {
                return _RadarImageUrl+".aux.xml";
            }
        }
        // <summary>
        /// <c>图片时间（距1970-1-1的秒数）</c>
        /// </summary>
        
        public double TimeSecond
        {
            get
            {
                return _TimeSecond;
            }
        }
        // <summary>
        /// <c>图片范围</c>
        /// </summary>
        
        public ImageExtent ImageExtent
        {
            get
            {
                return _ImageExtent;
            }
        }
        #endregion
    }

    /// <summary>
    /// <c>图片范围</c>
    /// </summary>
    
    public class ImageExtent
    {
        #region 成员变量
        double _left = double.NaN
            , _right = double.NaN
            , _top = double.NaN
            , _bottom = double.NaN;
        #endregion

        #region 构造函数
        /// <summary>
        /// <c>构造函数</c>
        /// </summary>
        /// <param name="left"><c>左边界</c></param>
        /// <param name="right"><c>右边界</c></param>
        /// <param name="top"><c>上边界</c></param>
        /// <param name="bottom"><c>下边界</c></param>
        public ImageExtent(double left, double right, double top, double bottom)
        {
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
        }
        #endregion

        #region 属性
        /// <summary>
        /// <c>左边界</c>
        /// </summary>
        
        public double Left
        {
            get
            {
                return _left;
            }
        }
        /// <summary>
        /// <c>右边界</c>
        /// </summary>
        
        public double Right
        {
            get
            {
                return _right;
            }
        }
        /// <summary>
        /// <c>上边界</c>
        /// </summary>
        
        public double Top
        {
            get
            {
                return _top;
            }
        }
        /// <summary>
        /// <c>下边界</c>
        /// </summary>
        
        public double Bottom
        {
            get
            {
                return _bottom;
            }
        }
        #endregion
    }

    /// <summary>
    /// <c>灾害描述</c>
    /// </summary>
    
    public class UpLoadDisaster
    {
        #region 成员变量
        double _Lon = double.NaN
            , _Lat = double.NaN;

        DateTime _OccurTime = DateTime.MinValue;
        DateTime _UpLoadTime = DateTime.MinValue;
        string _Type = "";
        string _Type_zhCN = "";
        string _Description = "";
        string _DamageDes = "";
        string _uri = "";
        string _base64Str = "";
        string _Tag = "";
        #endregion

        #region 构造函数
        /// <summary>
        /// <c>构造函数</c>
        /// </summary>
        /// <param name="lon"><c>经度</c></param>
        /// <param name="lat"><c>纬度</c></param>
        /// <param name="type"><c>灾害类型</c></param>
        /// <param name="Description"><c>灾害描述</c></param>
        /// <param name="DamageDes"><c>损失描述</c></param>
        /// <param name="occurTime"><c>发生时间</c></param>
        /// <param name="UploadTime"><c>上传时间</c></param>
        public UpLoadDisaster(double lon, double lat, string type, string Description, string DamageDes, DateTime occurTime, DateTime UploadTime)
        {
            _Lon = lon;
            _Lat = lat;
            _OccurTime = occurTime;
            _UpLoadTime = UploadTime;
            _Type = type;
            _Description = Description;
            _DamageDes = DamageDes;
        }

        public UpLoadDisaster()
        {
        }
        #endregion

        #region 属性
        /// <summary>
        /// <c>经度</c>
        /// </summary>
        
        public double Lon
        {
            get
            {
                return _Lon;
            }
            set
            {
                _Lon = value;
            }
        }
        /// <summary>
        /// <c>纬度</c>
        /// </summary>
        
        public double Lat
        {
            get
            {
                return _Lat;
            }
            set
            {
                _Lat = value;
            }
        }
        /// <summary>
        /// <c>发生时间</c>
        /// </summary>
        
        public DateTime OccurTime
        {
            get
            {
                return _OccurTime;
            }
            set
            {
                _OccurTime = value;
            }
        }
        /// <summary>
        /// <c>上传时间</c>
        /// </summary>
        
        public DateTime UpLoadTime
        {
            get
            {
                return _UpLoadTime;
            }
            set
            {
                _UpLoadTime = value;
            }
        }
        /// <summary>
        /// <c>灾情描述</c>
        /// </summary>
        
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }
        /// <summary>
        /// <c>灾害类型</c>
        /// </summary>
        
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }
        /// <summary>
        /// <c>灾害类型</c>
        /// </summary>
        
        public string Type_zhCN
        {
            get
            {
                return _Type_zhCN;
            }
            set
            {
                _Type_zhCN = value;
            }
        }
        /// <summary>
        /// <c>损失描述</c>
        /// </summary>
        
        public string DamageDes
        {
            get
            {
                return _DamageDes;
            }
            set
            {
                _DamageDes = value;
            }
        }
        /// <summary>
        /// <c>图片URL</c>
        /// </summary>
        
        public string ImageUrl
        {
            get
            {
                return _uri;
            }
            set
            {
                _uri = value;
            }
        }
        /// <summary>
        /// <c>图片Base64Str</c>
        /// </summary>
        
        public string Base64Str
        {
            get
            {
                return _base64Str;
            }
            set
            {
                _base64Str = value;
            }
        }
        /// <summary>
        /// <c>灾情标签</c>
        /// </summary>
        
        public string Tag
        {
            get
            {
                return _Tag;
            }
            set
            {
                _Tag = value;
            }
        }
        #endregion
    }

    public class Lightning
    {
        #region 成员变量
        double _Lon = double.NaN
            , _Lat = double.NaN;
        DateTime _Datetime = DateTime.MinValue;
        #endregion

        #region 构造函数
        /// <summary>
        /// <c>构造函数</c>
        /// </summary>
        /// <param name="lon"><c>经度</c></param>
        /// <param name="lat"><c>纬度</c></param>
        /// <param name="dataTime"><c>时间</c></param>
        public Lightning(double lon, double lat, DateTime dateTime)
        {
            _Lon = lon;
            _Lat = lat;
            Datetime = dateTime;
        }
        #endregion

        #region 属性
        /// <summary>
        /// <c>经度</c>
        /// </summary>
        
        public double Lon
        {
            get
            {
                return _Lon;
            }
            set
            {
                _Lon = value;
            }
        }
        /// <summary>
        /// <c>纬度</c>
        /// </summary>
        
        public double Lat
        {
            get
            {
                return _Lat;
            }
            set
            {
                _Lat = value;
            }
        }
        /// <summary>
        /// <c>发生时间</c>
        /// </summary>
        
        public DateTime Datetime
        {
            get
            {
                return _Datetime;
            }
            set
            {
                _Datetime = value;
            }
        }
        /// <summary>
        /// <c>发生时间</c>
        /// </summary>
        public double UnixDatetime
        {
            get
            {
                try
                {
                    return DateTime.Parse(_Datetime.ToString("yyyy-MM-dd HH:mm:00")).ToUniversalTime().Subtract(DateTime.Parse("1970-1-1")).TotalSeconds;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        /// <summary>
        /// <c>发生时间</c>
        /// </summary>
        public string StrDatetime
        {
            get
            {
                try
                {
                    return _Datetime.ToString("yyyyMMddHHmm00");
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// <c>天气头条</c>
    /// </summary>
    public class News
    {
        #region 成员变量
        string _title = "";
        string _SimpleDescription = "";
        string _PicPath = "";
        string _URI = "";
        string _Base64Str = "";
        string _StrTime = "";
        #endregion

        #region 构造函数
        #endregion

        #region 属性
        /// <summary>
        /// <c>标题</c>
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
        /// <summary>
        /// <c>简要描述</c>
        /// </summary>
        public string SimpleDescription
        {
            get
            {
                return _SimpleDescription;
            }
            set
            {
                _SimpleDescription = value;
            }
        }
        /// <summary>
        /// <c>图片Uri</c>
        /// </summary>
        public string ImageUrl
        {
            get
            {
                return _PicPath;
            }
            set
            {
                _PicPath = value;
            }
        }
        /// <summary>
        /// <c>正文Uri</c>
        /// </summary>
        public string Uri
        {
            get
            {
                return _URI;
            }
            set
            {
                _URI = value;
            }
        }
        /// <summary>
        /// <c>正文Uri</c>
        /// </summary>
        public string Base64Str
        {
            get
            {
                return _Base64Str;
            }
            set
            {
                _Base64Str = value;
            }
        }
        /// <summary>
        /// <c>时间字符串</c>
        /// </summary>
        public string StrTime
        {
            get
            {
                return _StrTime;
            }
            set
            {
                _StrTime = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// <c>天气提醒</c>
    /// </summary>
    public class WeatherRemind
    {
        #region 成员变量
        string _Content = "";
        string _StrPublishTime = "";
        int _Period = 0;
        string[] _users;
        #endregion

        #region 构造函数
        #endregion

        #region 属性
        /// <summary>
        /// <c>标题</c>
        /// </summary>
        public string Content
        {
            get
            {
                return _Content;
            }
            set
            {
                _Content = value;
            }
        }
        /// <summary>
        /// <c>标题</c>
        /// </summary>
        public string StrPublishTime
        {
            get
            {
                return _StrPublishTime;
            }
            set
            {
                _StrPublishTime = value;
            }
        }

        /// <summary>
        /// <c>标题</c>
        /// </summary>
        public int Period
        {
            get
            {
                return _Period;
            }
            set
            {
                _Period = value;
            }
        }

        /// <summary>
        /// <c>标题</c>
        /// </summary>
        public string[] Users
        {
            get
            {
                return _users;
            }
            set
            {
                _users = value;
            }
        }
        #endregion

        public override string ToString()
        {
            string strSQL = "";
            for (int i = 0; i < _users.Length; i++)
            {
                strSQL+= string.Format("INSERT INTO [APP_Risk_Data].[dbo].[T_WeatherRemind] ([Content], PublishTime,PERIOD,USERNAME)     VALUES ('{0}','{1}','{2}','{3}');", Content, StrPublishTime, Period, _users[i]);
            }
            return strSQL;
        }
    }
    public class ThunderTextObject
    {
        public int COUNT10;
        public int COUNT20;
        public int COUNT30;
        public string Desc;
    }
    public class ThunderObject
    {
        public string ObjectName;
        public double Lon;
        public double Lat;
        public double XMin;
        public double XMax;
        public double YMin;
        public double YMax;
        public ThunderObject(string objectKey)
        {
            string[] sss = ConfigurationManager.AppSettings[objectKey].ToString().Split(',');
            ObjectName = sss[0];
            Lon = double.Parse(sss[1]);
            Lat = double.Parse(sss[2]);

            XMin = Lon - 0.5;
            XMax = Lon + 0.5;
            YMin = Lat - 0.5;
            YMax = Lat + 0.5;

        }
    }
    public class HealthyMeteorological
    {
        List<Detail> _Deatails;
        public string Crow;
        public List<Detail> Deatails
        {
            get
            {
                if (_Deatails == null)
                    _Deatails = new List<Detail>();

                return _Deatails;
            }
        }
    }
    public class Detail
    {
        public string Date;
        public string WarningLevel;
        public string WarningDesc;
        public string Influ;
        public string Wat_guide;
        public string Guide;
    }

    public class AQI
    {
        List<AQIData> _AQIDatas = null;
        /// <summary>
        /// 标题
        /// </summary>
        [Description("创建一个新的员工")]
        public string Title;
        /// <summary>
        /// 发布时间
        /// </summary>
        public string PublisDate;
        public List<AQIData> AQIDatas
        {
            get
            {
                if (_AQIDatas == null)
                    _AQIDatas = new List<AQIData>();

                return _AQIDatas;
            }
        }
    }
    public class AQIData
    {
        public string Period;
        public string AQI;
        public string Level;
        public string Pripoll;
    }

    public class District
    {
        public string DM;
        public string Name;
    }
    public class Community
    {
        public string ID;
        public string Name;
        public string District_Name;
    }
    public class MeteorologicalIndex
    {
        public string Name;
        public string PublishTime;
        List<Description> _Descriptions = null;
        public List<Description> Descriptions
        {
            get
            {
                if (_Descriptions == null)
                    _Descriptions = new List<Description>();

                return _Descriptions;
            }
        }
    }    
    public class Description
    {
        public string Time;
        public string Level;
        public string Comfort;
        public string Descrip;
        public string Suggestion;
    }

    public class DistrictAutoStationData
    {
        public string Datetime = "";
        public double Tempreture=double.NaN;
        public double RainHour = double.NaN;
        public double WindSpd = double.NaN;
        public double WindDir = double.NaN;
        public double Visible = double.NaN;
        public bool IsDataComplete()
        {
            if (double.IsNaN(Tempreture) ||
                double.IsNaN(RainHour) ||
                double.IsNaN(WindSpd) ||
                double.IsNaN(WindDir)||
                 double.IsNaN(Visible))
                return false;
            else
                return true;
        }

    }
    public class CommunityAutoStationData
    {
        string _Datetime = "2000-01-01 00:00:00";
        public string Datetime
        {
            get
            {
                return _Datetime;
            }
            set 
            {
                _Datetime = value;
            }
        }
        public double Tempreture = double.NaN;
        public double RainHour = double.NaN;
        public double WindSpd = double.NaN;
        public double WindDir = double.NaN;

        public bool IsDataComplete()
        {
            if (double.IsNaN(Tempreture) ||
                double.IsNaN(RainHour) ||
                double.IsNaN(WindSpd) ||
                double.IsNaN(WindDir))
                return false;
            else
                return true;
        }

    }

}
