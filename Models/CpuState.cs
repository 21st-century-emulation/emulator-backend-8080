using System.Text.Json.Serialization;

namespace emulator_backend_8080.Models
{
    public struct CpuState
    {
        [JsonInclude]
        public CpuFlags Flags;

        [JsonInclude]
        public byte A;
        [JsonInclude]
        public byte B;
        [JsonInclude]
        public byte C;
        [JsonInclude]
        public byte D;
        [JsonInclude]
        public byte E;
        [JsonInclude]
        public byte H;
        [JsonInclude]
        public byte L;
        [JsonInclude]
        public ushort StackPointer;
        [JsonInclude]
        public ushort ProgramCounter;
        [JsonInclude]
        public ulong Cycles;
        [JsonInclude]
        public bool InterruptsEnabled;

        public override string ToString()
        {
            return $"PC:{ProgramCounter:X4} A:{A:X2} B:{B:X2} C:{C:X2} D:{D:X2} E:{E:X2} H:{H:X2} L:{L:X2} SP:{StackPointer:X4} S:{Flags.Sign} Z:{Flags.Zero} A:{Flags.AuxCarry} P:{Flags.Parity} C:{Flags.Carry} Cycles:{Cycles}";
        }
    }
}
