using System;
using System.Collections.Generic;
using System.Windows.Forms;
using IDx3DSharp.DemoApp.Demos;

namespace IDx3DSharp.DemoApp
{
    internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(new TestComponent2Old()));
		}
	}
}
