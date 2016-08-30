using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Fonlow.TraceHub;

namespace CoreTests
{
    public class Basic
    {
        [Fact]
        public void SingleAddress()
        {
            var ranges = IPAddressRangesHelper.ParseIPAddressRanges("123.223.22.13");
            Assert.True(ranges.IsInRanges("123.223.22.13"));
            Assert.False(ranges.IsInRanges("123.223.22.123"));
        }

        [Fact]
        public void SingleAddressRange()
        {
            var ranges = IPAddressRangesHelper.ParseIPAddressRanges("123.223.22.13-123.223.22.130");
            Assert.True(ranges.IsInRanges("123.223.22.13"));
            Assert.True(ranges.IsInRanges("123.223.22.123"));
        }

        [Fact]
        public void MultipleAddressRanges()
        {
            var ranges = IPAddressRangesHelper.ParseIPAddressRanges("123.223.22.13-123.223.22.130, 192.168.0.0-192.168.0.255");
            Assert.True(ranges.IsInRanges("123.223.22.13"));
            Assert.True(ranges.IsInRanges("123.223.22.123"));
            Assert.True(ranges.IsInRanges("192.168.0.123"));
        }

        [Fact]
        public void RangeNotDefined()
        {
            Assert.Null(IPAddressRangesHelper.ParseIPAddressRanges("   "));
        }

        [Fact]
        public void SingleAddressRangeInvalidSubnetMaskExpectException()
        {
            Assert.Throws<FormatException>(() =>
            IPAddressRangesHelper.ParseIPAddressRanges("123.223.22.13/130"));
        }

        [Fact]
        public void SingleAddressRangeInvalidRangekExpectException()
        {
            Assert.Throws<FormatException>(() =>
            IPAddressRangesHelper.ParseIPAddressRanges("123.223.22.13-123.223.22.888"));
        }


    }
}
