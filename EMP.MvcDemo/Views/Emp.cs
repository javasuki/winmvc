using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NXDO.Data.MVCP;
using EMP.MvcDemo.Models;

namespace EMP.MvcDemo.Views
{
    public partial class Emp : Form, IViewForm<Models.EmpModel>
    {
        public NXDO.Data.Factory.MvcHelper<Models.EmpModel> Mvc
        {
            get;
            set;
        }

        public void Register(NXDO.Data.Factory.MvcHelper<Models.EmpModel> mvc)
        {
            Action<DataGridView> act = g =>
            {
                g.Columns[0].Visible = false;
                g.Columns.Cast<DataGridViewColumn>().ToList().ForEach(c => mvc.ForTitle(c, col => col.HeaderText, c.Name));
            };
            mvc.FillDataSource(this.dataGridView1, act);
            mvc.Partial(this.empEdit1);

            //var lst = this.dataGridView1.DataSource as List<EmpModel>;
            //this.Mvc.Action("Edit", lst[1]);
            System.Diagnostics.Debug.WriteLine("Register");
        }

        public Emp()
        {
            InitializeComponent();
        }

        private void Emp_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            System.Diagnostics.Debug.WriteLine("Load");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var lst = this.dataGridView1.DataSource as List<EmpModel>;
            this.Mvc.Action("Edit", lst[1]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Mvc.Action("Refresh");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var r = this.Mvc.ActionDialog("Dlg");
            if (r == ActionMode.OK)
            {
                string xx = this.Mvc.ViewBag.RS;
                System.Diagnostics.Debug.WriteLine(xx);
            }
            System.Diagnostics.Debug.WriteLine(r);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Mvc.Action("Save");
        }


    }
}
