using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CParser.CScope;

namespace CParser {
    public class CScopeSystem{


        public void EnterScope(CScope.Namespace nspace, string name) {
        }

        public void ExitScope() {
        }

        public void AddSymbol(Namespace nspace, string key, Symbol symbol) {

        }

        private CScopeSystem() {
        }

        private CScope m_currentScope;
        private CScope m_globalScope;
        private static CScopeSystem? m_instance = null;

        public CScope MCurrentScope => m_currentScope;
        public CScope MGlobalScope  => m_globalScope;
           

        public static CScopeSystem GetInstance() {
            if (m_instance == null) {
                m_instance = new CScopeSystem();
            }

            return m_instance;

        }
    }
}
