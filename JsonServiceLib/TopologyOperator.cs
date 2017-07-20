using System;
using System.Collections.Generic;
using System.Configuration;
using DotSpatial.Data;
using DotSpatial.Projections;
using DotSpatial.Topology;
using DotSpatial.Topology.Algorithm;

namespace JsonServiceLib
{
    public class TopologyOperator
    {
        public static IList<IFeature> Streets { get; set; }

        public static void Initial()
        {
            var featureSet = FeatureSet.Open(ConfigurationManager.AppSettings["shpPath"]);
            Streets = featureSet.Features;
            featureSet.Dispose();
        }

        public string GetDistrics(Coordinate coordinate)
        {
            foreach (var item in Streets)
            {
                var pointLocator = new PointLocator();
                if (pointLocator.Intersects(coordinate, item.BasicGeometry as IGeometry))
                {
                    return item.DataRow["NAME"].ToString();
                }
            }
            return null;
        }

    }
}
