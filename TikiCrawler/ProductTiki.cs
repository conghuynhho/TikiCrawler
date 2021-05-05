using System;
using System.Collections.Generic;
using System.Text;
using FileHelpers;

namespace TikiCrawler
{
    [DelimitedRecord("@")]
    class ProductTiki
    {
        public string SKU { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SalePrice { get; set; }
        public string RegularPrice { get; set; }
        public string Categories { get; set; }
        public string Images { get; set; }
    }
}
