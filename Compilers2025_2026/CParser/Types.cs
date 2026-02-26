using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using static CParser.IntegerType;
using static CParser.PointerTypeAST;

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
            Qualifier,
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
        private CType m_parent;
        protected List<CType> m_typeparams; // e.g., function parameter types, struct member types, etc.

        // For Debugging purposes
        private int m_typeserial;
        private static int ms_typeserialCounter;

        public CType(TypeKind mTypekind) {
            m_typekind = mTypekind;
            m_typeparams = new List<CType>();
            // For debugging
            m_typeserial = ms_typeserialCounter++;
        }

        public void AddTypeParameter(CType param) {
            param.m_parent = this;
            m_typeparams.Add(param);
        }

        public virtual void TypeDebugLog(StreamWriter m_logFile=null) {
            if (m_parent == null) {
                m_logFile = new StreamWriter("type_log.dot");
                m_logFile.WriteLine("digraph G{ ");
                m_logFile.WriteLine($"\"{ToString()}_{m_typeserial}\"");
            } else {
                m_logFile.WriteLine($"\"{m_parent.ToString()}_{m_parent.m_typeserial}\"->\"{ToString()}_{m_typeserial}\"");
            }

            foreach (CType typeparam in m_typeparams) {
                typeparam.TypeDebugLog(m_logFile);
            }

            if (m_parent == null) {
                m_logFile.WriteLine("};");
                m_logFile.Close();
                TryGenerateTypeGraphImage("type_log.dot", "type_log.gif");
            }
        }

        private static void TryGenerateTypeGraphImage(string dotFilePath, string outputImagePath) {
            try {
                var processStartInfo = new ProcessStartInfo {
                    FileName = "dot",
                    Arguments = $"-Tgif \"{dotFilePath}\" -o \"{outputImagePath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                using (var process = new Process { StartInfo = processStartInfo }) {
                    process.Start();
                    process.WaitForExit();
                }
            } catch (Exception ex) {
                // Intentionally ignore failures in debug helper
                Console.WriteLine($"[ERROR] Αποτυχία εκτέλεσης του Graphviz (dot.exe): {ex.Message}");
            }
        }

        public override bool Equals(object? obj) {
            if (obj != null && obj is CType other) {
                return GetType() == other.GetType();
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

    public class PointerType : CType {
        public PointerType()
            : base(TypeKind.Pointer) {
        }

        public override bool Equals(object? obj) {
            return base.Equals(obj);
        }

        public bool Equals(CType t) {
            if (t is PointerType pt) {
                if (pt.m_typeparams.Count != m_typeparams.Count) {
                    return false;
                }

                for (int i = 0; i < m_typeparams.Count; i++) {
                    if (!m_typeparams[i].Equals(pt.m_typeparams[i])) {
                        return false;
                    }
                }
            }

            return false;
        }

        public override string ToString() {
            return "pointer";
        }
    }

    public class QualifierType : CType
    {
        public enum QualifierKind
        {
            Const,
            Volatile
        }

        private QualifierKind m_qualifier;

        public QualifierType(QualifierKind qualifier)
            : base(TypeKind.Qualifier)
        {
            m_qualifier = qualifier;
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(CType t)
        {
            if (t is QualifierType qt)
            {
                
                if (m_qualifier != qt.m_qualifier || m_typeparams.Count != qt.m_typeparams.Count)
                {
                    return false;
                }
                for (int i = 0; i < m_typeparams.Count; i++)
                {
                    if (!m_typeparams[i].Equals(qt.m_typeparams[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return m_qualifier == QualifierKind.Const ? "const" : "volatile";
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
                return m_integerkind == it.m_integerkind &&
                       m_size == it.m_size;
            }

            return false;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return m_typename;
            }

            string sign = m_integerkind == IntegerKind.Unsigned ? "unsigned " : "signed ";
            return $"{sign}int{m_size * 8}";
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
                return m_size == ft.m_size;
            }

            return false;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return m_typename;
            }

            return m_size switch {
                4 => "float",
                8 => "double",
                _ => $"float{m_size * 8}"
            };
        }
    }

    public class VoidType : CType {
        private int m_size;
        public VoidType(int size)
            : base(TypeKind.Void) {
            m_size = size;
        }
        public override bool Equals(object? obj) {
            return base.Equals(obj);
        }
        public bool Equals(CType t) {
            return t is VoidType;
        }
        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return m_typename;
            }
            return "void";
        }
    }

    public class CharType : CType {

        public enum CharacterKind
        {
            Signed,
            Unsigned
        }

        private CharacterKind m_characterkind;
        private int m_size;
        public CharType(CharacterKind characterkind, int size)
            : base(TypeKind.Char) {
            m_size = size;
            m_characterkind = characterkind;
        }
        public override bool Equals(object? obj) {
            return base.Equals(obj);
        }
        public bool Equals(CType t)
        {
            if (t is CharType it)
            {
                return m_characterkind == it.m_characterkind &&
                       m_size == it.m_size;
            }

            return false;
        }
        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return m_typename;
            }
            string sign = m_characterkind == CharacterKind.Unsigned ? "unsigned " : "signed ";
            return $"{sign}char{m_size}";
        }
    }

    public class StorageClassType : CType
    {
        public string StorageClass { get; }

        public StorageClassType(string storageClass) : base(TypeKind.Qualifier)
        {
            StorageClass = storageClass;
        }
        public void AddTypeParameter(CType typeParam)
        {
            // Κάλεσε την base μέθοδο ή πρόσθεσέ το στη λίστα των παιδιών, 
            // ακριβώς όπως το έχεις γράψει και μέσα στην κλάση QualifierType!
            base.AddTypeParameter(typeParam);
        }

        public override string ToString()
        {
            return StorageClass; // π.χ. θα τυπώσει "static" ή "auto"
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

        public void SetName(string name) {
            m_typename = name;
        }

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return $"struct {m_typename}";
            }

            return "struct";
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

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return $"union {m_typename}";
            }

            return "union";
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

        public override string ToString() {
            if (!string.IsNullOrEmpty(m_typename)) {
                return $"enum {m_typename}";
            }

            return "enum";
        }
    }

    public class FunctionType : CType {
        public FunctionType()
            : base(TypeKind.Function) {
        }

        public bool Equals(CType t) {
            if (t is FunctionType ft) {
                if (m_typeparams.Count != ft.m_typeparams.Count) {
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

        public override string ToString() {
            if (m_typeparams.Count == 0) {
                return "function()";
            }

            CType returnType = m_typeparams[0];
            List<string> paramStrings = new List<string>();

            for (int i = 1; i < m_typeparams.Count; i++) {
                paramStrings.Add(m_typeparams[i].ToString());
            }

            string parameters = string.Join(", ", paramStrings);
            return $"{returnType} ({parameters})";
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
                if (!m_elementType.Equals(at.m_elementType)) {
                    return false;
                }

                if (m_dimensionSize.Count != at.m_dimensionSize.Count) {
                    return false;
                }

                for (int i = 0; i < m_dimensionSize.Count; i++) {
                    if (m_dimensionSize[i] != at.m_dimensionSize[i]) {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override string ToString() {
            string result = "";

            foreach (int size in m_dimensionSize) {
                result += size > 0 ? $"[{size}]" : "[]";
            }

            return $"array {result}".Trim();
        }
    }
}
