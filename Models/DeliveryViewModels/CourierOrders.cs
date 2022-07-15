using System.Collections.Generic;
using Hometech.Controllers;

namespace Hometech.Models.DeliveryViewModels
{
    public class CourierOrders
    {
        public List<ProductInfo> Prods { get; set; }
        public Client ClientInfo { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int IdOrder { get; set; }
    }
}