using System;
using System.IO;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static public string genPackets;
        public static ushort packetId;
        static public string packetEnums;

        public static string clientRegister;
        public static string serverRegister;

        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
                return;

            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("invalid node packet");
                return;
            }

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("packet without name");
                return;
            }

            Tuple<string, string, string> t = ParseMember(r);
            genPackets += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3);
            packetEnums += string.Format(PacketFormat.enumFormat, packetName, ++packetId) + "\n\t";

            if (packetName.StartsWith("S_") || packetName.StartsWith("s_"))
                clientRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
            else
            {
                serverRegister += string.Format(PacketFormat.managerRegisterFormat, packetName) + Environment.NewLine;
            }
        }

        //{1} 멤버 변수
        //{2} 멤버 변수 Read
        //{3} 멤버 변수 Write
        public static Tuple<string, string, string> ParseMember(XmlReader r)
        {
            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1;
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;

                string member = r["name"];
                if (string.IsNullOrEmpty(member))
                {
                    Console.WriteLine("member without name");
                    return null;
                }

                if (string.IsNullOrEmpty(memberCode) == false)
                    memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) == false)
                    writeCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) == false)
                    readCode += Environment.NewLine;

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, member);
                        readCode += string.Format(PacketFormat.readByteFormat, member, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, member, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, member);
                        readCode += string.Format(PacketFormat.readFormat, member, ToMemberType(memberType),
                            memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, member, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, member);
                        readCode += string.Format(PacketFormat.readStringFormat, member);
                        writeCode += string.Format(PacketFormat.writeStringFormat, member, memberType);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                }
            }

            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");
            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if (string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("list without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMember(r);

            //{0} 리스트 이름-대문자
            //{1} 리스트 이름-소문자
            //{2} 멤버 변수
            //{3} 멤버 변수 Read
            //{4} 멤버 변수 Write
            string memberCode = string.Format(PacketFormat.memberListFormat, FirstCharToUpper(listName),
                FirstCharToLower(listName), t.Item1, t.Item2, t.Item3);
            string readCode = string.Format(PacketFormat.readListFormat, FirstCharToUpper(listName),
                FirstCharToLower(listName));
            string writeCode = string.Format(PacketFormat.writeListFormat, FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }

        public static string ToMemberType(string memberType)
        {
            string ret = "";
            switch (memberType)
            {
                case "bool":
                    ret = "ToBoolean";
                    break;
                case "short":
                    ret = "ToInt16";
                    break;
                case "ushort":
                    ret = "ToUInt16";
                    break;
                case "int":
                    ret = "ToInt32";
                    break;
                case "long":
                    ret = "ToInt64";
                    break;
                case "float":
                    ret = "ToSingle";
                    break;
                case "double":
                    return "ToDouble";
            }

            return ret;
        }

        static void Main(string[] args)
        {
            string pdlPath = "../PDL.xml";

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            if (args.Length > 0)
                pdlPath = args[0];


            using (XmlReader r = XmlReader.Create(pdlPath, settings))
            {
                r.MoveToContent();
                while (r.Read())
                {
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                }

                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", fileText);
                string clientText = string.Format(PacketFormat.managerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientText);
                string serverText = string.Format(PacketFormat.managerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverText);
            }
        }
    }
}