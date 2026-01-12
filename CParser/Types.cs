using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser {
    public class CType {

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

        public enum TypeGranularity {
            Basic,
            Composite
        }

        public TypeKind Kind => m_typekind;
        public TypeGranularity Granularity => m_granularity;

        protected string m_typename;
        protected TypeKind m_typekind;
        protected TypeGranularity m_granularity;
        protected List<CType> m_typeparams;

        public CType(TypeKind mTypekind) {
            m_typekind = mTypekind;
            m_typeparams = new List<CType>();
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


            if (m_typeparams.Count != t.m_typeparams.Count) {
                return false;
            }
            for (int j = 0; j < m_typeparams.Count; j++) {
                if (!m_typeparams[j].Equals(t.m_typeparams[j])) {
                    return false;
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


    public class IntegerType : CType {
        public enum IntegerKind {
            Signed,
            Unsigned
        }
        private IntegerKind m_integerkind;
        private int m_size; // in bytes
        public IntegerType(IntegerKind ikind, int size)
            : base(TypeKind.Int) {
            m_integerkind = ikind;
            m_size = size;
        }
        public override bool Equals(object? obj) {
            return base.Equals(obj);
        }

        public bool Equals(CType t) {
            if (t is IntegerType it) {
                return this.m_integerkind == it.m_integerkind &&
                       this.m_size == it.m_size;
            }
            return false;
        }
    }

    public class FloatingPointType : CType {
        private int m_size; // in bytes
        public FloatingPointType(int size)
            : base(TypeKind.Float) {
            m_size = size;
        }
        public override bool Equals(object? obj) {
            return base.Equals(obj);
        }
        public bool Equals(CType t) {
            if (t is FloatingPointType ft) {
                return this.m_size == ft.m_size;
            }
            return false;
        }
    }

    public class StructType : CType {
        public StructType()
            : base(TypeKind.Struct) {
        }
        public bool Equals(CType t) {
            if (t is StructType st) {
                if (st.m_typeparams.Count != m_typeparams.Count) {
                    return false;
                }
                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(st.m_typeparams[i])) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }

    public class UnionType : CType {
        public UnionType()
            : base(TypeKind.Union) {
        }
        public bool Equals(CType t) {
            if (t is UnionType ut) {
                if (ut.m_typeparams.Count != m_typeparams.Count) {
                    return false;
                }
                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(ut.m_typeparams[i])) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }

    public class EnumType : CType {
        public EnumType()
            : base(TypeKind.Enum) {
        }
        public bool Equals(CType t) {
            if (t is EnumType et) {
                if (et.m_typeparams.Count != m_typeparams.Count) {
                    return false;
                }
                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(et.m_typeparams[i])) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }


    public class FunctionType : CType {
        public FunctionType()
            : base(TypeKind.Function) {
        }
        public bool Equals(CType t) {
            if (t is FunctionType ft) {
                if (this.m_typeparams.Count != ft.m_typeparams.Count) {
                    return false;
                }
                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(ft.m_typeparams[i])) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public void AddParameterType(CType pt) {
            m_typeparams.Add(pt);
        }

    }

    public class ArrayType : CType {
        private CType m_elementType;

        // Dimension sizes for each dimension. Low-level to high-level meaning
        // first element is size of first dimension, second element is size of second dimension, etc.
        private List<int> m_dimensionSize;

        public CType MElementType {
            get => m_elementType;
            set => m_elementType = value ?? throw new ArgumentNullException(nameof(value));
        }

        public ArrayType(CType elementType)
            : base(TypeKind.Array) {
            m_elementType = elementType;
            m_dimensionSize = new List<int>();
        }

        public void AddHigherLevelDimensionSize(int size) {
            // place at the end
            m_dimensionSize.Add(size);
        }

        public void AddLowerLevelDimensionSize(int size) {
            // place at the beginning
            m_dimensionSize.Insert(0, size);

        }

        public bool Equals(CType t) {
            if (t is ArrayType at) {
                if (!this.m_elementType.Equals(at.m_elementType)) {
                    return false;
                }
                if (this.m_dimensionSize.Count != at.m_dimensionSize.Count) {
                    return false;
                }
                for (int i = 0; i < this.m_dimensionSize.Count; i++) {
                    if (this.m_dimensionSize[i] != at.m_dimensionSize[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;

        }
    }


}
