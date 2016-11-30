using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Oven;

namespace SO
{
    public class ObservedCollection<T> : TrackingGraph, IFilling, IObserved
    {
        private List<T> Items = new List<T>();

        public ObservedCollection()
        {
        }

        public object OnGetProperty(Type type, string key)
        {
            return typeof(List<T>).GetProperty(key)
                .GetValue(Items);
        }

        public object OnMethod(Type type, MethodInfo method, object[] args)
        {
            if (method.Name == nameof(ICollection<T>.Add))
            {
                HasChanges = true;

                if (args[0] is IObserved)
                {
                    ((TrackingGraph)((IObserved)args[0]).InnerImpl).AddReference(this);
                }
            }
            else if (method.Name == nameof(ICollection<T>.Clear))
            {
                HasChanges = true;

                if (typeof(T).GetInterface(nameof(IObserved)) != null)
                {
                    foreach (var item in Items)
                        ((TrackingGraph)((IObserved)item).InnerImpl).RemoveReference(this);
                }
            }
            else if (method.Name == nameof(ICollection<T>.Remove))
            {
                HasChanges = true;

                if (args[0] is IObserved)
                {
                    ((TrackingGraph)((IObserved)args[0]).InnerImpl).RemoveReference(this);
                }
            }

            return typeof(List<T>)
                .GetMethod(
                    method.Name,
                    method.GetParameters().Select(x => x.ParameterType).ToArray())
                .Invoke(Items, args);
        }

        public void OnSetProperty(Type type, string key, object value)
        {
        }
    }
}
