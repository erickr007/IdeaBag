using IdeaBag.Portable.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace IdeaBag.Portable.ViewModels
{
    public class MessagesViewModel : BaseViewModel
    {
        #region ICommands

        public ICommand MessageDetailsTouched { get; private set; }

        #endregion


        #region Private Properties

        private List<MessageModel> _messages;

        #endregion


        #region Public Properties

        public List<MessageModel> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;

                OnPropertyChanged("Messages");
            }
        }

        #endregion


        #region Events

        #endregion


        #region Constructor

        public MessagesViewModel()
        {
            //- TODO: GET MESSAGES FROM CLIENT DATABASE
            //-------------------------------------------

            //- TODO: GET UNRECEIVED MESSAGES FROM SERVER
            //--------------------------------------------
        }

        #endregion


        

    }
}
