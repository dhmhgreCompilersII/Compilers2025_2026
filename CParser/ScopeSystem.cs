using System;
using System.Collections.Generic;
using System.Data;
using System.Formats.Asn1;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using static CParser.CScope;

namespace CParser {
    public class CScopeSystem
    {

        public enum ScopeType
        {
            File,
            Function,
            Block,
            Function_Prorotype,
            StructUnionEnum
        };

        public void EnterScope(ScopeType stype, string name)
        {
            CScope newScope = stype switch
            {
                ScopeType.File => new CFileScope(),
                ScopeType.Block => new CBlockScope(m_currentScope),
                ScopeType.Function => new CFunctionScope(m_currentScope),
                ScopeType.Function_Prorotype => new CFunctionScope(m_currentScope),
                ScopeType.StructUnionEnum => new CStructUnionScope(m_currentScope),
                _ => throw new NotImplementedException()
            };
            m_Scopes.Push(newScope);

            if (stype == ScopeType.File)
            {
                m_globalScope = newScope;
            }

            m_currentScope = newScope;
        }

        public void ExitScope() {

        }

        public void AddSymbol(Namespace nspace, string key, Symbol symbol) {

        }

        private CScopeSystem() {
        }

        private Stack<CScope> m_Scopes;
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
