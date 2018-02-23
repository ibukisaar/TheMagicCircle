using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace 反射镜 {
	public class BitStreamReader : IBitStreamReader {
		private int position = 0;
		private BitArray bitArray;

		public BitStreamReader(byte[] bytes, int byteIndex, int bitLength) {
			var src = new BitArray(bytes);
			bitArray = new BitArray(bitLength);
			for (int i = 0; i < bitLength; i++) {
				bitArray[i] = src[i + byteIndex * 8];
			}
		}

		public BitStreamReader(BitArray bitArray) {
			this.bitArray = new BitArray(bitArray);
		}

		public int Read(out int bits, int bitLength) {
			if (bitLength < 0 || bitLength > 32) throw new ArgumentException("非法读取数量", nameof(bitLength));
			int readLen = Math.Min(bitArray.Length - position, bitLength);
			var result = 0;
			for (int i = 0; i < readLen; i++) {
				if (bitArray[position + i]) result |= 1 << i;
			}
			bits = result;
			position += readLen;
			return readLen;
		}

		public bool Read() {
			int len = Read(out var intBit, 1);
			if (len < 1) throw new System.IO.EndOfStreamException();
			return intBit != 0;
		}
	}
}
