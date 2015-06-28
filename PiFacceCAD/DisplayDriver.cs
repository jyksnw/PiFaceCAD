using System;
using System.Timers;

namespace PiFaceCAD
{
	public class DisplayDriver : IDisposable
	{
		readonly Controller m_PiFaceCAD;

		bool m_Disposed;
		bool m_DisplayIsOn;
		bool m_BacklightIsOn;

		Timer m_TimeOutTimer;
		bool m_TimeOutTimerRunning;

		/// <summary>
		/// Initializes a new instance of the <see cref="PiFaceCAD.DisplayDriver"/> class.
		/// </summary>
		/// <param name="hal">Hal.</param>
		public DisplayDriver (Controller controller)
		{
			m_PiFaceCAD = controller;
			m_TimeOutTimer = new Timer ();
			m_TimeOutTimer.Elapsed += TimeOutTimer_Elapsed;

			Init ();

			m_Disposed = false;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="PiFaceCAD.DisplayDriver"/> time out timer running.
		/// </summary>
		/// <value><c>true</c> if time out timer running; otherwise, <c>false</c>.</value>
		public bool TimeOutTimerRunning {
			get {
				return m_TimeOutTimerRunning;
			}
		}

		/// <summary>
		/// Write the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		public void Write (string message)
		{
			if (!m_DisplayIsOn || !m_BacklightIsOn) {
				InternalTurnOnDisplay ();
			}

			if (m_TimeOutTimerRunning) {
				InternalStopTimeOutTimer ();
			}

			m_PiFaceCAD.Write (message);
		}

		/// <summary>
		/// Write the specified message with a timeout.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="timeout">Timeout.</param>
		public void Write (string message, double timeout)
		{
			this.Write (message);

			InternalStartTimeOutTimer (timeout);
		}

		public void Clear ()
		{
			m_PiFaceCAD.Clear ();
		}

		//TODO: Implement logic to figure out if a new row is needed

		/// <summary>
		/// Toggles the display.
		/// </summary>
		public void ToggleDisplay ()
		{
			if (m_DisplayIsOn) {
				ToggleDisplay (PowerState.Off);
			} else {
				ToggleDisplay (PowerState.On);
			}
		}

		/// <summary>
		/// Toggles the display.
		/// </summary>
		/// <param name="state">Power state to toggle too</param>
		public void ToggleDisplay (PowerState state)
		{
			switch (state) {
			case PowerState.On:
				{
					if (!m_DisplayIsOn) {
						m_PiFaceCAD.DisplayOn ();
						m_DisplayIsOn = true;
					}

					break;
				}
			case PowerState.Off:
				{
					if (m_DisplayIsOn) {
						if (m_BacklightIsOn) {
							ToggleBacklight (PowerState.Off);
						}

						m_PiFaceCAD.DisplayOff ();
						m_DisplayIsOn = false;
					}

					break;
				}
			}
		}

		/// <summary>
		/// Toggles the backlight.
		/// </summary>
		public void ToggleBacklight ()
		{
			if (m_BacklightIsOn) {
				ToggleBacklight (PowerState.Off);
			} else {
				ToggleBacklight (PowerState.On);
			}
		}

		/// <summary>
		/// Toggles the backlight.
		/// </summary>
		/// <param name="state">Power state to toggle too</param>
		public void ToggleBacklight (PowerState state)
		{
			switch (state) {
			case PowerState.On:
				{
					if (!m_BacklightIsOn) {
						m_PiFaceCAD.BacklightOn ();
						m_BacklightIsOn = true;
					}

					break;
				}
			case PowerState.Off:
				{
					if (m_BacklightIsOn) {
						m_PiFaceCAD.BacklightOff ();
						m_BacklightIsOn = false;
					}

					break;
				}
			}
		}

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		private void Init ()
		{
			m_PiFaceCAD.Clear ();
			m_PiFaceCAD.BlinkOff ();
			m_PiFaceCAD.CursorOff ();
			m_PiFaceCAD.BacklightOff ();
			m_PiFaceCAD.DisplayOff ();

			m_BacklightIsOn = false;
			m_DisplayIsOn = false;
		}

		/// <summary>
		/// Internals the turn on display.
		/// </summary>
		private void InternalTurnOnDisplay ()
		{
			if (!m_DisplayIsOn) {
				ToggleDisplay (PowerState.On);
			}

			if (!m_BacklightIsOn) {
				ToggleBacklight (PowerState.On);
			}
		}

		/// <summary>
		/// Internals the turn off display.
		/// </summary>
		private void InternalTurnOffDisplay ()
		{
			if (m_DisplayIsOn) {
				m_PiFaceCAD.Clear ();

				// This call will also handle turning off the backlight
				ToggleDisplay (PowerState.Off);
			}
		}

		/// <summary>
		/// Internals the start time out timer.
		/// </summary>
		/// <param name="timeout">Timeout.</param>
		private void InternalStartTimeOutTimer (double timeout)
		{
			// Make sure the timer isn't already running
			if (m_TimeOutTimerRunning) {
				InternalStopTimeOutTimer ();
			}

			m_TimeOutTimer.Elapsed += TimeOutTimer_Elapsed;
			m_TimeOutTimer.Interval = timeout;
			m_TimeOutTimer.Start ();
			m_TimeOutTimerRunning = true;
		}

		/// <summary>
		/// Internals the stop time out timer.
		/// </summary>
		private void InternalStopTimeOutTimer ()
		{
			if (m_TimeOutTimerRunning) {
				m_TimeOutTimer.Stop ();
				m_TimeOutTimer.Elapsed -= TimeOutTimer_Elapsed;
				m_TimeOutTimerRunning = false;
			}
		}

		/// <summary>
		/// Is fired when the time out timer's interval has elapsed
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		private void TimeOutTimer_Elapsed (object sender, ElapsedEventArgs e)
		{
			InternalStopTimeOutTimer ();
			InternalTurnOffDisplay ();
		}

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the <see cref="PiFaceCAD.DisplayDriver"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="PiFaceCAD.DisplayDriver"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="PiFaceCAD.DisplayDriver"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="PiFaceCAD.DisplayDriver"/>
		/// so the garbage collector can reclaim the memory that the <see cref="PiFaceCAD.DisplayDriver"/> was occupying.</remarks>
		public void Dispose ()
		{
			if (!m_Disposed) {
				if (m_TimeOutTimerRunning) {
					InternalStopTimeOutTimer ();
				}

				m_TimeOutTimer.Dispose ();
				m_TimeOutTimer.Dispose ();

				m_Disposed = true;
			}
		}

		#endregion
	}
}

