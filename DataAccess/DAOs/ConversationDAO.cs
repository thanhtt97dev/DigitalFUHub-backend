using BusinessObject;
using BusinessObject.DataTransfer;
using BusinessObject.Entities;
using DTOs.Chat;
using DTOs.Conversation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
    public class ConversationDAO
    {
        private static ConversationDAO? instance;
        private static readonly object instanceLock = new object();

        public static ConversationDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new ConversationDAO();
                    }
                }
                return instance;
            }
        }

        internal List<ConversationResponseDTO> GetSenderConversations(long userId)
        {
                using (DatabaseContext context = new DatabaseContext())
                {
                var conversationIds = context.UserConversation.Where(x => x.UserId == userId)
                    .Select(x => x.ConversationId)                            
                    .ToList();

                var conversations = context.UserConversation
                                                .Include(_ => _.User)
                                                .Include(_ => _.Conversation)
                                                .Where(x => x.UserId != userId && conversationIds.Contains(x.ConversationId))
                                                .ToList();

                var groupedConversations = conversations
                    .GroupBy(x => new { x.Conversation.ConversationId, x.Conversation.ConversationName, x.Conversation.DateCreate, x.Conversation.IsActivate })
                    .Select(group => new ConversationResponseDTO
                    {
                        ConversationId = group.Key.ConversationId,
                        ConversationName = group.Key.ConversationName,
                        DateCreate = group.Key.DateCreate,
                        IsActivate = group.Key.IsActivate,
                        Users = group.Select(uc => new UserConversationResponseDTO {
                            UserId = uc.User.UserId,
                            RoleId = uc.User.RoleId,
                            Fullname = uc.User.Fullname,
                            Avatar = uc.User.Avatar
                        }).Distinct().ToList()
                    }).ToList();

                //    string sql = "EXECUTE dbo.GetSenderConversation @userId";
                ////List<SenderConversation> result = await context.SenderConversations.FromSqlRaw(sql,
                ////        new SqlParameter("@userId", userId)
                ////    ).ToListAsync();
                //List <SenderConversation> result = await context.Set<SenderConversation>().FromSqlRaw(sql,
                //        new SqlParameter("@userId", userId)
                //    ).ToListAsync();



                return groupedConversations;
                }
       
        }


        public long ConversationId { get; set; }
        public string? ConversationName { get; set; }
        public DateTime DateCreate { get; set; }
        public bool IsActivate { get; set; }
        public ICollection<UserConversationResponseDTO>? Users { get; set; }

        internal async Task SendChatMessage(SendChatMessageRequestDTO sendChatMessageRequest)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                string sql = "EXEC dbo.SendChatMessage @conversationId, @senderId, @recipientId, @content, @dateCreate, @messageType";
                 await context.Database.ExecuteSqlRawAsync(sql,
                        new SqlParameter("@conversationId", sendChatMessageRequest.ConversationId),
                        new SqlParameter("@senderId", sendChatMessageRequest.SenderId),
                        new SqlParameter("@recipientId", sendChatMessageRequest.RecipientId),
                        new SqlParameter("@content", sendChatMessageRequest.Content ?? ""),
                        new SqlParameter("@dateCreate", sendChatMessageRequest.DateCreate),
                        new SqlParameter("@messageType", sendChatMessageRequest.MessageType)
                    );

            }

        }

        internal async Task<List<Message>> GetListMessage(long conversationId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<Message> result = await context.Messages
                    .Where(m => m.ConversationId == conversationId)
                    .ToListAsync();

                return result;
            }

        }

        internal List<UserConversation> GetUserConversation(long senderId, long recipientId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<UserConversation> result = context.UserConversation
                    .Where(u => u.UserId == senderId || u.UserId == recipientId)
                    .ToList();

                return result;
            }

        }
    }
}
