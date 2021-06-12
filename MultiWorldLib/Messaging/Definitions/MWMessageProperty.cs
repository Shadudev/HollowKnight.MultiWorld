using System;
using System.Reflection;

namespace MultiWorldLib.Messaging.Definitions
{
    public class MWMessageProperty<DT, T> : IMWMessageProperty
    {
        public string Name;
        private PropertyInfo _property;
        public Type Type { get; private set; }

        public MWMessageProperty(string name)
        {
            Name = name;
            _property = typeof(T).GetProperty(name, typeof(DT));
            if (_property == null)
            {
                throw new InvalidOperationException(String.Format("No property {0} in class {1}", name, typeof(T)));
            }
            else if (_property.GetSetMethod() == null || _property.GetGetMethod() == null)
            {
                throw new InvalidOperationException(String.Format("Property {0} in class {1} needs publicly accessible getter and setter", name, typeof(T)));
            }
            Type = typeof(DT);
        }

        public void SetValue(object target, object val)
        {
            _property.SetValue(target, val, null);
        }

        public object GetValue(object target)
        {
            return _property.GetValue(target, null);
        }
    }
}
