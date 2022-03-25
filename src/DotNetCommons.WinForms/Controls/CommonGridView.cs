using System;
using System.Windows.Forms;

namespace DotNetCommons.WinForms.Controls;

public class CommonGridView : DataGridView
{
    protected override void CreateHandle()
    {
        DoubleBuffered = true;
        base.CreateHandle();
    }
}