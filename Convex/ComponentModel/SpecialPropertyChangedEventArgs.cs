#region usings

using System.ComponentModel;

#endregion

namespace Convex.ComponentModel {
    public sealed class SpecialPropertyChangedEventArgs : PropertyChangedEventArgs {
        public SpecialPropertyChangedEventArgs(string propertyName, string name, object newValue) : base(propertyName) {
            Name = name;
            NewValue = newValue;
        }

        public string Name { get; private set; }
        public object NewValue { get; private set; }
    }
}