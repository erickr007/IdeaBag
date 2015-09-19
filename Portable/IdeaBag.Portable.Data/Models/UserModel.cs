using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace IdeaBag.Portable.Data.Models
{
    [DataContract(Name="UserModel")]
    public class UserModel
    {
        #region Private Properties

        /// <summary>
        /// List of device connection id's this User is associated with
        /// </summary>
        private List<string> _connectionIDs;

        /// <summary>
        /// Unique identifier for this user
        /// </summary>
        private string _globalID;

        /// <summary>
        /// User specified Email Address
        /// </summary>
        private string _userID;

        /// <summary>
        /// Hashed user password string
        /// </summary>
        private string _passwordHash;

        /// <summary>
        /// Determines the login method applied by this user
        /// </summary>
        private LoginType _userLoginType;

        /// <summary>
        /// User First Name
        /// </summary>
        private string _firstName;

        /// <summary>
        /// User Last Name
        /// </summary>
        private string _lastName;

        /// <summary>
        /// Date User signed up 
        /// </summary>
        private DateTime _createDate;

        /// <summary>
        /// Date of most recent edit to the User
        /// </summary>
        private DateTime _lastModified;

        /// <summary>
        /// Contains the active state of this User
        /// </summary>
        private bool _isActivated;

        #endregion


        #region Public Properties

        [DataMember]
        public List<string> ConnectionIDs
        {
            get { return _connectionIDs; }
            set { _connectionIDs = value; }
        }

        [DataMember]
        public string GlobalID
        {
            get { return _globalID; }
            set
            {
                _globalID = value;
            }
        }

        [DataMember]
        public string UserID
        {
            get { return _userID; }
            set
            {
                _userID = value;
            }
        }

        [DataMember]
        public string PasswordHash
        {
            get { return _passwordHash; }
            set { _passwordHash = value; }
        }

        [DataMember]
        public LoginType UserLoginType
        {
            get { return _userLoginType; }
            set { _userLoginType = value; }
        }

        [DataMember]
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        [DataMember]
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        [DataMember]
        public DateTime CreateDate
        {
            get { return _createDate; }
            set { _createDate = value; }
        }

        [DataMember]
        public DateTime LastModified
        {
            get { return _lastModified; }
            set { _lastModified = value; }
        }

        [DataMember]
        public bool IsActivated
        {
            get { return _isActivated; }
            set { _isActivated = value; }
        }

        #endregion


        #region Constructor

        public UserModel()
        {
            _connectionIDs = new List<string>();
            _createDate = DateTime.UtcNow;
            _lastModified = DateTime.UtcNow;
            _firstName = string.Empty;
            _globalID = string.Empty;
            _isActivated = false;
            _userLoginType = LoginType.Manual;
            _lastName = string.Empty;
            _passwordHash = string.Empty;
            _userID = string.Empty;
        }

        #endregion

    }

    [DataContract]
    public enum LoginType
    {
        Manual = 1,
        Facebook = 2
    }
}
