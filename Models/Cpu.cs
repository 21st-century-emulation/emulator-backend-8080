using System.Text.Json.Serialization;

namespace emulator_backend_8080.Models
{
    public struct Cpu
    {
        [JsonInclude]
        public byte Opcode;

        [JsonInclude]
        public string Id;

        [JsonInclude]
        public CpuState State;
    }
}
