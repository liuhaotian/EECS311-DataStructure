using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Linq;

namespace PathPlanner
{
    public static class Spreadsheet
    {
        public static object[][] Read(string path, char delimiter)
        {
            using (TextReader r = File.OpenText(path))
            {
                StringBuilder b = new StringBuilder();
                List<object[]> allRows = new List<object[]>();
                List<object> currentRow = new List<object>();

                int peek = r.Peek();
                while (peek >= 0)
                {
                    if (peek == delimiter)
                    {
                        r.Read(); // Skip over delimiter
                        currentRow.Add(ReadItem(r, delimiter, b));
                    }
                    else if (peek == '\r' || peek == '\n')
                    {
                        // end of line - swallow cr and/or lf
                        r.Read();
                        if (peek == '\r')
                        {
                            // Swallow LF if CRLF
                            peek = r.Peek();
                            if (peek == '\n')
                                r.Read();
                        }
                        allRows.Add(currentRow.ToArray());
                        currentRow.Clear();
                    }
                    else
                        currentRow.Add(ReadItem(r, delimiter, b));
                    peek = r.Peek();
                }
                // End of file
                return allRows.ToArray();
            }
        }

        static string ReadItem(TextReader input, char delimiter, StringBuilder b)
        {
            bool quoted = false;
            b.Clear();
            int peek = (char)input.Peek();
            if (peek == delimiter)
                return "";
            if (peek == '\"')
            {
                quoted = true;
                input.Read();
            }
        getNextChar:
            peek = input.Peek();
            if (peek < 0)
                goto done;
            if (quoted && peek == '\"')
            {
                input.Read();  // Swallow quote
                if ((char)input.Peek() == '\"')
                {
                    // It was an escaped quote
                    input.Read();
                    b.Append('\"');
                    goto getNextChar;
                }
                else
                {
                    // It was the end of the item
// ReSharper disable RedundantJumpStatement
                    goto done;
// ReSharper restore RedundantJumpStatement
                }
            }
            else if (!quoted && (peek == delimiter || peek == '\r' || peek == '\n'))
// ReSharper disable RedundantJumpStatement
                goto done;
// ReSharper restore RedundantJumpStatement
            else
            {
                b.Append((char)peek);
                input.Read();
                goto getNextChar;
            }
            //System.Diagnostics.Debug.Assert(false, "Line should not be reachable.");
        done:
            return b.ToString();
        }

        public static object[][] ConvertAllNumbers(object[][] spreadsheet)
        {
            for (int i = 0; i < spreadsheet.Length; i++)
            {
                object[] row = spreadsheet[i];
                for (int j = 0; j < row.Length; j++)
                {
                    string s = row[j] as string;
                    double parsed;
                    if (s != null && double.TryParse(s, out parsed))
                        row[j] = parsed;
                }
            }
            return spreadsheet;
        }

        public static object[][] TrimWhitespace(object[][] spreadsheet)
        {
            for (int i = 0; i < spreadsheet.Length; i++)
            {
                object[] row = spreadsheet[i];
                for (int j = 0; j < row.Length; j++)
                {
                    string s = row[j] as string;
                    if (s != null)
                        row[j] = s.Trim();
                }
            }
            return spreadsheet;
        }

        public static void Write(IList rows, string path, char delimiter)
        {
            List<IList> data = new List<IList>(rows.Count);
            foreach (IList l in rows)
                data.Add(l);

            StringBuilder b = new StringBuilder();
            File.WriteAllLines(path, data.Select((l, i) => Format(l, delimiter, b)));
        }

        static string Format(IList items, char delimiter, StringBuilder b)
        {
            b.Clear();
            bool firstOne = true;
            foreach (var item in items)
            {
                if (!firstOne)
                    b.Append(delimiter);
                else
                    firstOne = false;
                if (item is string)
                {
                    b.Append('\"');
                    b.Append(((string)item).Replace("\"", "\"\""));
                    b.Append('\"');
                }
                else
                    b.Append(item);
            }
            return b.ToString();
        }
    }
}
