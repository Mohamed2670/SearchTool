using CsvHelper.Configuration.Attributes;

namespace SearchTool_ServerSide.Models
{

    public class NadacRecord
    {
        [Name("NDC")]
        public string NDC { get; set; }

        [Name("NDC Description")]
        public string DrugName { get; set; }

        [Name("NADAC Per Unit")]
        public decimal NadacPerUnit { get; set; }

        [Name("Effective Date")]
        public DateTime EffectiveDate { get; set; }

        [Name("Pricing Unit")]
        public string PricingUnit { get; set; }

        [Name("Pharmacy Type Indicator")]
        public string PharmacyTypeIndicator { get; set; }

        [Name("OTC")]
        public string OTC { get; set; }  // Y/N as string

        [Name("Explanation Code")]
        public string ExplanationCode { get; set; }

        [Name("Classification for Rate Setting")]
        public string ClassificationForRateSetting { get; set; }

        [Name("Corresponding Generic Drug NADAC Per Unit")]
        public string CorrespondingGenericNadacPerUnit { get; set; }  // Can be empty or decimal, keep as string if unsure

        [Name("Corresponding Generic Drug Effective Date")]
        public string CorrespondingGenericEffectiveDate { get; set; } // Same here

        [Name("As of Date")]
        public DateTime AsOfDate { get; set; }
    }

}