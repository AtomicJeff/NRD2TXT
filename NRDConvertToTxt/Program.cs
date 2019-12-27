using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NinjaTrader.Cbi;
//using NinjaTrader.Data;
using System.Data.SqlServerCe;
using System.IO;

namespace NRDConvertToTxt
{
    class Program
    {
        private static string product = "ZN 12-19";
        private static List<MI> lstMaster = new List<MI>();
        static void Main(string[] args)
        {
            
            SqlCeConnection connection = new SqlCeConnection("data source=" + (Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\NinjaTrader 8\\db\\NinjaTrader.sdf"));
            connection.Open();
            SqlCeDataReader sqlCeDataReader = new SqlCeCommand("SELECT * FROM MasterInstruments ORDER BY Name", connection).ExecuteReader();
            while (sqlCeDataReader.Read())
            {
                
                lstMaster.Add(new MI()
                {
                    Name = sqlCeDataReader[5].ToString(),
                    TickSize = Convert.ToDouble(sqlCeDataReader[8]),
                    Type = (InstrumentType)sqlCeDataReader[3]
                });
            }
            sqlCeDataReader.Close();

            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + $"\\NinjaTrader 8\\db\\replay\\{product}\\";
            var files = Directory.EnumerateFiles(md);
            

            foreach (var file in files)
            {

                Console.WriteLine($"Converting file {file} from .nrd to .txt");

                var name = Path.GetFileName(file);
                string[] strArray = product.Split(' ');
                string MIname = strArray[0];

                MI mi = lstMaster.First<MI>((Func<MI, bool>)(i => i.Name == MIname));
                MasterInstrument masterInstrument = new MasterInstrument();
                masterInstrument.Name = mi.Name;
                masterInstrument.InstrumentType = mi.Type;
                masterInstrument.TickSize = mi.TickSize;

                DateTime dateTime1 = new DateTime(2099, 12, 12);
                if (strArray.Length == 2)
                {
                    int int16 = (int)Convert.ToInt16(strArray[1].Split('-')[0]);
                    dateTime1 = new DateTime(2000 + (int)Convert.ToInt16(strArray[1].Split('-')[1]), int16, 15);
                }

                Instrument instrument = new Instrument()
                {
                    Exchange = Exchange.Default,
                    Expiry = dateTime1,
                    MasterInstrument = masterInstrument
                };

                DateTime dateTime2 = new DateTime((int)Convert.ToInt16(name.Substring(0, 4)), (int)Convert.ToInt16(name.Substring(4, 2)), (int)Convert.ToInt16(name.Substring(6, 2)));

                if (!Directory.Exists("CONVERTED\\" + product + "\\"))
                    Directory.CreateDirectory("CONVERTED\\" + product + "\\");

                NinjaTrader.Data.MarketReplay.DumpMarketDepth(instrument, dateTime2, dateTime2.AddDays(1), "CONVERTED\\" + product + "\\" + name.Substring(0, 8) + ".txt");


            }





        }

        

        public class MI
        {
            public string Name { get; set; }

            public double TickSize { get; set; }

            public InstrumentType Type { get; set; }
        }
    }
}
