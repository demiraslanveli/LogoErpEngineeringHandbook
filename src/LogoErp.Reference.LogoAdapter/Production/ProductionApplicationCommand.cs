using System;

namespace LogoErp.Reference.LogoAdapter.Production
{
    public sealed class ProductionApplicationCommand
    {
        public string OrderNumber { get; set; }
        public string ItemCode { get; set; }
        public double PlannedQuantity { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }
}
