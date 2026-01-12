using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.Symbol;

namespace CParser {

            CScopeSystem.GetInstance().EnterScope(ScopeType.File);
            base.VisitTranslationUnit(node, info);
            CScopeSystem.GetInstance().ExitScope();
            return 0;
        }


            // 1. Visit function name and place it to current scope (global scope)
            IDENTIFIER? functionName =
                node.GetChild<IDENTIFIER>(FunctionDefinitionAST.DECLARATOR);
            if (functionName == null) {
                throw new Exception("FunctionDefinitionAST has no function name.");
            }

            return 0;
        }
    }
}
