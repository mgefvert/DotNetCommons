using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DotNetCommons.WinForms;

public static class DataGridViewExtensions
{
    public static void SelectRows<T>(this DataGridView control, Func<T, bool> predicate)
    {
        if (!(control.DataSource is BindingSource dataSource))
            throw new ArgumentException("DataGridView must have a BindingSource as the data object.", nameof(control));

        var selected = new List<int>();
        for (var i = 0; i < dataSource.Count - 1; i++)
        {
            if (predicate((T) dataSource.List[i]))
                selected.Add(i);
        }

        control.ClearSelection();
        foreach (var i in selected)
            control.Rows[i].Selected = true;
    }
}