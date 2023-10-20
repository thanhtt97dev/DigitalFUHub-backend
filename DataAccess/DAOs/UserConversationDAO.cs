using BusinessObject.Entities;
using BusinessObject;
using DTOs.UserConversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comons;

namespace DataAccess.DAOs
{
    public class UserConversationDAO
    {
        private static UserConversationDAO? instance;
        private static readonly object instanceLock = new object();

        public static UserConversationDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new UserConversationDAO();
                    }
                }
                return instance;
            }
        }


        public (bool, string) Update (UpdateUserConversationRequestDTO request)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var userConversation = context
                    .UserConversation
                    .Where(x => request.UserId == x.UserId && x.ConversationId == request.ConversationId);
                if (userConversation == null) throw new ArgumentNullException("Not found user Conversation");
                var message = context.Messages.Where(x => x.ConversationId == request.ConversationId).ToList();
                if (message.Count() > 0)
                {
                    foreach (var item in userConversation)
                    {
                        item.IsRead = request.IsRead;
                    }
                    context.SaveChanges();
                    return (true, Constants.RESPONSE_CODE_SUCCESS);
                }

                return (false, Constants.RESPONSE_CODE_FAILD);
            }
        }
    }
}
