using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace QXJLHandleDatatable
{
    public class StationClass
    {

        public string id;    ///1   代表STATIONID
        public string name; ///2    代表STATIONNAME
        public string value;   ///6 代表RAINHOUR
        public DateTime date; ///0  代表DATETIME
        public string type;   ///记录同一个站点，排在第一位的序号
        public string period;
        public string lon = "";    ////  3代表LON
        public string lat = "";    ////  4代表LAT
        public string temperature = "";   ///// 5 代表TEMPERATURE
        public string windspeed = "";     ////  7 代表WINDSPEED
        public string winddirection = ""; ////  8 代表WINDDIRECTION
    }

    public class ChartStationInOneSegment
    {
        public StationClass maxStation;
        public StationClass minStation;
        public int sumNumStations;
    }


    public static class handle_Datatable_all_to_Datatable_Leijia
    {

        public static DataTable Trancefor_DataTable_To_DataTable_By_Type(DataTable table, int type)
        {
            DateTime dt1 = DateTime.Now;
            ////
            List<StationClass> list = new List<StationClass>();

            ///对所有的station进行区分，根据不同的站点名称和ID，划分不同的type
            List<StationClass> listStationType = new List<StationClass>();
            ///获取所有站点的ID
            List<string> listStationIDS = new List<string>();
            List<string> list_string_Types = new List<string>();

            ////1个小时的区间数据
            int lenSatations = table.Rows.Count;
            if (lenSatations > 1)
            {
                ///第一行是列名,最后一行为空，去掉首尾
                for (int i = 1; i < lenSatations - 1; i++)
                {
                    DataRow row = table.Rows[i];
                    StationClass station = new StationClass();

                    ///id,name,lon,lat,tem,windspeed ,winddirection都是可以确定的
                    string stationID = row.ItemArray[1].ToString();
                    if (!listStationIDS.Contains(stationID))
                    {
                        listStationIDS.Add(stationID);
                    }
                    station.id = stationID;
                    station.name = row.ItemArray[2].ToString();

                    string datetime = row.ItemArray[0].ToString();
                    station.date = Convert.ToDateTime(datetime);

                    station.lon = row.ItemArray[3].ToString();
                    station.lat = row.ItemArray[4].ToString();

                    station.temperature = row.ItemArray[5].ToString();
                    station.windspeed = row.ItemArray[7].ToString();
                    station.winddirection = row.ItemArray[8].ToString();

                    string rain = row.ItemArray[6].ToString();
                    station.value = rain;
                    station.type = i.ToString();

                    listStationType.Add(station);
                }
            }

            int len_after_type_stations = listStationType.Count;

            for (int m = 0; m < len_after_type_stations; m++)
            {
                StationClass stationExit = listStationType[m];
                string stationID = stationExit.id;
                if (listStationIDS.Contains(stationID))
                {
                    // stationExit.type = listStationIDS.FindIndex(stationID);
                    stationExit.type = listStationIDS.FindIndex(delegate(string s) { return s == stationID; }).ToString();


                }



            }



            /////统计站点名称的数量
            for (int i = 0; i < len_after_type_stations; i++)
            {
                StationClass statio = listStationType[i];
                string strType = statio.type;
                if (!list_string_Types.Contains(strType))
                {
                    list_string_Types.Add(strType);

                }
            }


            DateTime dt2 = DateTime.Now;
            TimeSpan span = dt2 - dt1;
            double usetime = span.TotalSeconds;




            /////处理每个站点名称相同的系列

            int len_Type_Sum = list_string_Types.Count;
            for (int i = 0; i < len_Type_Sum; i++)
            {
                List<StationClass> listTemp = new List<StationClass>();
                string strType = list_string_Types[i];
                for (int j = 0; j < len_after_type_stations; j++)
                {
                    StationClass station = listStationType[j];
                    if (station.type == strType)
                    {
                        listTemp.Add(station);
                    }
                }
                ////对相同名称的系列开始处理日期对象
                StationClass right_station_in_list = handle_leijia_station_in_list(listTemp, type);
                list.Add(right_station_in_list);
            }



            //////将累加数据拼成一个DATATABLE返回
            DataTable back_DataTable = new DataTable();
            back_DataTable.Columns.Add(new DataColumn("DATETIME", typeof(string)));
            back_DataTable.Columns.Add(new DataColumn("STATIONID", typeof(string)));
            back_DataTable.Columns.Add(new DataColumn("STATIONNAME", typeof(string)));
            back_DataTable.Columns.Add(new DataColumn("LON", typeof(string)));
            back_DataTable.Columns.Add(new DataColumn("LAT", typeof(string)));
            back_DataTable.Columns.Add(new DataColumn("TEMPERATURE", typeof(string)));
            back_DataTable.Columns.Add(new DataColumn("RAINHOUR", typeof(string)));
            back_DataTable.Columns.Add(new DataColumn("WINDSPEED", typeof(string)));
            back_DataTable.Columns.Add(new DataColumn("WINDDIRECTION", typeof(string)));

            for (int i = 0; i < list.Count; i++)
            {
                StationClass station = list[i];
                DataRow rowThis = back_DataTable.NewRow();
                rowThis["DATETIME"] = station.date.ToString("yyyy/M/d HH:mm:ss");
                rowThis["STATIONID"] = station.id.ToString();
                rowThis["STATIONNAME"] = station.name.ToString();
                rowThis["LON"] = station.lon.ToString();
                rowThis["LAT"] = station.lat.ToString();
                rowThis["TEMPERATURE"] = station.temperature.ToString();
                rowThis["RAINHOUR"] = station.value.ToString();
                rowThis["WINDSPEED"] = station.windspeed.ToString();
                rowThis["WINDDIRECTION"] = station.winddirection.ToString();
                back_DataTable.Rows.Add(rowThis);

            }
            return back_DataTable;
        }

        /// <summary>
        ///  根据算法推到出同名自动站不同时刻降水集合
        /// </summary>
        /// <param name="listInput"></param>
        /// <returns></returns>
        /// 
        private static StationClass handle_leijia_station_in_list(List<StationClass> listInput, int hour_segment)
        {
            ///要返回的数据
            StationClass stationRight = new StationClass();
            int lenStations = listInput.Count;
            List<int> listHour = new List<int>();
            for (int i = 0; i < lenStations; i++)
            {
                StationClass station = listInput[i];
                DateTime dt = station.date;
                int hour = dt.Hour;
                if (!listHour.Contains(hour))
                {
                    listHour.Add(hour);
                }
            }
            ////之前考虑年月日时的不同，因为DATATABLE返回来的记录规律，只考虑时的不同

            if (listHour.Count > 1)     /////如果返回来的小时数>1
            {
                ///////////////////根据时间段来进行调整
                ///////////////////找到时间段内的最大值和最小值
                if (hour_segment == 1)
                {
                    List<int> list_num_in_every_hour = new List<int>();
                    Double input_vlaue_sum = 0.0;

                    for (int h = 0; h < listHour.Count; h++)
                    {
                        int fix_hour = listHour[h];
                        int station_number_in_this_hour = find_right_station_num_in_list_by_onehour(listInput, fix_hour);
                        list_num_in_every_hour.Add(station_number_in_this_hour);
                    }

                    //////先就1一小时来考虑
                    int num_this_hour = list_num_in_every_hour[0];
                    if (num_this_hour == 1)
                    {
                        stationRight = listInput[0];
                    }
                    else if (num_this_hour > 1) ///1小时内跨多个时间段
                    {
                        ChartStationInOneSegment chartStationInOneSegment_0 = chart_station_num_in_list_by_onesegment(listInput, listHour[0]);
                        StationClass max_Station = chartStationInOneSegment_0.maxStation;
                        StationClass zero_Station = chartStationInOneSegment_0.minStation;

                        ChartStationInOneSegment chartStationInOneSegment_1 = chart_station_num_in_list_by_onesegment(listInput, listHour[1]);
                        StationClass small_max_Station = chartStationInOneSegment_1.minStation;

                        input_vlaue_sum = Convert.ToDouble(zero_Station.value) - Convert.ToDouble(small_max_Station.value);
                        if (input_vlaue_sum < 0)
                        {
                            input_vlaue_sum = 0;
                        }
                        max_Station.value = (Convert.ToDouble(max_Station.value) + input_vlaue_sum).ToString();
                        stationRight = max_Station;
                    }
                }

                else  ////时间段为3小时或6小时
                {
                    List<int> list_num_in_every_hour = new List<int>();
                    Double input_vlaue_sum = 0.0;
                    for (int h = 0; h < listHour.Count; h++)
                    {
                        int fix_hour = listHour[h];
                        int station_number_in_this_hour = find_right_station_num_in_list_by_onehour(listInput, fix_hour);
                        list_num_in_every_hour.Add(station_number_in_this_hour);
                    }

                    for (int i = 0; i < listHour.Count; i++)
                    {
                        //////先就第一小时来考虑
                        if (i == 0)
                        {
                            int num_this_hour = list_num_in_every_hour[i];
                            if (num_this_hour == 1)
                            {
                                continue;
                            }
                            else if (num_this_hour > 1)
                            {
                                int thisHour = listHour[i];
                                StationClass max_Station = find_max_station_num_in_list_by_onehour(listInput, thisHour);
                                double part_input_vlaue = Convert.ToDouble(max_Station.value);
                                if (part_input_vlaue < 0)
                                {
                                    part_input_vlaue = 0;
                                }
                                input_vlaue_sum += Convert.ToDouble(part_input_vlaue);

                            }

                        }
                        else if (i < listHour.Count - 1)
                        {
                            int nextHour = listHour[i - 1];
                            StationClass zero_Station = find_min_station_num_in_list_by_onehour(listInput, nextHour);
                            double part_input_vlaue = Convert.ToDouble(zero_Station.value);
                            if (part_input_vlaue < 0)
                            {
                                part_input_vlaue = 0;
                            }
                            input_vlaue_sum += Convert.ToDouble(part_input_vlaue);

                        }
                        else
                        {

                            int thisHour = listHour[i];
                            int nextHour = listHour[i - 1];


                            StationClass zero_Station = find_min_station_num_in_list_by_onehour(listInput, nextHour);
                            StationClass min_Station = find_min_station_num_in_list_by_onehour(listInput, thisHour);
                            double part_input_vlaue = Convert.ToDouble(zero_Station.value) - Convert.ToDouble(min_Station.value);
                            if (part_input_vlaue < 0)
                            {
                                part_input_vlaue = 0;
                            }
                            input_vlaue_sum += Convert.ToDouble(part_input_vlaue);
                        }

                    }

                    stationRight = listInput[0];
                    stationRight.value = input_vlaue_sum.ToString();

                }



            }
            else     /////如果返回来的小时数=1,那就取最近的那个时段，因为时间段是从1，3，6开始的
            {
                /////////////////找到min最大的，和最小的station, value相减////////////
                stationRight = listInput[0];
            }
            return stationRight;
        }

        /// <summary>
        /// 由于传回来的数据是5分钟逐次，这个方法统计1个小时内的同名自动站数量
        /// </summary>
        /// <param name="listInput"></param>
        /// <param name="fix_hour"></param>
        /// <returns></returns>
        private static int find_right_station_num_in_list_by_onehour(List<StationClass> listInput, int fix_hour)
        {
            int back_number = 0;
            int lenStations = listInput.Count;
            for (int i = 0; i < lenStations; i++)
            {
                StationClass station = listInput[i];
                DateTime dt = station.date;
                int hour = dt.Hour;
                if (hour == fix_hour)
                {
                    back_number++;
                }
            }
            return back_number;
        }

        private static StationClass find_max_station_num_in_list_by_onehour(List<StationClass> listInput, int fix_hour)
        {
            List<StationClass> list = new List<StationClass>();
            int back_number = 0;
            int lenStations = listInput.Count;

            for (int i = 0; i < lenStations; i++)
            {
                StationClass station = listInput[i];
                DateTime dt = station.date;
                int hour = dt.Hour;
                if (hour == fix_hour)
                {
                    back_number++;
                    list.Add(station);
                }
            }
            int len_list = list.Count;
            return list[0];
        }

        private static StationClass find_min_station_num_in_list_by_onehour(List<StationClass> listInput, int fix_hour)
        {
            List<StationClass> list = new List<StationClass>();
            int back_number = 0;
            int lenStations = listInput.Count;

            for (int i = 0; i < lenStations; i++)
            {
                StationClass station = listInput[i];
                DateTime dt = station.date;
                int hour = dt.Hour;
                if (hour == fix_hour)
                {
                    back_number++;
                    list.Add(station);
                }
            }
            int len_list = list.Count;
            return list[len_list - 1];
        }

        /// <summary>
        /// 统计一个时间段内所有的自动站信息
        /// </summary>
        /// <param name="listInput"></param>
        /// <param name="fix_hour"></param>
        /// <returns></returns>
        private static ChartStationInOneSegment chart_station_num_in_list_by_onesegment(List<StationClass> listInput, int fix_hour)
        {
            List<StationClass> list = new List<StationClass>();
            int back_number = 0;
            int lenStations = listInput.Count;

            for (int i = 0; i < lenStations; i++)
            {
                StationClass station = listInput[i];
                DateTime dt = station.date;
                int hour = dt.Hour;
                if (hour == fix_hour)
                {
                    back_number++;
                    list.Add(station);
                }
            }
            int len_list = list.Count;


            ChartStationInOneSegment chartStation = new ChartStationInOneSegment();
            chartStation.maxStation = list[0];
            chartStation.minStation = list[len_list - 1];
            chartStation.sumNumStations = back_number;
            return chartStation;
        }




    }
}
