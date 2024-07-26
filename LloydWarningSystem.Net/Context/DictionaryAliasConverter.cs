using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace LloydWarningSystem.Net.Context;

public class DictionaryValueConverter : ValueConverter<Dictionary<string, string>, string>
{
    public DictionaryValueConverter() : base(
        v => JsonConvert.SerializeObject(v),
        v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v))
    {
    }
}