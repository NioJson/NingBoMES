using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissionBossAot.Models
{
    /// <summary>
    /// 账号参数
    /// </summary>
    public class UserParameters
    {
        public bool UserRememberKey = false;
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }
        public string UserAccount { get; set; } = "";
        /// <summary>
        /// 账号名称
        /// </summary>
        public string UserName { get; set; } = "";
        /// <summary>
        /// 账号密码
        /// </summary>
        public string UserPassword { get; set; } = "";
        /// <summary>
        /// 账号类型，操作员、技术员、管理员
        /// </summary>
        public string UserType { get; set; } = "";
        /// <summary>
        /// 账号权限，1、2、3
        /// </summary>
        public int UserRank { get; set; } = 0;
        /// <summary>
        /// 账号注册日期
        /// </summary>
        public string UserRegisterDate { get; set; } = "";
        /// <summary>
        /// 账号描述
        /// </summary>
        public string UserDescription { get; set; } = "";
        /// <summary>
        /// 账号头像
        /// </summary>
        public string UserHeadImagePath { get; set; } = "";
    }
}
