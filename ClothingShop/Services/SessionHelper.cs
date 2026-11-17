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

        public static int? CurrentUserId
        {
            get { return CurrentUser?.UserId; }
        }

        public static SessionUser GetCurrentUser(HttpSessionStateBase session)
        {
            return session?[UserKey] as SessionUser;
        }

        public static void SignIn(HttpSessionStateBase session, SessionUser user)
        {
            if (session == null) return;
            session[UserKey] = user;
            if (user != null)
            {
                session["UserId"] = user.UserId; // Đồng bộ Session["UserId"] phục vụ tương thích toàn hệ thống
            }
        }

        public static void SignOut(HttpSessionStateBase session)
        {
            if (session == null) return;
            session.Clear();
            session.Abandon();
        }
    }
}