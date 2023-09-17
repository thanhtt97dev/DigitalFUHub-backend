using BusinessObject;
using DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
    public class ChatDAO
    {
        private static ChatDAO? instance;
        private static readonly object instanceLock = new object();

        public static ChatDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new ChatDAO();
                    }
                }
                return instance;
            }
        }

        internal async Task<List<SenderConversation>> GetSenderConversations(long userId)
        {
                using (ApiContext context = new ApiContext())
                {
                    string sql = "EXECUTE dbo.GetSenderConversation @userId";
                    List<SenderConversation> result = await context.SenderConversations.FromSqlRaw(sql,
                            new SqlParameter("@userId", userId)
                        ).ToListAsync();

                    return result;
                }
       
        }

        internal async Task SendChatMessage(ChatRequestDTO chatRequestDTO)
        {
            using (ApiContext context = new ApiContext())
            {
                string sql = "EXEC dbo.SendChatMessage @conversationId, @senderId, @recipientId, @content, @dateCreate";
                 await context.Database.ExecuteSqlRawAsync(sql,
                        new SqlParameter("@conversationId", chatRequestDTO.ConversationId),
                        new SqlParameter("@senderId", chatRequestDTO.SenderId),
                        new SqlParameter("@recipientId", chatRequestDTO.RecipientId),
                        new SqlParameter("@content", chatRequestDTO.Content),
                        new SqlParameter("@dateCreate", chatRequestDTO.DateCreate)
                    );

            }

        }
    }
}
