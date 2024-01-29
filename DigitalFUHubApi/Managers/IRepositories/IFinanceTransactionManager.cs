using DigitalFUHubApi.Comons;

namespace DigitalFUHubApi.Managers.IRepositories
{
    public interface IFinanceTransactionManager
    {
        void Enqueue(FinanceTransaction transaction);
        FinanceTransaction? Dequeue();
        FinanceTransaction? Peek();
        void Clear();
        int Count();
    }
}
