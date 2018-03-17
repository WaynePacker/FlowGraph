using FlowGraph.UI.NetworkModel.Base;
using System;
using System.Windows;
using Utils;

namespace FlowGraph.UI.NetworkModel
{
    /// <summary>
    /// Defines a connector (aka connection point) that can be attached to a node and is used to connect the node to another node.
    /// </summary>
    public sealed class ConnectorViewModel : AbstractModelBase
    {
        #region Internal Data Members

        /// <summary>
        /// The connections that are attached to this connector, or null if no connections are attached.
        /// </summary>
        private ImpObservableCollection<AConnectionViewModel> attachedConnections = null;

        /// <summary>
        /// The hotspot (or center) of the connector.
        /// This is pushed through from ConnectorItem in the UI.
        /// </summary>
        private Point hotspot;

        #endregion Internal Data Members

        public ConnectorViewModel(string name)
        {
            this.Name = name;
            this.Type = ConnectorType.Undefined;
        }

        /// <summary>
        /// The name of the connector.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Defines the type of the connector.
        /// </summary>
        public ConnectorType Type
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns 'true' if the connector connected to another node.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                foreach (var connection in AttachedConnections)
                {
                    if (connection.SourceConnector != null &&
                        connection.DestConnector != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Returns 'true' if a connection is attached to the connector.
        /// The other end of the connection may or may not be attached to a node.
        /// </summary>
        public bool IsConnectionAttached
        {
            get
            {
                return AttachedConnections.Count > 0;
            }
        }

        /// <summary>
        /// The connections that are attached to this connector, or null if no connections are attached.
        /// </summary>
        public ImpObservableCollection<AConnectionViewModel> AttachedConnections
        {
            get
            {
                if (attachedConnections == null)
                {
                    attachedConnections = new ImpObservableCollection<AConnectionViewModel>();
                    attachedConnections.ItemsAdded += new EventHandler<CollectionItemsChangedEventArgs>(AttachedConnections_ItemsAdded);
                    attachedConnections.ItemsRemoved += new EventHandler<CollectionItemsChangedEventArgs>(AttachedConnections_ItemsRemoved);
                }

                return attachedConnections;
            }
        }

        /// <summary>
        /// The parent node that the connector is attached to, or null if the connector is not attached to any node.
        /// </summary>
        public ANodeViewModel ParentNode
        {
            get;
            internal set;
        }

        /// <summary>
        /// The hotspot (or center) of the connector.
        /// This is pushed through from ConnectorItem in the UI.
        /// </summary>
        public Point Hotspot
        {
            get
            {
                return hotspot;
            }
            set
            {
                if (hotspot == value)
                {
                    return;
                }

                hotspot = value;

                OnHotspotUpdated();
            }
        }

        /// <summary>
        /// Event raised when the connector hotspot has been updated.
        /// </summary>
        public event EventHandler<EventArgs> HotspotUpdated;

        #region Private Methods

        /// <summary>
        /// Debug checking to ensure that no connection is added to the list twice.
        /// </summary>
        private void AttachedConnections_ItemsAdded(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (AConnectionViewModel connection in e.Items)
            {
                connection.ConnectionChanged += new EventHandler<EventArgs>(connection_ConnectionChanged);
            }

            if ((AttachedConnections.Count - e.Items.Count) == 0)
            {
                // 
                // The first connection has been added, notify the data-binding system that
                // 'IsConnected' should be re-evaluated.
                //
                RaisePropertyChanged("IsConnectionAttached");
                RaisePropertyChanged("IsConnected");
            }
        }

        /// <summary>
        /// Event raised when connections have been removed from the connector.
        /// </summary>
        private void AttachedConnections_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (AConnectionViewModel connection in e.Items)
            {
                connection.ConnectionChanged -= new EventHandler<EventArgs>(connection_ConnectionChanged);
            }

            if (AttachedConnections.Count == 0)
            {
                // 
                // No longer connected to anything, notify the data-binding system that
                // 'IsConnected' should be re-evaluated.
                //
                RaisePropertyChanged("IsConnectionAttached");
                RaisePropertyChanged("IsConnected");
            }
        }

        /// <summary>
        /// Event raised when a connection attached to the connector has changed.
        /// </summary>
        private void connection_ConnectionChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("IsConnectionAttached");
            RaisePropertyChanged("IsConnected");
        }

        /// <summary>
        /// Called when the connector hotspot has been updated.
        /// </summary>
        private void OnHotspotUpdated()
        {
            RaisePropertyChanged("Hotspot");
            HotspotUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion Private Methods
    }
}
