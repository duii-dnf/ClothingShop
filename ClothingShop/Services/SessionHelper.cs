using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClothingShop.ViewModels;

namespace ClothingShop.Services
{
    public static class SessionHelper
    {
        public const string UserKey = "CURRENT_USER";
        public const string CartCountKey = "CartCount"; // TV3 cập nhật (kiểu int) mỗi khi giỏ thay đổi

        // Dùng trong View hoặc nơi không có HttpSessionStateBase
        public static SessionUser CurrentUser
        {
            get { return HttpContext.Current?.Session?[UserKey] as SessionUser; }
        }

        public static SessionUser GetCurrentUser(HttpSessionStateBase session)
        {
            return session?[UserKey] as SessionUser;
        }

        public static void SignIn(HttpSessionStateBase session, SessionUser user)
        {
            session[UserKey] = user;
        }

        public static void SignOut(HttpSessionStateBase session)
        {
            session.Clear();
            session.Abandon();
        }
    }
}