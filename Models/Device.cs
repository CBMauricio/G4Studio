using Windows.Devices.Geolocation;

namespace G4Studio.Models
{
    public class Device
    {
        public string DeviceID { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DeviceName { get; set; }
        public BasicGeoposition DevicePosition { get; set; }

        public Device() 
        : this(string.Empty, new BasicGeoposition()) 
        { }


        public Device(string name, BasicGeoposition position)
        {
            DeviceName = name;
            DevicePosition = position;
        }
    }
}
