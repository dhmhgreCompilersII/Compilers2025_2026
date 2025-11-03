using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser {
    public class ASTPrinterVisitor :BaseASTVisitor<int>{

        public ASTPrinterVisitor() { }


        public override int VisitTranslationUnit(TranslationUnitAST node) {
            Console.WriteLine("Visiting Translation Unit AST Node");
            return VisitChildren(node);
        }
    }
}
