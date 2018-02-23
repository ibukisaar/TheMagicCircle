using System;
using System.Collections.Generic;
using System.Text;

namespace 反射镜 {
	public interface IBitStreamWriter {
		void Write(int bits, int bitLength);
		void Write(bool bit);
	}

	public interface IBitStreamReader {
		int Read(out int bits, int bitLength);
		bool Read();
	}
}
