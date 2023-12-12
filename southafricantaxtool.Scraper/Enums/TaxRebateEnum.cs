using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace southafricantaxtool.Scraper.Enums
{
    public enum TaxRebateEnum
    {
        //Under 65 years old
        Primary = 1,

        //Over 65 and under 75 years old
        Secondary = 2,

        //Over 75 years old
        Tertiary = 3
    }
}
