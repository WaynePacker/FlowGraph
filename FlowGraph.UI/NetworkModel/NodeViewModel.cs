using FlowGraph.UI.Interfaces;
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
    public sealed class NodeViewModel : ANodeViewModel, INodeViewModel
    {

        #region Internal Data Members

        private ConnectorViewModel parentNodeConnection;

        #endregion Internal Data Members

        public NodeViewModel() : base()
        {
            ConnectorAddedEvent += NodeViewModel_ConnectorAddedEvent;
            ConnectorRemovedEvent += NodeViewModel_ConnectorRemovedEvent;

            ParentNodeConnection = new ConnectorViewModel("Parent")
            {
                ParentNode = this,
                Type = ConnectorType.Path
            };

            ChildNodeConnection = new ConnectorViewModel("Child")
            {
                ParentNode = this,
                Type = ConnectorType.Path
            };
        }

        public NodeViewModel(string name, Point nodeLocation)
            : base(name, nodeLocation)
        {

            ConnectorAddedEvent += NodeViewModel_ConnectorAddedEvent;
            ConnectorRemovedEvent += NodeViewModel_ConnectorRemovedEvent;

            ParentNodeConnection = new ConnectorViewModel("Parent")
            {
                ParentNode = this,
                Type = ConnectorType.Path
            };

            ChildNodeConnection = new ConnectorViewModel("Child")
            {
                ParentNode = this,
                Type = ConnectorType.Path
            };
        }


        #region Public Properties
        /// <summary>
        /// A helper property that retrieves a list (a new list each time) of all connections attached to the node. 
        /// </summary>
        public override IEnumerable<AConnectionViewModel> AttachedConnections
        {
            get
            {
                var baseAttachedConnections = base.AttachedConnections;
                var attachedConnections = new List<AConnectionViewModel>();

                attachedConnections.AddRange(ParentNodeConnection.AttachedConnections);
                attachedConnections.AddRange(ChildNodeConnection.AttachedConnections);

                return baseAttachedConnections.Concat(attachedConnections);
            }
        }

        /// <summary>
        /// The connector to the Parent node of this node
        /// </summary>
        public ConnectorViewModel ParentNodeConnection
        {
            get { return parentNodeConnection; }
            set { SetAndNotify(ref parentNodeConnection, value); }
        }

        #endregion Public Properties

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
