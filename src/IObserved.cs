   public interface IObserved
    {
        bool HasChanges { get; }

        void ConfirmChanges();

        object InnerImpl { get; set; }
    }


    public class TrackingGraph : IObserved
    {
        private bool _HasChanges;
        public bool HasChanges
        {
            get
            {
                return _HasChanges;
            }
            set
            {
                // ISSUE : 쌍방참조
                _HasChanges = value;

                if (value)
                {
                    foreach (var parent in Parents)
                        parent.HasChanges = true;
                }
            }
        }

        public object InnerImpl { get; set; }

        private List<TrackingGraph> Parents = new List<TrackingGraph>();
        //private TrackingGraph Children { get; set; }

        public void AddReference(TrackingGraph parent)
        {
            Parents.Add(parent);
        }
        public void RemoveReference(TrackingGraph parent)
        {
            Parents.Remove(parent);
        }
        public void ConfirmChanges()
        {
            _HasChanges = false;
        }
    }


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