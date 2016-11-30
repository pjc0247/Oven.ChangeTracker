using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SO
{
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
}
