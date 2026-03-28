using System;

namespace TechStoreApp4.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ClientName { get; set; }
        public DateTime AppointmentDate { get; set; }   // вместо OrderDate
        public int SessionsCount { get; set; }          // вместо Quantity
        public Service Service { get; set; }

        public decimal Cost => (Service?.Price ?? 0) * SessionsCount;
    }
}