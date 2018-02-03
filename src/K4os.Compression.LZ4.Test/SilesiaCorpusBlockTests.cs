﻿using System;
using System.IO;
using Xunit;

namespace K4os.Compression.LZ4.Test
{
	public class SilesiaCorpusBlockTests
	{
		private static uint Adler32(byte[] data)
		{
			const uint modAdler = 65521;

			uint a = 1, b = 0;
			var len = data.Length;

			for (var index = 0; index < len; ++index)
			{
				a = (a + data[index]) % modAdler;
				b = (b + a) % modAdler;
			}

			return (b << 16) | a;
		}

		private static byte[] LoadFileChunk(string filename, int index, int length)
		{
			using (var file = File.OpenRead(filename))
			{
				var src_len = length < 0 ? file.Length - index : length;
				var src = new byte[src_len];
				file.Seek(index, SeekOrigin.Begin);
				file.Read(src, 0, (int) src_len);
				return src;
			}
		}

		private static int TestChunk(byte[] source, byte[] target, int length)
		{
			for (var i = 0; i < length; i++)
			{
				if (source[i] != target[i])
					return i;
			}

			return -1;
		}

		[Theory]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/dickens", 0, 10192446, 6428742, 0x17278caf, "+qMqKlRoZSBQcm9qZWN0IEd1dGVuYmVyZyBFdGV4dCBvZiBBIENoaWxkJ3MgSGlzdG9yeSBvZiBFbmds")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/mozilla", 0, 51220480, 26435667, 0x929ed42e, "n21vemlsbGEvAAEASPQGIDQwNzU1IAAgIDI2MDAgACAgICAgCAABAgD/CDAgIDc0NzU3NDI3NjEgIDEw")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/mr", 0, 9970564, 5440937, 0x5d218bd6, "+EgIAAUACgAAAElTT19JUiAxMDAIAAgAFgAAAE9SSUdJTkFMXFBSSU1BUllcT1RIRVIIABYAGgAAADEu")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/nci", 0, 33553445, 5533040, 0x20c1d85f, "9hwxNTU1NDIKUk90Y2xzZXJ2ZTExMTUwMDExMjEyRCAwICAgMC4wMDAwMCAgDAD/BDEwNDk1MjEKIAog")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/ooffice", 0, 6152192, 4338918, 0xf6e8e90, "8gNNWpAAAwAAAAQAAAD//wAAuAABABJABwAPAgAK8y7wAAAADh+6DgC0Cc0huAFMzSFUaGlzIHByb2dy")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/osdb", 0, 10085684, 5256666, 0x6f12d3ea, "8w4DAE8BYAIThAEAAHUlBa4AAAC5za/NhecSTgw2MQIA8T0uMDAJNy8xNy8xOTQ0RmhYVHViOlpRTjVt")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/reymont", 0, 6627202, 3181387, 0x41648906, "8hElUERGLTEuMwozIDAgb2JqIDw8Ci9MZW5ndGggMTUzIAEA8SgKPj4Kc3RyZWFtCjEgMCAwIDEgMjQ0")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/samba", 0, 21606400, 7716839, 0x93086c52, "73NhbWJhLTIuMi4zYS8AAQBD8QgwMDQwNzU1ADAwMDE3NjEAMDAwMDE1MggAAwIA/wcAMDc0MjcxMDQw")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/sao", 0, 7251944, 6790273, 0x81d09df4, "sAAAAAABAAAAtfMDCwABDADwSQEAAAAcAAAA1Ke7C7dKOD9rphXawBf3P0Ew0AKZBiK1qpQmMrdL+Jif")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/webster", 0, 41458703, 20139988, 0x52b7fc61, "+sQNClRoZSBQcm9qZWN0IEd1dGVuYmVyZyBFdGV4dCBvZiBUaGUgMTkxMyBXZWJzdGVyIFVuYWJyaWRn")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/xml", 0, 5345280, 1227495, 0x3ae4f54c, "n2VsdHMueG1sAAEASOkxMDA3NzUgACAgIDc2NAgA/w8gIDMzNDc1NyAgNzE3NDM2NjM3MCAgMTIyMDEA")]
		[InlineData("D:/Projects/K4os.Compression.LZ4/.corpus/x-ray", 0, 8474240, 8390195, 0xcd8a167b, "/w/QAQAQB2wItgAQAAEBEQ6zRlNfQS4zMTk3LmltZwABAA9vQkxLTTE4KAAPBAIAUzgwMDEwMQDzDjgw")]
		public void CompressedFilesBinaryIdentical(
			string filename, int index, int length, int expectedCompressedLength, uint expectedChecksum, string expectedBytes64)
		{
			var src = LoadFileChunk(filename, index, length);

			var dst = LZ4Interface.Encode(src);
			var cmp = LZ4Interface.Decode(dst, src.Length);

			string AsHex(uint value) => $"0x{value:x8}";

			Assert.Equal(src, cmp);

			var expectedBytes = Convert.FromBase64String(expectedBytes64);
			Assert.Equal(-1, TestChunk(expectedBytes, dst, expectedBytes.Length));
			Assert.Equal(expectedCompressedLength, dst.Length);
			Assert.Equal(AsHex(expectedChecksum), AsHex(Adler32(dst)));
		}
	}
}