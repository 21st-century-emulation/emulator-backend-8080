using System;
using System.Collections.Generic;

namespace emulator_backend_8080.Models
{
    internal static class CpuStaticData
    {
        internal static int[] NumberOfBytesPerOpcode = new int[0x100]
        {
            1,3,1,1,1,1,2,1,1,1,1,1,1,1,2,1,
            1,3,1,1,1,1,2,1,1,1,1,1,1,1,2,1,
            1,3,3,1,1,1,2,1,1,1,3,1,1,1,2,1,
            1,3,3,1,1,1,2,1,1,1,3,1,1,1,2,1,
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
            1,1,3,3,3,1,2,1,1,1,3,3,3,3,2,1,
            1,1,3,2,3,1,2,1,1,1,3,2,3,3,2,1,
            1,1,3,1,3,1,2,1,1,1,3,1,3,3,2,1,
            1,1,3,1,3,1,2,1,1,1,3,1,3,3,2,1,
        };

        internal static Dictionary<Opcode, string> OpcodeUrl = new Dictionary<Opcode, string>
        {
            { Opcode.ACI, Environment.GetEnvironmentVariable("ACI") },
            { Opcode.ADC, Environment.GetEnvironmentVariable("ADC") },
            { Opcode.ADD, Environment.GetEnvironmentVariable("ADD") },
            { Opcode.ADI, Environment.GetEnvironmentVariable("ADI") },
            { Opcode.ANA, Environment.GetEnvironmentVariable("ANA") },
            { Opcode.ANI, Environment.GetEnvironmentVariable("ANI") },
            { Opcode.CALL, Environment.GetEnvironmentVariable("CALL") },
            { Opcode.CMA, Environment.GetEnvironmentVariable("CMA") },
            { Opcode.CMC, Environment.GetEnvironmentVariable("CMC") },
            { Opcode.CMP, Environment.GetEnvironmentVariable("CMP") },
            { Opcode.CPI, Environment.GetEnvironmentVariable("CPI") },
            { Opcode.DAA, Environment.GetEnvironmentVariable("DAA") },
            { Opcode.DAD, Environment.GetEnvironmentVariable("DAD") },
            { Opcode.DCR, Environment.GetEnvironmentVariable("DCR") },
            { Opcode.DCX, Environment.GetEnvironmentVariable("DCX") },
            { Opcode.DI, Environment.GetEnvironmentVariable("DI") },
            { Opcode.EI, Environment.GetEnvironmentVariable("EI") },
            { Opcode.HLT, Environment.GetEnvironmentVariable("HLT") },
            { Opcode.IN, Environment.GetEnvironmentVariable("IN") },
            { Opcode.INR, Environment.GetEnvironmentVariable("INR") },
            { Opcode.INX, Environment.GetEnvironmentVariable("INX") },
            { Opcode.JMP, Environment.GetEnvironmentVariable("JMP") },
            { Opcode.LDA, Environment.GetEnvironmentVariable("LDA") },
            { Opcode.LDAX, Environment.GetEnvironmentVariable("LDAX") },
            { Opcode.LHLD, Environment.GetEnvironmentVariable("LHLD") },
            { Opcode.LXI, Environment.GetEnvironmentVariable("LXI") },
            { Opcode.MOV, Environment.GetEnvironmentVariable("MOV") },
            { Opcode.MVI, Environment.GetEnvironmentVariable("MVI") },
            { Opcode.NOOP, Environment.GetEnvironmentVariable("NOOP") },
            { Opcode.ORA, Environment.GetEnvironmentVariable("ORA") },
            { Opcode.ORI, Environment.GetEnvironmentVariable("ORI") },
            { Opcode.OUT, Environment.GetEnvironmentVariable("OUT") },
            { Opcode.PCHL, Environment.GetEnvironmentVariable("PCHL") },
            { Opcode.POP, Environment.GetEnvironmentVariable("POP") },
            { Opcode.PUSH, Environment.GetEnvironmentVariable("PUSH") },
            { Opcode.RAL, Environment.GetEnvironmentVariable("RAL") },
            { Opcode.RAR, Environment.GetEnvironmentVariable("RAR") },
            { Opcode.RET, Environment.GetEnvironmentVariable("RET") },
            { Opcode.RLC, Environment.GetEnvironmentVariable("RLC") },
            { Opcode.RRC, Environment.GetEnvironmentVariable("RRC") },
            { Opcode.RST, Environment.GetEnvironmentVariable("RST") },
            { Opcode.SBB, Environment.GetEnvironmentVariable("SBB") },
            { Opcode.SBI, Environment.GetEnvironmentVariable("SBI") },
            { Opcode.SHLD, Environment.GetEnvironmentVariable("SHLD") },
            { Opcode.SPHL, Environment.GetEnvironmentVariable("SPHL") },
            { Opcode.STA, Environment.GetEnvironmentVariable("STA") },
            { Opcode.STAX, Environment.GetEnvironmentVariable("STAX") },
            { Opcode.STC, Environment.GetEnvironmentVariable("STC") },
            { Opcode.SUB, Environment.GetEnvironmentVariable("SUB") },
            { Opcode.SUI, Environment.GetEnvironmentVariable("SUI") },
            { Opcode.XCHG, Environment.GetEnvironmentVariable("XCHG") },
            { Opcode.XRA, Environment.GetEnvironmentVariable("XRA") },
            { Opcode.XRI, Environment.GetEnvironmentVariable("XRI") },
            { Opcode.XTHL, Environment.GetEnvironmentVariable("XTHL") },
        };

        internal static Opcode[] OpcodeName = new Opcode[0x100]
        {
            Opcode.NOOP,Opcode.LXI,Opcode.STAX,Opcode.INX,Opcode.INR,Opcode.DCR,Opcode.MVI,Opcode.RLC,Opcode.NOOP,Opcode.DAD,Opcode.LDAX,Opcode.DCX,Opcode.INR,Opcode.DCR,Opcode.MVI,Opcode.RRC,
            Opcode.NOOP,Opcode.LXI,Opcode.STAX,Opcode.INX,Opcode.INR,Opcode.DCR,Opcode.MVI,Opcode.RAL,Opcode.NOOP,Opcode.DAD,Opcode.LDAX,Opcode.DCX,Opcode.INR,Opcode.DCR,Opcode.MVI,Opcode.RAR,
            Opcode.NOOP,Opcode.LXI,Opcode.SHLD,Opcode.INX,Opcode.INR,Opcode.DCR,Opcode.MVI,Opcode.DAA,Opcode.NOOP,Opcode.DAD,Opcode.LHLD,Opcode.DCX,Opcode.INR,Opcode.DCR,Opcode.MVI,Opcode.CMA,
            Opcode.NOOP,Opcode.LXI,Opcode.STA,Opcode.INX,Opcode.INR,Opcode.DCR,Opcode.MVI,Opcode.STC,Opcode.NOOP,Opcode.DAD,Opcode.LDA,Opcode.DCX,Opcode.INR,Opcode.DCR,Opcode.MVI,Opcode.CMC,
            Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,
            Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,
            Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,
            Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.HLT,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,Opcode.MOV,
            Opcode.ADD,Opcode.ADD,Opcode.ADD,Opcode.ADD,Opcode.ADD,Opcode.ADD,Opcode.ADD,Opcode.ADD,Opcode.ADC,Opcode.ADC,Opcode.ADC,Opcode.ADC,Opcode.ADC,Opcode.ADC,Opcode.ADC,Opcode.ADC,
            Opcode.SUB,Opcode.SUB,Opcode.SUB,Opcode.SUB,Opcode.SUB,Opcode.SUB,Opcode.SUB,Opcode.SUB,Opcode.SBB,Opcode.SBB,Opcode.SBB,Opcode.SBB,Opcode.SBB,Opcode.SBB,Opcode.SBB,Opcode.SBB,
            Opcode.ANA,Opcode.ANA,Opcode.ANA,Opcode.ANA,Opcode.ANA,Opcode.ANA,Opcode.ANA,Opcode.ANA,Opcode.XRA,Opcode.XRA,Opcode.XRA,Opcode.XRA,Opcode.XRA,Opcode.XRA,Opcode.XRA,Opcode.XRA,
            Opcode.ORA,Opcode.ORA,Opcode.ORA,Opcode.ORA,Opcode.ORA,Opcode.ORA,Opcode.ORA,Opcode.ORA,Opcode.CMP,Opcode.CMP,Opcode.CMP,Opcode.CMP,Opcode.CMP,Opcode.CMP,Opcode.CMP,Opcode.CMP,
            Opcode.RET,Opcode.POP,Opcode.JMP,Opcode.JMP,Opcode.CALL,Opcode.PUSH,Opcode.ADI,Opcode.RST,Opcode.RET,Opcode.RET,Opcode.JMP,Opcode.JMP,Opcode.CALL,Opcode.CALL,Opcode.ACI,Opcode.RST,
            Opcode.RET,Opcode.POP,Opcode.JMP,Opcode.OUT,Opcode.CALL,Opcode.PUSH,Opcode.SUI,Opcode.RST,Opcode.RET,Opcode.RET,Opcode.JMP,Opcode.IN,Opcode.CALL,Opcode.CALL,Opcode.SBI,Opcode.RST,
            Opcode.RET,Opcode.POP,Opcode.JMP,Opcode.XTHL,Opcode.CALL,Opcode.PUSH,Opcode.ANI,Opcode.RST,Opcode.RET,Opcode.PCHL,Opcode.JMP,Opcode.XCHG,Opcode.CALL,Opcode.CALL,Opcode.XRI,Opcode.RST,
            Opcode.RET,Opcode.POP,Opcode.JMP,Opcode.DI,Opcode.CALL,Opcode.PUSH,Opcode.ORI,Opcode.RST,Opcode.RET,Opcode.SPHL,Opcode.JMP,Opcode.EI,Opcode.CALL,Opcode.CALL,Opcode.CPI,Opcode.RST
        };
    }
}