using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser {
    public class CType{

        public enum TypeKind {
            Void,
            Char,
            Int,
            Float,
            Double,
            Struct,
            Union,
            Enum,
            Pointer,
            Array,
            Function,
            Typedef,
            Qualifier
        }

        private string m_typename;
        TypeKind m_typekind;
        List<CType>[] m_typeparams;

        public CType(TypeKind mTypekind, uint contexts) {
            m_typekind = mTypekind;
            m_typeparams = new List<CType>[contexts];
            for (int i = 0; i < contexts; i++) {
                m_typeparams[i] = new List<CType>();
            }
        }


        public override bool Equals(object? obj) {
            if (obj != null && obj is CType other) {
                return this.GetType() == other.GetType();
            }
            return base.Equals(obj);
        }

        public bool Equals(CType t) {
            if (t == null) {
                return false;
            }

            if (m_typekind != t.m_typekind) {
                return false;
            }

            if (m_typeparams.Length != t.m_typeparams.Length) {
                return false;
            }
            for (int i = 0; i < m_typeparams.Length; i++) {
                if (m_typeparams[i].Count != t.m_typeparams[i].Count) {
                    return false;
                }
                for (int j = 0; j < m_typeparams[i].Count; j++) {
                    if (!m_typeparams[i][j].Equals(t.m_typeparams[i][j])) {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool operator ==(CType? a, CType? b) {
            if (ReferenceEquals(a, b)) {
                return true;
            }
            if (a is null || b is null) {
                return false;
            }
            return a.Equals(b);
        }
        public static bool operator !=(CType? a, CType? b) {
            return !(a == b);
        }
    }



}
