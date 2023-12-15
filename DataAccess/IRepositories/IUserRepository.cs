using BusinessObject.Entities;
using DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IUserRepository
	{
		User? GetUserByUsernameAndPassword(string? email, string? password);
		User? GetUserByEmail(string? email);
		User? GetUserByRefreshToken(string? refreshTokenId);
		List<User> GetUsers(UserRequestDTO user);
		User? GetUserById(long id);
		void EditUserInfo(User user);
		void Update2FA(int id);
		void AddUser(User user);
		User? GetUserByUsername(string username);
		bool IsExistUsernameOrEmail(string username, string email);
		User? GetUser(string email, string username, string fullname);
		void UpdateUser(User user);
		void UpdateSettingPersonalInfo(User userUpdate);

        List<User> GetUsers(long userId, string email, string fullName, int roleId, int status, int page);
		User? GetUserInfo (int id);
		bool CheckUsersExisted(List<long> userIds);
		void UpdateUserOnlineStatus(long userId, bool isOnline);
		string GenerateRandomUsername(string email);
        User? GetUserByUserNameOtherUserId(long userId, string userName);
		void ActiveUserNameAndPassword(long userId, string userName, string password);
		long GetNumberNewUserInCurrentMonth();
		User? GetUserInfoById(long id);
		int GetNumberUserWithCondition(long userId, string email, string fullName, int roleId, int status);
		string GetAvatarUser(long userId);

    }
}
