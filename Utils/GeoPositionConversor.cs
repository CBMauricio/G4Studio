using Hyperion.Platform.Tests.Core.ExedraLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G4Studio.Utils
{
    internal static class GeoPositionConversor
    {
        public static List<Windows.Devices.Geolocation.BasicGeoposition> Parse(List<BasicGeoposition> basicGeoPositions)
        {
            List<Windows.Devices.Geolocation.BasicGeoposition> output = new List<Windows.Devices.Geolocation.BasicGeoposition>();

            foreach (var position in basicGeoPositions)
            {
                output.Add(Parse(position));
            }

            return output;
        }

        public static Windows.Devices.Geolocation.BasicGeoposition Parse(BasicGeoposition position)
        {
            return new Windows.Devices.Geolocation.BasicGeoposition() { Latitude = position.Latitude, Longitude = position.Longitude };
        }
    }
}
