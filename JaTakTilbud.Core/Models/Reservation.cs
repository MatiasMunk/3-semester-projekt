using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaTakTilbud.Core.Models
{
    internal class Reservation
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int CampaignProductId { get; set; }

        public int CollectingNumber { get; set; }

        public DateTime ReservedAt { get; set; }

        public int Quantity { get; set; }

        public string Status { get; set; }
    }
    public enum Status
    {
        Confirmed = 0,
        Cancelled = 1,
        Expired = 2,
    }
}
