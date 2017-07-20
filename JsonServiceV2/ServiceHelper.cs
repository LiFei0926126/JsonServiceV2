namespace JsonService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using Newtonsoft.Json;
    public static class ServiceHelper
    {
        /// <summary>
        /// 将WCF结果转换为流。
        /// </summary>
        /// <param name="wcf"></param>
        /// <returns></returns>
        public static Stream ConvertWCFResult2Stream(object obj)
        {
            string strObj = JsonConvert.SerializeObject(obj);
            return new MemoryStream(Encoding.UTF8.GetBytes(strObj));
        }
        
        /// <summary>
        /// 根据风速获取风力等级
        /// </summary>
        /// <param name="windspeed">风速</param>
        /// <returns>风力等级</returns>
        public static int GetWindLevByWindSpeed(double windspeed)
        {
            if (windspeed <= 0.2)
                return 0;
            if (windspeed <= 1.5)
                return 1;
            if (windspeed <= 3.3)
                return 2;
            if (windspeed <= 5.4)
                return 3;
            if (windspeed <= 7.9)
                return 4;
            if (windspeed <= 10.7)
                return 5;
            if (windspeed <= 13.8)
                return 6;
            if (windspeed <= 17.1)
                return 7;
            if (windspeed <= 20.7)
                return 8;
            if (windspeed <= 24.4)
                return 9;
            if (windspeed <= 28.4)
                return 10;
            if (windspeed <= 32.6)
                return 11;
            return 12;
        }
        /// <summary>
        /// 根据台风风速获取台风等级
        /// </summary>
        /// <param name="windSpeed">台风风速</param>
        /// <returns>台风等级</returns>
        public static string GetTyphoonLevelByWindSpeed(double windSpeed)
        {
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
    }
}
