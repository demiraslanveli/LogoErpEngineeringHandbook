namespace LogoErp.Reference.LogoAdapter.Documents
{
    /// <summary>
    /// Version-specific Logo Objects identifiers for dispatch and invoice IData objects.
    /// Exact values must be populated only after verification against the installed SDK.
    /// </summary>
    public sealed class DispatchInvoiceDataMappingProfile
    {
        public string DispatchDataObjectTypeKey { get; set; }
        public string InvoiceDataObjectTypeKey { get; set; }

        public string DocumentNumberField { get; set; }
        public string DateField { get; set; }
        public string CustomerCodeField { get; set; }
        public string LinesCollectionKey { get; set; }

        public string ItemCodeField { get; set; }
        public string QuantityField { get; set; }
        public string UnitPriceField { get; set; }
        public string WarehouseNumberField { get; set; }
        public string VatRateField { get; set; }

        public void ValidateForDispatch()
        {
            ValidateCommon(DispatchDataObjectTypeKey, "DispatchDataObjectTypeKey");
        }

        public void ValidateForInvoice()
        {
            ValidateCommon(InvoiceDataObjectTypeKey, "InvoiceDataObjectTypeKey");
        }

        private void ValidateCommon(string objectTypeKey, string objectTypeName)
        {
            Require(objectTypeKey, objectTypeName);
            Require(DocumentNumberField, nameof(DocumentNumberField));
            Require(DateField, nameof(DateField));
            Require(CustomerCodeField, nameof(CustomerCodeField));
            Require(LinesCollectionKey, nameof(LinesCollectionKey));
            Require(ItemCodeField, nameof(ItemCodeField));
            Require(QuantityField, nameof(QuantityField));
            Require(UnitPriceField, nameof(UnitPriceField));
            Require(WarehouseNumberField, nameof(WarehouseNumberField));
            Require(VatRateField, nameof(VatRateField));
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new System.InvalidOperationException(name + " is not configured.");
        }
    }
}
