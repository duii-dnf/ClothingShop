using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClothingShop.Models;

namespace ClothingShop.ViewModels
{
    [Serializable]
    public class SessionUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        public bool IsAdmin => Role == AppRoles.Admin;
    }
}