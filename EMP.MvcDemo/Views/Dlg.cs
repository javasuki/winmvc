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
    public partial class Dlg : Form, IViewForm<EmpModel>
    {
        public Dlg()
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
        }

        private void Dlg_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Mvc.ViewBag.RS = "xx";
        }


    }
}
