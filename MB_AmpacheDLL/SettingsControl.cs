using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Protocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch((string)Protocol.SelectedValue)
            {
                case "HTTP":
                    Port.Value = 80;
                    break;
                case "HTTPS":
                    Port.Value = 443;
                    break;
                default:
                    Port.Value = 8080;
                    break;
            }
        }
    }
}
