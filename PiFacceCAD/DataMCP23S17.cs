namespace PiFaceCAD
{
	internal static class DataMCP23S17
	{
		public static uint WRITE_CMD = 0;
		public static uint READ_CMD = 1;

		internal static class RegisterAddress
		{
			/// <summary>
			/// I/O direction A
			/// </summary>
			public static uint IODIRA = 0x00;

			/// <summary>
			/// I/O direction B
			/// </summary>
			public static uint IODIRB = 0x01;

			/// <summary>
			/// I/O polarity A
			/// </summary>
			public static uint IPOLA = 0x02;

			/// <summary>
			/// I/O polarity B
			/// </summary>
			public static uint IPOLB = 0x03;

			/// <summary>
			/// Interupt enable A
			/// </summary>
			public static uint GPINTENA = 0x04;

			/// <summary>
			/// Interupt enable B
			/// </summary>
			public static uint GPINTENB = 0x05;

			/// <summary>
			/// Register default value A (interupts)
			/// </summary>
			public static uint DEFVALA = 0x06;

			/// <summary>
			/// Register defualt value B (interupts)
			/// </summary>
			public static uint DEFVALB = 0x07;

			/// <summary>
			/// Interupt control A
			/// </summary>
			public static uint INTCONA = 0x08;

			/// <summary>
			/// Interupt control B
			/// </summary>
			public static uint INTCONB = 0x09;

			/// <summary>
			/// I/O config
			/// </summary>
			public static uint IOCON = 0x0A;

			/// <summary>
			/// I/O config
			/// </summary>
			public static uint IOCONB = 0x0B;

			/// <summary>
			/// Port A pullups
			/// </summary>
			public static uint GPPUA = 0x0C;

			/// <summary>
			/// Port B pullups
			/// </summary>
			public static uint GPPUB = 0x0D;

			/// <summary>
			/// Interupt flag A
			/// <remarks>Where the interupt came from</remarks>
			/// </summary>
			public static uint INTFA = 0x0E;

			/// <summary>
			/// Interupt flag B
			/// </summary>
			public static uint INTFB = 0x0F;

			/// <summary>
			/// Interupt capture A
			/// <remarks>Value at interupt is saved here</remarks>
			/// </summary>
			public static uint INTCAPA = 0x10;

			/// <summary>
			/// Interupt capture B
			/// </summary>
			public static uint INTCAPB = 0x11;

			/// <summary>
			/// Port A
			/// </summary>
			public static uint GPIOA = 0x12;

			/// <summary>
			/// Port B
			/// </summary>
			public static uint GPIOB = 0x13;

			/// <summary>
			/// Output latch A
			/// </summary>
			public static uint OLATA = 0x14;

			/// <summary>
			/// Output latch B
			/// </summary>
			public static uint OLATB = 0x15;
		}

		internal static class IO_Config
		{
			internal static class AddressingMode
			{
				public static uint BANK_OFF = 0x00;
				public static uint BANK_ON = 0x80;
			}

			internal static class InteruptMirror
			{
				public static uint INT_MIRROR_ON = 0x40;
				public static uint INT_MIRROR_OFF = 0x00;
			}

			internal static class IncrementAddressPointer
			{
				public static uint SEQOP_OFF = 0x20;
				public static uint SEQOP_ON = 0x00;
			}

			internal static class SlewRate
			{
				public static uint DISSLW_ON = 0x10;
				public static uint DISSLW_OFF = 0x00;
			}

			internal static class HardwareAddressing
			{
				public static uint HAEN_ON = 0x08;
				public static uint HAEN_OFF = 0x00;
			}

			internal static class InteruptOpenDrain
			{
				public static uint ODR_ON = 0x04;
				public static uint ODR_OFF = 0x00;
			}

			internal static class InteruptPolarity
			{
				public static uint INTPOL_HIGH = 0x02;
				public static uint INTPOL_LOW = 0x00;
			}
		}

		public static uint GPIO_INTERRUPT_PIN = 25;
	}
}

