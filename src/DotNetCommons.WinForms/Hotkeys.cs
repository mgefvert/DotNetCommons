using System;
using System.Collections.Generic;
using System.Windows.Forms;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.WinForms;

public class HotKeys
{
    private readonly IntPtr _handle;
    private readonly Dictionary<int, Action> _hotkeys = new();
    private int _counter = 1;
    private bool _succeeded = true;

    public HotKeys(IntPtr handle)
    {
        _handle = handle;
    }

    public bool Add(uint modifiers, uint keycode, Action execute)
    {
        var result = WinApi.RegisterHotKey(_handle, _counter, modifiers, keycode);
        if (result)
            _hotkeys[_counter++] = execute;

        _succeeded &= result;

        return result;
    }

    public bool AllSucceeded()
    {
        return _succeeded;
    }

    public void Clear()
    {
        foreach (var hotkey in _hotkeys.Keys)
            WinApi.UnregisterHotKey(_handle, hotkey);

        _hotkeys.Clear();
    }

    public int Count()
    {
        return _hotkeys.Count;
    }

    public void Process(ref Message msg)
    {
        if (msg.Msg != (int)WinApi.WM.HOTKEY)
            return;

        int hotkey = (int)msg.WParam;
        if (!_hotkeys.ContainsKey(hotkey))
            return;

        _hotkeys[hotkey]();
        msg.Result = (IntPtr)1;
    }
}