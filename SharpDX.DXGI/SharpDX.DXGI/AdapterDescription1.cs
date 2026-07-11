using System;

namespace SharpDX.DXGI;

public struct AdapterDescription1
{
	internal struct __Native
	{
		public char Description;

		private char __Description1;

		private char __Description2;

		private char __Description3;

		private char __Description4;

		private char __Description5;

		private char __Description6;

		private char __Description7;

		private char __Description8;

		private char __Description9;

		private char __Description10;

		private char __Description11;

		private char __Description12;

		private char __Description13;

		private char __Description14;

		private char __Description15;

		private char __Description16;

		private char __Description17;

		private char __Description18;

		private char __Description19;

		private char __Description20;

		private char __Description21;

		private char __Description22;

		private char __Description23;

		private char __Description24;

		private char __Description25;

		private char __Description26;

		private char __Description27;

		private char __Description28;

		private char __Description29;

		private char __Description30;

		private char __Description31;

		private char __Description32;

		private char __Description33;

		private char __Description34;

		private char __Description35;

		private char __Description36;

		private char __Description37;

		private char __Description38;

		private char __Description39;

		private char __Description40;

		private char __Description41;

		private char __Description42;

		private char __Description43;

		private char __Description44;

		private char __Description45;

		private char __Description46;

		private char __Description47;

		private char __Description48;

		private char __Description49;

		private char __Description50;

		private char __Description51;

		private char __Description52;

		private char __Description53;

		private char __Description54;

		private char __Description55;

		private char __Description56;

		private char __Description57;

		private char __Description58;

		private char __Description59;

		private char __Description60;

		private char __Description61;

		private char __Description62;

		private char __Description63;

		private char __Description64;

		private char __Description65;

		private char __Description66;

		private char __Description67;

		private char __Description68;

		private char __Description69;

		private char __Description70;

		private char __Description71;

		private char __Description72;

		private char __Description73;

		private char __Description74;

		private char __Description75;

		private char __Description76;

		private char __Description77;

		private char __Description78;

		private char __Description79;

		private char __Description80;

		private char __Description81;

		private char __Description82;

		private char __Description83;

		private char __Description84;

		private char __Description85;

		private char __Description86;

		private char __Description87;

		private char __Description88;

		private char __Description89;

		private char __Description90;

		private char __Description91;

		private char __Description92;

		private char __Description93;

		private char __Description94;

		private char __Description95;

		private char __Description96;

		private char __Description97;

		private char __Description98;

		private char __Description99;

		private char __Description100;

		private char __Description101;

		private char __Description102;

		private char __Description103;

		private char __Description104;

		private char __Description105;

		private char __Description106;

		private char __Description107;

		private char __Description108;

		private char __Description109;

		private char __Description110;

		private char __Description111;

		private char __Description112;

		private char __Description113;

		private char __Description114;

		private char __Description115;

		private char __Description116;

		private char __Description117;

		private char __Description118;

		private char __Description119;

		private char __Description120;

		private char __Description121;

		private char __Description122;

		private char __Description123;

		private char __Description124;

		private char __Description125;

		private char __Description126;

		private char __Description127;

		public int VendorId;

		public int DeviceId;

		public int SubsystemId;

		public int Revision;

		public PointerSize DedicatedVideoMemory;

		public PointerSize DedicatedSystemMemory;

		public PointerSize SharedSystemMemory;

		public long Luid;

		public AdapterFlags Flags;

		internal void __MarshalFree()
		{
		}
	}

	public string Description;

	public int VendorId;

	public int DeviceId;

	public int SubsystemId;

	public int Revision;

	public PointerSize DedicatedVideoMemory;

	public PointerSize DedicatedSystemMemory;

	public PointerSize SharedSystemMemory;

	public long Luid;

	public AdapterFlags Flags;

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (char* description = &@ref.Description)
		{
			Description = Utilities.PtrToStringUni((IntPtr)description, 128);
		}
		VendorId = @ref.VendorId;
		DeviceId = @ref.DeviceId;
		SubsystemId = @ref.SubsystemId;
		Revision = @ref.Revision;
		DedicatedVideoMemory = @ref.DedicatedVideoMemory;
		DedicatedSystemMemory = @ref.DedicatedSystemMemory;
		SharedSystemMemory = @ref.SharedSystemMemory;
		Luid = @ref.Luid;
		Flags = @ref.Flags;
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (char* description = Description)
		{
			fixed (char* description2 = &@ref.Description)
			{
				Utilities.CopyMemory((IntPtr)description2, (IntPtr)description, Description.Length * 2);
			}
		}
		@ref.VendorId = VendorId;
		@ref.DeviceId = DeviceId;
		@ref.SubsystemId = SubsystemId;
		@ref.Revision = Revision;
		@ref.DedicatedVideoMemory = DedicatedVideoMemory;
		@ref.DedicatedSystemMemory = DedicatedSystemMemory;
		@ref.SharedSystemMemory = SharedSystemMemory;
		@ref.Luid = Luid;
		@ref.Flags = Flags;
	}
}
