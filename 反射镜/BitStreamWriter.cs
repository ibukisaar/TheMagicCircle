using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace 反射镜 {
	public class BitStreamWriter : IBitStreamWriter {
		private int position = 0;
		private BitArray bitArray;

		public BitStreamWriter(int capacity) {
			bitArray = new BitArray(capacity);
		}

		public BitStreamWriter() : this(1024) { }

		public int Count => position;

		public void TryResize(int bitLength) {
			if (position + bitLength > bitArray.Length) {
				var newLength = Math.Max(position + bitLength, bitArray.Length * 2);
				var newBitArray = new BitArray(newLength);
				for (int i = 0; i < bitArray.Length; i++) {
					newBitArray[i] = bitArray[i];
				}
				bitArray = newBitArray;
			}
		}

		public void Write(int bits, int bitLength) {
			if (bitLength < 0 || bitLength > 32) throw new ArgumentException("非法写入数量", nameof(bitLength));
			TryResize(bitLength);
			for (int i = 0; i < bitLength; i++) {
				bitArray[position + i] = (bits & (1 << i)) != 0;
			}
			position += bitLength;
		}

		public void Write(bool bit) {
			Write(bit ? 1 : 0, 1);
		}

		public void CopyTo(byte[] bytes, int dstIndex) {
			if ((bytes.Length - dstIndex) * 8 < position) throw new ArgumentOutOfRangeException();
			var result = new BitArray(position);
			for (int i = 0; i < position; i++) {
				result[i] = bitArray[i];
			}
			result.CopyTo(bytes, dstIndex);
		}
	}
}
