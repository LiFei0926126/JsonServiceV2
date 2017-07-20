using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.ServiceModel.Web;
using System.ComponentModel;

namespace JsonServiceLib
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IJsonService”。
    [ServiceContract]
    public interface IJsonService
    {
        #region GET
        /// <summary>
        /// <c>获取雷达图片URL及Extent字符串</c>
        /// </summary>
        /// <param name="dtStart">开始时间</param>
        /// <param name="dtEnd">结束时间</param>
        /// <returns></returns>
        /// <exception cref=" "></exception>
        [OperationContract]
        [WebGet(UriTemplate = "/GetRadarStr?strStart={strStart}&strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetRadarStr(string strStart, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/VerifyUser?userName={userName}&passWord={passWord}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream VerifyUser(string userName, string passWord);

        [OperationContract]
        [WebGet(UriTemplate = "/GetAutoStationData/{userName}?strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAutoStationData(string userName, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/GetAutoStationData1/{userName}?strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAutoStationData1(string userName, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/GetDataByID?stationID={stationID}&WaterStationID={WaterStationID}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetDataByID(string stationID, string WaterStationID);

        [OperationContract]
        [WebGet(UriTemplate = "/Get5DayForecast", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream Get5DayForecast();

        [OperationContract]
        [WebGet(UriTemplate = "/GetZXSForecast", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetZXSForecast();

        [OperationContract]
        [WebGet(UriTemplate = "/GetDisaster?userName={userName}&strStart={strStart}&strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetDisaster(string userName,string strStart, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/GetLightning?strStart={strStart}&strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetLightning(string strStart, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/Get_2H_Lightning?strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream Get_2H_Lightning(string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/GetGeometry?userName={userName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetGeometry(string userName);

        [OperationContract]
        [WebGet(UriTemplate = "/GetGeometry_V2?userName={userName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetGeometry_V2(string userName);

        [OperationContract]
        [WebGet(UriTemplate = "/GetGeometry_PSQK_YP", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetGeometry_PSQK_YP();


        [OperationContract]
        [WebGet(UriTemplate = "/GetAvailableCommunityList", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAvailableCommunityList();


        [OperationContract]
        [WebGet(UriTemplate = "/GetTouTiao?strStart={strStart}&strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetTouTiao(string strStart, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/GetQXKP?strStart={strStart}&strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetQXKP(string strStart, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/GetYXYB?strStart={strStart}&strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetYXYB(string strStart, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/GetNumofTouTiao?strTime={strTime}&number={number}&updown={updown}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetNumofTouTiao(string strTime, string number, string updown);

        [OperationContract]
        [WebGet(UriTemplate = "/GetNumofQixiangkepu?strTime={strTime}&number={number}&updown={updown}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetNumofQixiangkepu(string strTime, string number, string updown);

        [OperationContract]
        [WebGet(UriTemplate = "/GetNumofYingxiangyubao?strTime={strTime}&number={number}&updown={updown}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetNumofYingxiangyubao(string strTime, string number, string updown);

        [OperationContract]
        [WebGet(UriTemplate = "/GetRTAutoStationData?userName={userName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetRTAutoStationData(string userName);
        
        [OperationContract]
        [WebGet(UriTemplate = "/GetWeatherRemind", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWeatherRemind();

        [OperationContract]
        [WebGet(UriTemplate = "/GetWeatherRemindByUserName/{userName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWeatherRemindByUserName(string userName);

        [OperationContract]
        [WebGet(UriTemplate = "/GetWeatherRemindByTime/{startTime}/{endTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWeatherRemindByTime(string startTime, string endTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetAutoStationModelData/{strQueryTime}/{strType}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAutoStationModelData(string strQueryTime, string strType);


        [OperationContract]
        [WebGet(UriTemplate = "/GetQPF?lon={lon}&lat={lat}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetQPF(string lon, string lat);
       
        [OperationContract]
        [WebGet(UriTemplate = "/GetQPFByUserName/{userName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetQPFByUserName(string userName);

        [OperationContract]
        [WebGet(UriTemplate = "/GetDisasterHistory?strStart={strStart}&strEnd={strEnd}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetDisasterHistory(string strStart, string strEnd);

        [OperationContract]
        [WebGet(UriTemplate = "/GetMinDisaster?strEnd={strEnd}&AddTimes={AddTimes}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetMinDisaster(string strEnd, string AddTimes);

        [OperationContract]
        [WebGet(UriTemplate = "/GetCurrentTypgoon", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCurrentTypgoon();
        [OperationContract]
        [WebGet(UriTemplate = "/GetCurrentTypgoonTest", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCurrentTypgoonTest();

        [OperationContract]
        [WebGet(UriTemplate = "/GetCurrentWeatherReport", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCurrentWeatherReport();

        [OperationContract]
        [WebGet(UriTemplate = "/GetEditionURL", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetEditionURL();
        
        [OperationContract]
        [WebGet(UriTemplate = "/GetWarnning/{JDName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWarnning(string JDName);

        [OperationContract]
        [WebGet(UriTemplate = "/GetWordText/{obejctName}/{starttime}/{endtime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWordText(string obejctName, string starttime, string endtime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetWordText1/{obejctName}/{starttime}/{endtime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWordText1(string obejctName, string starttime, string endtime);
        #endregion

        #region POST
        [OperationContract]
        [WebInvoke(UriTemplate = "/PostTouTiao", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream PostTouTiao(Stream strImage);
        [OperationContract]
        [WebInvoke(UriTemplate = "/PostTouTiao_Update", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream PostTouTiao_Update(Stream Update);

        [OperationContract]
        [WebInvoke(UriTemplate = "/PostDisaster", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream PostDisaster(Stream strImage);

        [OperationContract]
        [WebInvoke(UriTemplate = "/PostWeatherRemind", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream PostWeatherRemind(Stream strWeatherRemind);

        

        [OperationContract]
        [WebInvoke(UriTemplate = "/PostAPPAlive", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream PostAPPAlive(Stream data);

        [OperationContract]
        [WebInvoke(UriTemplate = "/PostQixiangkepu", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream PostQixiangkepu(Stream strImage);
        [OperationContract]
        [WebInvoke(UriTemplate = "/PostQixiangkepu_Update", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream PostQixiangkepu_Update(Stream Update);
        #endregion


        #region 微信
        #region GET
        [OperationContract]
        [WebGet(UriTemplate = "/Get_1H_Lightning/{EndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream Get_1H_Lightning(string EndTime);
        [OperationContract]
        [WebGet(UriTemplate = "/GetHealthyMeteorological", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetHealthyMeteorological();
        [OperationContract]
        [WebGet(UriTemplate = "/GetAirQuality", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAirQuality();

        [OperationContract]
        [WebGet(UriTemplate = "/GetDistrictList", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetDistrictList();

        [OperationContract]
        [WebGet(UriTemplate = "/GetCommunityListByDistrict/{District_DM}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCommunityListByDistrict(string District_DM);

        [OperationContract]
        [WebGet(UriTemplate = "/GetRiskAlarmByUsername/{userName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetRiskAlarmByUsername(string userName);

        [OperationContract]
        [WebGet(UriTemplate = "/GetRiskAlarmByUsername_V2/{userName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetRiskAlarmByUsername_V2(string userName);

        [OperationContract]
        [WebGet(UriTemplate = "/GetCommunityListByDistrictName/{District_Name}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCommunityListByDistrictName(string District_Name);
        
        [OperationContract]
        [WebGet(UriTemplate = "/GetCommunityByXY/{lon}/{lat}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCommunityByXY(string lon,string lat);

        [OperationContract]
        [WebGet(UriTemplate = "/GetThunderRemind/{tag}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetThunderRemind(string tag);
        [OperationContract]
        [WebGet(UriTemplate = "/GetIndex", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetIndex();

        [OperationContract]
        [WebGet(UriTemplate = "/GetCommunityAutoStationData/{lon}/{lat}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCommunityAutoStationData(string lon, string lat);

        [OperationContract]
        [WebGet(UriTemplate = "/GetCommunityAutoStationDataByCommunityID/{CommunityID}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCommunityAutoStationDataByCommunityID(string CommunityID);

        [OperationContract]
        [WebGet(UriTemplate = "/GetCommunityWaterStationData/{CommunityID}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCommunityWaterStationData(string CommunityID);

        [OperationContract]
        [WebGet(UriTemplate = "/GetDistrictAutoStationData/{District_DM}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetDistrictAutoStationData(string District_DM);

        [OperationContract]
        [WebGet(UriTemplate = "/GetDistrictAutoStationDataByName/{District_Name}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetDistrictAutoStationDataByName(string District_Name);

        [OperationContract]
        [WebGet(UriTemplate = "/Get5DayForecast_Weixin", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream Get5DayForecast_Weixin();

        [OperationContract]
        [WebGet(UriTemplate = "/GetWeatherWarnning", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWeatherWarnning();

        [OperationContract]
        [WebGet(UriTemplate = "/GetRiskWarnning/{CommunityID}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetRiskWarnning(string CommunityID);

        [OperationContract]
        [WebGet(UriTemplate = "/GetPotentialPoints", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetPotentialPoints();

        [OperationContract]
        [WebGet(UriTemplate = "/GetRiskAlarmUsernames", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetRiskAlarmUsernames();

        [OperationContract]
        [WebGet(UriTemplate = "/GetWaterStationData/{strQueryTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWaterStationData(string strQueryTime);
        [OperationContract]
        [WebGet(UriTemplate = "/Get_YP_WaterStationData/{strQueryTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream Get_YP_WaterStationData(string strQueryTime);

        [WebGet(UriTemplate = "/GetPSQKForecast/{startTime}/{endTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetPSQKForecast(string startTime, string endTime);
        [OperationContract]
        [WebGet(UriTemplate = "/GetPSQKForecastModel/{startTime}/{endTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetPSQKForecastModel(string startTime, string endTime);
        #endregion

        #region POST

        #endregion
        #endregion

        #region 台风
        //[OperationContract]
        //[WebGet(UriTemplate = "/GetTyphoonWays", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        //Stream GetTyphoonWays();

        [OperationContract]
        [WebGet(UriTemplate = "/GetTyphoons/{year}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetTyphoons(string year);

        [OperationContract]
        [WebGet(UriTemplate = "/GetTyphoonRealTimePoints/{tfbh}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetTyphoonRealTimePoints(string tfbh);

        [OperationContract]
        [WebGet(UriTemplate = "/GetTyphoonForecastPoints/{tfbh}/{way}/{strQBTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetTyphoonForecastPoints(string tfbh, string way, string strQBTime);
        #endregion

        #region Test
        [OperationContract]
        [WebGet(UriTemplate = "/Test", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("创建一个新的员工")]
        AQI Test();

        #endregion


        #region 三维

        #endregion
    }
}
