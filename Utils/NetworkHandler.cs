using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace G4Studio.Utils
{
    public static class NetworkHandler
    {
        public static string GetIP()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;

            var hostname =
                NetworkInformation.GetHostNames()
                    .Last(
                        hn =>
                            hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

            // the ip address

            foreach (var item in NetworkInformation.GetHostNames())
            {
                Debug.WriteLine("item: " + item.CanonicalName);
            }

            return hostname?.CanonicalName;
        }
    }
}
