using System;
using StateMachine.Finite_State_Machine_Interaces;

namespace StateMachine.Finite_State_Machine_class
{
	public class FuncPredicate : IPredicate 
	{
		readonly Func<bool> func;

		public FuncPredicate(Func<bool> func)
		{
			this.func = func;
		}
	
	
		public bool Evaluate()
		{
			return func.Invoke();
		}
	}
}