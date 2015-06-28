namespace PiFaceCAD
{
	internal static class DataPiFaceCAD
	{
		internal static class Delay
		{
			public static int DELAY_PULSE_NS = 1;
			public static int DELAY_SETTLE_NS = 1;
			public static int DELAY_CLEAR_NS = 3;
			public static int DELAY_SETUP_0_NS = 15;
			public static int DELAY_SETUP_1_NS = 5;
			public static int DELAY_SETUP_2_NS = 1;
		}

		internal static class Pin
		{
			public static uint PIN_D4 = 0;
			public static uint PIN_D5 = 1;
			public static uint PIN_D6 = 2;
			public static uint PIN_D7 = 3;
			public static uint PIN_ENABLE = 4;
			public static uint PIN_RW = 5;
			public static uint PIN_RS = 6;
			public static uint PIN_BACKLIGHT = 7;
		}

		internal static class Command
		{
			public static uint LCD_CLEARDISPLAY = 0x01;
			public static uint LCD_RETURNHOME = 0x02;
			public static uint LCD_ENTRYMODESET = 0x04;
			public static uint LCD_DISPLAYCONTROL = 0x08;
			public static uint LCD_CURSORSHIFT = 0x10;
			public static uint LCD_FUNCTIONSET = 0x20;
			public static uint LCD_SETCGRAMADDR = 0x40;
			public static uint LCD_SETDDRAMADDR = 0x80;
			public static uint LCD_NEWLINE = 0xC0;
		}

		internal static class Flag
		{
			public static uint LCD_ENTRYRIGHT = 0x00;
			public static uint LCD_ENTRYLEFT = 0x02;
			public static uint LCD_ENTRYSHIFTINCREMENT = 0x01;
			public static uint LCD_ENTRYSHIFTDECREMENT = 0x00;
			public static uint LCD_DISPLAYON = 0x04;
			public static uint LCD_DISPLAYOFF = 0x00;
			public static uint LCD_CURSORON = 0x02;
			public static uint LCD_CURSOROFF = 0x00;
			public static uint LCD_BLINKON = 0x01;
			public static uint LCD_BLINKOFF = 0x00;
			public static uint LCD_DISPLAYMOVE = 0x08;
			public static uint LCD_CURSORMOVE = 0x00;
			public static uint LCD_MOVERIGHT = 0x04;
			public static uint LCD_MOVELEFT = 0x00;
			public static uint LCD_8BITMODE = 0x10;
			public static uint LCD_4BITMODE = 0x00;
			public static uint LCD_2LINE = 0x08;
			public static uint LCD_1LINE = 0x00;
			public static uint LCD_5X10DOTS = 0x04;
			public static uint LCD_5X8DOTS = 0x00;
		}

		public static uint LCD_MAX_LINES = 2;
		public static uint LCD_WIDTH = 16;
		public static uint LCD_RAM_WIDTH = 80;
		public static uint[] ROW_OFFSETS = { 0, 0x40 };
	}
}

