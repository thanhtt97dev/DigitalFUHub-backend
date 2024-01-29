using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Managers.IRepositories;

namespace DigitalFUHubApi.Managers.Repositories
{
    public class FinanceTransactionManager : IFinanceTransactionManager
    {
        public static Queue<FinanceTransaction> data = new Queue<FinanceTransaction>();

		public void Clear()
		{
			data.Clear();
		}

		public int Count()
		{
			return data.Count();
		}

		public FinanceTransaction? Dequeue()
		{
			if(data.Count == 0) return null;	
			return data.Dequeue();
		}

		public void Enqueue(FinanceTransaction transaction)
		{
			if (transaction == null) return;
			data.Enqueue(transaction);
		}

		public FinanceTransaction? Peek()
		{
			if(data.Count == 0) return null;	
			return data.Peek();
		}
	}
}
