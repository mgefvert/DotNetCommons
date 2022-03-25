using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test;

[TestClass]
public class LittleStateMachineClassTest
{
    public class State: IComparable<State>
    {
        public string Title { get; }

        public State(string title)
        {
            Title = title;
        }

        public int CompareTo(State? other) => string.CompareOrdinal(Title, other?.Title);
        public override string ToString() => Title;
    }

    private readonly State _initialized = new("Initialized");
    private readonly State _running = new("Running");
    private readonly State _stopped = new("Stopped");

    private List<string> _log = null!;
    private LittleStateMachine<State> _lsm = null!;

    [TestInitialize]
    public void Setup()
    {
        _log = new List<string>();

        _lsm = new LittleStateMachine<State>();
            
        _lsm.ConfigureState(_initialized, s => _log.Add($"at:{s}"));
        _lsm.ConfigureState(_running, s => _log.Add($"at:{s}"), s2 => _log.Add($"leave:{s2}"));
        _lsm.ConfigureState(_stopped, s => _log.Add($"at:{s}"));

        _lsm.ConfigureTransition(_initialized, _running);
        _lsm.ConfigureTransition(_running, _stopped);
        _lsm.ConfigureTransition(_stopped, _running);
    }

    [TestMethod]
    public void Test()
    {
        _lsm.Initialize(_initialized);
        _lsm.MoveTo(_running);
        _lsm.MoveTo(_stopped);

        Assert.AreEqual(4, _log.Count);
        Assert.AreEqual("at:Initialized", _log[0]);
        Assert.AreEqual("at:Running", _log[1]);
        Assert.AreEqual("leave:Running", _log[2]);
        Assert.AreEqual("at:Stopped", _log[3]);
    }

    [TestMethod, ExpectedException(typeof(StateMachineException))]
    public void TestInvalidState()
    {
        _lsm.Initialize(new State(null!));
    }

    [TestMethod, ExpectedException(typeof(StateMachineException))]
    public void TestInvalidTransition()
    {
        _lsm.Initialize(_initialized);
        _lsm.MoveTo(_stopped);
    }
}