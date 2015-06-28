using System;
using System.Timers;
using System.Collections.Generic;

namespace PiFacceCAD
{
	public class InputDriver : IDisposable
	{
		private const uint DEFAULT_BUTTON_STATE = 255;

		static readonly object _btnLock = new object ();
		readonly Controller m_PiFaceCAD;

		uint m_LastButtonState;

		Timer m_ButtonTimer;
		bool m_Disposed;
		bool m_TimerStarted = false;
		Dictionary<uint, Action> m_RegisteredButtons;

		/// <summary>
		/// Initializes a new instance of the <see cref="PiFaceCAD.InputDriver"/> class.
		/// </summary>
		/// <param name="hal">Hal.</param>
		public InputDriver (Controller controller)
		{
			m_PiFaceCAD = controller;
			m_RegisteredButtons = new Dictionary<uint, Action> ();
			m_ButtonTimer = new Timer ();

			this.Interval = 100;	//Start by sampling the switches every 1/10 of a second

			InternalStartTimer ();

			m_Disposed = false;
		}

		/// <summary>
		/// Gets or sets the interval in which to poll the switchs for state change
		/// </summary>
		/// <value>The interval</value>
		public double Interval { 
			get {
				return m_ButtonTimer.Interval;
			}
			set {
				if (m_TimerStarted) {
					InternalStopTimer ();

					m_ButtonTimer.Interval = value;

					InternalStartTimer ();
				}
			} 
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="PiFaceCAD.InputDriver"/> is monitoring.
		/// </summary>
		/// <value><c>true</c> if monitoring; otherwise, <c>false</c>.</value>
		public bool Monitoring {
			get {
				return m_TimerStarted;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="PiFaceCAD.InputDriver"/> is disposed.
		/// </summary>
		/// <value><c>true</c> if disposed; otherwise, <c>false</c>.</value>
		public bool Disposed {
			get {
				return m_Disposed;
			}
		}

		/// <summary>
		/// Registers the single button.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="onButtonPressed">On button pressed callback function</param>
		public void RegisterSingleButton (int button, Action onButtonPressed)
		{
			var buttonAddress = (uint)(255 - (Math.Pow (2, button - 1)));

			if (!m_RegisteredButtons.ContainsKey (buttonAddress)) {
				m_RegisteredButtons.Add (buttonAddress, onButtonPressed);
			}
		}

		/// <summary>
		/// Registers combo button.
		/// </summary>
		/// <param name="buttonCombo">Button combo.</param>
		/// <param name="onButtonsPressed">On buttons pressed callback function</param>
		public void RegisterComboButton (int[] buttonCombo, Action onButtonsPressed)
		{
			uint buttonOffset = 0;

			for (int i = 0; i < buttonCombo.Length; i++) {
				buttonOffset += (uint)Math.Pow (2, buttonCombo [0]);
			}

			var buttonAddress = (uint)(DEFAULT_BUTTON_STATE - buttonOffset - 1);

			if (!m_RegisteredButtons.ContainsKey (buttonAddress)) {
				m_RegisteredButtons.Add (buttonAddress, onButtonsPressed);
			}
		}

		/// <summary>
		/// Internals the start timer.
		/// </summary>
		private void InternalStartTimer ()
		{
			if (m_TimerStarted) {
				InternalStopTimer ();		
			}

			m_ButtonTimer.Elapsed += ReadSwitchState;
			m_ButtonTimer.Start ();

			m_TimerStarted = true;
		}

		/// <summary>
		/// Internals the stop timer.
		/// </summary>
		private void InternalStopTimer ()
		{
			m_ButtonTimer.Stop ();
			m_ButtonTimer.Elapsed -= ReadSwitchState;

			m_TimerStarted = false;
		}

		/// <summary>
		/// Reads the state of the switch.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void ReadSwitchState (object sender, ElapsedEventArgs e)
		{
			var currentPinState = m_PiFaceCAD.ReadSwitches ();

			if (currentPinState != m_LastButtonState) {
				m_LastButtonState = currentPinState;

				// FireSwitchEvent is placed here to prevent multiple events being fired
				// should the user hold down the button.
				// TODO: Look into timed button events (i.e. button held for 4 seconds fires a different event than a momentary press)
				FireButtonEvent ();
			}
		}

		/// <summary>
		/// Fires the button event.
		/// </summary>
		private void FireButtonEvent ()
		{
			Action buttonEvent;

			if (m_RegisteredButtons.TryGetValue (m_LastButtonState, out buttonEvent)) {
				lock (_btnLock) {
					// Button events should only be fired one at a time to prevent
					// thread corruption and/or undesired outcomes.
					buttonEvent ();
				}
			}
		}

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the <see cref="PiFaceCAD.InputDriver"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="PiFaceCAD.InputDriver"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="PiFaceCAD.InputDriver"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="PiFaceCAD.InputDriver"/> so
		/// the garbage collector can reclaim the memory that the <see cref="PiFaceCAD.InputDriver"/> was occupying.</remarks>
		public void Dispose ()
		{
			if (!m_Disposed) {
				if (m_TimerStarted && m_ButtonTimer != null) {
					InternalStopTimer ();

					m_RegisteredButtons.Clear ();
					m_ButtonTimer.Dispose ();
					m_ButtonTimer = null;

					m_Disposed = true;
				}
			}
		}

		#endregion
	}
}

