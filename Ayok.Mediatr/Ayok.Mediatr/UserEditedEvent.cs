using MediatR;

namespace Ayok.Mediatr
{
    /// <summary>
    /// 用来传递 修改User的领域事件类
    /// </summary>
    /// <param name="Item"></param>
    public record UserEditedEvent(string userName, int age) : INotification;
}
