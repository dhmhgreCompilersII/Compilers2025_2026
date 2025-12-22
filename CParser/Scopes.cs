using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser{
    public class CScope{
        public enum Namespace{
            Labels,
            Tags,
            Members,
            Ordinary
        }

        // Lexical scoping - parent scope
        CScope? m_parent;

        // Different namespaces within this scope
        Dictionary<Namespace, SymbolTable> m_namespaces = new Dictionary<Namespace, SymbolTable>();
        public CScope? MParent => m_parent;

        public CScope(CScope? parent) {
            m_parent = parent;
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
        public CFunctionScope(CScope parent) : base(parent) {
            InitializeNamespace(Namespace.Ordinary);
            InitializeNamespace(Namespace.Labels);
            InitializeNamespace(Namespace.Tags);
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
        public CFunctionPrototypeScope(CScope parent) : base(parent) {
            InitializeNamespace(Namespace.Ordinary);
            InitializeNamespace(Namespace.Tags);
        }
    }
    public class CStructUnionScope : CScope{
        public CStructUnionScope(CScope parent) : base(parent) {
            InitializeNamespace(Namespace.Members);
            InitializeNamespace(Namespace.Tags);
        }
    }
}
