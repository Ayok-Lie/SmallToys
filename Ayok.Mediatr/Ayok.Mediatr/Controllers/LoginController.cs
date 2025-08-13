using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ayok.Mediatr.Controllers
{
    /// <summary>
    /// 测试Api
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private EFCore6xDBContext dbContext;
        public LoginController(EFCore6xDBContext dbContext)
        {
            this.dbContext = dbContext;
        }
        /// <summary>
        /// 校验登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public string CheckLogin()
        {
            return "ok";
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userAge"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> AddUser(string userName, int userAge)
        {
            try
            {
                UserInfo userInfo = new(userName, userAge);
                dbContext.Add(userInfo);

                await dbContext.SaveChangesAsync();
                return "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "error";
            }
        }
    }
}
