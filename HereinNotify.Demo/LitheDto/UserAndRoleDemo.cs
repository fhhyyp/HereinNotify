using HereinNotify;
using System;
using System.Collections.Generic;
using System.Text;

namespace HereinNotifyDemo.LitheDtoTest
{


    public class UserDbo
    {
        /// <summary>
        /// 数据库ID
        /// </summary>
        [LitheDtoName("UserDbId")]
        public int Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string? UserName { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string? UserPwd { get; set; }

        
    }

    public class RoleDbo
    {

        /// <summary>
        /// 数据库ID
        /// </summary>
        [LitheDtoName("RoleDbId")]
        public int Id { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; } = string.Empty;



    }


    [LitheDto(Source = typeof(UserDbo), IsUseINPC = true)]
    [LitheDto(Source = typeof(RoleDbo), IsUseINPC = true)]
    public partial class UserModel
    {

    }

    /*public partial class UserModel
    {

    }*/


    public partial class UserViewModel : HereinNotifyObject
    {
        [HereinNotifyProperty]
        private UserModel? _model;


    }

    internal static class UserAndRoleDemo
    {
        public static void Run()
        {
            Console.WriteLine("\r\n===============");
            UserViewModel userViewModel = new UserViewModel();
            var model = new UserModel();
            model.PropertyChanged +=  (s, e) =>
            {
                Console.WriteLine($"属性变更：{e.PropertyName}");
            };
            userViewModel.Model = model.InputUserDbo(new UserDbo
            {
                Id = 1,
                UserName = "Job",
                UserPwd = "123456",
            }).InputRoleDbo(new RoleDbo
            {
                Id = 1,
                RoleId = 999,
                RoleName = "Admin"
            });

            Console.WriteLine($"用户主键：{userViewModel.Model.UserDbId}");
            Console.WriteLine($"用户名称：{userViewModel.Model.UserName}");
            Console.WriteLine($"用户密码：{userViewModel.Model.UserPwd}");
            Console.WriteLine($"角色主键：{userViewModel.Model.RoleDbId}");
            Console.WriteLine($"角色ID  ：{userViewModel.Model.RoleId}");
            Console.WriteLine($"角色名称：{userViewModel.Model.RoleName}");
        }
    }
}
