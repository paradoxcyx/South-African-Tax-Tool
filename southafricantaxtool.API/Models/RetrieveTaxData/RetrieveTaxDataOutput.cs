namespace southafricantaxtool.API.Models.RetrieveTaxData
{
    public class RetrieveTaxDataOutput : RetrieveTaxOutput
    {
        public string? Rule { get; set; }
        public List<string>? FormulaSteps { get; set; }
    }
}
