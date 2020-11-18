using Microsoft.WindowsAzure.Storage.Table;

namespace G4Studio.Models
{
    public class G3GatewayTableEntity : TableEntity
    {
        public string tenant { get; set; }
        public string ID { get; set; }

        public G3GatewayTableEntity()
        {
        }
    }
}
