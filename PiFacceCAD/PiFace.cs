using System;

namespace PiFaceCAD
{
	public class PiFace
	{
		private Controller m_Controller;
		private DisplayDriver m_Display;
		private InputDriver m_Input;

		public PiFace ()
		{
			m_Controller = Controller.Instance;
			m_Display = new DisplayDriver (m_Controller);
			m_Input = new InputDriver (m_Controller);
		}

		/// <summary>
		/// Allows for interaction with the PiFaceCAD Display
		/// </summary>
		/// <value>The display.</value>
		public DisplayDriver Display
		{
			get { return m_Display; }
		}

		/// <summary>
		/// Allows for interaction with the PiFaceCAD Input Buttons
		/// </summary>
		/// <value>The input.</value>
		public InputDriver Input
		{
			get { return m_Input; }
		}
	}
}

