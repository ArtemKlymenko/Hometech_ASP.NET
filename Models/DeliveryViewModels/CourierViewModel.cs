using System.Collections.Generic;

namespace Hometech.Models.DeliveryViewModels
{
    public class CourierViewModel
    {
        public List<CourierInfo> Couriers { get; set; }
    }
    public class CourierInfo
    {
        public int id_courier { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public Status status { get; set; }
    }
}