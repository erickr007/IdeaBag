using IdeaBag.Portable.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaBag.Portable.Client.DataAccess
{
    public interface IClientDatabaseManager
    {
        #region ApplicationSettings

        Task<ApplicationSettingsModel> GetApplicationSettings();
        void InsertInitialApplicationSettings(ApplicationSettingsModel settings);

        #endregion


        #region Messages Methods

        Task<List<MessageModel>> GetUserMessages(string userid);
        List<MessageModel> GetUserMessages(string userid, int startindex, int endindex);
        List<string> GetMessageLinks(string userid, string messageid);
        List<byte[]> GetMessageImages(string userid, string messageid);
        void InsertUserMessage(string userid, MessageModel message);
        void DeleteUserMessage(string userid, string messageid);

        #endregion


        #region Contact Messages

        List<UserModel> GetUserContacts(string userid);

        #endregion

    }
}
