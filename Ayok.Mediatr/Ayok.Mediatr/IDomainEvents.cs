using MediatR;

namespace Ayok.Mediatr
{
    /// <summary>
    /// 领域事件接口
    /// </summary>
    public interface IDomainEvents
    {
        /// <summary>
        /// 获取所有领域事件
        /// </summary>
        /// <returns></returns>
        List<INotification> GetAllDomainEvents();
        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="item"></param>
        void AddDomainEvent(INotification item);
        /// <summary>
        /// 添加事件(前提是不存在)
        /// </summary>
        /// <param name="item"></param>
        void AddDomainEventIfNoExist(INotification item);
        /// <summary>
        /// 清空所有事件
        /// </summary>
        void ClearAllDomainEvents();
    }
}
