using FTAnalyzer.Properties;
using FTAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace FTAnalyzer
{
    class GedcomToXml
    {
        public static XmlDocument LoadFile(Stream stream, Encoding encoding, IProgress<string> outputText)
        {
            XmlDocument doc = null;
            StreamReader reader;
            stream.Position = 0;
            reader = FileHandling.Default.RetryFailedLines
                ? new StreamReader(CheckInvalidCR(stream), encoding)
                : new StreamReader(stream, encoding);
            doc = Parse(reader, outputText, true);
            if (doc?.SelectNodes("GED/INDI").Count == 0)
            { // if there is a problem with the file return with opposite line ends
                reader = FileHandling.Default.RetryFailedLines
                    ? new StreamReader(stream, encoding)
                    : new StreamReader(CheckInvalidCR(stream), encoding);
                doc = Parse(reader, outputText, false);
            }
            return doc;
        }

        public static XmlDocument LoadAnselFile(Stream stream, IProgress<string> outputText)
        {
            XmlDocument doc = null;
            StreamReader reader;
            stream.Position = 0;
            reader = FileHandling.Default.RetryFailedLines
                    ? new AnselInputStreamReader(CheckInvalidCR(stream))
                    : new AnselInputStreamReader(stream);
                doc = Parse(reader, outputText, true);
                if (doc?.SelectNodes("GED/INDI").Count == 0)
                {  // if there is a problem with the file return with opposite line ends
                    reader = FileHandling.Default.RetryFailedLines
                        ? new AnselInputStreamReader(stream)
                        : new AnselInputStreamReader(CheckInvalidCR(stream));
                    doc = Parse(reader, outputText, false);
                }
                return doc;
            }


            static MemoryStream CheckInvalidCR(Stream infs)
        {
            MemoryStream outfs = new MemoryStream();
            long streamLength = infs.Length;
            byte b; 
            while (infs.Position < streamLength)
            {
                b = (byte)infs.ReadByte();
                if (b == 0x0d)
                {
                    b = (byte)infs.ReadByte();
                    if (b == 0x0a)// we have 0x0d 0x0a so write the 0x0d so that normal write works.
                        outfs.WriteByte(0x0d);
                    else
                        outfs.WriteByte(b);
                }
                else
                    outfs.WriteByte(b);
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

        static XmlDocument Parse(StreamReader reader, IProgress<string> outputText, bool reportBadLines)
        {
            long lineNr = 0;
            int badLineCount = 0;
            int badLineMax = 30;
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
                            line = line.Replace('?', '-').Replace('?', '-').Replace("***Data is already there***", ""); // "data is already there" is some Ancestry anomaly
                            cpos1 = line.IndexOf(" ", StringComparison.Ordinal);
                            if (cpos1 < 0) throw new InvalidGEDCOMException($"No space found in line: '{line}'");

                            level = FirstWord(line);
                            if (level.StartsWithNumeric())
                                thislevel = int.Parse(level);
                            else
                                throw new InvalidGEDCOMException($"First character in a should be numeric '{line}'");

                            // check the level number
                            
                            if (thislevel > prevlevel && !(thislevel == prevlevel + 1))
                                throw new InvalidGEDCOMException($"Level numbers must increase by 1");
                            if (thislevel < 0)
                                throw new InvalidGEDCOMException("Level number must not be negative");

                            line = Remainder(line);
                            token1 = FirstWord(line);
                            line = Remainder(line);

                            if (token1.StartsWith("@", StringComparison.Ordinal))
                            {
                                if (token1.Length == 1 || !token1.EndsWith("@", StringComparison.Ordinal))
                                    throw new InvalidGEDCOMException("Bad xref_id invalid @ character in line.");

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
                                        throw new InvalidGEDCOMException("Bad pointer value");
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
                            if (tag.Equals("CHAR"))
                            {
                                if (!(valtrim.Equals("ANSEL") || valtrim.Equals("ASCII") || valtrim.Equals("ANSI") ||
                                     valtrim.Equals("UTF-8") || valtrim.Equals("UNICODE")))
                                {
                                    outputText.Report($"WARNING: Character set is {value}: should be ANSEL, ANSI, ASCII, UTF-8 or UNICODE\n");
                                }
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
                        catch (InvalidGEDCOMException ige)
                        {
                            if (reportBadLines)
                                outputText.Report($"Invalid GEDCOM, Line: {lineNr}. {ige.Message}\n");
                            badLineCount++;
                        }
                        catch (Exception e)
                        {
                            if (reportBadLines)
                                outputText.Report($"Unhandled Exception, bad line {lineNr}: '{line}'. Error was : {e.Message}\n");
                            badLineCount++;
                        }
                    }
                    line = nextline;
                    if (badLineCount > badLineMax)
                    {
#if __PC__
                        string message = $"Found more than {badLineMax} consecutive errors in the GEDCOM file.";
                        if (!FileHandling.Default.RetryFailedLines)
                            message += "\n\nNB. You may get less errors if you turn on the option to 'Retry failed lines by looking for bad line breaks' from the File Handling section of the Tools Options menu.";
                        message += "\n\nContinue Loading?";
                        int result = UIHelpers.ShowYesNo(message);
                        if (result == UIHelpers.Yes)
                        {
                            badLineCount = 0;
                            badLineMax *= 2; // double count of errors before next act
                        }
                        else
                        {
                            document = null;
                            break;
                        }
#endif
                    }
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
            i = inp.IndexOf(" ", StringComparison.Ordinal);
            return i == 0 ? FirstWord(inp.Trim()) : i < 0 ? inp : inp.Substring(0, i).Trim();
        }

        /**
          * Procedure to return the text after the first word in a string
          */

        static string Remainder(string inp)
        {
            int i;
            i = inp.IndexOf(" ", StringComparison.Ordinal);
            return i == 0 ? Remainder(inp.Trim()) : i < 0 ? "" : inp.Substring(i + 1).Trim();
        }
    }
}
