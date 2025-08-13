using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ayok.Mediatr
{
    public partial class UserInfo
    {
        [Key]
        [StringLength(32)]
        [Unicode(false)]
        public string id { get; set; } = null!;
        [StringLength(50)]
        [Unicode(false)]
        public string? userName { get; set; }
        [StringLength(500)]
        [Unicode(false)]
        public string? userPwd { get; set; }
        [StringLength(5)]
        [Unicode(false)]
        public string? userGender { get; set; }
        public int? userAge { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? addTime { get; set; }
        public int? delflag { get; set; }
    }

    /// <summary>
    /// UserInfo的领域事件类
    /// </summary>
    public partial class UserInfo : BaseEntity
    {
        public UserInfo()
        {
            //提供无参构造方法。避免EF Core加载数据的时候调用有参的构造方法触发领域事件
        }
        public UserInfo(string userName, int userAge)
        {
            this.id = Guid.NewGuid().ToString("N");
            this.userName = userName;
            this.userAge = userAge;
            this.userGender = "男";
            this.userPwd = "123456";
            this.addTime = DateTime.Now;
            this.delflag = 0;
            AddDomainEvent(new UserAddedEvent(this));
        }
        public void ChangeAge(int newAge)
        {
            this.userAge = newAge;
            AddDomainEventIfNoExist(new UserEditedEvent(this.userName, newAge));
        }
    }
}
