using IdeaBag.Portable.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IdeaBag.Portable.ViewModels
{
    public class ConnectionsViewModel : BaseViewModel
    {
        #region ICommands

        public ICommand ConnectionDetailsTouched { get; private set; }

        #endregion


        #region Private Properties

        private List<UserModel> _connections;

        #endregion


        #region Public Properties

        public List<UserModel> Connections
        {
            get { return _connections; }
            set
            {
                _connections = value;
                OnPropertyChanged("Connections");
            }
        }

        #endregion


        #region Constructor

        public ConnectionsViewModel()
        {
            //- TODO:  RETRIEVE PENDING CONNECTION REQUESTS FROM SERVER

            //- TODO:  RETRIEVE CURRENT CONNECTIONS FROM LOCAL STORAGE

            this._connections = new List<UserModel>();
        }

        #endregion

    }
}
