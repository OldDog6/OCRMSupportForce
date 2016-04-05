using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;


namespace OCRMSupportForce.Supporting
{
    public static class StringHandling
    {
        public static String Truncate(String value, int maxlen)
        {
            if (String.IsNullOrEmpty(value)) return String.Empty;
            return value.Length <= maxlen ? value : value.Substring(0, maxlen);
        }

        public static String StringOrEmpty(String value)
        {
            if (value == null)
                return String.Empty;
            else
                return value;
        }

        public static String SafeGetString(this MySqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            else
                return String.Empty;
        }

        public static String FirstCapThenlower(String value)
        {
            String rval = String.Empty;

            if ((value == null) || (value == String.Empty))
                return rval;

            rval = value.First().ToString().ToUpper() + String.Join("", value.ToLower().Skip(1));

            // Place a space betweeen number followed by a letter
            rval = Regex.Replace(rval, @"(?<=\d)(?=\p{L})", " ");


            // Capitalize Letter followed by period
            rval = Regex.Replace(rval,@"(?<=(^|[.;:])\s*)[a-z]",(match) => { return match.Value.ToUpper(); });

            return rval;
        }
    
        public static String SanitizeAddressField(String value)
        {
            // strip garbage
            String rval = String.Empty;
            String tmp = Regex.Replace(value, "[^0-9a-zA-Z#]+' '", "");

            // if text, Capitalize first letter, the rest lower case
            // if period, Capitialize one char before

            string[] address = Regex.Split(tmp, " ");

            for (int i = 0; i < address.Count(); i++ )
            {
                rval = rval + StringHandling.FirstCapThenlower(address[i])+' ';
            }

            return rval;
        }
    }

    public class SplitName
    {
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String MiddleName { get; set; }

        public SplitName(string inputStr, string splitChar)
        {
            FirstName  = String.Empty;
            LastName   = String.Empty;
            MiddleName = String.Empty;

            string[] names = Regex.Split(inputStr, splitChar);

            if (names.Length == 1)
            {
                FirstName = String.Empty;
                LastName = names[0];
            }
            else if (names.Length == 2)
            {
                FirstName = names[0];
                LastName = names[names.Count()-1];
            }
            else if (names.Length > 2)
            {
                FirstName  = names[0];
                LastName   = names[names.Count() - 1];
                for (int i=1; i < names.Count();i++)
                {
                    MiddleName = MiddleName + names[i];
                }
            }
        }
    }

    public class USStates
    {
        private Dictionary<String, String> states;

        public USStates()
        {
            states = new Dictionary<string, string>();
 
            states.Add("AL", "alabama");
            states.Add("AK", "alaska");
            states.Add("AZ", "arizona");
            states.Add("AR", "arkansas");
            states.Add("CA", "california");
            states.Add("CO", "colorado");
            states.Add("CT", "connecticut");
            states.Add("DE", "delaware");
            states.Add("DC", "district of Columbia");
            states.Add("FL", "florida");
            states.Add("GA", "georgia");
            states.Add("HI", "hawaii");
            states.Add("ID", "idaho");
            states.Add("IL", "illinois");
            states.Add("IN", "indiana");
            states.Add("IA", "iowa");
            states.Add("KS", "kansas");
            states.Add("KY", "kentucky");
            states.Add("LA", "louisiana");
            states.Add("ME", "maine");
            states.Add("MD", "maryland");
            states.Add("MA", "massachusetts");
            states.Add("MI", "michigan");
            states.Add("MN", "minnesota");
            states.Add("MS", "mississippi");
            states.Add("MO", "missouri");
            states.Add("MT", "montana");
            states.Add("NE", "nebraska");
            states.Add("NV", "nevada");
            states.Add("NH", "new hampshire");
            states.Add("NJ", "new jersey");
            states.Add("NM", "new mexico");
            states.Add("NY", "new york");
            states.Add("NC", "north carolina");
            states.Add("ND", "north dakota");
            states.Add("OH", "ohio");
            states.Add("OK", "oklahoma");
            states.Add("OR", "oregon");
            states.Add("PA", "pennsylvania");
            states.Add("RI", "rhode Island");
            states.Add("SC", "south carolina");
            states.Add("SD", "south dakota");
            states.Add("TN", "tennessee");
            states.Add("TX", "texas");
            states.Add("UT", "utah");
            states.Add("VT", "vermont");
            states.Add("VA", "virginia");
            states.Add("WA", "washington");
            states.Add("WV", "west virginia");
            states.Add("WI", "wisconsin");
            states.Add("WY", "wyoming");
        }
        
        public bool IsValidState(String value)
        {
            if (value.TrimEnd(' ').Length == 2)
            {
                return states.ContainsKey(value.TrimEnd(' ').ToUpper());    
            }
            else
                if (value.Length > 2)
                {
                    return states.ContainsValue(value.TrimEnd(' ').ToLower());
                }
                else
                    return false;
        }

        public String StateAsAbbr(String value)
        {
            if ((value.TrimEnd(' ').Length == 2) && IsValidState(value))
            {
                return value.TrimEnd(' ').ToUpper();
            }            
            else
            {
                var myValue = states.FirstOrDefault(x => x.Value == value.TrimEnd(' ').ToLower()).Key;
                if (myValue != null)
                    return myValue;
                else
                    return String.Empty;
            }
        }
    }

}
