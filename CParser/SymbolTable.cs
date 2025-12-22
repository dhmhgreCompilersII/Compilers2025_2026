using Antlr4.Runtime.Sharpen;
using CParser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CParser.CScope;

namespace CParser {


    public class Symbol {
        public enum SymbolType
        {
            Variable,
            Function,
            Type
        }
        public string m_name;
        private ASTElement m_node;
        public SymbolType m_type;

        public Symbol(){}

        public Symbol(String name, SymbolType symbol, ASTElement node)
        {
            m_name = name;
            m_node = node;
            m_type = symbol;
        }
    }

    public class CScope
    {
        public enum Namespace{
            Labels,
            Tags,
            Members,
            Ordinary
        }

        private CScope m_parent;

        private Dictionary<Namespace, SymbolTable> m_namespaces = new Dictionary<Namespace, SymbolTable>();

        public CScope(CScope? parent)
        {
            m_parent = parent;
        }

        public void AddSymbol(Namespace nspace, string key, Symbol symbol)
        {

        }

        public Symbol LookUpSymbol(string key)
        {
            return null;
        }

        protected void InitializeNamespace(Namespace n_space)
        {
            if (!m_namespaces.ContainsKey(n_space))
            {

            }
        }
        
    }

    public class CScopeSystem{

        public void EnterScope(CScope.Namespace nspace, string name) {

        }

        public void ExitScope() {

        }

        private CScopeSystem(){}

        private CScope m_currentScope;
        private CScope m_globalScope;
        private CScopeSystem? m_instance = null;

        public CScope MCurrentScope => m_currentScope;

        public CScope MGlobalScope=>m_globalScope;

        public void AddSymbol(Namespace nspace, string key, Symbol symbol)
        {

        }

        public static CScopeSystem GetInstance()
        {

        }
    }

    public class SymbolTable{
        private SymbolTable m_parent;

        Dictionary<string,Symbol> symbols = new Dictionary<string,Symbol>();

        // Lexical scoping - parent symbol table
        public Symbol LookupSymbol(string key) {
            if (symbols.ContainsKey(key)) {
                return symbols[key];
            } else if (m_parent != null) {
                return m_parent.LookupSymbol(key);
            } else {
                return null;
            }
        }
        public void AddSymbol(string key, Symbol symbol) {
            if (symbols.ContainsKey(key)) {
                throw new InvalidOperationException($"Symbol '{key}' already exists in the current scope.");
            }
            
            if (symbol == null) {
                throw new ArgumentNullException(nameof(symbol), "Cannot add null symbol to symbol table.");
            }
            
            if (string.IsNullOrEmpty(symbol.m_name)) {
                throw new ArgumentException("Symbol must have a valid name.", nameof(symbol));
            }
            symbols.Add(key, symbol);
        }

    }
}

public class CFunctionScope : CScope
{
    public CFunctionScope() : base(){

    }
}

public class CBlockScope : CScope
{
    public CBlockScope() : base()
    {

    }
}

public class CFunctionPrototypeScope : CScope
{
    public CFunctionPrototypeScope() : base()
    {

    }
}

public class CFileScope : CScope
{
    public CFileScope() : base(null)
    {

    }
}
    