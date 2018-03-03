using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowGraph.Nodes
{
    public class NodeBase<T>
    {
        private ObservableCollection<object> inputConnectors = null;
        private ObservableCollection<object> outputConnectors = null;
    }
}
