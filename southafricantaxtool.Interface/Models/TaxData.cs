﻿namespace southafricantaxtool.Interface.Models;

public class TaxData
{
    public List<TaxBracket> TaxBrackets { get; set; }
    public List<TaxRebate> TaxRebates { get; set; }
}