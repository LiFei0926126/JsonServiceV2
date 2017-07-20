using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
//using Readearth.LogManager;
namespace SHFL
{
    public class Diamond4
    {
        #region 成员变量
        string _Description;
        DateTime _ForecastTime;
        int _Period;
        string _Layer;
        double _dx, _dy;
        double _orgionX, _endX, _orgionY, _endY;
        int _XCount, _YCount;
        double _LineInterval, _LineStart, _LineEnd;
        double _SmoothCoefficient, _ThickValue;

        double[,] _data;
        string _read;

        //ProductType m_ProductType;
        #endregion

        #region 属性

        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description
        {
            get
            {
                return _Description;
            }
        }

        /// <summary>
        /// 起报时间
        /// </summary>
        public DateTime ForecastTime_BJ
        {
            get
            {
                return _ForecastTime.AddHours(8);
            }
        }

        public string read
        {
            get
            {
                return _read;
            }
        }

        /// <summary>
        /// 起报时间
        /// </summary>
        public DateTime ForecastTime_UTC
        {
            get
            {
                return _ForecastTime;
            }
        }

        /// <summary>
        /// 层次
        /// </summary>
        public string Layer
        {
            get
            {
                return _Layer;
            }
        }

        /// <summary>
        /// 经度格距
        /// </summary>
        public double DX
        {
            get
            {
                return _dx;
            }
        }

        /// <summary>
        /// 纬度格距
        /// </summary>
        public double DY
        {
            get
            {
                return _dy;
            }
        }

        /// <summary>
        /// 起始经度
        /// </summary>
        public double OrgionX
        {
            get
            {
                return _orgionX;
            }
        }

        /// <summary>
        /// 终止经度
        /// </summary>
        public double EndX
        {
            get
            {
                return _endX;
            }
        }

        /// <summary>
        /// 起始纬度
        /// </summary>
        public double OrgionY
        {
            get
            {
                return _orgionY;
            }
        }

        /// <summary>
        /// 终止纬度
        /// </summary>
        public double EndY
        {
            get
            {
                return _endY;
            }
        }

        /// <summary>
        ///经向格点数
        /// </summary>
        public int XCount
        {
            get { return _XCount; }
        }

        /// <summary>
        /// 纬向格点数
        /// </summary>
        public int YCount
        {
            get { return _YCount; }
        }

        /// <summary>
        /// 等值线间隔
        /// </summary>
        public double LineInterval
        {
            get
            {
                return _LineInterval;
            }
        }

        /// <summary>
        /// 等值线起始值
        /// </summary>
        public double LineStart
        {
            get
            {
                return _LineStart;
            }
        }

        /// <summary>
        /// 等值线终止值
        /// </summary>
        public double LineEnd
        {
            get
            {
                return _LineEnd;
            }
        }

        /// <summary>
        /// 等值线平滑系数
        /// </summary>
        public double SmoothCoefficient
        {
            get
            {
                return _SmoothCoefficient;
            }
        }

        /// <summary>
        /// 等值线加粗线值
        /// </summary>
        public double ThickValue
        {
            get
            {
                return _ThickValue;
            }
        }

        /// <summary>
        /// 数据
        /// </summary>
        public double[,] Data
        {
            get
            {
                return _data;
            }
        }

        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数（含解析过程）
        /// </summary>
        /// <param name="path"></param>
        public Diamond4(string path)//,ProductType pt)
        {
            //m_ProductType = pt;
            FileInfo sourceFile = new FileInfo(path);

            StreamReader sr = new StreamReader(sourceFile.FullName, Encoding.Default);

            #region Read Header
            string strLine = sr.ReadLine();
            strLine += " " + sr.ReadLine();

            string[] headers = Regex.Split(strLine, "\\s+");
            if (headers[3].Length == 6)
            {
                headers[3] = headers[3].Insert(4, " ");
                headers = Regex.Split(string.Join(" ", headers), "\\s+");
            }
            _Description = headers[2];
            _Period = int.Parse(sourceFile.Name.Split('.')[1]);
            if (sourceFile.Name.Length == 14)
            {
                string strTime = "20" + sourceFile.Name.Split('.')[0].Insert(2, "-").Insert(5, "-").Insert(8, " ").Insert(11, ":") + ":00";
                _ForecastTime = DateTime.Parse(strTime);

                _Layer = headers[8];
                _dx = double.Parse(headers[9]);
                _dy = double.Parse(headers[10]);

                _orgionX = double.Parse(headers[11]);
                _endX = double.Parse(headers[12]);

                _orgionY = double.Parse(headers[13]);
                _endY = double.Parse(headers[14]);

                _YCount = int.Parse(headers[15]);
                _XCount = int.Parse(headers[16]);

                _LineInterval = double.Parse(headers[17]);
                _LineStart = double.Parse(headers[18]);
                _LineEnd = double.Parse(headers[19]);

                _SmoothCoefficient = double.Parse(headers[20]);
                _ThickValue = double.Parse(headers[21]);
            }
            else if (sourceFile.Name.Length == 12)
            {
                string strTime = "20" + sourceFile.Name.Split('.')[0].Insert(2, "-").Insert(5, "-").Insert(8, " ").Insert(11, ":") + "00:00";
                _ForecastTime = DateTime.Parse(strTime);

                _Layer = headers[7];
                _dx = double.Parse(headers[8]);
                _dy = double.Parse(headers[9]);

                _orgionX = double.Parse(headers[10]);
                _endX = double.Parse(headers[11]);

                _orgionY = double.Parse(headers[12]);
                _endY = double.Parse(headers[13]);

                _YCount = int.Parse(headers[14]);
                _XCount = int.Parse(headers[15]);

                _LineInterval = double.Parse(headers[16]);
                _LineStart = double.Parse(headers[17]);
                _LineEnd = double.Parse(headers[18]);

                _SmoothCoefficient = double.Parse(headers[19]);
                _ThickValue = double.Parse(headers[20]);
            }
            else if (sourceFile.Name.Length == 16)
            {
                string strTime = sourceFile.Name.Split('.')[0];
                _ForecastTime = DateTime.ParseExact(strTime, "yyyyMMddHHmm", null);

                _Layer = headers[8];
                _dx = double.Parse(headers[9]);
                _dy = double.Parse(headers[10]);

                _orgionX = double.Parse(headers[11]);
                _endX = double.Parse(headers[12]);

                _orgionY = double.Parse(headers[13]);
                _endY = double.Parse(headers[14]);

                _YCount = int.Parse(headers[15]);
                _XCount = int.Parse(headers[16]);

                _LineInterval = double.Parse(headers[17]);
                _LineStart = double.Parse(headers[18]);
                _LineEnd = double.Parse(headers[19]);

                _SmoothCoefficient = double.Parse(headers[20]);
                _ThickValue = double.Parse(headers[21]);
            }
            #endregion

            _read = sr.ReadToEnd();

            _data = new double[_YCount, _XCount];
            string[] lineDatas = Regex.Split(_read.Trim(), "\\s+");

            for (int y = 0; y < _YCount; y++)
            {
                for (int x = 0; x < _XCount; x++)
                {
                    _data[y, x] = double.Parse(lineDatas[(_YCount - 1 - y) * _XCount + x]);
                }
            }
        }
        #endregion

        #region 公有函数
        /// <summary>
        /// 输出Diamond 4 为ASC文件
        /// </summary>
        /// <param name="outPath"></param>
        public void ExportToASCII(string outPath)
        {
            StreamWriter sw = new StreamWriter(outPath);

            #region Header
            sw.WriteLine("ncols         " + XCount);
            sw.WriteLine("nrows         " + YCount);
            sw.WriteLine("xllcorner     " + OrgionX);
            sw.WriteLine("yllcorner     " + OrgionY);
            sw.WriteLine("cellsize      " + DX);
            sw.WriteLine("NODATA_value  -999");
            #endregion

            for (int y = 0; y < YCount; y++)
            {
                string[] line = new string[XCount];
                for (int x = 0; x < XCount; x++)
                {

                    line[x] = Data[y, x].ToString();
                }

                sw.WriteLine(string.Join(" ", line));
            }
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

        public void ExportToTxt(string outPath)
        {
            StreamWriter sw = new StreamWriter(outPath);
            StringBuilder sb = new StringBuilder("x,y,xindex,yindex,value" + Environment.NewLine);
            for (int x = 0; x < XCount; x++)
            {
                for (int y = 0; y < YCount; y++)
                {
                    //if (x > 200 && x < 300 & y > 200 & y < 300)
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4}", _orgionX + _dx * x, _orgionY + _dy * y, x, y, Data[x, y]));
                    //int lev = getLevel(Data[x, YCount - 1 - y]);
                    //pBitmap.SetPixel(x, y, ColorPalette.getPalette(m_ProductType)[lev]);
                }
            }
            sw.Write(sb.ToString());
            sw.Flush();
            sb.Length = 0;
            sw.Close();
            sw.Dispose();
            #region PRJ

            //"GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]]"
            #endregion
        }

        public double[,] ReadData()
        {
            return _data;
        }
        public string ReadData(double lon, double lat)
        {
            string re = "";
            double res = double.NaN;
            if (lon < _orgionX || lon > _endX)
                re = "经度错误。";
            if (lat < _orgionY || lat > _endY)
                re = re == "" ? "纬度错误。" : "经度错误,纬度错误。";

            else if (lon >= _orgionX && lon <= _endX)
            {
                int x, y;
                x = (int)((lon - _orgionX - DX / 2) / DX);
                y = (int)((lat - _orgionY - DX / 2) / DX);
                res = _data[y, x];
                //res = _data[x, y];
            }
            re += "-" + res.ToString();
            //re = res.ToString();
            return re;
        }
        #endregion
    }

    public class ForStationData
    {
        public decimal leastTime(DataTable dt, DateTime dtTime)
        {
            decimal result = decimal.Zero;
            DataView dv = new DataView(dt);
            dv.RowFilter = "DateTime>'" + dtTime.ToString() + "'";
            DataTable dt1 = dv.ToTable();
            dv.RowFilter = "DateTime<'" + dtTime.ToString() + "'";
            DataTable dt2 = dv.ToTable();
            if (dt1.Rows.Count > 0 && dt2.Rows.Count == 0)
                result = Convert.ToDecimal(dt1.Rows[dt1.Rows.Count - 1]["Temperature"]);
            else if (dt1.Rows.Count == 0 && dt2.Rows.Count > 0)
                result = Convert.ToDecimal(dt2.Rows[0]["Temperature"]);
            else if (dt1.Rows.Count > 0 && dt2.Rows.Count > 0)
            {
                DateTime dt1Time = DateTime.Parse(dt1.Rows[dt1.Rows.Count - 1]["Datetime"].ToString());
                DateTime dt2Time = DateTime.Parse(dt2.Rows[0]["Datetime"].ToString());
                if (dt1Time - dtTime < dtTime - dt2Time)
                    result = Convert.ToDecimal(dt1.Rows[dt1.Rows.Count - 1]["Temperature"]);
                else
                    result = Convert.ToDecimal(dt2.Rows[0]["Temperature"]);
            }
            return result;
        }

        //public decimal anotherway(DataTable dt, DateTime dtTime)
        //{
        //    decimal result = decimal.Zero;
        //    DataView dv = new DataView(dt);
        //    dv.RowFilter = "DateTime='" + dtTime.ToString() + "'";
        //    int i = 0;
        //    while (dv.ToTable().Rows.Count == 0 && i > -576)
        //    {
        //        i = i - 1;
        //        dv.RowFilter = "DateTime='" + dtTime.AddMinutes(5 * i).ToString() + "'";
        //        if (dv.ToTable().Rows.Count > 0 || dt.Rows.Count == 0)
        //            break;
        //    }
        //    if(dv.ToTable().Rows.Count != 0)
        //        result = Convert.ToDecimal(dv.ToTable().Rows[0]["Temperature"]);
        //    return result;
        //}
    }
}
