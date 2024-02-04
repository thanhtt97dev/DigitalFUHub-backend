using DigitalFUHubApi.Comons;

namespace DigitalFUHubApi.Managers.IRepositories
{
    public interface IFinanceTransactionManager
    {
		void Enqueues(List<FinanceTransaction> transaction);
		void Enqueue(FinanceTransaction transaction);
        FinanceTransaction? Dequeue();
        FinanceTransaction? Peek();
        void Clear();
        int Count();
        Queue<FinanceTransaction> GetData();
	}
}
