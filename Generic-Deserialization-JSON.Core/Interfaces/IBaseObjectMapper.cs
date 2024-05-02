namespace Generic_Deserialization_JSON.Core.Interfaces
{
    public interface IBaseObjectMapper<TObject>
    {
        TObject MapToSingle(string inputString, string configString);

        IList<TObject> MapToList(string inputString, string configString, string? rootElement = null);
    }
}
