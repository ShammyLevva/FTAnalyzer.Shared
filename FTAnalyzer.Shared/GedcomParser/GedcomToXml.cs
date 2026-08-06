#pragma warning disable CA2000 // Modeless WinForms forms are owned by the Windows message loop; lifetime is managed externally
using FTAnalyzer.Utilities;
using FTAnalyzer.Properties;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml;

namespace FTAnalyzer
{
    public static class GedcomToXml
    {
        // A single logical GEDCOM line (after CONT/CONC joining or tolerant re-joining of broken lines)
        // never legitimately reaches this size. The cap stops a corrupt or hostile file from
        // concatenating an unbounded run of malformed lines into one huge string and exhausting memory.
        // Real-world files are far below this, so parsing behaviour is unchanged for legitimate input.
        const int MaxLogicalLineLength = 4 * 1024 * 1024;

        public static XmlDocument? LoadFile(Stream stream, Encoding encoding, IProgress<string> outputText, bool reportBadLines, IProgress<int>? parseProgress = null)
        {
            XmlDocument? doc;
            Stream cloned = PrepareStream(stream, parseProgress);
            bool retryFailed = FileHandling.Default.RetryFailedLines;
            // leaveOpen only when we're passing the original seekable stream directly (no CheckInvalidCR wrapper),
            // so the stream stays open if a retry parse is needed.
            bool leaveOpen = stream.CanSeek && !retryFailed;
            using (StreamReader reader = new(retryFailed ? CheckInvalidCR(cloned) : cloned, encoding, detectEncodingFromByteOrderMarks: false, bufferSize: -1, leaveOpen: leaveOpen))
            {
                doc = Parse(reader, outputText, reportBadLines, parseProgress);
            }
            if ((doc?.SelectNodes("GED/INDI")?.Count ?? 0) == 0)
            { // if there is a problem with the file return with opposite line ends
                cloned = PrepareStream(stream);
                retryFailed = FileHandling.Default.RetryFailedLines;
                // This branch picks the raw stream when retryFailed (the inverse of the block above), so
                // leaveOpen must follow that same inversion — otherwise a caller-side re-parse (e.g.
                // LoadTreeHeader's charset-based retry) can find the stream already closed.
                bool leaveOpenRetry = stream.CanSeek && retryFailed;
                using StreamReader reader = new(retryFailed ? cloned : CheckInvalidCR(cloned), encoding, detectEncodingFromByteOrderMarks: false, bufferSize: -1, leaveOpen: leaveOpenRetry);
                doc = Parse(reader, outputText, false);
            }
            return doc;
        }

        public static XmlDocument? LoadAnselFile(Stream stream, IProgress<string> outputText, bool reportBadLines, IProgress<int>? parseProgress = null)
        {
            XmlDocument? doc;
            Stream cloned = PrepareStream(stream, parseProgress);
            bool retryFailed = FileHandling.Default.RetryFailedLines;
            bool leaveOpen = stream.CanSeek && !retryFailed;
            using (AnselInputStreamReader reader = new(retryFailed ? CheckInvalidCR(cloned) : cloned, leaveOpen))
            {
                doc = Parse(reader, outputText, reportBadLines, parseProgress);
            }
            if ((doc?.SelectNodes("GED/INDI")?.Count ?? 0) == 0)
            {
                // if there is a problem with the file return with opposite line ends
                cloned = PrepareStream(stream);
                retryFailed = FileHandling.Default.RetryFailedLines;
                // See the matching comment in LoadFile: this branch picks the raw stream when retryFailed,
                // so leaveOpen must follow that inversion to avoid closing a stream the caller still needs.
                bool leaveOpenRetry = stream.CanSeek && retryFailed;
                using AnselInputStreamReader reader = new(retryFailed ? cloned : CheckInvalidCR(cloned), leaveOpen: leaveOpenRetry);
                doc = Parse(reader, outputText, false);
            }
            return doc;
        }

        // For seekable streams (FileStream from disk), avoids copying to MemoryStream — which would throw
        // IOException for files larger than ~2 GB. Falls back to a MemoryStream copy for non-seekable streams.
        static Stream PrepareStream(Stream stream, IProgress<int>? progress = null)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
                return stream;
            }
            MemoryStream mstream = new();
            if (progress is null || stream.Length == 0)
            {
                stream.CopyTo(mstream);
            }
            else
            {
                long total = stream.Length;
                byte[] buffer = new byte[81920];
                int read;
                long done = 0;
                int lastPct = -1;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    mstream.Write(buffer, 0, read);
                    done += read;
                    int pct = (int)(done * 100 / total);
                    if (pct != lastPct)
                    {
                        progress.Report(pct);
                        lastPct = pct;
                    }
                }
            }
            mstream.Position = 0;
            return mstream;
        }

        // Returns a streaming wrapper that filters CR bytes on-the-fly with no buffering,
        // avoiding OutOfMemoryException on files larger than ~2 GB.
        static Stream CheckInvalidCR(Stream infs) => new CrFilterStream(infs);

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

        static XmlDocument? Parse(StreamReader reader, IProgress<string> outputText, bool reportBadLines, IProgress<int>? parseProgress = null)
        {
            long lineNr = 0;
            long streamLength = reader.BaseStream.Length;
            int badLineCount = 0;
            int badLineMax = 30;
            string? line, nextline;
            string token1, token2;
            string level;
            int thislevel;
            int prevlevel = -1;
            string iden, tag, xref, value;
            int cpos1;
            Dictionary<long, Tuple<string, string>> lineErrors = [];
            Stack<string> stack = new();
            stack.Push("GED");
            XmlDocument? document = new() { XmlResolver = null };
            XmlNode? node = document.CreateElement("GED");
            document.AppendChild(node);
            string currentName = string.Empty;
            try
            {
                line = reader.ReadLine();
                while (line is not null)
                {
                    lineNr++;
                    if (parseProgress is not null && lineNr % 1000 == 0 && streamLength > 0)
                        parseProgress.Report((int)(reader.BaseStream.Position * 100 / streamLength));
                    nextline = reader.ReadLine();
                    if (FileHandling.Default.RetryFailedLines)
                    {
                        StringBuilder sb = new();
                        sb.Append(line);
                        //need to check if nextline is valid if not line=line+nextline and nextline=reader.ReadLine();
                        while (sb.Length < MaxLogicalLineLength &&
                               (nextline?.Length <= 1 || (nextline?.Length > 1 && (!char.IsNumber(nextline[0]) || !nextline[1].Equals(' ')))))
                        {  // concat if next line not a number space combo
                            sb.Append(nextline);
                            lineNr++;
                            nextline = reader.ReadLine();
                        }
                        line = sb.ToString();
                    }
                    // parse the GEDCOM line into five fields: level, iden, tag, xref, valu
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        try
                        {
                            line = line.Replace('�', '-').Replace('�', '-').Replace("&nbsp;", " ", StringComparison.Ordinal).Replace(" * **Data is already there***", "", StringComparison.Ordinal); // "data is already there" is some Ancestry anomaly
                            cpos1 = line.IndexOf(' ', StringComparison.Ordinal);
                            if (cpos1 < 0) throw new InvalidGEDCOMException($"No space found in line: '{line}'", line, lineNr);

                            level = FirstWord(line);
                            if (level.StartsWithNumeric())
                                thislevel = int.Parse(level);
                            else
                                throw new InvalidGEDCOMException($"First character in a should be numeric '{line}'", line, lineNr);

                            // check the level number

                            if (thislevel > prevlevel && (thislevel != prevlevel + 1))
                                throw new InvalidGEDCOMException($"Level numbers must increase by 1", line, lineNr);
                            if (thislevel < 0)
                                throw new InvalidGEDCOMException("Level number must not be negative", line, lineNr);

                            line = Remainder(line);
                            token1 = FirstWord(line);
                            line = Remainder(line);
                            if (thislevel == 1 && token1 == "NAME")
                                currentName = line;
                            if (token1.StartsWith('@'))
                            {
                                if (token1.EndsWith("@@?"))
                                    token1 = token1.TrimEnd('?');
                                if (token1.Length == 1 || !token1.EndsWith('@'))
                                    throw new InvalidGEDCOMException($"Bad xref_id invalid @ character in line. Check notes for use of @ symbol", line, lineNr);

                                iden = token1[1..^1];
                                tag = FirstWord(line);
                                line = Remainder(line);
                            }
                            else
                            {
                                iden = "";
                                tag = token1;
                            }

                            xref = "";
                            if (line.StartsWith('@') && tag != "_HASHTAG" && tag != "NAME")
                            {
                                if (!token1.Equals("CONT", StringComparison.Ordinal) && !token1.Equals("CONC", StringComparison.Ordinal))
                                {
                                    token2 = FirstWord(line);
                                    if (token2.EndsWith("@@?"))
                                        token2 = token2.TrimEnd('?');
                                    if (token2.Length == 1 || (!token2.EndsWith('@') && !token2.EndsWith("@,", StringComparison.Ordinal)))
                                        throw new InvalidGEDCOMException($"Bad pointer value. Check notes for use of @ symbol", line, lineNr);
                                    xref = token2.EndsWith("@,", StringComparison.Ordinal)
                                        ? token2[1..^2]
                                        : token2[1..^1];
                                    line = Remainder(line);
                                }
                            }
                            if (token1.Equals("CONT", StringComparison.Ordinal) || token1.Equals("CONC", StringComparison.Ordinal))
                            {
                                StringBuilder sb = new();
                                sb.Append(line);
                                // check if nextline does not start with a number ie: could be a wrapped line, if so then concatenate
                                while (sb.Length < MaxLogicalLineLength && nextline is not null && !nextline.Trim().StartsWithNumeric())
                                {
                                    sb.Append($"\n{nextline.Trim()}");
                                    nextline = reader.ReadLine();
                                }
                                line = sb.ToString().Trim();
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
                            while (thislevel <= prevlevel && node is not null)
                            {
                                stack.Pop();
                                node = node.ParentNode;
                                prevlevel--;
                            }

                            if (!tag.Equals("TRLR", StringComparison.Ordinal))
                            {
                                XmlNode newNode = document.CreateElement(tag);
                                node?.AppendChild(newNode);
                                node = newNode;

                                if (!string.IsNullOrEmpty(iden))
                                {
                                    XmlAttribute attr = document.CreateAttribute("ID");
                                    attr.Value = iden;
                                    node.Attributes?.Append(attr);
                                }
                                if (!string.IsNullOrEmpty(xref))
                                {
                                    XmlAttribute attr = document.CreateAttribute("REF");
                                    attr.Value = xref;
                                    node.Attributes?.Append(attr);
                                }
                                stack.Push(tag);
                                prevlevel = thislevel;
                            }

                            if (value.Length > 0)
                            {
                                // Some exporters (e.g. Family Tree Maker) incorrectly HTML/XML-escape values
                                // when writing GEDCOM - which is plain text and needs no such escaping - so
                                // "&" in a name/place/note comes through the file as the literal characters
                                // "&amp;". We store this value directly via CreateTextNode (not by serialising
                                // and re-parsing XML text), so that literal "&amp;" is never decoded and shows
                                // up as-is everywhere the value is displayed. HtmlDecode only touches
                                // recognised entities (&amp; &lt; &gt; &quot; &#39; etc.) and leaves a genuine
                                // standalone "&" untouched, so this is safe for files without the quirk too.
                                XmlText text = document.CreateTextNode(WebUtility.HtmlDecode(value));
                                node?.AppendChild(text);
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
                        int result = UIHelpers.ShowYesNo(message, "FTAnalyzer");
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
                if (badLineCount > 0 && reportBadLines)
                    ShowBadLines(reader.BaseStream, lineErrors);
                reader.Close();
            }
            return document;
        }

        static void ShowBadLines(Stream stream, Dictionary<long, Tuple<string, string>> lineErrors)
        {
            try
            {
                int result = UIHelpers.ShowYesNo("Would you like to view the line error report?", "FTAnalyzer");
                if (result == UIHelpers.Yes)
                {
                    string tempFile = CreateTempFile();
                    if (!string.IsNullOrEmpty(tempFile))
                    {
                        tempFile = tempFile[..^3] + "html";
                        stream.Position = 0;
                        FileStream fileStream = new(tempFile, FileMode.Create, FileAccess.Write);
                        using (StreamWriter writer = new(fileStream))
                        {
                            // leaveOpen: the caller (Parse's finally block) still owns this stream and may
                            // need it for a subsequent charset-based re-parse in LoadTreeHeader.
                            using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
                            writer.WriteLine("<html><head><Title>Gedcom File</Title></head><body>");
                            writer.WriteLine("<h4>Line Errors</h4>");
                            writer.WriteLine("<table border='1'><tr><th>Line Number</th><th>Line Contents</th><th>Error Description</th></tr>");
                            foreach (KeyValuePair<long, Tuple<string, string>> kvp in lineErrors)
                                writer.WriteLine($"<tr><td><a href='#{kvp.Key}'>{kvp.Key}</a></td><td>{kvp.Value.Item1}</td><td>{kvp.Value.Item2}</td></tr>");
                            writer.WriteLine("</table><h4>GEDCOM Contents</h4><table border='1'><tr><th>Line Number</th><th>Line Contents</th></tr>");
                            string? line = reader.ReadLine();
                            long lineNr = 1;
                            while (line is not null)
                            {
                                if (lineErrors.ContainsKey(lineNr))
                                    writer.WriteLine($"<tr id='{lineNr}'><td>{lineNr++}</td><td>{line}</td></tr>");
                                else
                                    writer.WriteLine($"<tr><td>{lineNr++}</td><td>{line}</td></tr>");
                                line = reader.ReadLine();
                            }
                            writer.Write("</table></body></html>");
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
                fileName = Path.GetRandomFileName();
                FileInfo fileInfo = new(fileName)
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
            i = inp.IndexOf(' ', StringComparison.Ordinal);
            return i == 0 ? FirstWord(inp.Trim()) : i < 0 ? inp : inp[..i].Trim();
        }

        /**
          * Procedure to return the text after the first word in a string
          */

        static string Remainder(string inp)
        {
            int i;
            i = inp.IndexOf(' ', StringComparison.Ordinal);
            return i == 0 ? Remainder(inp.Trim()) : i < 0 ? "" : inp[(i + 1)..].Trim();
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
            int count = file.Read(buffer, 0, 5);
            if (count == 5)
            {
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
            }
            return enc;
        }

        // Streaming CR filter — replicates CheckInvalidCR logic without buffering the whole file.
        // \r\n → \r (StreamReader treats \r as a line ending); bare \r → dropped.
        // Length/Position are forwarded from inner so Parse's progress reporting still works.
        // The inner stream is NOT disposed here; lifetime is managed by the caller.
        sealed class CrFilterStream(Stream inner) : Stream
        {
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => inner.Length;
            public override long Position { get => inner.Position; set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count)
            {
                int written = 0;
                while (written < count)
                {
                    int b = inner.ReadByte();
                    if (b < 0) break;
                    if (b == 0x0d)
                    {
                        int next = inner.ReadByte();
                        if (next < 0) break; // trailing bare \r at EOF — drop it
                        if (next == 0x0a)
                            buffer[offset + written++] = 0x0d; // \r\n → emit \r, consume \n
                        else
                            buffer[offset + written++] = (byte)next; // bare \r → drop, emit what followed
                    }
                    else
                    {
                        buffer[offset + written++] = (byte)b;
                    }
                }
                return written;
            }

            protected override void Dispose(bool disposing) => base.Dispose(disposing);
        }
    }
}
