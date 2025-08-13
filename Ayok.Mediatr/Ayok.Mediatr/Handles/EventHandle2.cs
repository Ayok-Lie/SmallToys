using MediatR;

namespace Ayok.Mediatr.Handles
{
    /// <summary>
    /// 事件接收者2
    /// </summary>
    public class EventHandle2 : INotificationHandler<UserAddedEvent>
    {
        public Task Handle(UserAddedEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"EventHandle1收到新消息:{notification.Item.id} 登录成功了");
            return Task.CompletedTask;
        }
    }
}
