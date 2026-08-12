using System;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Data
{
    /// <summary>
    /// Version-neutral wrapper around a Logo IData instance.
    /// The concrete COM object must stay inside LogoAdapter.
    /// </summary>
    public interface ILogoDataObject : IDisposable
    {
        OperationResult SetField(string fieldName, object value);

        OperationResult AppendLine(
            string collectionName,
            Action<ILogoDataObjectLine> mapLine);

        OperationResult Post();

        string ErrorCode { get; }
        string ErrorDescription { get; }
    }

    public interface ILogoDataObjectLine
    {
        OperationResult SetField(string fieldName, object value);
    }
}
