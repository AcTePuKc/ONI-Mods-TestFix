using System;

namespace PeterHan.PLib.Core;

public static class PStateMachines
{
	public static State<T, I, IStateMachineTarget, object> CreateState<T, I>(this GameStateMachine<T, I> sm, string name) where T : GameStateMachine<T, I, IStateMachineTarget, object> where I : GameInstance<T, I, IStateMachineTarget, object>
	{
		State<T, I, IStateMachineTarget, object> val = new State<T, I, IStateMachineTarget, object>();
		if (string.IsNullOrEmpty(name))
		{
			name = "State";
		}
		if (sm == null)
		{
			throw new ArgumentNullException("sm");
		}
		((BaseState)val).defaultState = ((StateMachine)sm).GetDefaultState();
		((StateMachine)sm).CreateStates((object)val);
		((StateMachine<T, I, IStateMachineTarget, object>)(object)sm).BindState((State<T, I, IStateMachineTarget, object>)(object)((GameStateMachine<T, I, IStateMachineTarget, object>)(object)sm).root, (State<T, I, IStateMachineTarget, object>)(object)val, name);
		return val;
	}

	public static State<T, I, M, object> CreateState<T, I, M>(this GameStateMachine<T, I, M> sm, string name) where T : GameStateMachine<T, I, M, object> where I : GameInstance<T, I, M, object> where M : IStateMachineTarget
	{
		State<T, I, M, object> val = new State<T, I, M, object>();
		if (string.IsNullOrEmpty(name))
		{
			name = "State";
		}
		if (sm == null)
		{
			throw new ArgumentNullException("sm");
		}
		((BaseState)val).defaultState = ((StateMachine)sm).GetDefaultState();
		((StateMachine)sm).CreateStates((object)val);
		((StateMachine<T, I, M, object>)(object)sm).BindState((State<T, I, M, object>)(object)((GameStateMachine<T, I, M, object>)(object)sm).root, (State<T, I, M, object>)(object)val, name);
		return val;
	}

	public static void ClearEnterActions(this BaseState state)
	{
		state?.enterActions.Clear();
	}

	public static void ClearExitActions(this BaseState state)
	{
		state?.exitActions.Clear();
	}

	public static void ClearTransitions(this BaseState state)
	{
		state?.transitions.Clear();
	}
}
