using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser {


    public class Symbol {
        public string Name;
        private ASTElement node;
    }

    public class CScopeSystem{

        public enum CNamespaces{
            Tags,
            Labels,
            Members,
            Ordinary
        };
        

        public void EnterScope(string name) {
        }
        public void ExitScope() {
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
            
            if (string.IsNullOrEmpty(symbol.Name)) {
                throw new ArgumentException("Symbol must have a valid name.", nameof(symbol));
            }
            symbols.Add(key, symbol);
        }

    }
}
