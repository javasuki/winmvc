using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NXDO.Data.MVCP;
using EMP.MvcDemo.Models;

namespace EMP.MvcDemo.Views
{
    public partial class EmpEdit : UserControl, IViewControl<EmpModel>
    {
        public EmpEdit()
        {
            InitializeComponent();
        }

        public NXDO.Data.Factory.MvcHelper<EmpModel> Mvc
        {
            get;
            set;
        }

        public void Register(NXDO.Data.Factory.MvcHelper<EmpModel> mvc)
        {
            mvc.ForTitle(this.label1, l => l.Text, m => m.ID);
            mvc.ForTitle(this.label2, l => l.Text, m => m.Name);
            mvc.ForTitle(this.label3, l => l.Text, m => m.Sex);
            mvc.ForTitle(this.label4, l => l.Text, m => m.Birthday);
            mvc.ForText(this.textBox1, m => m.ID);
            mvc.ForText(this.textBox2, m => m.Name);

            var lst = mvc.ViewBag.ListData as List<SexModel>;
            mvc.FillDataList(this.comboBox1, lst, (SexModel sm) => new { sm.Value, sm.Name });
            mvc.ForKnown(this.comboBox1, c => c.SelectedValue, m => m.Sex);
            mvc.ForValue(this.dateTimePicker1, m => m.Birthday);
        }

        private void EmpEdit_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Mvc.Action("Save");
        }
    }
}
