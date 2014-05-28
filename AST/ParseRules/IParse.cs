using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AST.ParseRules {
    public interface IParse {
        void Parse(string text, out string inspection, out string node, out List<string> children);
    }
}
