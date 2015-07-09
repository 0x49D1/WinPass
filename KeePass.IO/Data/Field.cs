using System;
using System.Diagnostics;

namespace KeePass.IO.Data
{
    [DebuggerDisplay("Field {Name}")]
    public class Field
    {
        public string Name { get; set; }

        public bool Protected { get; set; }

        public string Value { get; set; }

        public Field Clone()
        {
            return (Field)MemberwiseClone();
        }
        public override bool Equals(object obj)
        {
            var eq = obj as Field;
            return eq != null ? eq.Name.Equals(Name) : base.Equals(obj);
        }
    }
}