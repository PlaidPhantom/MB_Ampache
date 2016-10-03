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
        public Protocol Protocol
        {
            get { return (Protocol)Enum.Parse(typeof(Protocol), (string)ProtocolSelect.SelectedItem); }
            set { ProtocolSelect.SelectedItem = Enum.GetName(typeof(Protocol), value); }
        }

        public string Server
        {
            get { return ServerField.Text; }
            set { ServerField.Text = value; }
        }

        public int Port
        {
            get { return (int)PortSpinner.Value; }
            set { PortSpinner.Value = value; }
        }

        public string Username
        {
            get { return UsernameField.Text; }
            set { UsernameField.Text = value; }
        }

        public string Password
        {
            get { return PasswordField.Text; }
            set { PasswordField.Text = value; }
        }

        public SettingsControl() : base()
        {
            InitializeComponent();
            ProtocolSelect.Items.Clear();
            ProtocolSelect.Items.AddRange(Enum.GetNames(typeof(Protocol)));
        }

        private void ProtocolSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch((string)ProtocolSelect.SelectedItem)
            {
                case "HTTP":
                    PortSpinner.Value = 80;
                    break;
                case "HTTPS":
                    PortSpinner.Value = 443;
                    break;
                default:
                    PortSpinner.Value = 8080;
                    break;
            }
        }
    }
}
