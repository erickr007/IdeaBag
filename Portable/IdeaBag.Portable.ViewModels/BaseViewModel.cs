using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IdeaBag.Portable.ViewModels
{
    public abstract class BaseViewModel: INotifyPropertyChanged
    {
        #region ICommands

        public ICommand ShowPageOptionsTouched { get; private set; }

        #endregion


        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion


        #region INotifyPropertyChanged Method

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

    }
}
