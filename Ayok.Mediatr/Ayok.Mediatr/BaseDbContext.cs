using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ayok.Mediatr
{
    /// <summary>
    /// 改造DbContext类，用于重写SaveChanges
    /// </summary>
    public class BaseDbContext : DbContext
    {
        private IMediator mediator;
        public BaseDbContext(DbContextOptions options, IMediator mediator) : base(options)
        {
            this.mediator = mediator;
        }
        /// <summary>
        /// 重写SaveChangesAsync方法【改造顺序】
        /// 改造顺序，将publish放在SaveChangesAsync后，当SaveChangesAsync失败就不会发送消息，但依旧清空消息
        /// </summary>
        /// <param name="acceptAllChangesOnSuccess"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            //1.获取所有实现IDomainEvents接口 且 含有未发布事件的对象
            var domainEntities = this.ChangeTracker.Entries<IDomainEvents>().Where(u => u.Entity.GetAllDomainEvents().Any());
            //2. 获取所有待发布的消息【剖析selectMany的作用，两次查找】
            var domainEvents = domainEntities.SelectMany(u => u.Entity.GetAllDomainEvents()).ToList();

            //操作数据库
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            //3. 清空所有待发布的消息
            domainEntities.ToList().ForEach(u => u.Entity.ClearAllDomainEvents());
            //4. 发送消息
            foreach (var item in domainEvents)
            {
                await mediator.Publish(item, cancellationToken);
            }
            return result;
        }
    }
}
