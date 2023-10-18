using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.UserConversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class UserConversationRepository : IUserConversationRepository
    {
        public (bool, string) Update(UpdateUserConversationRequestDTO request) => UserConversationDAO.Instance.Update(request);
    }
}
