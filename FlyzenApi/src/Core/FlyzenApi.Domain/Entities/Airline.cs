using System;
using System.Collections.Generic;
using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class Airline : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;

        public ICollection<Flight> Flights { get; set; } = new List<Flight>();
    }
}
