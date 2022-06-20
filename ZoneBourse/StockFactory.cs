namespace TVScreener.ZoneBourse
{
    public static class StockFactory
    {
        public static Stock CreateStock(string code, string name, string zbExchange, string currency)
        {
            return new Stock(code, name, GetExchange(zbExchange ?? "", currency));
        }

        private static string GetExchange(string zoneBourseExchange, string currency)
        {
            switch (zoneBourseExchange)
            {
                // North america
                case "Nasdaq":
                    return "NASDAQ";
                case "Nyse":
                    return "NYSE";
                case "Toronto Stock Exchange":
                    return "TSX";
                case "Bourse De Toronto":
                case "Bourse de Toronto":
                    return "TSXV";
                case "Bolsas Y Mercados Espanol…":
                    return "BME";
                case "Mexican Stock Exchange":
                    return "BMV";
                case "CANADIAN NATIONAL STOCK EXCHANGE":
                case "Canadian National Stock E&#133;":
                    return "CSE";
                case "Neo Exchange - Neo-l (mar&#133;":
                    return "NEO";
                case "OTC":
                case "Otc Markets":
                    return "OTC";

                // Europe
                case "Nasdaq Copenhagen":
                    return "OMXCOP";
                case "Nasdaq Helsinki":
                    return "OMXHEX";
                case "Nasdaq Iceland":
                    return "OMXICE";
                case "Nasdaq Riga":
                    return "OMXRSE";
                case "Nasdaq Stockholm":
                    return "OMXSTO";
                case "Nasdaq Tallinn":
                    return "OMXTSE";
                case "Euronext Amsterdam":
                case "Euronext Bruxelles":
                case "Euronext Lisbonne":
                case "Euronext Paris":
                case "Euronext Oslo":
                    return "EURONEXT";
                case "Xetra":
                    return "XETR";
                case "London Stock Exchange":
                    return currency == "GBX" ? "LSE" : "LSIN";
                case "Oslo Bors":
                    return "OSL";
                case "Belgrade Stock Exchange":
                    return "BELEX";
                case "Moscow Micex - Rts":
                    return "MOEX";
                case "Borsa Italiana":
                    return "MIL";
                case "Athens Stock Exchange":
                    return "ATHEX";
                case "Warsaw Stock Exchange":
                    return "GPW";
                case "Bucharest Stock Exchange":
                    return "BVB";
                case "Bolsas Y Mercados Espanol&#133;":
                    return "BME";
                case "Swiss Exchange":
                    return "SIX";
                case "Luxembourg Stock Exchange":
                    return "LUXSE";
                case "Nordic Growth Market":
                    return "NGM";
                case "Wiener Boerse":       // Check this!
                case "Deutsche Boerse Ag":  // Check this!
                    return "FWB";
                case "Boerse Muenchen":
                    return "MUN";
                case "Boerse Duesseldorf":
                    return "DUS";
                case "Hanseatische Wertpapierbo&#133;":
                    return "HAM";
                case "Börse Stuttgart":
                    return "SWB";

                // Unavailables
                case "Prague Stock Exchange":
                case "Ljubljana Stock Exchange":
                case "Zagreb Stock Exchange":
                case "Budapest Stock Exchange":
                case "Pfts Stock Exchange":
                case "Bulgaria Stock Exchange":
                case "Irish Stock Exchange":
                case "Norwegian Over The Counte&#133;":
                case "Berne Stock Exchange":
                case "":
                    return "";

                default:
                    return "";
            }
        }
    }
}
