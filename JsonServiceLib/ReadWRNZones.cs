using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using DotSpatial.Topology;

namespace JsonServiceLib
{
    public class Diamond9
    {
        #region 成员变量
        string m_Path;
        DateTime m_DatetimeUTC = new DateTime();
        List<Coordinate> m_Blue = new List<Coordinate>();
        List<Coordinate> m_Yellow = new List<Coordinate>();
        List<Coordinate> m_Orange = new List<Coordinate>();
        List<Coordinate> m_Red = new List<Coordinate>();
        #endregion

        public Diamond9(string path)
        {
            m_Path = path;
            FileInfo fi = new FileInfo(m_Path);
            string fileName = fi.Name;
            m_DatetimeUTC = DateTime.ParseExact(fileName.Split(new char[]{'.','_'})[1], "yyyyMMddHHmm", null);

            Decode();


        }

        void Decode()
        {
            StreamReader sr = new StreamReader(m_Path);
            string strLine = "";
            List<Coordinate> tmp = new List<Coordinate>();
            while ((strLine = sr.ReadLine()) != null)
            {
                string[] datas = Regex.Split(strLine.Trim(), "\\s+");
                if (datas[0] == "diamond")
                    continue;
                else if (datas.Length > 5)
                    continue;
                else if (datas.Length == 5)
                {
                    //24 6 3 33
                    if (datas[2] == "24")
                        tmp = m_Blue;
                    if (datas[2] == "6")
                        tmp = m_Yellow;
                    if (datas[2] == "3")
                        tmp = m_Orange;
                    if (datas[2] == "33")
                        tmp = m_Red;
                }
                else
                {
                    Coordinate c = new Coordinate(double.Parse(datas[0]), double.Parse(datas[1]));
                    //pxy.x = double.Parse(datas[0]);
                    //pxy.y = double.Parse(datas[1]);

                    tmp.Add(c);
                }

            }

        }



        public List<Coordinate> Blue
        {
            get { return m_Blue; }
        }
        public List<Coordinate> Yellow
        {
            get { return m_Yellow; }
        }
        public List<Coordinate> Orange
        {
            get { return m_Orange; }
        }
        public List<Coordinate> Red
        {
            get { return m_Red; }
        }
        public DateTime ForecastTimeBJS
        {
            get
            {
                return m_DatetimeUTC.AddHours(8);
            }
        }
    }
}
