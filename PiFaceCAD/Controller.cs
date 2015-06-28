using System;
using System.Timers;

namespace PiFaceCAD
{
	public class Controller
	{
		public const int ERROR = -1;

		private const int BUS = 0;
		private const int CHIP_SELECT = 1;
		private const int HARDWARE_ADDRESS = 0;

		private static uint SWITCH_PORT = DataMCP23S17.RegisterAddress.GPIOA;
		private static uint LCD_PORT = DataMCP23S17.RegisterAddress.GPIOB;

		private static uint IO_Config = DataMCP23S17.IO_Config.AddressingMode.BANK_OFF |
			DataMCP23S17.IO_Config.InteruptMirror.INT_MIRROR_OFF |
			DataMCP23S17.IO_Config.IncrementAddressPointer.SEQOP_OFF |
			DataMCP23S17.IO_Config.SlewRate.DISSLW_OFF |
			DataMCP23S17.IO_Config.HardwareAddressing.HAEN_ON |
			DataMCP23S17.IO_Config.InteruptOpenDrain.ODR_OFF |
			DataMCP23S17.IO_Config.InteruptPolarity.INTPOL_LOW;

		int m_FileDescriptor = 0;
		uint m_CurrentAddress = 0;
		uint m_CurrentEntryMode = 0;
		uint m_CurrentFunctionSet = 0;
		uint m_CurrentDisplayControl = 0;

		uint m_RowPos = 0;

		private static readonly Lazy<Controller> m_Instance = new Lazy<Controller> (() => new Controller ());

		public static Controller Instance {
			get {
				return m_Instance.Value;
			}
		}

		private Controller ()
		{
			Open ();
		}

		public uint CurrentDisplayAddress {
			get {
				return m_CurrentAddress;
			}
		}

		/// <summary>
		/// Opens and initializes a PiFace Control and Display.
		/// </summary>
		/// <remarks>This should only be called if Close is called befor it. 
		/// Otherwise it is called during object creation</remarks>
		/// <returns>A file descriptor for making raw SPI transactions to the MCP23S17 (for advanced users only)</returns>
		public int Open ()
		{
			var openState = OpenNoinit ();

			if (openState != ERROR) {
				// Set the IO config
				WrapperMCP23S17.mcp23s17_write_reg (IO_Config, DataMCP23S17.RegisterAddress.IOCON, HARDWARE_ADDRESS, m_FileDescriptor);

				// Set GPIO Port A as inputs (switches)
				WrapperMCP23S17.mcp23s17_write_reg (0xFF, DataMCP23S17.RegisterAddress.IODIRA, HARDWARE_ADDRESS, m_FileDescriptor);
				WrapperMCP23S17.mcp23s17_write_reg (0xFF, DataMCP23S17.RegisterAddress.GPPUA, HARDWARE_ADDRESS, m_FileDescriptor);

				// Set GPIO Port B as outputs (connected to HD44790 - Display)
				WrapperMCP23S17.mcp23s17_write_reg (0x00, DataMCP23S17.RegisterAddress.IODIRB, HARDWARE_ADDRESS, m_FileDescriptor);

				// Enable interrupts
				WrapperMCP23S17.mcp23s17_write_reg (0xFF, DataMCP23S17.RegisterAddress.GPINTENA, HARDWARE_ADDRESS, m_FileDescriptor);

				Init ();

				return m_FileDescriptor;
			} else {
				return ERROR;
			}
		}

		/// <summary>
		/// Closes a PiFace Control and Display (turns off interrupts, closes file descriptor)
		/// </summary>
		public void Close ()
		{
			// Disable interrupts if enabled
			var interruptsEnabled = WrapperMCP23S17.mcp23s17_read_reg (DataMCP23S17.RegisterAddress.GPINTENA, HARDWARE_ADDRESS, m_FileDescriptor);

			if (interruptsEnabled == 0) {
				WrapperMCP23S17.mcp23s17_write_reg (0, DataMCP23S17.RegisterAddress.GPINTENA, HARDWARE_ADDRESS, m_FileDescriptor);
			}

			// TODO: This should be marshalled
			LinuxWrapper.close (m_FileDescriptor);
		}

		/// <summary>
		/// Reads entire switch port
		/// </summary>
		/// <returns>The switch bits</returns>
		public uint ReadSwitches ()
		{
			return WrapperMCP23S17.mcp23s17_read_reg (SWITCH_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
		}

		/// <summary>
		/// Reads a single switch
		/// </summary>
		/// <returns>The switch value</returns>
		/// <param name="switchNumber">Switch number</param>
		public uint ReadSwitch (int switchNumber)
		{
			var switchBits = ReadSwitches ();
			var switchValue = (switchBits >> switchNumber) & 1;

			return switchValue;
		}

		/// <summary>
		/// Writes a message to the LCD screen starting from the current cursor position. Accepts '\n'.
		/// </summary>
		/// <param name="message">The current address</param>
		public uint Write (string message)
		{
			SendCommand (DataPiFaceCAD.Command.LCD_SETDDRAMADDR | m_CurrentAddress);

			foreach (char c in message) {
				if (c == '\n') {
					m_RowPos = (m_RowPos + 1) % DataPiFaceCAD.LCD_MAX_LINES;

					SetCursor (0, m_RowPos);
				} else {
					SendData (c);

					m_CurrentAddress++;
				}
			}

			return m_CurrentAddress;
		}

		/// <summary>
		/// Sets the cursor position on the screen
		/// </summary>
		/// <returns>The current address</returns>
		/// <param name="col">The desired column</param>
		/// <param name="row">The desired row</param>
		public uint SetCursor (uint col, uint row)
		{
			col = Math.Max (0, Math.Min (col, (DataPiFaceCAD.LCD_RAM_WIDTH / 2) - 1));
			row = Math.Max (0, Math.Min (row, DataPiFaceCAD.LCD_MAX_LINES - 1));

			InternalSetCursorAddress (ColRow2Address (col, row));

			return m_CurrentAddress;
		}

		/// <summary>
		/// Clears the screen and returns the cursor to home (0, 0)
		/// </summary>
		public void Clear ()
		{
			SendCommand (DataPiFaceCAD.Command.LCD_CLEARDISPLAY);

			InternalSleep (DataPiFaceCAD.Delay.DELAY_CLEAR_NS);

			m_CurrentAddress = 0;
		}

		/// <summary>
		/// Returns the cursor to home (0, 0)
		/// </summary>
		public void Home ()
		{
			SendCommand (DataPiFaceCAD.Command.LCD_RETURNHOME);

			InternalSleep (DataPiFaceCAD.Delay.DELAY_CLEAR_NS);

			m_CurrentAddress = 0;
		}

		/// <summary>
		/// Turns the display on
		/// </summary>
		public void DisplayOn ()
		{
			m_CurrentDisplayControl |= DataPiFaceCAD.Flag.LCD_DISPLAYON;

			UpdateDisplayControl ();
		}

		/// <summary>
		/// Turns the display off
		/// </summary>
		public void DisplayOff ()
		{
			m_CurrentDisplayControl &= 0xFF ^ DataPiFaceCAD.Flag.LCD_DISPLAYON;

			UpdateDisplayControl ();
		}

		/// <summary>
		/// Turns the blink on
		/// </summary>
		public void BlinkOn ()
		{
			m_CurrentDisplayControl |= DataPiFaceCAD.Flag.LCD_BLINKON;

			UpdateDisplayControl ();
		}

		/// <summary>
		/// Turns the blink off
		/// </summary>
		public void BlinkOff ()
		{
			m_CurrentDisplayControl &= 0xFF ^ DataPiFaceCAD.Flag.LCD_BLINKON;

			UpdateDisplayControl ();
		}

		/// <summary>
		/// Turns the cursor on
		/// </summary>
		public void CursorOn ()
		{
			m_CurrentDisplayControl |= DataPiFaceCAD.Flag.LCD_CURSORON;

			UpdateDisplayControl ();
		}

		/// <summary>
		/// Turns the cursor off
		/// </summary>
		public void CursorOff ()
		{
			m_CurrentDisplayControl &= 0xFF ^ DataPiFaceCAD.Flag.LCD_CURSORON;

			UpdateDisplayControl ();
		}

		/// <summary>
		/// Turns the backlight on
		/// </summary>
		public void BacklightOn ()
		{
			SetBacklight (1);
		}

		/// <summary>
		/// Turns the backlight off
		/// </summary>
		public void BacklightOff ()
		{
			SetBacklight (0);
		}

		/// <summary>
		/// Moves the cursor left
		/// </summary>
		public void MoveCursorLeft ()
		{
			SendCommand (DataPiFaceCAD.Command.LCD_CURSORSHIFT | DataPiFaceCAD.Flag.LCD_DISPLAYMOVE | DataPiFaceCAD.Flag.LCD_MOVELEFT);
		}

		/// <summary>
		/// Moves the cursor right
		/// </summary>
		public void MoveCursorRight ()
		{
			SendCommand (DataPiFaceCAD.Command.LCD_CURSORSHIFT | DataPiFaceCAD.Flag.LCD_DISPLAYMOVE | DataPiFaceCAD.Flag.LCD_MOVERIGHT);
		}

		/// <summary>
		/// The cursor will move to the right after printing causing text to read left to right
		/// </summary>
		public void SetDisplayLeftToRight ()
		{
			m_CurrentEntryMode |= DataPiFaceCAD.Flag.LCD_ENTRYLEFT;

			SendCommand (DataPiFaceCAD.Command.LCD_ENTRYMODESET | m_CurrentEntryMode);
		}

		/// <summary>
		/// The cursor will move to the left after printing cuasing text to read right to left
		/// </summary>
		public void SetDisplayRightToLeft ()
		{
			m_CurrentEntryMode &= 0xFF ^ DataPiFaceCAD.Flag.LCD_ENTRYLEFT;

			SendCommand (DataPiFaceCAD.Command.LCD_ENTRYMODESET | m_CurrentEntryMode);
		}

		/// <summary>
		/// The screen will follow text if it moves out of view
		/// </summary>
		public void AutoscrollOn ()
		{
			m_CurrentDisplayControl |= DataPiFaceCAD.Flag.LCD_ENTRYSHIFTINCREMENT;

			SendCommand (DataPiFaceCAD.Command.LCD_ENTRYMODESET | m_CurrentDisplayControl);
		}

		/// <summary>
		/// The screen will not follow text if it moves out of view
		/// </summary>
		public void AutoscrollOff ()
		{
			m_CurrentDisplayControl &= 0xFF ^ DataPiFaceCAD.Flag.LCD_ENTRYSHIFTINCREMENT;

			SendCommand (DataPiFaceCAD.Command.LCD_ENTRYMODESET | m_CurrentDisplayControl);
		}

		/// <summary>
		/// Writes the custom bitmap stored at the specified bank location to the display
		/// </summary>
		/// <param name="location">The memory location of the bitmap</param>
		public void WriteCustomBitmap (uint location)
		{
			SendCommand (DataPiFaceCAD.Command.LCD_SETDDRAMADDR | m_CurrentAddress);
			SendData (location);
			m_CurrentAddress++;
		}

		/// <summary>
		/// Stores a custom bitmap to the location specified (up to 8: 0-7).
		/// </summary>
		/// <param name="location">The memory location to store the bitmap</param>
		/// <param name="bitmap">The bitmap</param>
		public void StoreCustomBitmap (uint location, byte[] bitmap)
		{
			location &= 0x7;	// There is only 8 locations 0-7;

			SendCommand (DataPiFaceCAD.Command.LCD_SETCGRAMADDR | (location << 3));

			// Load in the bitmap
			for (int i = 0; i < 8; i++) {
				SendData (bitmap [i]);
			}
		}

		/// <summary>
		/// Returns an address calculated from a column and a row
		/// </summary>
		/// <returns>The calculated address</returns>
		/// <param name="col">The column</param>
		/// <param name="row">The row</param>
		public uint ColRow2Address (uint col, uint row)
		{
			return col + DataPiFaceCAD.ROW_OFFSETS [row];
		}

		/// <summary>
		/// Returns the column calculated from an address
		/// </summary>
		/// <returns>The column for the given address</returns>
		/// <param name="address">The address</param>
		public uint Address2Col (uint address)
		{
			return address % DataPiFaceCAD.ROW_OFFSETS [1];
		}

		/// <summary>
		/// Returns a row calculated from an address
		/// </summary>
		/// <returns>The row for the given address</returns>
		/// <param name="address">The address</param>
		public uint Address2Row (uint address)
		{
			return (uint)(address > DataPiFaceCAD.ROW_OFFSETS [1] ? 1 : 0);
		}

		/// <summary>
		/// Opens PiFace Control and Display without initializing it.
		/// </summary>
		/// <returns>A file descriptor for making raw SPI transactions to the MCP23S7 (for advanced users only)</returns>
		private int OpenNoinit ()
		{
			return (m_FileDescriptor = WrapperMCP23S17.mcp23s17_open (BUS, CHIP_SELECT)) < 0 ? ERROR : m_FileDescriptor;
		}

		/// <summary>
		/// Initialize the LCD
		/// </summary>
		private void Init ()
		{
			// Setup sequence
			InternalSleep (DataPiFaceCAD.Delay.DELAY_SETUP_0_NS);
			WrapperMCP23S17.mcp23s17_write_reg (0x3, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
			PulseEnable ();

			InternalSleep (DataPiFaceCAD.Delay.DELAY_SETUP_1_NS);
			WrapperMCP23S17.mcp23s17_write_reg (0x3, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
			PulseEnable ();

			InternalSleep (DataPiFaceCAD.Delay.DELAY_SETUP_2_NS);
			WrapperMCP23S17.mcp23s17_write_reg (0x3, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
			PulseEnable ();

			WrapperMCP23S17.mcp23s17_write_reg (0x2, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
			PulseEnable ();

			m_CurrentFunctionSet |= DataPiFaceCAD.Flag.LCD_4BITMODE | DataPiFaceCAD.Flag.LCD_2LINE | DataPiFaceCAD.Flag.LCD_5X8DOTS;
			SendCommand (DataPiFaceCAD.Command.LCD_FUNCTIONSET | m_CurrentFunctionSet);

			m_CurrentDisplayControl |= DataPiFaceCAD.Flag.LCD_DISPLAYOFF | DataPiFaceCAD.Flag.LCD_CURSOROFF | DataPiFaceCAD.Flag.LCD_BLINKOFF;
			UpdateDisplayControl ();

			Clear ();

			m_CurrentEntryMode |= DataPiFaceCAD.Flag.LCD_ENTRYLEFT | DataPiFaceCAD.Flag.LCD_ENTRYSHIFTDECREMENT;
			SendCommand (DataPiFaceCAD.Command.LCD_ENTRYMODESET | m_CurrentEntryMode);

			m_CurrentDisplayControl |= DataPiFaceCAD.Flag.LCD_DISPLAYON | DataPiFaceCAD.Flag.LCD_CURSORON | DataPiFaceCAD.Flag.LCD_BLINKON;
			UpdateDisplayControl ();
		}

		/// <summary>
		/// Internally sets the cursor position on the screen (address = col + row*40)
		/// </summary>
		/// <param name="address">The address</param>
		private void InternalSetCursorAddress (uint address)
		{
			m_CurrentAddress = address % DataPiFaceCAD.LCD_RAM_WIDTH;

			SendCommand (DataPiFaceCAD.Command.LCD_SETDDRAMADDR | m_CurrentAddress);
		}

		/// <summary>
		/// Updates the display control.
		/// </summary>
		private void UpdateDisplayControl ()
		{
			SendCommand (DataPiFaceCAD.Command.LCD_DISPLAYCONTROL | m_CurrentDisplayControl);
		}

		/// <summary>
		/// Sends a command to the HD44780
		/// </summary>
		/// <param name="command">Command.</param>
		private void SendCommand (uint command)
		{
			SetRS (0);
			SendByte (command);
			InternalSleep (DataPiFaceCAD.Delay.DELAY_SETTLE_NS);
		}

		/// <summary>
		/// Send data to the HD44780
		/// </summary>
		/// <param name="data">Data.</param>
		private void SendData (uint data)
		{
			SetRS (1);
			SendByte (data);
			InternalSleep (DataPiFaceCAD.Delay.DELAY_SETTLE_NS);
		}

		/// <summary>
		/// Send a byte to the HD44780
		/// </summary>
		/// <param name="b">The blue component.</param>
		private void SendByte (uint b)
		{
			// Get current LCD port state and clear the data bits
			var currentState = WrapperMCP23S17.mcp23s17_read_reg (LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
			currentState &= 0xF0;

			// Send first nibble (ObXXXX0000)
			var newByte = currentState | ((b >> 4) & 0xF);
			WrapperMCP23S17.mcp23s17_write_reg (newByte, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
			PulseEnable ();

			// Send second nibble (0b0000XXXX)
			newByte = currentState | (b & 0xF);
			WrapperMCP23S17.mcp23s17_write_reg (newByte, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
			PulseEnable ();
		}

		/// <summary>
		/// Set the RS pin on the HD44780
		/// </summary>
		/// <param name="state">State.</param>
		private void SetRS (uint state)
		{
			WrapperMCP23S17.mcp23s17_write_bit (state, DataPiFaceCAD.Pin.PIN_RS, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
		}

		/// <summary>
		/// Set the RW pin on the HD44780
		/// </summary>
		/// <param name="state">State.</param>
		private void SetRW (uint state)
		{
			WrapperMCP23S17.mcp23s17_write_bit (state, DataPiFaceCAD.Pin.PIN_RW, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
		}

		/// <summary>
		/// Set the enable pin on the HD44780
		/// </summary>
		/// <param name="state">State.</param>
		private void SetEnable (uint state)
		{
			WrapperMCP23S17.mcp23s17_write_bit (state, DataPiFaceCAD.Pin.PIN_ENABLE, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
		}

		/// <summary>
		/// Set the backlight pin on the HD44780
		/// </summary>
		/// <param name="state">State.</param>
		private void SetBacklight (uint state)
		{
			WrapperMCP23S17.mcp23s17_write_bit (state, DataPiFaceCAD.Pin.PIN_BACKLIGHT, LCD_PORT, HARDWARE_ADDRESS, m_FileDescriptor);
		}

		/// <summary>
		/// Pulses the enable pin on the HD44780
		/// </summary>
		private void PulseEnable ()
		{
			SetEnable (1);
			InternalSleep (DataPiFaceCAD.Delay.DELAY_PULSE_NS);
			SetEnable (0);
			InternalSleep (DataPiFaceCAD.Delay.DELAY_PULSE_NS);
		}

		private void InternalSleep (int interval)
		{
			var timeSpan = TimeSpan.FromMilliseconds(interval);

			System.Threading.Thread.Sleep(timeSpan);
		}
	}
}

