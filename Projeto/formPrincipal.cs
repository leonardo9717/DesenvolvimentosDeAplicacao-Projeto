﻿using System;
using System.Windows.Forms;

namespace Projeto
{
    public partial class formPrincipal : Form
    {
        public formPrincipal()
        {
            InitializeComponent();
        }

        private void btUtilizadores_Click(object sender, EventArgs e)
        {
            formGestaoUtilizadores gestaousers = new formGestaoUtilizadores();
            gestaousers.Show();
        }

        private void formPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}