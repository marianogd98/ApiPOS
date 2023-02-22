using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace wpfPos_Rios
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            Process[] procesos = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (procesos.Length > 1)
            {
                //ya se está ejecutando, no hacemos nada
                MessageBox.Show("La aplicacion se encuentra en ejecucion.");
                Application.Current.Shutdown();
            }
            else
            {
                base.OnStartup(e);
            }
        }
    }
}
