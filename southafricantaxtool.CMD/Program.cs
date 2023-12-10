using southafricantaxtool.Shared;
using southafricantaxtool.Shared.Models;


Console.WriteLine("Loading Tax Bracket Data...");
var taxBrackets = await TaxScraper.RetrieveTaxBrackets();

bool ShouldContinue()
{
    Console.Write("Go Again? (Y/N): ");
    return Console.ReadLine()?.ToUpperInvariant() == "Y";
}

while (true)
{
    Console.Write("Tax Bracket Year: ");
    if (!int.TryParse(Console.ReadLine(), out int year))
    {
        Console.WriteLine("Invalid year entered! Try again.");
        if (!ShouldContinue()) break;
        continue;
    }

    var taxBracket = taxBrackets
        .FirstOrDefault(x => x.Start?.Year is { } startYear && x.End?.Year is { } endYear && startYear <= year && endYear >= year);

    if (taxBracket == null)
    {
        Console.WriteLine($"No tax brackets exist for year: {year}");
        if (!ShouldContinue()) break;
        continue;
    }

    decimal annualIncome = 0;

    Console.Write("Do you want to calculate by Monthly or Annual? (M/A): ");
    var monthlyOrAnnualInput = Console.ReadLine();

    var monthly = monthlyOrAnnualInput?.ToUpperInvariant() == "M";

    if (monthly)
    {
        Console.Write("Enter your monthly income: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal monthlyIncome))
        {
            Console.WriteLine("Invalid amount entered! Try again.");
            if (!ShouldContinue()) break;
            continue;
        }

        annualIncome = monthlyIncome * 12;
    }
    else
    {
        Console.Write("Enter your annual income: ");
        if (!decimal.TryParse(Console.ReadLine(), out annualIncome))
        {
            Console.WriteLine("Invalid amount entered! Try again.");
            if (!ShouldContinue()) break;
            continue;
        }
    }

    var correctBracket = taxBracket.Brackets.FirstOrDefault(x => (x.IncomeFrom <= annualIncome && x.IncomeTo >= annualIncome) || (x.IncomeFrom <= annualIncome && !x.IncomeTo.HasValue));

    if (correctBracket == null)
    {
        WriteError("Could not find a valid tax bracket!");
        if (!ShouldContinue()) break;
        continue;
    }

    Console.Write("Do you want to see the Formula? (Y/N): ");
    var showFormulaInput = Console.ReadLine();

    var showFormula = showFormulaInput?.ToUpperInvariant() == "Y";

    DisplayTaxableAmount(correctBracket, annualIncome, showFormula);

    if (!ShouldContinue()) break;
}



void WriteError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ForegroundColor = ConsoleColor.White;
}

void DisplayTaxableAmount(Bracket bracket, decimal annualIncome, bool showFormula = false)
{
    if (showFormula)
    {
        Console.WriteLine("==== FORMULA ===");

        if (bracket.Rule.BaseAmount.HasValue)
        {
            var taxableIncomeAboveThreshold = annualIncome - bracket.IncomeFrom;
            var percentageTax = taxableIncomeAboveThreshold * bracket.Rule.Percentage / 100;

            Console.WriteLine($"Rule: {bracket.Rule.BaseAmount.Value:C2} + {bracket.Rule.Percentage}% of taxable income above {bracket.IncomeFrom:C2}");
            Console.WriteLine($"TAX = {bracket.Rule.BaseAmount.Value:C2} + {bracket.Rule.Percentage}% X ({annualIncome:C2} - {bracket.IncomeFrom:C2})");
            Console.WriteLine($"TAX = {bracket.Rule.BaseAmount.Value:C2} + {bracket.Rule.Percentage}% X {taxableIncomeAboveThreshold:C2}");
            Console.WriteLine($"TAX = {bracket.Rule.BaseAmount.Value:C2} + {percentageTax:C2}");
        }
        else
        {
            Console.WriteLine($"Rule: {bracket.Rule.Percentage}% of Taxable Income");
            Console.WriteLine($"TAX = {annualIncome:C2} X {bracket.Rule.Percentage}%");
        }
    }

    var tax = bracket.Rule.BaseAmount.HasValue
        ? bracket.Rule.BaseAmount.Value + (annualIncome - bracket.IncomeFrom) * bracket.Rule.Percentage / 100
        : Math.Round(annualIncome * ((decimal)bracket.Rule.Percentage / 100), 2);

    Console.WriteLine($"MONTHLY TAX = {tax / 12:C2}");
    Console.WriteLine($"ANNUAL TAX = {tax:C2}");
}

