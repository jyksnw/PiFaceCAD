using System.Runtime.InteropServices;

namespace PiFacceCAD
{
	internal static class WrapperMCP23S17
	{
		/// <summary>
		/// Returns a file descriptor for the SPI device through which the MCP23S
		/// port expander can be accessed
		/// </summary>
		/// <returns>The MCP23S17 port expander</returns>
		/// <param name="bus">The SPI bus</param>
		/// <param name="chip_select">The SPI chip select</param>
		[DllImport ("libmcp23s17.so")]
		internal static extern int mcp23s17_open (int bus, int chip_select);

		/// <summary>
		/// Returns the 8 bit value from the register specified. Must also specify
		/// which hardware address and file descriptor to use.
		/// </summary>
		/// <returns>The 8 bit value from the register specified</returns>
		/// <param name="reg">The register to read from (example: IODIRA, GPIOA)</param>
		/// <param name="hw_addr">The hardware address of the MCP23S17</param>
		/// <param name="fd">The file descriptor returned from <see cref="mcp23s17_open"/></param>
		[DllImport ("libmcp23s17.so")]
		internal static extern uint mcp23s17_read_reg (uint reg, uint hw_addr, int fd);

		/// <summary>
		/// Writes an 8 bit value to the register specified. Must also specify
		/// which hardware address and file descriptor to use.
		/// </summary>
		/// <param name="data">The data byte to be written</param>
		/// <param name="reg">The register to write to (example: IODIRA, GPIOA)</param>
		/// <param name="hw_addr">The hardware address of the MCP23S17</param>
		/// <param name="fd">The file descriptor returned from <see cref="mcp23s17_open"/></param>
		[DllImport ("libmcp23s17.so")]
		internal static extern void mcp23s17_write_reg (uint data, uint reg, uint hw_addr, int fd);

		/// <summary>
		/// Reads a single bit from the register specified. Must also specify
		/// which hardware address and file descriptor to use.
		/// </summary>
		/// <returns>The single bit from the register</returns>
		/// <param name="bit_num">The bit number to read</param>
		/// <param name="reg">The register to read from (example: IODIRA, GPIOA)</param>
		/// <param name="hw_addr">The hardware address of the MCP23S17</param>
		/// <param name="fd">The file descriptor returned from <see cref="mcp23s17_open"/></param>
		[DllImport ("libmcp23s17.so")]
		internal static extern uint mcp23s17_read_bit (uint bit_num, uint reg, uint hw_addr, int fd);

		/// <summary>
		/// Writes a single bit to the register specified. Must also specify
		/// which hardware address and file descriptor to use.
		/// </summary>
		/// <param name="data">The data to write</param>
		/// <param name="bit_num">The bit number to write to</param>
		/// <param name="reg">The register to write to (example: IODIRA, GPIOA)</param>
		/// <param name="hw_addr">The hardware address of the MCP23S17</param>
		/// <param name="fd">The file descriptor returned from <see cref="mcp23s17_open"/></param>
		[DllImport ("libmcp23s17.so")]
		internal static extern void mcp23s17_write_bit (uint data, uint bit_num, uint reg, uint hw_addr, int fd);

		/// <summary>
		/// Enables interrupts and exports to the GPIO connection from the mcp23s17
		/// </summary>
		/// <returns>0 on success</returns>
		[DllImport ("libmcp23s17.so")]
		internal static extern int mcp23s17_enable_interrupts ();

		/// <summary>
		/// Disables interrupts and exports to the GPIO connection from the mcp23s17
		/// </summary>
		/// <returns>0 on success</returns>
		[DllImport ("libmcp23s17.so")]
		internal static extern int mcp23s17_disable_interrupts ();

		/// <summary>
		/// Waits for an interrupt from the mcp23s17 or until timeout is reached
		/// <remarks>
		/// This method does NOT reset the interrupt - which is
		/// done automatically for you by reading the input state
		/// register.  Calling this method twice in a row without
		/// reading the input register will cause it to always wait
		/// for your timeout value, regardless of button presses.
		/// To avoid this, read the input register after every call
		/// to this method.</remarks></summary>
		/// <returns>The number of file descriptors ready for the 
		/// requested I/O, 0 if no file descriptor became ready
		/// during the requested timeout milliseconds, or -1 on error</returns>
		/// <param name="timeout">Maximum ms to wait for input, -1 for forever</param>
		[DllImport ("libmcp23s17.so")]
		internal static extern int mcp23s17_wait_for_interrupt (int timeout);
	}
}

