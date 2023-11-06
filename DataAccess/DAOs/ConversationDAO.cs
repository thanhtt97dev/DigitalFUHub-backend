using BusinessObject;

using BusinessObject.Entities;
using Comons;
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
				var userConversations = context.UserConversation.Where(x => x.UserId == userId)
					.ToList();

				var conversationIds = userConversations
					.Select(x => x.ConversationId)
					.ToList();

				var conversations = context.UserConversation
												.Include(_ => _.User)
												.Include(_ => _.Conversation)
												.Where(x => x.UserId != userId && conversationIds.Contains(x.ConversationId))
												.ToList();

				var groupedConversations = conversations
					.GroupBy(x => new { x.Conversation.ConversationId, x.Conversation.ConversationName, x.Conversation.DateCreate, x.Conversation.IsActivate, x.Conversation.IsGroup })
					.Select(group => new ConversationResponseDTO
					{
						ConversationId = group.Key.ConversationId,
						ConversationName = group.Key.ConversationName,
						DateCreate = group.Key.DateCreate,
						IsActivate = group.Key.IsActivate,
						IsRead = userConversations.FirstOrDefault(x => x.ConversationId == group.Key.ConversationId)?.IsRead ?? Constants.USER_CONVERSATION_TYPE_UN_READ,
						LatestMessage = context.Messages.Where(x => x.ConversationId == group.Key.ConversationId)
							.Select(x => new ConversationLatestMessageResponseDTO
							{
								Content = x.Content,
								DateCreate = x.DateCreate
							}).OrderByDescending(x => x.DateCreate).FirstOrDefault(),
						IsGroup = group.Key.IsGroup,
						Users = group.Select(uc => new UserConversationResponseDTO
						{
							UserId = uc.User.UserId,
							RoleId = uc.User.RoleId,
							Fullname = uc.User.Fullname,
							Avatar = uc.User.Avatar,
                        }).Distinct().ToList()
					}).ToList();

                // check online
                    for (int i = 0; i < groupedConversations.Count(); i++)
                    {
						if (groupedConversations[i] == null) continue;

						// check group and update
                        if (groupedConversations[i].IsGroup == false)
                        {
							var firstUser = groupedConversations[i].Users?.FirstOrDefault();
							if (firstUser != null)
							{
								var findUser = context.User.FirstOrDefault(x => x.UserId == firstUser.UserId);
								groupedConversations[i].LastTimeOnline = findUser?.LastTimeOnline ?? DateTime.Now;
                                groupedConversations[i].IsOnline = findUser?.IsOnline ?? false;
                            }
                        } else {
							var userIds = groupedConversations[i].Users?.Select(x => x.UserId);
							if (userIds != null)
							{
                                // users status online
                                var users = context.User.Where(x => userIds.Contains(x.UserId) && x.IsOnline == true);

                                if (users.Count() == 0)
								{
                                    groupedConversations[i].IsOnline = false;
                                } else
								{
                                    groupedConversations[i].IsOnline = true;
                                }
								
                            }
						}
            
                }
	
 
				return groupedConversations;
			}

		}

		internal long AddConversation(AddConversationRequestDTO addConversation)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				List<long> listUserId = new List<long>();
				listUserId.AddRange(addConversation.RecipientIds);
				listUserId.Add(addConversation.UserId);

				var userConversation = context.UserConversation.ToList();
				if (userConversation != null)
				{
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
                        IsGroup = addConversation.RecipientIds.Count > 1 ? true : false
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
							IsRead = Constants.USER_CONVERSATION_TYPE_UN_READ
                        };
						context.UserConversation.Add(newUserConversation);
					}
					context.SaveChanges();
					transaction.Commit();
					return conversationId;
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}
			}

		}

		internal (bool, string) ValidateAddConversation(AddConversationRequestDTO addConversation)
		{
			using (DatabaseContext context = new DatabaseContext())
			{

				List<long> listUserId = new List<long>();
				listUserId.AddRange(addConversation.RecipientIds);
				listUserId.Add(addConversation.UserId);

				if (listUserId.Count == 0 || listUserId.Count < 2
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
				}
				catch (Exception ex)
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

		internal long GetNumberConversationUnReadOfUser(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				long numberConversationUnRead = context.UserConversation.Count(x => x.UserId == userId && x.IsRead == Constants.USER_CONVERSATION_TYPE_UN_READ);

                return numberConversationUnRead;
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

		internal List<UserConversationDTO> GetRecipientUserIdHasConversation(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var userConversations = (from userConversation in context.UserConversation
							  join conversation in context.Conversations
								 on userConversation.ConversationId equals conversation.ConversationId
							  where userConversation.UserId != userId &&
								 (from us in context.UserConversation
								  where us.UserId == userId
								  select us.ConversationId
								 ).Contains(userConversation.ConversationId)
							  select new UserConversationDTO
							  {
								  ConversationId = userConversation.ConversationId,
								  UserId = userConversation.UserId,
								  IsGroup = conversation.IsGroup,
								  MembersInGroup = conversation.IsGroup ?
													(from member in context.UserConversation
													where member.ConversationId == userConversation.ConversationId &&
														member.UserId != userId
													select member.UserId).ToList()
													:
													new List<long>(),
							  }
							  ).ToList();
				return userConversations;

			}
		}

		internal long GetConversation(long shopId, long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var conversationIdOfSeller = context.UserConversation
					.Where(x => x.UserId == shopId).ToList();

				foreach (var item in conversationIdOfSeller)
				{
					var conversationId = (from userConversation in context.UserConversation
										  join conversation in context.Conversations
											  on userConversation.ConversationId equals conversation.ConversationId
										  where userConversation.UserId == userId &&
												userConversation.ConversationId == item.ConversationId
										  select userConversation.ConversationId).FirstOrDefault();
					if (conversationId != 0)
					{
						return conversationId;
					}
				}
				var transaction = context.Database.BeginTransaction();
				try
				{
					// add new conversation
					Conversation newConversation = new Conversation
					{
						DateCreate = DateTime.Now,
						IsGroup = false,
						IsActivate = true
					};
					context.Conversations.Add(newConversation);
					context.SaveChanges();

					var sellerConversation = new UserConversation
					{
						UserId = shopId,
						ConversationId = newConversation.ConversationId
					};

					var customerConversation = new UserConversation
					{
						UserId = userId,
						ConversationId = newConversation.ConversationId
					};

					context.UserConversation.AddRange(sellerConversation, customerConversation);
					context.SaveChanges();

					transaction.Commit();

					return newConversation.ConversationId;
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}

			}
		}

		internal List<long> GetConversationsUnRead(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var conversationsUnRead = context.UserConversation
					.Where(x => x.UserId == userId && x.IsRead == false)
					.ToList();	
				return conversationsUnRead.Select(x => x.ConversationId).ToList();	
			}
		}
	}
}
