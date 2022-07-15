﻿namespace Hometech.Models
{
    public class Courier
    {
        public int id_courier { get; set; }
        public string courier_login { get; set; }
        public int id_admin { get; set; }
        public Status status { get; set; }
    }
    public enum Status
    {
        Available,
        Unavailable
    }
}