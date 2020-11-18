using G4Studio.Models;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;

namespace G4Studio.Utils
{
    public static class TwinHandler
    {
        public static BasicGeoposition GetTwinPosition(Twin twin)
        {
            BasicGeoposition snPosition = new BasicGeoposition();

            if (twin != null)
            {
                foreach (KeyValuePair<string, object> item in twin.Properties.Desired)
                {
                    if (item.Key.Equals("location", StringComparison.InvariantCulture))
                    {
                        var position = JsonConvert.DeserializeObject<Position>(item.Value.ToString());

                        snPosition = new BasicGeoposition { Latitude = position.latitude, Longitude = position.longitude };
                    }
                }
            }

            return snPosition;
        }
    }
}
