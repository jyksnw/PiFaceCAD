using System;
using PiFacceCAD;

namespace PiFaceCAD.Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Starting test...");

			PiFace pi = new PiFace ();

			pi.Display.Write ("PiFaceCAD \nTest App");

			pi.Input.RegisterSingleButton (1, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write ("Button 1", 5000);
			});

			pi.Input.RegisterSingleButton (2, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write ("Button 2", 5000);
			});

			pi.Input.RegisterSingleButton (3, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write ("Button 3", 5000);
			});

			pi.Input.RegisterSingleButton (4, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write ("Button 4", 5000);
			});

			pi.Input.RegisterSingleButton (5, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write ("Button 5", 5000);
			});

			pi.Input.RegisterSingleButton (6, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write ("Button 6", 5000);
			});

			pi.Input.RegisterSingleButton (7, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write ("Button 7", 5000);
			});

			pi.Input.RegisterSingleButton (8, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write ("Button 8", 5000);
			});

			pi.Input.RegisterComboButton (new int[] { 1, 3 }, () => {
				Console.WriteLine("Button(s) pressed...");
				pi.Display.Clear();
				pi.Display.Write("Combo button", 5000);
			});

			while (true) {
				System.Threading.Thread.Sleep (100);
			}
		}

		private void NotifyConsoleOfButtonPress()
		{
			Console.WriteLine ("Button has been pressed...");
		}
	}
}
