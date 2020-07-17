using Microsoft.Toolkit.Graph.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinForms_Core_GraphApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            ProviderManager.Instance.GlobalProvider = new MockProvider();

            InitializeComponent();

            windowsXamlHost1.InitialTypeName = "UWP_XamlApplication.SamplePage";
        }
    }
}
