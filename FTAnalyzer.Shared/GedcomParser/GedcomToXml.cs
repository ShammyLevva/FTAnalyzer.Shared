using FTAnalyzer.Utilities;
using FTAnalyzer.Windows.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

#if __MACOS__
using Foundation;
#endif

namespace FTAnalyzer
{
    class GedcomToXml
    {
        public static XmlDocument LoadFile(Stream stream, Encoding encoding, IProgress<string> outputText, bool reportBadLines)
        {
            XmlDocument doc;
            MemoryStream cloned = CloneStream(stream);
            using (var reader = new StreamReader(FileHandling.Default.RetryFailedLines ? CheckInvalidCR(cloned) : cloned, encoding))
            {
                doc = Parse(reader, outputText, reportBadLines);
            }
            if (doc?.SelectNodes("GED/INDI").Count == 0)
            { // if there is a problem with the file return with opposite line ends
                cloned = CloneStream(stream);
                using (var reader = new StreamReader(FileHandling.Default.RetryFailedLines ? cloned : CheckInvalidCR(cloned), encoding))
                {
                    doc = Parse(reader, outputText, false);
                }
            }
            return doc;
        }

        public static XmlDocument LoadAnselFile(Stream stream, IProgress<string> outputText, bool reportBadLines)
        {
            XmlDocument doc;
            MemoryStream cloned = CloneStream(stream);
            using (var reader = new AnselInputStreamReader(FileHandling.Default.RetryFailedLines ? CheckInvalidCR(cloned) : cloned))
            {
                doc = Parse(reader, outputText, reportBadLines);
            }
            if (doc?.SelectNodes("GED/INDI").Count == 0)
            {
                // if there is a problem with the file return with opposite line ends
                cloned = CloneStream(stream);
                using (var reader = new AnselInputStreamReader(FileHandling.Default.RetryFailedLines ? cloned : CheckInvalidCR(cloned)))
                {
                    doc = Parse(reader, outputText, false);
                }
            }
            return doc;
        }

        static MemoryStream CloneStream(Stream stream)
        {
            MemoryStream mstream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(mstream);
            mstream.Position = 0;
            return mstream;
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
                    var x = infs.ReadByte();
                    if (x >= 0)
                    {
                        b = (byte)x;
                        if (b == 0x0a)// we have 0x0d 0x0a so write the 0x0d so that normal write works.
                            outfs.WriteByte(0x0d);
                        else
                            outfs.WriteByte(b);
                    }
                }
                else
                    outfs.WriteByte(b);
            }
            outfs.Position = 0;
            return outfs;
        }

        //static MemoryStream CheckSpuriousOD(MemoryStream infs)
        //{
        //    MemoryStream outfs = new MemoryStream();
        //    byte b = (byte)infs.ReadByte();
        //    long streamLength = infs.Length;
        //    while (infs.Position < streamLength)
        //    {
        //        while (b == 0x0d && infs.Position < streamLength)
        //        {
        //            b = (byte)infs.ReadByte();
        //            if (b == 0x0a)
        //            { // we have 0x0d 0x0a so write out the 0x0d and the 0x0a will follow in the normal write.
        //                outfs.WriteByte(0x0d);
        //            } // otherwise we drop though and have ignored the 0x0d on its own
        //        }
        //        outfs.WriteByte(b);
        //        b = (byte)infs.ReadByte();
        //    }
        //    outfs.Position = 0;
        //    return outfs;
        //}

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
            Dictionary<long, Tuple<string, string>> lineErrors = new Dictionary<long, Tuple<string, string>>();
            Stack<string> stack = new Stack<string>();
            stack.Push("GED");
            XmlDocument document = new XmlDocument() { XmlResolver = null };
            XmlNode node = document.CreateElement("GED");
            document.AppendChild(node);
            string currentName = string.Empty;
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
                            line += nextline;
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
                            line = line.Replace('–', '-').Replace('—', '-').Replace("&nbsp;"," ").Replace(" * **Data is already there***", ""); // "data is already there" is some Ancestry anomaly
                            cpos1 = line.IndexOf(" ", StringComparison.Ordinal);
                            if (cpos1 < 0) throw new InvalidGEDCOMException($"No space found in line: '{line}'", line, lineNr);

                            level = FirstWord(line);
                            if (level.StartsWithNumeric())
                                thislevel = int.Parse(level);
                            else
                                throw new InvalidGEDCOMException($"First character in a should be numeric '{line}'", line, lineNr);

                            // check the level number

                            if (thislevel > prevlevel && !(thislevel == prevlevel + 1))
                                throw new InvalidGEDCOMException($"Level numbers must increase by 1", line, lineNr);
                            if (thislevel < 0)
                                throw new InvalidGEDCOMException("Level number must not be negative", line, lineNr);

                            line = Remainder(line);
                            token1 = FirstWord(line);
                            line = Remainder(line);
                            if (thislevel == 1 && token1 == "NAME")
                                currentName = line;
                            if (token1.StartsWith("@", StringComparison.Ordinal))
                            {
                                if (token1.EndsWith("@@?"))
                                    token1 = token1.TrimEnd('?');
                                if (token1.Length == 1 || !token1.EndsWith("@", StringComparison.Ordinal))
                                    throw new InvalidGEDCOMException($"Bad xref_id invalid @ character in line. Check notes for use of @ symbol", line, lineNr);

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
                                if (!token1.Equals("CONT", StringComparison.Ordinal) && !token1.Equals("CONC", StringComparison.Ordinal))
                                {
                                    token2 = FirstWord(line);
                                    if (token2.EndsWith("@@?"))
                                        token2 = token2.TrimEnd('?');
                                    if (token2.Length == 1 || (!token2.EndsWith("@", StringComparison.Ordinal) && !token2.EndsWith("@,", StringComparison.Ordinal)))
                                        throw new InvalidGEDCOMException($"Bad pointer value. Check notes for use of @ symbol", line, lineNr);
                                    xref = token2.EndsWith("@,", StringComparison.Ordinal)
                                        ? token2.Substring(1, token2.Length - 3)
                                        : token2.Substring(1, token2.Length - 2);
                                    line = Remainder(line);
                                }
                            }
                            if (token1.Equals("CONT", StringComparison.Ordinal) || token1.Equals("CONC", StringComparison.Ordinal))
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
                            if (tag.Equals("CHAR", StringComparison.Ordinal))
                            {
                                if (!(valtrim.Equals("ANSEL", StringComparison.Ordinal) || valtrim.Equals("ASCII", StringComparison.Ordinal) || valtrim.Equals("ANSI", StringComparison.Ordinal) ||
                                     valtrim.Equals("UTF-8", StringComparison.Ordinal) || valtrim.Equals("UNICODE", StringComparison.Ordinal)))
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

                            if (!tag.Equals("TRLR", StringComparison.Ordinal))
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
                                outputText.Report($"Invalid GEDCOM, Line: {lineNr}: '{line}'. Last Name Seen: {currentName}. Error was: {ige.Message}\n");
                            lineErrors.Add(lineNr, new Tuple<string, string>(line, ige.Message));
                            badLineCount++;
                        }
                        catch (Exception e)
                        {
                            if (reportBadLines)
                                outputText.Report($"Unhandled Exception, bad line {lineNr}: '{line}'. Last Name Seen: {currentName}. Error was: {e.Message}\n");
                            lineErrors.Add(lineNr, new Tuple<string, string>(line, e.Message));
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
#if !__IOS__
                if (badLineCount > 0 && reportBadLines)
                    ShowBadLines(reader.BaseStream, lineErrors);
#endif
                reader.Close();
            }
            return document;
        }

#if __MACOS__
        static readonly NSObject Invoker = new NSObject();
#endif
        static void ShowBadLines(Stream stream, Dictionary<long, Tuple<string, string>> lineErrors)
        {
#if __MACOS__
            if (!NSThread.IsMain)
            {
                
                Invoker.InvokeOnMainThread(() => ShowBadLines(stream, lineErrors));
                return;
            }
#endif
            try
            {
                int result = UIHelpers.ShowYesNo("Would you like to view the line error report?");
                if (result == UIHelpers.Yes)
                {
                    string tempFile = CreateTempFile();
                    if (!string.IsNullOrEmpty(tempFile))
                    {
                        tempFile = tempFile.Substring(0, tempFile.Length - 3) + "html";
                        stream.Position = 0;
                        FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
                        using (StreamWriter writer = new StreamWriter(fileStream))
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                writer.WriteLine("<html><head><Title>Gedcom File</Title></head><body>");
                                writer.WriteLine("<h4>Line Errors</h4>");
                                writer.WriteLine("<table border='1'><tr><th>Line Number</th><th>Line Contents</th><th>Error Description</th></tr>");
                                foreach (KeyValuePair<long, Tuple<string, string>> kvp in lineErrors)
                                    writer.WriteLine($"<tr><td><a href='#{kvp.Key}'>{kvp.Key}</a></td><td>{kvp.Value.Item1}</td><td>{kvp.Value.Item2}</td></tr>");
                                writer.WriteLine("</table><h4>GEDCOM Contents</h4><table border='1'><tr><th>Line Number</th><th>Line Contents</th></tr>");
                                string line = reader.ReadLine();
                                long lineNr = 1;
                                while (line != null)
                                {
                                    if (lineErrors.ContainsKey(lineNr))
                                        writer.WriteLine($"<tr id='{lineNr}'><td>{lineNr++}</td><td>{line}</td></tr>");
                                    else
                                        writer.WriteLine($"<tr><td>{lineNr++}</td><td>{line}</td></tr>");
                                    line = reader.ReadLine();
                                }
                                writer.Write("</table></body></html>");
                            }
                        }
                        SpecialMethods.VisitWebsite(tempFile);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e.Message}");
            }
        }

        static string CreateTempFile()
        {
            string fileName = string.Empty;
            try
            {
                fileName = Path.GetTempFileName();
                FileInfo fileInfo = new FileInfo(fileName)
                {
                    Attributes = FileAttributes.Temporary
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to create TEMP file or set its attributes: " + ex.Message);
            }
            return fileName;
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

        /// <summary>
        /// Detects the byte order mark of a file and returns
        /// an appropriate encoding for the file.
        /// </summary>
        /// <param name="srcFile"></param>
        /// <returns></returns>
        public static Encoding GetFileEncoding(FileStream file)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.Default;

            // *** Detect byte order mark if any - otherwise assume default
            byte[] buffer = new byte[5];
            file.Read(buffer, 0, 5);
            file.Seek(0, SeekOrigin.Begin);

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                enc = Encoding.BigEndianUnicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                enc = Encoding.UTF32;
            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
#pragma warning disable SYSLIB0001 // Type or member is obsolete
                enc = Encoding.UTF7;
#pragma warning restore SYSLIB0001 // Type or member is obsolete
            else if (buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0 && buffer[3] == 0) // UTF32 little endian
                enc = Encoding.UTF32;
            else if (buffer[0] == 0xff && buffer[1] == 0xfe) // UTF16 little endian
                enc = Encoding.Unicode;
            return enc;
        }
    }
}
