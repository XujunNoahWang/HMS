using HMS.Models;
using System;

// Since there are 5 user roles, the app needs to know which user is logged in and what their role is.
namespace HMS.Services
{
    public class UserStateService
    {
        private User _currentUser;
        public event Action OnUserChanged; 

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            OnUserChanged?.Invoke(); 
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public void ClearCurrentUser()
        {
            _currentUser = null;
            OnUserChanged?.Invoke(); 
        }
    }
}