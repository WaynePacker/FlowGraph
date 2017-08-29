using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Utils
{
    /// <summary>
    /// Abstract base for view-model classes that need to implement INotifyPropertyChanged.
    /// </summary>
    public abstract class AbstractModelBase : INotifyPropertyChanged
    {
#if DEBUG
        private static int nextObjectId = 0;
        private int objectDebugId = nextObjectId++;

        public int ObjectDebugId => objectDebugId;

#endif //  DEBUG

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetAndNotify<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!Equals(oldValue, newValue))
            {
                oldValue = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Event raised to indicate that a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
