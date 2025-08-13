using MediatR;

namespace Ayok.Mediatr
{
    /// <summary>
    /// 领域事件实现类
    /// （这里声明为抽象类，不能直接实例化，继承的子类可以直接使用里面的普通方法，也可以里面的普通方法进行override）
    /// </summary>
    public abstract class BaseEntity : IDomainEvents
    {

        //[NotMapped]  //.Net6.0 中不需要加这个特性了
        private List<INotification> DoaminEventList = new();

        /// <summary>
        /// 获取所有领域事件
        /// </summary>
        /// <returns></returns>
        public List<INotification> GetAllDomainEvents()
        {
            return DoaminEventList;
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="item">实现了INotification接口的record类</param>
        public void AddDomainEvent(INotification item)
        {
            DoaminEventList.Add(item);
        }


        /// <summary>
        /// 添加事件(前提是不存在)
        /// </summary>
        /// <param name="item">实现了INotification接口的record类</param>
        public void AddDomainEventIfNoExist(INotification item)
        {
            if (!DoaminEventList.Contains(item))
            {
                DoaminEventList.Add(item);
            }
        }

        /// <summary>
        /// 清空所有事件
        /// </summary>
        public void ClearAllDomainEvents()
        {
            DoaminEventList.Clear();
        }


    }
}
