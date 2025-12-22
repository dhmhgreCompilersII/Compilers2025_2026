using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser{

    public enum ScopeType {
        File,
        Function,
        Block,
        FunctionPrototype,
        StructUnionEnum
    }

    public class CScope{
        public enum Namespace{
            Labels,
            Tags,
            Members,
            Ordinary
        }

        // Lexical scoping - parent scope
        CScope? m_parent;
        ScopeType m_scopeType;
        private List<CScope> m_childScopes;

        // Different namespaces within this scope
        Dictionary<Namespace, SymbolTable> m_namespaces = new Dictionary<Namespace, SymbolTable>();
        public CScope? MParent => m_parent;
        public ScopeType MScopeType => m_scopeType;

        public CScope(CScope? parent) {
            m_parent = parent;
            m_childScopes = new List<CScope>();
        }

        public void AddChildScope(CScope child) {
            m_childScopes.Add(child);
        }

        protected void InitializeNamespace(Namespace nspace) {
            if (!m_namespaces.ContainsKey(nspace)) {
                m_namespaces[nspace] = new SymbolTable();
            }
        }

        public void AddSymbol(Namespace nspace, string key, Symbol symbol) {
            if (m_namespaces.ContainsKey(nspace)) {
                m_namespaces[nspace].AddSymbol(key, symbol);
            }
            else {
                throw new Exception("Namespace not initialized in this scope.");
            }
        }

        public Symbol LookupSymbol(Namespace nspace, string key) {
            if (m_namespaces.ContainsKey(nspace)) {
                Symbol? symbol = m_namespaces[nspace].LookupSymbol(key);
                if (symbol != null) {
                    return symbol;
                }
                else if (m_parent != null) {
                    return m_parent.LookupSymbol(nspace, key);
                }
                else {
                    return null;
                }
            }
            else {
                throw new Exception("Namespace not initialized in this scope.");
            }
        }
    }

    public class CFunctionScope : CScope{
        private string m_name;
        public string MName => m_name;
        public CFunctionScope(CScope parent, string name) : base(parent) {
            InitializeNamespace(Namespace.Ordinary);
            InitializeNamespace(Namespace.Labels);
            InitializeNamespace(Namespace.Tags);
            m_name = name;
        }
    }

    public class CFileScope : CScope{
        public CFileScope() : base(null) {
            InitializeNamespace(Namespace.Ordinary);
            InitializeNamespace(Namespace.Tags);
        }
    }

    public class CBlockScope : CScope{
        public CBlockScope(CScope parent) : base(parent) {
            InitializeNamespace(Namespace.Ordinary);
            InitializeNamespace(Namespace.Tags);
        }
    }

    public class CFunctionPrototypeScope : CScope{
        private string m_name;
        public string MName => m_name;

        public CFunctionPrototypeScope(CScope parent,string name) : base(parent) {
            InitializeNamespace(Namespace.Ordinary);
            InitializeNamespace(Namespace.Tags);
            m_name = name;

        }
    }
    public class CStructUnionEnumScope : CScope{
        private string m_name;
        public CStructUnionEnumScope(CScope parent,string name) : base(parent) {
            InitializeNamespace(Namespace.Members);
            InitializeNamespace(Namespace.Tags);
            m_name = name;
        }
    }
}
