using System;
using System.Xml;
//XML 파싱의 예제
namespace PacketGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader r = XmlReader.Create("PDL.xml", settings))
            {
                r.MoveToContent();

                while (r.Read())
                {
                    
                    if (r.Depth == 1&&r.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(r);
                    }

                    Console.WriteLine(r.Name + " " + r["name"]);
                }
            }
        }
        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)
                return;
            if (r.Name.ToLower() != "packet")
            {
                Console.WriteLine("Invalid packet node");
                return;
            }

            string packetName = r["name"];
            if (string.IsNullOrEmpty(packetName) )
            {
                Console.WriteLine("Packet without name");
                return;
            }

            ParseMembers(r);
        }
        public static void ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            //깊이값+1하여 원하는 element만 가져오기 위해
            int depth = r.Depth +1;
            while (r.Read())
            {
                if (r.Depth != depth)
                {
                    break;
                }

                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return;
                }
                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "bool":
                        break;
                    case "byte":
                        break;
                    case "short":
                        break;
                    case "ushort":
                        break;
                    case "int":
                        break;
                    case "long":
                        break;
                    case "float":
                        break;
                    case "double":
                        break;
                    case "string":
                        break;
                    case "list":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}