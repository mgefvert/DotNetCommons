using System;
using System.Windows.Forms;

namespace DotNetCommons.WinForms.Controls
{
    public class CommonGridView : DataGridView
    {
        public CommonGridView()
        {
            // AllowUserToAddRows = false;
            // AllowUserToDeleteRows = false;
            // AllowUserToResizeRows = false;
            // AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            // BackgroundColor = SystemColors.Window;
            // BorderStyle = BorderStyle.Fixed3D;
            // ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            // GridColor = SystemColors.Control;
            // MultiSelect = false;
            // ReadOnly = true;
            // SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        /*
        [DefaultValue(false)]
        public new bool AllowUserToAddRows
        {
            get { return base.AllowUserToAddRows; }
            set { base.AllowUserToAddRows = value; }
        }

        [DefaultValue(false)]
        public new bool AllowUserToDeleteRows
        {
            get { return base.AllowUserToDeleteRows; }
            set { base.AllowUserToDeleteRows = value; }
        }

        [DefaultValue(false)]
        public new bool AllowUserToResizeRows
        {
            get { return base.AllowUserToResizeRows; }
            set { base.AllowUserToResizeRows = value; }
        }

        [DefaultValue(DataGridViewAutoSizeColumnsMode.AllCells)]
        public new DataGridViewAutoSizeColumnsMode AutoSizeColumnsMode
        {
            get { return base.AutoSizeColumnsMode; }
            set { base.AutoSizeColumnsMode = value; }
        }

        [DefaultValue(BorderStyle.Fixed3D)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        [DefaultValue(DataGridViewColumnHeadersHeightSizeMode.AutoSize)]
        public new DataGridViewColumnHeadersHeightSizeMode ColumnHeadersHeightSizeMode
        {
            get { return base.ColumnHeadersHeightSizeMode; }
            set { base.ColumnHeadersHeightSizeMode = value; }
        }

        [DefaultValue(false)]
        public new bool MultiSelect
        {
            get { return base.MultiSelect; }
            set { base.MultiSelect = value; }
        }

        [DefaultValue(false)]
        public new bool ReadOnly
        {
            get { return base.ReadOnly; }
            set { base.ReadOnly = value; }
        }

        [DefaultValue(DataGridViewSelectionMode.FullRowSelect)]
        public new DataGridViewSelectionMode SelectionMode
        {
            get { return base.SelectionMode; }
            set { base.SelectionMode = value; }
        }*/

        protected override void CreateHandle()
        {
            DoubleBuffered = true;
            base.CreateHandle();
        }
    }
}
