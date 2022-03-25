﻿using System;
using System.Collections.Generic;
using System.Linq;
using CollectionExtensions = DotNetCommons.Collections.CollectionExtensions;

namespace DotNetCommons;

public class StateMachineException : Exception
{
    public StateMachineException(string message) : base(message)
    {
    }
}

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
    private readonly List<(T, T)> _transitions = new();

    public T? Current { get; set; }
    public DateTime LastTransition { get; private set; }
    public TimeSpan TimeSinceTransition => DateTime.Now - LastTransition;

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
            return new List<T>();

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

        var diff = CollectionExtensions.Intersect(leave, arrive);

        // Call leave actions in ascending order
        foreach (var item in diff.Left)
            _states[item].Leave?.Invoke(item);

        LastTransition = DateTime.Now;
        Current = state;

        // Call arrive actions in descending order
        foreach (var item in diff.Right.AsEnumerable().Reverse())
            _states[item].Arrive?.Invoke(item);
    }

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