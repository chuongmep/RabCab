using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace RabCab.Analysis
{
    internal sealed class EntMap : ClassMap<EntInfo>
    {
        private EntMap()
        {
            Map(m => m.FilePath).Index(0).Name("Filepath");
            Map(m => m.RcName).Index(1).Name("Name");
            Map(m => m.RcQtyTotal).Index(2).Name("Qty");
            Map(m => m.RcInfo).Index(3).Name("Notes");
            Map(m => m.EntMaterial).Index(4).Name("Material");
            Map(m => m.Length).Index(5).Name("Length");
            Map(m => m.Width).Index(6).Name("Width");
            Map(m => m.Thickness).Index(7).Name("Thickness");
        }
    }
}
