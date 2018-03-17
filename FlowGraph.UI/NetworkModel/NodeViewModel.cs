using FlowGraph.UI.NetworkModel.Base;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FlowGraph.UI.NetworkModel
{
    /// <summary>
    /// Defines a node in the view-model.
    /// Nodes are connected to other nodes through attached connectors (aka anchor/connection points).
    /// </summary>
    public sealed class NodeViewModel : ANodeViewModel
    {
        /// <summary>
        /// Each node will have one parent and one child nodes
        /// exceptions to this will be starting nodes which will only have a child node
        /// and also nodes whos' output connectors are also Nodes like a branch( e.g an if then else statement i.e there are two paths)
        /// </summary>
        
        private ConnectorViewModel parentNodeConnection;
        private ConnectorViewModel childNodeConnection;

        public NodeViewModel(string name, Point nodeLocation)
            : base(name, nodeLocation)
        {

            ConnectorAddedEvent += NodeViewModel_ConnectorAddedEvent;
            ConnectorRemovedEvent += NodeViewModel_ConnectorRemovedEvent;

            ParentNodeConnection = new ConnectorViewModel("Parent")
            {
                ParentNode = this,
                Type = ConnectorType.Parent
            };

            ChildNodeConnection = new ConnectorViewModel("Child")
            {
                ParentNode = this,
                Type = ConnectorType.Child
            };
        }

        /// <summary>
        /// A helper property that retrieves a list (a new list each time) of all connections attached to the node. 
        /// </summary>
        public override IEnumerable<AConnectionViewModel> AttachedConnections
        {
            get
            {
                var baseAttachedConnections = base.AttachedConnections;
                var attachedConnections = new List<AConnectionViewModel>();

                if (!isRootNode)
                    attachedConnections.AddRange(ParentNodeConnection.AttachedConnections);

                if (!isBranch)
                    attachedConnections.AddRange(ChildNodeConnection.AttachedConnections);

                return baseAttachedConnections.Concat(attachedConnections);
            }
        }

        public ConnectorViewModel ParentNodeConnection
        {
            get { return parentNodeConnection; }
            set { SetAndNotify(ref parentNodeConnection, value); }
        }


        public ConnectorViewModel ChildNodeConnection
        {
            get { return childNodeConnection; }
            set { SetAndNotify(ref childNodeConnection, value); }
        }

        #region Private Methods

        private void NodeViewModel_ConnectorAddedEvent(ConnectorViewModel connector)
        {
            connector.ParentNode = this;
        }

        private void NodeViewModel_ConnectorRemovedEvent(ConnectorViewModel connector)
        {
            connector.ParentNode = null;
        }

        #endregion Private Methods
    }
}
