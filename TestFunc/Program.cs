using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonServiceLib;
using System.IO;
namespace TestFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonServiceLib.JsonService js = new JsonServiceLib.JsonService();
            JsonServiceLib.JsonService_Geliku js1 = new JsonServiceLib.JsonService_Geliku();
            //js.GetThunderRemind("egd");
            //js.GetPSQKForecast("20170705000000", "20170709200000");
            //Stream s = new StreamReader(@"C:\Users\Administrator\Desktop\JSON.txt",Encoding.UTF8).BaseStream;
            //js.GetRTAutoStationData("ypq");
            js.GetAutoStationData1("ypq","20170718150000");
            //js.GetRiskAlarmByUsername_V2("wjc");
            //js1.GetDisasterDetailData_Geliku("20170620000000", "20170621000000");
            //js1.GetRealDisasterDetailData_Geliku("20170620000000", "20170621000000");
            //js.GetTyphoonForecastPoints("1702","babj","20170612020000");

        }
    }
}
