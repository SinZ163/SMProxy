using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LuaInterface;

namespace SMProxy
{
    static partial class Program
    {
        private static Lua ConfigureLua(PacketReader PacketReader, StreamWriter outputWriter)
        {
            Lua LuaInterpreter = new Lua();

            LuaInterpreter.RegisterFunction("readByte", PacketReader, typeof(PacketReader).GetMethods().Where(
                    m => m.Name == "ReadByte" && m.GetParameters().Length == 0
                ).First());
            LuaInterpreter.RegisterFunction("readShort", PacketReader, typeof(PacketReader).GetMethods().Where(
                    m => m.Name == "ReadShort" && m.GetParameters().Length == 0
                ).First());
            LuaInterpreter.RegisterFunction("readInt", PacketReader, typeof(PacketReader).GetMethods().Where(
                    m => m.Name == "ReadInt" && m.GetParameters().Length == 0
                ).First());
            LuaInterpreter.RegisterFunction("readLong", PacketReader, typeof(PacketReader).GetMethods().Where(
                    m => m.Name == "ReadLong" && m.GetParameters().Length == 0
                ).First());
            LuaInterpreter.RegisterFunction("readString", PacketReader, typeof(PacketReader).GetMethods().Where(
                    m => m.Name == "ReadString" && m.GetParameters().Length == 0
                ).First());
            LuaInterpreter.RegisterFunction("readSlot", PacketReader, typeof(PacketReader).GetMethods().Where(
                    m => m.Name == "ReadSlot" && m.GetParameters().Length == 0
                ).First());
            LuaInterpreter.RegisterFunction("readMob", PacketReader, typeof(PacketReader).GetMethods().Where(
                    m => m.Name == "ReadMobMetadata" && m.GetParameters().Length == 0
                ).First());
            LuaInterpreter.RegisterFunction("readBytes", PacketReader, typeof(PacketReader).GetMethod("ReadBytes"));
            LuaInterpreter.RegisterFunction("readBoolean", PacketReader, typeof(PacketReader).GetMethods().Where(
                    m => m.Name == "ReadBoolean" && m.GetParameters().Length == 0
                ).First());

            return LuaInterpreter;
        }
    }
}
