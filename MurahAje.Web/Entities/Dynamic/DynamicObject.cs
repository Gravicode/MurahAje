using System;
using System.Net;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;

namespace MurahAje.Web.Entities
{
    public abstract class DynamicObject : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> valuesStorage;

        public event PropertyChangedEventHandler PropertyChanged;

        protected DynamicObject()
        {
            this.valuesStorage = new Dictionary<string,object>();
        }

        protected internal virtual T GetValue<T>(string propertyName)
        {
            object value;
            if (!this.valuesStorage.TryGetValue(propertyName, out value))
	        {
                return default(T);
	        }

            return (T) value;
        }

        protected internal virtual void SetValue<T>(string propertyName, T value)
        {
            this.valuesStorage[propertyName] = value;

            this.RaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var hanlder = this.PropertyChanged;
            if (hanlder != null)
            {
                hanlder(this, args);
            }
        }
    }
}
