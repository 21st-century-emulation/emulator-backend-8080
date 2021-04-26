namespace emulator_backend_8080.Models
{
    public struct CpuState
    {
        public CpuFlags Flags;

        public byte A;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        public ushort StackPointer;
        public ushort ProgramCounter;
        public ulong Cycles;
        public bool InterruptsEnabled;

        public override string ToString()
        {
            return $"PC:{ProgramCounter:X4} A:{A:X2} B:{B:X2} C:{C:X2} D:{D:X2} E:{E:X2} H:{H:X2} L:{L:X2} SP:{StackPointer:X4} S:{Flags.Sign} Z:{Flags.Zero} A:{Flags.AuxCarry} P:{Flags.Parity} C:{Flags.Carry} Cycles:{Cycles}";
        }
    }
}