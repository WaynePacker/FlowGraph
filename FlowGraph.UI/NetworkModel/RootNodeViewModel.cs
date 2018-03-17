using FlowGraph.UI.NetworkModel.Base;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FlowGraph.UI.NetworkModel
{
    public sealed class RootNodeViewModel : ANodeViewModel
    {
        #region Internal Data Members
        private ConnectorViewModel childNodeConnection;

        #endregion Internal Data Members

        public RootNodeViewModel(string name, Point nodeLocation)
            : base(name, nodeLocation)
        {
            isRootNode = true;

            ConnectorAddedEvent += NodeViewModel_ConnectorAddedEvent;
            ConnectorRemovedEvent += NodeViewModel_ConnectorRemovedEvent;

            ChildNodeConnection = new ConnectorViewModel("Child")
            {
                ParentNode = this,
                Type = ConnectorType.Child
            };
        }

        #region Public Properties

        /// <summary>
        /// The connector to the Child node of this node
        /// </summary>
        public ConnectorViewModel ChildNodeConnection
        {
            get { return childNodeConnection; }
            set { SetAndNotify(ref childNodeConnection, value); }
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

                attachedConnections.AddRange(ChildNodeConnection.AttachedConnections);

                return baseAttachedConnections.Concat(attachedConnections);
            }
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
