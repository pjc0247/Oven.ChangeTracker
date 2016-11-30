using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Oven;

namespace SO
{
    public class ObservedEntity : TrackingGraph, IFilling
    {
        private Dictionary<string, object> storage = new Dictionary<string, object>();

        public static T Create<T>()
        {
            object impl;
            var obj = Bakery.Bake(typeof(T), typeof(ObservedEntity), out impl);
            ((IObserved)obj).InnerImpl = impl;
            return (T)obj;
        }

        public ObservedEntity()
        {
        }

        public object OnMethod(Type type, MethodInfo method, object[] args)
        {
            if (method.Name == nameof(IObserved.ConfirmChanges))
                ConfirmChanges();

            return null;
        }

        public object OnGetProperty(Type type, string key)
        {
            if (key == nameof(IObserved.HasChanges))
                return HasChanges;
            if (key == nameof(IObserved.InnerImpl))
                return InnerImpl;

            if (type.IsInterface == false)
                return storage[key];

            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                object observed;
                var obj = Bakery.Bake(
                    type, typeof(ObservedCollection<>).MakeGenericType(type.GetGenericArguments()[0]),
                    out observed);
                //((IObserved)obj).InnerImpl = observed;
                ((TrackingGraph)observed).AddReference(this);
                return obj;
            }

            else
            {
                object observed;
                var obj = Bakery.Bake(type, typeof(ObservedEntity), out observed);
                ((IObserved)obj).InnerImpl = observed;
                ((TrackingGraph)observed).AddReference(this);
                return obj;
            }
        }
        public void OnSetProperty(Type type, string key, object value)
        {
            if (key == nameof(IObserved.InnerImpl))
                InnerImpl = value;

            else if (type.IsInterface == false)
                storage[key] = value;

            else // if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
                throw new InvalidOperationException("cannot assign non-prmitive properties");

            HasChanges = true;
        }
    }
}
