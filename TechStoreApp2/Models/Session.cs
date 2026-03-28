using System;
using TechStoreApp2.Models;

namespace TechStoreApp2.Models
{
    public class Session
    {
        public int Id { get; set; }
        public int ComputerId { get; set; }
        public string ClientName { get; set; }    // логин клиента
        public DateTime SessionDate { get; set; } // дата сеанса
        public int Hours { get; set; }            // количество часов
        public Computer Computer { get; set; }    // для навигации

        // Вычисляемая стоимость
        public decimal Cost => (Computer?.PricePerHour ?? 0) * Hours;
    }
}