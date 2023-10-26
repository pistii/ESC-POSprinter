using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Text;

namespace WebApplication1
{
    public class PrinterSetup
    {
        //Fő könyvtár: https://github.com/lukevp/ESC-POS-.NET/tree/master
        //Tesztelve:   https://github.com/roydejong/EscPosEmulator
        ImmediateNetworkPrinter printer;
        public PrinterSetup()
        {
            var hostnameOrIp = "192.168.0.16";
            var port = 1234; 
            printer = new ImmediateNetworkPrinter(new ImmediateNetworkPrinterSettings() { ConnectionString = $"{hostnameOrIp}:{port}" });
        }

        public async Task CreateNew(List<Items> items)
        {
            var e = new EPSON();
            var generatedCommands = new List<byte>(); 
            foreach (var item in items)
            {
                // Formázzuk az item nevét balra igazított szöveggé 30-as jobb oldali térközt használva
                string leftAlignedName = item.ItemName.PadRight(30);

                // Formázzuk az árat jobbra igazított szöveggé
                string rightAlignedPrice = item.Price.ToString().PadLeft(10); 

                // Kiírjuk az item nevét annyiszor, amennyi a mennyiség
                for (int i = 0; i < item.Quantity; i++)
                {
                    // Létrehozzuk az ESC/POS parancsokat az item nevének és árának kiírására
                    var command = ByteSplicer.Combine(
                        e.LeftAlign(), 
                        e.PrintLine(leftAlignedName), 
                        e.RightAlign(),
                        e.PrintLine(rightAlignedPrice)
                    );
                    generatedCommands.AddRange(command);
                }
                await SendToPrinter(generatedCommands);
            }
        }

        public async Task SendToPrinter(List<byte> commands)
        {
            var gc = commands.ToArray();
            await printer.WriteAsync( //WriteAsync az async művelet a hálózati használat miatt
                ByteSplicer.Combine(gc));
        }
    }
}
