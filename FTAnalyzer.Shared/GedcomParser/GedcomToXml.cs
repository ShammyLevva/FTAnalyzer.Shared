using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using FTAnalyzer.Utilities;
using FTAnalyzer.Properties;

namespace FTAnalyzer
{
    class GedcomToXml
    {
        static readonly Encoding isoWesternEuropean = Encoding.GetEncoding(28591);
        static readonly Encoding ansiLatin1 = Encoding.GetEncoding(1252);

        public static XmlDocument Load(MemoryStream stream, IProgress<string> outputText)
        {
            var reader = new StreamReader(stream);
            if (FileHandling.Default.LoadWithFilters)
            {
                reader = FileHandling.Default.RetryFailedLines
                    ? new AnselInputStreamReader(CheckInvalidCR(stream))
                    : new AnselInputStreamReader(stream);
            }
            reader = FileHandling.Default.RetryFailedLines ? new StreamReader(CheckInvalidCR(stream)) : new StreamReader(stream);
            return Parse(reader, outputText);
        }

        public static XmlDocument LoadFile(string path, IProgress<string> outputText) { return LoadFile(path, ansiLatin1, outputText); }
        public static XmlDocument LoadFile(string path, Encoding encoding, IProgress<string> outputText)
        {
            StreamReader reader;
            if (FileHandling.Default.LoadWithFilters)
            {
                reader = FileHandling.Default.RetryFailedLines
                    ? new AnselInputStreamReader(CheckInvalidLineEnds(path))
                    : new AnselInputStreamReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            }
            reader = FileHandling.Default.RetryFailedLines
                ? new StreamReader(CheckInvalidLineEnds(path), encoding)
                : new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read), encoding);
            return Parse(reader, outputText);
        }

        static MemoryStream CheckInvalidLineEnds(string path)
        {
            FileStream infs = new FileStream(path, FileMode.Open, FileAccess.Read);
            return CheckInvalidCR(infs);
        }

        static MemoryStream CheckInvalidCR(Stream infs)
        {
            MemoryStream outfs = new MemoryStream();
            long streamLength = infs.Length;
            byte b = (byte)infs.ReadByte();
            while (infs.Position < streamLength)
            {
                if (b == 0x0d)
                {
                    b = (byte)infs.ReadByte();
                    if (b == 0x0a)
                    { // we have 0x0d 0x0a so write the 0x0d so that normal write works.
                        outfs.WriteByte(0x0d);
                    }
                }
                else
                {
                    outfs.WriteByte(b);
                }
                b = (byte)infs.ReadByte();
            }
            outfs.Position = 0;
            return outfs;
        }

        static MemoryStream CheckSpuriousOD(MemoryStream infs)
        {
            MemoryStream outfs = new MemoryStream();
            byte b = (byte)infs.ReadByte();
            long streamLength = infs.Length;
            while (infs.Position < streamLength)
            {
                while (b == 0x0d && infs.Position < streamLength)
                {
                    b = (byte)infs.ReadByte();
                    if (b == 0x0a)
                    { // we have 0x0d 0x0a so write out the 0x0d and the 0x0a will follow in the normal write.
                        outfs.WriteByte(0x0d);
                    } // otherwise we drop though and have ignored the 0x0d on its own
                }
                outfs.WriteByte(b);
                b = (byte)infs.ReadByte();
            }
            outfs.Position = 0;
            return outfs;
        }

        static XmlDocument Parse(StreamReader reader, IProgress<string> outputText)
        {
            long lineNr = 0;
            int badLineCount = 0;
//            int badLineMax = 30;

            string line, nextline, token1, token2;
            string level;
            int thislevel;
            int prevlevel = -1;
            string iden, tag, xref, value;
            int cpos1;
            Stack<string> stack = new Stack<string>();
            stack.Push("GED");
            XmlDocument document = new XmlDocument();
            XmlNode node = document.CreateElement("GED");
            document.AppendChild(node);

            try
            {
                line = reader.ReadLine();
                while (line != null)
                {
                    lineNr++;
                    nextline = reader.ReadLine();
                    if (FileHandling.Default.RetryFailedLines)
                    {
                        //need to check if nextline is valid if not line=line+nextline and nextline=reader.ReadLine();
                        while (nextline?.Length <= 1 || (nextline?.Length > 1 && (!char.IsNumber(nextline[0]) || !nextline[1].Equals(' '))))
                        {  // concat if next line not a number space combo
                            line = line + nextline;
                            lineNr++;
                            nextline = reader.ReadLine();
                        }
                    }
                    // parse the GEDCOM line into five fields: level, iden, tag, xref, valu
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        try
                        {
                            line = line.Replace('–', '-').Replace('—', '-').Replace("***Data is already there***", ""); // "data is already there" is some Ancestry anomaly
                            cpos1 = line.IndexOf(' ');
                            if (cpos1 < 0) throw new Exception("No space in line");

                            level = FirstWord(line);
                            thislevel = int.Parse(level);

                            // check the level number

                            if (thislevel > prevlevel && !(thislevel == prevlevel + 1))
                                throw new Exception("Level numbers must increase by 1");
                            if (thislevel < 0)
                                throw new Exception("Level number must not be negative");

                            line = Remainder(line);
                            token1 = FirstWord(line);
                            line = Remainder(line);

                            if (token1.StartsWith("@", StringComparison.Ordinal))
                            {
                                if (token1.Length == 1 || !token1.EndsWith("@", StringComparison.Ordinal))
                                    throw new Exception("Bad xref_id");

                                iden = token1.Substring(1, token1.Length - 2);
                                tag = FirstWord(line);
                                line = Remainder(line);
                            }
                            else
                            {
                                iden = "";
                                tag = token1;
                            }

                            xref = "";
                            if (line.StartsWith("@", StringComparison.Ordinal) && tag != "_HASHTAG" & tag != "NAME")
                            {
                                if (!token1.Equals("CONT") && !token1.Equals("CONC"))
                                {
                                    token2 = FirstWord(line);
                                    if (token2.Length == 1 || (!token2.EndsWith("@", StringComparison.Ordinal) && !token2.EndsWith("@,", StringComparison.Ordinal)))
                                        throw new Exception("Bad pointer value");
                                    xref = token2.EndsWith("@,", StringComparison.Ordinal)
                                        ? token2.Substring(1, token2.Length - 3)
                                        : token2.Substring(1, token2.Length - 2);
                                    line = Remainder(line);
                                }
                            }
                            if (token1.Equals("CONT") || token1.Equals("CONC"))
                            {
                                // check if nextline does not start with a number ie: could be a wrapped line, if so then concatenate
                                while (nextline != null && !nextline.Trim().StartsWithNumeric())
                                {
                                    line = line + "\n" + nextline.Trim();
                                    nextline = reader.ReadLine();
                                }
                            }

                            value = line;

                            // perform validation on the CHAR field (character code)
                            string valtrim = value.Trim();
                            if (tag.Equals("CHAR") &&
                                !(valtrim.Equals("ANSEL") || valtrim.Equals("ASCII") || valtrim.Equals("ANSI") || valtrim.Equals("UTF-8") || valtrim.Equals("UNICODE")))
                            {
                                outputText.Report("WARNING: Character set is " + value + ": should be ANSEL, ANSI, ASCII, UTF-8 or UNICODE\n");
                            }

                            // insert any necessary closing tags
                            while (thislevel <= prevlevel)
                            {
                                stack.Pop();
                                node = node.ParentNode;
                                prevlevel--;
                            }

                            if (!tag.Equals("TRLR"))
                            {
                                XmlNode newNode = document.CreateElement(tag);
                                node.AppendChild(newNode);
                                node = newNode;

                                if (!string.IsNullOrEmpty(iden))
                                {
                                    XmlAttribute attr = document.CreateAttribute("ID");
                                    attr.Value = iden;
                                    node.Attributes.Append(attr);
                                }
                                if (!string.IsNullOrEmpty(xref))
                                {
                                    XmlAttribute attr = document.CreateAttribute("REF");
                                    attr.Value = xref;
                                    node.Attributes.Append(attr);
                                }
                                stack.Push(tag);
                                prevlevel = thislevel;
                            }

                            if (value.Length > 0)
                            {
                                XmlText text = document.CreateTextNode(value);
                                node.AppendChild(text);
                            }
                        }
                        catch (Exception e)
                        {
                            outputText.Report("Found bad line " + lineNr + ": '" + line + "'. " + "Error was : " + e.Message + "\n");
                            badLineCount++;
                        }
                    }
                    line = nextline;
                    //if (badLineCount > badLineMax)
                    //{
                    //    string message = "Found more than " + badLineMax + " consecutive errors in the GEDCOM file.";
                    //    if (!FileHandling.Default.LoadWithFilters)
                    //        message += "\n\nNB. You might get less errors if you turn on the option to 'Use Special Character Filters When Loading' from the Tools Options menu.";
                    //    message += "\n\nContinue Loading?";
                    //    DialogResult result = MessageBox.Show(message, "Continue Loading?", MessageBoxButtons.YesNo);
                    //    if (result == DialogResult.Yes)
                    //    {
                    //        badLineCount = 0;
                    //        badLineMax *= 2; // double count of errors before next act
                    //    }
                    //    else
                    //    {
                    //        document = null;
                    //        break;
                    //    }
                    //}

                } // end while

            }
            finally
            {
                reader.Close();
            }
            return document;
        }

        /**
         * Procedure to return the first word in a string
         */
        static string FirstWord(string inp)
        {
            int i;
            i = inp.IndexOf(' ');
            return i == 0 ? FirstWord(inp.Trim()) : i < 0 ? inp : inp.Substring(0, i).Trim();
        }

        /**
          * Procedure to return the text after the first word in a string
          */

        static string Remainder(string inp)
        {
            int i;
            i = inp.IndexOf(' ');
            return i == 0 ? Remainder(inp.Trim()) : i < 0 ? "" : inp.Substring(i + 1).Trim();
        }
    }
}
