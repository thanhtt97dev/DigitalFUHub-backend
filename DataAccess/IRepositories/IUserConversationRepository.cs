using DTOs.UserConversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IUserConversationRepository
    {
        (bool, string) Update(UpdateUserConversationRequestDTO request);
    }
}
