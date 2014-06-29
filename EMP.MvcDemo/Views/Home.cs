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
    public partial class Home : Form, IViewForm<HomeMenuModel>
    {

        public NXDO.Data.Factory.MvcHelper<HomeMenuModel> Mvc
        {
            get;
            set;
        }

        public void Register(NXDO.Data.Factory.MvcHelper<HomeMenuModel> mvc)
        {
            mvc.ForTitle(this.mnuEmp, mnu => mnu.Text, m => m.Emp);
            mvc.ActionClick(this.mnuEmp, "LoadEmps", ToActionFlag.ChildActived);

            mvc.ForTitle(this.mnuSubView, mnu => mnu.Text, m => m.Form1);
            mvc.ActionClick(this.mnuSubView, "LoadSubView", ToActionFlag.ChildActived);
        }


        public Home()
        {
            InitializeComponent();
        }

        private void Home_Load(object sender, EventArgs e)
        {
            
        }
    }
}
