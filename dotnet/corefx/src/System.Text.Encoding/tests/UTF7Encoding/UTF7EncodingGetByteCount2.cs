// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace System.Text.Tests
{
    public class UTF7EncodingGetByteCount2
    {
        private readonly Char[] _ARRAY_DIRECTCHARS = { '\t', '\n', '\r', 'X', 'Y', 'Z', 'a', 'b', 'c', '1', '2', '3' };
        private const int c_INT_DIRECTCHARSLENGTH = 12;

        private readonly Char[] _ARRAY_OPTIONALCHARS = { '!', '\"', '#', '$', '%', '&', '*', ';', '<', '=' };     // "!\"#$%&*;<=>@[]^_`{|}";
        private const int c_INT_OPTIONALCHARSLENTTH = 10;

        private readonly Char[] _ARRAY_SPECIALCHARS = { '\u03a0', '\u03a3' };                        // "\u03a0\u03a3";
        private const int c_INT_SPECIALCHARSLENGTH = 8;

        private readonly Char[] _ARRAY_EMPTY = new Char[0];
        private const int c_INT_EMPTYlENGTH = 0;

        // PosTest1: to test direct chars with new UTF7Encoding().
        [Fact]
        public void PosTest1()
        {
            UTF7Encoding utf7 = new UTF7Encoding();
            Assert.Equal(c_INT_DIRECTCHARSLENGTH, utf7.GetByteCount(_ARRAY_DIRECTCHARS, 0, c_INT_DIRECTCHARSLENGTH));
        }

        // PosTest2: to test direct chars with new UTF7Encoding(true).
        [Fact]
        public void PosTest2()
        {
            UTF7Encoding utf7 = new UTF7Encoding(true);
            Assert.Equal(c_INT_DIRECTCHARSLENGTH, utf7.GetByteCount(_ARRAY_DIRECTCHARS, 0, c_INT_DIRECTCHARSLENGTH));
        }

        // PosTest3: to test OPTIONALCHARS with new UTF7Encoding().
        [Fact]
        public void PosTest3()
        {
            UTF7Encoding utf7 = new UTF7Encoding();
            Assert.NotEqual(c_INT_OPTIONALCHARSLENTTH, utf7.GetByteCount(_ARRAY_OPTIONALCHARS, 0, c_INT_OPTIONALCHARSLENTTH));
        }

        // PosTest4: to test OPTIONALCHARS with new UTF7Encoding(true).
        [Fact]
        public void PosTest4()
        {
            Char[] CHARS = { '!', '\"', '#', '$', '%', '&', '*', ';', '<', '=' };
            UTF7Encoding utf7 = new UTF7Encoding(true);
            Assert.Equal(c_INT_OPTIONALCHARSLENTTH, utf7.GetByteCount(_ARRAY_OPTIONALCHARS, 0, c_INT_OPTIONALCHARSLENTTH));
        }

        // PosTest5: to test SPECIALCHARS with new UTF7Encoding().
        [Fact]
        public void PosTest5()
        {
            UTF7Encoding utf7 = new UTF7Encoding();
            Assert.Equal(c_INT_SPECIALCHARSLENGTH, utf7.GetByteCount(_ARRAY_SPECIALCHARS, 0, _ARRAY_SPECIALCHARS.Length));
        }

        // PosTest6: to test SPECIALCHARS with new UTF7Encoding(true).
        [Fact]
        public void PosTest6()
        {
            UTF7Encoding utf7 = new UTF7Encoding(true);
            Assert.Equal(c_INT_SPECIALCHARSLENGTH, utf7.GetByteCount(_ARRAY_SPECIALCHARS, 0, _ARRAY_SPECIALCHARS.Length));
        }

        // PosTest7: to test Empty Char[] with new UTF7Encoding().
        [Fact]
        public void PosTest7()
        {
            UTF7Encoding utf7 = new UTF7Encoding();
            Assert.Equal(c_INT_EMPTYlENGTH, utf7.GetByteCount(_ARRAY_EMPTY, 0, 0));
        }
    }
}