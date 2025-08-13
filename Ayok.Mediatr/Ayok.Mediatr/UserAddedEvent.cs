using MediatR;

namespace Ayok.Mediatr
{
    /// <summary>
    /// 用来传递 新增User的领域事件类
    /// </summary>
    /// <param name="Item"></param>
    public record UserAddedEvent(UserInfo Item) : INotification;
}
