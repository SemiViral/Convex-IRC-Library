#region usings

using System.ComponentModel;

#endregion

namespace Convex.ComponentModel {
    public class SpecialPropertyChangedEventArgs : PropertyChangedEventArgs {
        public SpecialPropertyChangedEventArgs(string propertyName, string name, object newValue) : base(propertyName) {
            Name = name;
            NewValue = newValue;
        }

        public virtual string Name { get; private set; }
        public virtual object NewValue { get; private set; }
    }
}