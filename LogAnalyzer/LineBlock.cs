using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzer {
    public class LineBlock {
        public List<LogLine> Lines { get; set; }
        public int? StartLineNumber { get; set; }
        public int? EndLineNumber { get; set; }
        public string Title { get; set; }
    }
}
