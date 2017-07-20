using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.ServiceModel.Web;

namespace JsonServiceLib
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IJsonService”。
    [ServiceContract]
    public interface IJsonService_Geliku
    {
        #region GET
        [OperationContract]
        [WebGet(UriTemplate = "/GetAutoStationData_Geliku/{strQueryTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAutoStationData_Geliku(string strQueryTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetAutoStationDataByDatetime_Geliku/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAutoStationDataByDatetime_Geliku(string strStartTime, string strEndTime);
        
        [OperationContract]
        [WebGet(UriTemplate = "/GetAutoStationDataByDatetime_5mi_SanWei/{strStartTime}/{strEndTime}/{type}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAutoStationDataByDatetime_5mi_SanWei(string strStartTime, string strEndTime, string type);

        [OperationContract]
        [WebGet(UriTemplate = "/GetAutoStationDataByDatetime_5mi_Geliku/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetAutoStationDataByDatetime_5mi_Geliku(string strStartTime, string strEndTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetCurrentTyphoon", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetCurrentTyphoon();

        [OperationContract]
        [WebGet(UriTemplate = "/GetHistoryTyphoon/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetHistoryTyphoon(string strStartTime, string strEndTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetRadarData/{strStationCode}/{strElevation}/{strQueryTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetRadarData(string strStationCode, string strElevation, string strQueryTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetThunderData/{strSource}/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetThunderData(string strSource,string strStartTime, string strEndTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetWeatherWarnning", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWeatherWarnning();

        [OperationContract]
        [WebGet(UriTemplate = "/GetWaterStationData_Geliku/{strQueryTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWaterStationData_Geliku(string strQueryTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetWaterStationDataByDatetime_Geliku/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWaterStationDataByDatetime_Geliku(string strStartTime, string strEndTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetGridRainData_Geliku/{strType}/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetGridRainData_Geliku(string strType, string strStartTime, string strEndTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetQPFData_Geliku/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetQPFData_Geliku(string strStartTime, string strEndTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetUrbanFloodData_Geliku/{strType}/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetUrbanFloodData_Geliku(string strType, string strStartTime, string strEndTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetDisasterDetailData_Geliku/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetDisasterDetailData_Geliku(string strStartTime, string strEndTime);

        [OperationContract]
        [WebGet(UriTemplate = "/GetRealDisasterDetailData_Geliku/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetRealDisasterDetailData_Geliku(string strStartTime, string strEndTime);

         [OperationContract]
        [WebGet(UriTemplate = "/GetWaterOut_Geliku/{strQueryTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Stream GetWaterOut_Geliku(string strQueryTime);

         [OperationContract]
         [WebGet(UriTemplate = "/GetWaterOutByDatetime_Geliku/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
         Stream GetWaterOutByDatetime_Geliku(string strStartTime, string strEndTime);

         [OperationContract]
         [WebGet(UriTemplate = "/GetYPWaterStation/{strQueryTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
         Stream GetYPWaterStation(string strQueryTime);

         [OperationContract]
         [WebGet(UriTemplate = "/GetYPWaterStationByTime/{strStartTime}/{strEndTime}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
         Stream GetYPWaterStationByTime(string strStartTime, string strEndTime);
        #endregion
    }
}
