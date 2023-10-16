using BusinessObject;

using BusinessObject.Entities;
using Comons;
using DTOs.Chat;
using DTOs.Conversation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
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

                return groupedConversations;
                }
       
        }

        internal long AddConversation (AddConversationRequestDTO addConversation)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<long> listUserId = addConversation.UserIds;
                var userConversation = context.UserConversation.ToList();
                if (userConversation != null) {
                    var groupUserConversation = userConversation
                     .GroupBy(x => x.ConversationId)
                     .Select(group => new
                     {
                         ConversationId = group.Key,
                         Count = group.Distinct().Count()
                     }).ToList();

                    if (groupUserConversation != null && groupUserConversation.Count > 0)
                    {
                        foreach (var item in groupUserConversation)
                        {
                            if (item.Count == listUserId.Count)
                            {
                                long conversationId = item.ConversationId;
                                var findUserConversation = context.UserConversation.Where(x => x.ConversationId == conversationId && listUserId.Contains(x.UserId)).ToList();
                                if (findUserConversation.Count == item.Count)
                                {
                                    return conversationId;
                                }
                            }
                        }
                    }
                }
                var transaction = context.Database.BeginTransaction();
                try
                {
                    Conversation conversation = new Conversation
                    {
                        ConversationName = addConversation.ConversationName ?? null,
                        DateCreate = addConversation.DateCreate,
                        IsActivate = true,
                    };
                    context.Conversations.Add(conversation);
                    context.SaveChanges();
                    long conversationId = conversation.ConversationId;
                    foreach (long userId in listUserId)
                    {
                        UserConversation newUserConversation = new UserConversation
                        {
                            UserId = userId,
                            ConversationId = conversationId,
                        };
                        context.UserConversation.Add(newUserConversation);
                    }
                    context.SaveChanges();
                    transaction.Commit();
                    return conversationId;
                } catch (Exception ex) {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }

        }

        internal (bool, string) ValidateAddConversation (AddConversationRequestDTO addConversation)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<long> listUserId = addConversation.UserIds;
                if (listUserId == null || listUserId.Count < 2
                    || (listUserId.Count == 2 && !string.IsNullOrEmpty(addConversation.ConversationName))
                    || (listUserId.Count > 2 && string.IsNullOrEmpty(addConversation.ConversationName)))
                {
                    return (false, "Missing name conversation or invalid number of Users");
                }
                var duplicates = listUserId
                      .GroupBy(x => x)
                      .Where(group => group.Count() > 1)
                      .Select(group => group.Key)
                      .ToList();
                if (duplicates.Count > 0)
                {
                    return (false, "Elements appear more than once");
                }
                var users = context.User.Where(x => listUserId.Contains(x.UserId)).ToList();
                if (users.Count != listUserId.Count)
                {
                    return (false, "Appears that the user does not exist in the system");
                }

                return (true, "Success");
            }
        }




        internal async Task SendMessageConversation(List<Message> messages)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var transaction = context.Database.BeginTransaction();
                try
                {
                    context.Messages.AddRange(messages);
                   
                    await context.SaveChangesAsync();
                    transaction.Commit();
                } catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }


        internal List<Message> GetMessages(long conversationId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<Message> result = context.Messages
                    .Include(_ => _.User)
                    .Where(m => m.ConversationId == conversationId && m.IsDelete == false)
                    .ToList();

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
