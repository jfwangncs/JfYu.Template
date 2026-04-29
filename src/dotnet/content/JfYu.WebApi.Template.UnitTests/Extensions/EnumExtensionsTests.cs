using System.ComponentModel;
using JfYu.WebApi.Template.Extensions;

namespace JfYu.WebApi.Template.UnitTests.Extensions
{
    public class EnumExtensionsTests
    {
        private enum SampleEnum
        {
            [Description("first option")]
            First = 1,

            [Description("second option")]
            Second = 2,

            NoDescription = 3,
        }

        [Fact]
        public void GetDescription_ReturnsAttributeText_WhenPresent()
        {
            SampleEnum.First.GetDescription().Should().Be("first option");
            SampleEnum.Second.GetDescription().Should().Be("second option");
        }

        [Fact]
        public void GetDescription_FallsBackToEnumName_WhenAttributeMissing()
        {
            SampleEnum.NoDescription.GetDescription().Should().Be(nameof(SampleEnum.NoDescription));
        }
    }
}
