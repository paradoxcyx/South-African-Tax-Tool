﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace southafricantaxtool.Interface.Models
{
    public class BracketRule
    {
        public decimal? BaseAmount { get; set; }
        public int Percentage { get; set; }
        public decimal? Threshold { get; set; }
    }
}
