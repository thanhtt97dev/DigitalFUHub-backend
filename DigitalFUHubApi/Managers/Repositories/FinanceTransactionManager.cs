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

		public void Enqueues(List<FinanceTransaction> transactions)
		{
			foreach (var item in transactions)
			{
				Enqueue(item);
			}
		}

		public Queue<FinanceTransaction> GetQueuesAndClearItemsGeted()
		{
			//copy
			Queue<FinanceTransaction> copy = data;
			// compare data and copy what is same -> Dequeue
			int countItemRemved = 0;
			int countCopy = copy.Count;	
			while (countItemRemved < countCopy) 
			{
				if (copy.Contains(data.Peek())){
					data.Dequeue();
					countItemRemved++;
				}
			}
			return copy;	
		}

		public FinanceTransaction? Peek()
		{
			if(data.Count == 0) return null;	
			return data.Peek();
		}
	}
}
