namespace DotNetCommons;

public class StateMachineException : Exception
{
    public StateMachineException(string message) : base(message)
    {
    }
}

/// <summary>
/// Minimal state machine that handles states, substates and transitions between states.
/// </summary>
/// <typeparam name="T"></typeparam>
public class LittleStateMachine<T> where T : notnull
{
    private class StateRecord
    {
        public T State { get; init; } = default!;
        public T? SubStateOf { get; init; }
        public Action<T>? Arrive { get; init; }
        public Action<T>? Leave { get; init; }
        public bool HasParent { get; init; }
    }

    private readonly Dictionary<T, StateRecord> _states = new();
    private readonly List<(T, T)> _transitions = [];

    /// <summary>
    /// Current state.
    /// </summary>
    public T? Current { get; set; }

    /// <summary>
    /// Time of the last transition.
    /// </summary>
    public DateTime LastTransition { get; private set; }

    /// <summary>
    /// Time since the last transition.
    /// </summary>
    public TimeSpan TimeSinceTransition => DateTime.Now - LastTransition;

    /// <summary>
    /// Configure a given state, including actions to perform when arriving and departing from the state.
    /// </summary>
    /// <param name="state">State to add</param>
    /// <param name="arrive">Optional action to perform when arriving in this state</param>
    /// <param name="leave">Optional action to perform when departing this state</param>
    public LittleStateMachine<T> ConfigureState(T state, Action<T>? arrive = null, Action<T>? leave = null)
    {
        if (_states.ContainsKey(state))
            throw new ArgumentException($"State {state} is already defined.");

        _states.Add(state, new StateRecord
        {
            State = state,
            HasParent = false,
            Arrive = arrive,
            Leave = leave
        });

        return this;
    }

    /// <summary>
    /// Configure a given state as a substate of a different state, including actions to perform when arriving and departing from the state.
    /// Several substates can have a given state as a parent state; transitions between substates will not affect the parent state or cause
    /// parent actions to be performed.
    /// </summary>
    /// <param name="state">State to add</param>
    /// <param name="subStateOf">Parent state</param>
    /// <param name="arrive">Optional action to perform when arriving in this state</param>
    /// <param name="leave">Optional action to perform when departing this state</param>
    public LittleStateMachine<T> ConfigureState(T state, T subStateOf, Action<T>? arrive = null, Action<T>? leave = null)
    {
        if (_states.ContainsKey(state))
            throw new ArgumentException($"State {state} is already defined.");
        if (!_states.ContainsKey(subStateOf))
            throw new ArgumentException($"Parent state {subStateOf} has not been defined.");

        _states.Add(state, new StateRecord
        {
            State = state,
            SubStateOf = subStateOf,
            HasParent = true,
            Arrive = arrive,
            Leave = leave
        });

        return this;
    }

    /// <summary>
    /// Allow transition from a particular state to another.
    /// </summary>
    public LittleStateMachine<T> ConfigureTransition(T from, T to)
    {
        var transition = (from, to);
        if (_transitions.Contains(transition))
            throw new ArgumentException($"Transition {transition} is already defined.");

        _transitions.Add(transition);

        return this;
    }

    private StateRecord FindState(T state)
    {
        return _states.TryGetValue(state, out var sr) ? sr : throw new StateMachineException($"State {state} has not been configured.");
    }

    private List<T> GetStateHierarchy(T? state)
    {
        if (state == null)
            return [];

        var sr = FindState(state);
        var result = new List<T>();
        for (; ; )
        {
            if (result.Contains(sr.State))
                throw new StateMachineException($"Cyclical parent state not allowed for {sr.State}");

            result.Add(sr.State);
            if (!sr.HasParent)
                break;

            sr = sr.SubStateOf != null ? _states.GetValueOrDefault(sr.SubStateOf) : null;
            if (sr == null)
                break;
        }

        return result;
    }

    /// <summary>
    /// Initialize the state machine and set a given starting state.
    /// </summary>
    public void Initialize(T state)
    {
        if (state == null)
            throw new StateMachineException("null state not allowed.");

        InternalTransition(state);
    }

    private void InternalTransition(T state)
    {
        var leave = GetStateHierarchy(Current);
        var arrive = GetStateHierarchy(state);

        var both = leave.Intersect(arrive).ToList();
        foreach (var s in both)
        {
            leave.Remove(s);
            arrive.Remove(s);
        }

        // Call leave actions in ascending order
        foreach (var item in leave)
            _states[item].Leave?.Invoke(item);

        LastTransition = DateTime.Now;
        Current = state;

        // Call arrive actions in descending order
        foreach (var item in arrive.AsEnumerable().Reverse())
            _states[item].Arrive?.Invoke(item);
    }

    /// <summary>
    /// Move from the current state to a new state, executing arrive and leave actions as needed and
    /// throwing an exception if the transition is not allowed.
    /// </summary>
    /// <param name="state"></param>
    /// <exception cref="StateMachineException"></exception>
    public void MoveTo(T state)
    {
        if (state == null)
            throw new StateMachineException("null state not allowed.");

        if (state.Equals(Current))
            return;

        var currentStates = GetStateHierarchy(Current);
        foreach (var c in currentStates)
        {
            var transition = (c, state);
            if (!_transitions.Contains(transition))
                continue;

            InternalTransition(state);
            return;
        }

        throw new StateMachineException($"No transition exists between {Current} and {state}.");
    }
}