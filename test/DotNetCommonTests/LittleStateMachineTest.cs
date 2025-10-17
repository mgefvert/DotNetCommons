﻿using DotNetCommons;

namespace DotNetCommonTests;

[TestClass]
public class LittleStateMachineTest
{
    public enum States
    {
        Initialized,
        Starting,
        Running,
        RunningHigh,
        RunningLow,
        Stopped,
        Unconfigured
    }

    private List<string> _log = null!;
    private LittleStateMachine<States> _lsm = null!;

    [TestInitialize]
    public void Setup()
    {
        _log = new List<string>();

        _lsm = new LittleStateMachine<States>();

        _lsm.ConfigureState(States.Initialized);
        _lsm.ConfigureState(States.Starting, s => _log.Add($"at:{s}"));
        _lsm.ConfigureState(States.Running, s => _log.Add($"at:{s}"), s2 => _log.Add($"leave:{s2}"));
        _lsm.ConfigureState(States.RunningLow, States.Running, s => _log.Add($"at:{s}"), s2 => _log.Add($"leave:{s2}"));
        _lsm.ConfigureState(States.RunningHigh, States.Running, s => _log.Add($"at:{s}"), s2 => _log.Add($"leave:{s2}"));
        _lsm.ConfigureState(States.Stopped, s => _log.Add($"at:{s}"));

        _lsm.ConfigureTransition(States.Initialized, States.Starting);
        _lsm.ConfigureTransition(States.Starting, States.RunningLow);
        _lsm.ConfigureTransition(States.Starting, States.Stopped);
        _lsm.ConfigureTransition(States.RunningLow, States.RunningHigh);
        _lsm.ConfigureTransition(States.RunningHigh, States.RunningLow);
        _lsm.ConfigureTransition(States.Running, States.Stopped);
        _lsm.ConfigureTransition(States.Stopped, States.Starting);
    }

    [TestMethod]
    public void Test()
    {
        _lsm.Initialize(States.Initialized);
        _lsm.MoveTo(States.Starting);
        _lsm.MoveTo(States.RunningLow);
        _lsm.MoveTo(States.RunningHigh);
        _lsm.MoveTo(States.Stopped);

        Console.WriteLine(string.Join("\r\n", _log));

        Assert.HasCount(8, _log);
        Assert.AreEqual("at:Starting", _log[0]);
        Assert.AreEqual("at:Running", _log[1]);
        Assert.AreEqual("at:RunningLow", _log[2]);
        Assert.AreEqual("leave:RunningLow", _log[3]);
        Assert.AreEqual("at:RunningHigh", _log[4]);
        Assert.AreEqual("leave:RunningHigh", _log[5]);
        Assert.AreEqual("leave:Running", _log[6]);
        Assert.AreEqual("at:Stopped", _log[7]);
    }

    [TestMethod]
    public void TestInvalidState()
    {
        Assert.ThrowsExactly<StateMachineException>(() => _lsm.Initialize(States.Unconfigured));
    }

    [TestMethod]
    public void TestInvalidTransition()
    {
        _lsm.Initialize(States.Initialized);
        _lsm.MoveTo(States.Starting);
        Assert.ThrowsExactly<StateMachineException>(() => _lsm.MoveTo(States.Running));
    }

    [TestMethod]
    public void TestInvalidSubstateTransition()
    {
        _lsm.Initialize(States.Initialized);
        _lsm.MoveTo(States.Starting);
        _lsm.MoveTo(States.RunningLow);
        Assert.ThrowsExactly<StateMachineException>(() => _lsm.MoveTo(States.Running));
    }
}