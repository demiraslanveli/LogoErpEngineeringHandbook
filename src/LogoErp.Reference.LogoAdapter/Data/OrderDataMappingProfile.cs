using System;

namespace LogoErp.Reference.LogoAdapter.Data
{
    public sealed class OrderDataMappingProfile
    {
        public string DataObjectTypeKey { get; set; }
        public string DocumentNumberField { get; set; }
        public string DateField { get; set; }
        public string CustomerCodeField { get; set; }
        public string LinesCollection { get; set; }
        public string ItemCodeField { get; set; }
        public string QuantityField { get; set; }
        public string UnitPriceField { get; set; }
        public string WarehouseField { get; set; }

        public void Validate()
        {
            Require(DataObjectTypeKey, nameof(DataObjectTypeKey));
            Require(DateField, nameof(DateField));
            Require(CustomerCodeField, nameof(CustomerCodeField));
            Require(LinesCollection, nameof(LinesCollection));
            Require(ItemCodeField, nameof(ItemCodeField));
            Require(QuantityField, nameof(QuantityField));
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(name + " must be verified and configured.");
        }
    }
}
