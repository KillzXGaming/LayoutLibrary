
using LayoutLibrary.Cafe;
using LayoutLibrary.Files;
using LayoutLibrary.XmlConverter;

namespace MetaphorMessageConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            args = new string[] { "mch_ch_mii_02.bclyt" };

            if (args.Length == 0 || args.Contains("-h"))
            {
                Console.WriteLine($"Tool by KillzXGaming");
                Console.WriteLine($"Usage:");

                Console.WriteLine($" Layout:");
                Console.WriteLine($"  LayoutLibrary.CLI layout.bflyt (converts to xml)");
                Console.WriteLine($"  LayoutLibrary.CLI layout.bflyt.xml (converts back to bflyt)");
                Console.WriteLine($"");
                Console.WriteLine($" Animations:");
                Console.WriteLine($"  LayoutLibrary.CLI anim.bflan (converts to xml)");
                Console.WriteLine($"  LayoutLibrary.CLI anim.bflan.xml (converts back to bflan)");
                return;
            }

            foreach (var arg in args)
            {
                if (File.Exists(arg))
                {
                    var stream = File.OpenRead(arg);
                    if (BflytFile.Identity(stream))
                    {
                        BflytFile bflyt = new BflytFile(stream);
                        File.WriteAllText($"{arg}" + ".xml", XMLayoutConverter.ToXml(bflyt));
                    }
                    if (BflanFile.Identity(stream))
                    {
                        BflanFile bflan = new BflanFile(stream);
                        File.WriteAllText($"{arg}" + ".xml", XMLAnimationConverter.ToXml(bflan));
                    }

                    //todo check xml what layout type rather than extension
                    if (arg.EndsWith("lyt.xml"))
                    {
                        BflytFile bflyt = XMLayoutConverter.FromXml(File.ReadAllText(arg));
                        bflyt.Save(arg.Replace(".xml", ""));
                    }
                    if (arg.EndsWith("lan.xml"))
                    {
                        BflanFile bflan = XMLAnimationConverter.FromXml(File.ReadAllText(arg));
                        bflan.Save(arg.Replace(".xml", ""));
                    }
                }
            }
        }
    }
}