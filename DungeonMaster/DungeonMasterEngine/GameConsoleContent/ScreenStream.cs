﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace DungeonMasterEngine.GameConsoleContent
{
    internal class ScreenStream : Stream
    {
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => 0;//TODO

        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }


        string line = "";
        private List<string> lines = new List<string>();
        public IReadOnlyList<string> Lines => lines;
        public override void Write(byte[] buffer, int offset, int count)
        {
            var decoder = Encoding.Default.GetDecoder();
            char[] text = new char[decoder.GetCharCount(buffer, offset, count)];
            decoder.GetChars(buffer, offset, count, text, 0, true);
            line += new string(text, offset, count);

            var s = line.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            lines.AddRange(s.TakeWhile((str, index) => index < (s.Length - 1)));
            line = s[s.Length - 1];//TODO Lengt bounds
        }
    }
}