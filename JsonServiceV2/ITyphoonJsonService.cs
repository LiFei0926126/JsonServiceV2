using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.IO;

namespace JsonService
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“ITyphoon”。
    [ServiceContract]
    public interface ITyphoonJsonService
    {
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
    }
}
