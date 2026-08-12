using System;

namespace LogoErp.Reference.LogoAdapter.Data
{
    public sealed class MaterialDataMappingProfile
    {
        public string DataObjectTypeKey { get; set; }
        public string CodeField { get; set; }
        public string NameField { get; set; }

        public void Validate()
        {
            Require(DataObjectTypeKey, nameof(DataObjectTypeKey));
            Require(CodeField, nameof(CodeField));
            Require(NameField, nameof(NameField));
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(name + " must be verified and configured.");
        }
    }

    public sealed class CustomerDataMappingProfile
    {
        public string DataObjectTypeKey { get; set; }
        public string CodeField { get; set; }
        public string TitleField { get; set; }
        public string TaxNumberField { get; set; }
        public string TaxOfficeField { get; set; }

        public void Validate()
        {
            Require(DataObjectTypeKey, nameof(DataObjectTypeKey));
            Require(CodeField, nameof(CodeField));
            Require(TitleField, nameof(TitleField));
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(name + " must be verified and configured.");
        }
    }
}
