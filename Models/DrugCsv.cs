using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
namespace SearchTool_ServerSide.Models
{
    public class DrugCsv
    {
        [Name("drug_name")]
        public string DrugName { get; set; } = "";

        [Name("ndc")]
        public string NDC { get; set; } = "";

        [Name("form")]
        public string Form { get; set; } = "";

        [Name("strength")]
        public string? Strength { get; set; } = "";

        [Name("acq")]
        public decimal ACQ { get; set; } = decimal.Zero;

        [Name("awp")]
        public decimal AWP { get; set; } = decimal.Zero;

        [Name("rxcui")]
        [TypeConverter(typeof(NullableIntConverter))] // Custom converter for empty values

        public int Rxcui { get; set; } = 0;

        [Name("drug_class")]
        public string DrugClass { get; set; } = "";
    }
    public class NullableIntConverter : CsvHelper.TypeConversion.Int32Converter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return string.IsNullOrWhiteSpace(text) ? (int?)null : base.ConvertFromString(text, row, memberMapData);
        }
    }
}