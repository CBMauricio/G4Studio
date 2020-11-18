using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace G4Studio.Utils
{
    public static class GeopositionHandler
    {
        public static BasicGeoposition GetPolygonCentroid(List<BasicGeoposition> coordinates)
        {
            if (coordinates == null) return new BasicGeoposition();

            double nCoordinates = coordinates.Count;
            double centroidLatitude = 0.0;
            double centroidLongitude = 0.0;

            foreach (var item in coordinates)
            {
                centroidLatitude += item.Latitude;
                centroidLongitude += item.Longitude;
            }

            centroidLatitude /= nCoordinates;
            centroidLongitude /= nCoordinates;

            return new BasicGeoposition() { Latitude = centroidLatitude, Longitude = centroidLongitude };
        }

        public static BasicGeoposition GetNorthWestPosition(List<BasicGeoposition> coordinates, BasicGeoposition centroid)
        {
            if (coordinates == null) return new BasicGeoposition();

            double latitude = 90;
            double longitude = -180;
            double inc = 0.001;

            foreach (var item in coordinates)
            {
                if (item.Longitude < centroid.Longitude && item.Latitude > centroid.Latitude)
                {
                    longitude = Math.Max(longitude, item.Longitude) + inc;
                    latitude = Math.Min(latitude, item.Latitude) - inc;
                }
            }

            return new BasicGeoposition() { Latitude = latitude, Longitude = longitude };
        }

        public static BasicGeoposition GetSouthEastPosition(List<BasicGeoposition> coordinates, BasicGeoposition centroid)
        {
            if (coordinates == null) return new BasicGeoposition();

            double latitude = -90;
            double longitude = 180;
            double inc = 0.001;

            foreach (var item in coordinates)
            {
                if (item.Longitude > centroid.Longitude && item.Latitude < centroid.Latitude)
                {
                    longitude = Math.Min(longitude, item.Longitude) - inc;
                    latitude = Math.Max(latitude, item.Latitude) + inc;
                }
            }

            return new BasicGeoposition() { Latitude = latitude, Longitude = longitude };
        }
    }
}
