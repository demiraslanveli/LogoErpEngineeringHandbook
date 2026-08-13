using System;
using System.Collections.Generic;

namespace LogoErp.Reference.LogoAdapter.Data
{
    public static class LogoLineWriter
    {
        public static void AppendLine(
            ILogoDataObject dataObject,
            string lineCollectionKey,
            IReadOnlyDictionary<string, object> fields)
        {
            if (dataObject == null)
                throw new ArgumentNullException(nameof(dataObject));

            if (string.IsNullOrWhiteSpace(lineCollectionKey))
                throw new ArgumentException("Line collection key is required.", nameof(lineCollectionKey));

            var line = dataObject.AppendLine(lineCollectionKey);
            if (line == null)
                throw new InvalidOperationException($"Logo line collection returned null: {lineCollectionKey}");

            if (fields == null)
                return;

            foreach (var field in fields)
                line.SetField(field.Key, field.Value);
        }
    }
}
