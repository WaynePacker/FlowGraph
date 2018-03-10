using System;
using System.Windows;
using System.Windows.Media;
using Utils;


namespace FlowGraph.UI.NetworkModel
{
    public abstract class AConnectionViewModel : AbstractModelBase
    {
        /// <summary>
        /// The source connector the connection is attached to.
        /// </summary>
        private ConnectorViewModel sourceConnector = null;

        /// <summary>
        /// The destination connector the connection is attached to.
        /// </summary>
        private ConnectorViewModel destConnector = null;

        /// <summary>
        /// The source and dest hotspots used for generating connection points.
        /// </summary>
        private Point sourceConnectorHotspot;
        private Point destConnectorHotspot;

        /// <summary>
        /// Points that make up the connection.
        /// </summary>
        private PointCollection points = null;

        /// <summary>
        /// The source connector the connection is attached to.
        /// </summary>
        public ConnectorViewModel SourceConnector
        {
            get { return sourceConnector; }
            set
            {
                var previousSourceConnector = sourceConnector;

                if (SetAndNotify(ref sourceConnector, value))
                {
                    if (previousSourceConnector != null)
                    {
                        previousSourceConnector.AttachedConnections.Remove(this);
                        previousSourceConnector.HotspotUpdated -= new EventHandler<EventArgs>(SourceConnector_HotspotUpdated);
                    }

                    if (sourceConnector != null)
                    {
                        sourceConnector.AttachedConnections.Add(this);
                        sourceConnector.HotspotUpdated += new EventHandler<EventArgs>(SourceConnector_HotspotUpdated);
                        SourceConnectorHotspot = sourceConnector.Hotspot;
                    }

                    OnConnectionChanged();

                }
            }
        }

        /// <summary>
        /// The destination connector the connection is attached to.
        /// </summary>
        public ConnectorViewModel DestConnector
        {
            get { return destConnector; }
            set
            {
                var previousSourceConnector = destConnector;

                if (SetAndNotify(ref destConnector, value))
                {
                    if (previousSourceConnector != null)
                    {
                        previousSourceConnector.AttachedConnections.Remove(this);
                        previousSourceConnector.HotspotUpdated -= new EventHandler<EventArgs>(DestConnector_HotspotUpdated);
                    }

                    if (destConnector != null)
                    {
                        destConnector.AttachedConnections.Add(this);
                        destConnector.HotspotUpdated += new EventHandler<EventArgs>(DestConnector_HotspotUpdated);
                        DestConnectorHotspot = destConnector.Hotspot;
                    }

                    OnConnectionChanged();
                } 
            }
        }

        /// <summary>
        /// The source and dest hotspots used for generating connection points.
        /// </summary>
        public Point SourceConnectorHotspot
        {
            get { return sourceConnectorHotspot; }
            set
            {
                sourceConnectorHotspot = value;

                ComputeConnectionPoints();

                RaisePropertyChanged(nameof(SourceConnectorHotspot));
            }
        }

        public Point DestConnectorHotspot
        {
            get { return destConnectorHotspot; }
            set
            {
                destConnectorHotspot = value;

                ComputeConnectionPoints();

                RaisePropertyChanged(nameof(DestConnectorHotspot));
            }
        }

        /// <summary>
        /// Points that make up the connection.
        /// </summary>
        public PointCollection Points
        {
            get { return points; }
            set { SetAndNotify(ref points, value); }
        }

        public event EventHandler<EventArgs> ConnectionChanged;

        private void OnConnectionChanged()
        {
            ConnectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when the hotspot of the source connector has been updated.
        /// </summary>
        private void SourceConnector_HotspotUpdated(object sender, EventArgs e)
        {
            SourceConnectorHotspot = SourceConnector.Hotspot;
        }

        /// <summary>
        /// Event raised when the hotspot of the dest connector has been updated.
        /// </summary>
        private void DestConnector_HotspotUpdated(object sender, EventArgs e)
        {
            DestConnectorHotspot = DestConnector.Hotspot;
        }

        /// <summary>
        /// Rebuild connection points.
        /// </summary>
        private void ComputeConnectionPoints()
        {
            var computedPoints = new PointCollection();
            computedPoints.Add(SourceConnectorHotspot);

            var deltaX = Math.Abs(DestConnectorHotspot.X - SourceConnectorHotspot.X);
            var deltaY = Math.Abs(DestConnectorHotspot.Y - SourceConnectorHotspot.Y);

            if (deltaX > deltaY)
            {
                var midPointX = SourceConnectorHotspot.X + ((DestConnectorHotspot.X - SourceConnectorHotspot.X) / 2);
                computedPoints.Add(new Point(midPointX, SourceConnectorHotspot.Y));
                computedPoints.Add(new Point(midPointX, DestConnectorHotspot.Y));
            }
            else
            {
                var midPointY = SourceConnectorHotspot.Y + ((DestConnectorHotspot.Y - SourceConnectorHotspot.Y) / 2);
                computedPoints.Add(new Point(SourceConnectorHotspot.X, midPointY));
                computedPoints.Add(new Point(DestConnectorHotspot.X, midPointY));
            }

            computedPoints.Add(DestConnectorHotspot);
            computedPoints.Freeze();

            Points = computedPoints;
        }
    }
}
