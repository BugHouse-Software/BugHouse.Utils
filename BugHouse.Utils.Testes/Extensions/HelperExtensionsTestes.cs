
using BugHouse.Utils.Extensions;
using System;
namespace BugHouse.Utils.Testes.Tools.Extensions
{

    public class HelperExtensionsTestes
    {

        #region Base64

        [Theory]
        [InlineData("   ")]
        [InlineData("valid")]
        [InlineData("   valid   ")]
        [InlineData("valid string")]
        public void Test_ToBase64(string eq)
        {
            var inBase64 = eq.ToBase64();

            Assert.NotEmpty(inBase64);

            var result = inBase64.ToBase64ToString();

            Assert.Equal(eq, result);
        }
        [Theory]
        [InlineData(new byte[] { 0x00 })]
        [InlineData(new byte[] { 0x01, 0x02, 0x03 })]
        [InlineData(new byte[] { 0xFF, 0xFE, 0xFD, 0xFC })]
        public void Test_ToBase64Bytes(byte[] eq)
        {
            var inBase64 = eq.ToBase64();

            Assert.NotEmpty(inBase64);

            var result = inBase64.ToBase64ToBytes();

            Assert.Equal(eq, result);
        }

        #endregion

        #region Testes To Int
        [Theory]
        [InlineData("", 0)]
        [InlineData(0, 0)]
        [InlineData("1", 1)]
        [InlineData(2, 2)]
        [InlineData("a8", 0)]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void Test_ToInt(object value, int assert)
        {

            if (value is string x)
            {
                var result = x.ToInt();
                Assert.Equal(assert, result);
            }
            else
            {
                var result = value.ToInt();
                Assert.Equal(assert, result);
            }
        }


        [Theory]
        [InlineData("", null)]
        [InlineData(0, 0)]
        [InlineData("1", 1)]
        [InlineData(2, 2)]
        [InlineData("a8", null)]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void Test_ToIntNulable(object value, int? assert)
        {

            if (value is string x)
            {
                var result = x.ToIntNullable();
                Assert.Equal(assert, result);
            }
            else
            {
                var result = value.ToIntNullable();
                Assert.Equal(assert, result);
            }
        }
        #endregion
        #region Teste DateTime
        [Fact]
        public void Test_ToDateTime()
        {
            string value = "2024-01-01";

            if (value is string x)
            {
                var result = x.ToDateTime();
                Assert.Equal(new DateTime(2024, 01, 01), result);
            }
            else
            {
                var result = value.ToDateTime();
                Assert.Equal(new DateTime(2024, 01, 01), result);
            }
        }


        [Fact]
        public void Test_ToDateTimeNulable()
        {

            string value = "2024-01-01";

            if (value is string x)
            {
                var result = x.ToDateTimeNulable();
                Assert.Equal(new DateTime(2024, 01, 01), result);
            }
            else
            {
                var result = value.ToDateTimeNulable();
                Assert.Equal(new DateTime(2024, 01, 01), result);
            }
        }
        #endregion

        #region Testes To Decimal
        [Theory]
        [InlineData("", null)]
        [InlineData(0, "0")]
        [InlineData("1.8", "1.8")]
        [InlineData(2, "2")]
        [InlineData("a8", null)]
        [InlineData("1,9", "1,9")]
        [InlineData(8.5f, "8.5")]
        public void Test_ToDecimalNulable(object value, string assert)
        {

            if (value is string x)
            {
                var result = x.ToDecimalNulable();
                Assert.Equal(assert.ToDecimalNulable(), result);
            }
            else
            {
                var result = value.ToDecimalNulable();
                Assert.Equal(assert.ToDecimalNulable(), result);
            }
        }


        [Theory]
        [InlineData("", 0d)]
        [InlineData(0, 0d)]
        [InlineData("1.8", 1.8d)]
        [InlineData(2, 2d)]
        [InlineData("a8", 0d)]
        [InlineData("1,9", 1.9d)]
        [InlineData(8.5f, 8.5d)]
        public void Test_ToDecimal(object value, decimal assert)
        {

            if (value is string x)
            {
                var result = x.ToDecimal();
                Assert.Equal(assert, result);
            }
            else
            {
                var result = value.ToDecimal();
                Assert.Equal(assert, result);
            }
        }

        #endregion
        #region Testes To Boolean
        [Theory]
        [InlineData("", null)]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData("1", true)]
        [InlineData("3", true)]
        [InlineData(true, true)]
        [InlineData("0", false)]
        [InlineData(false, false)]
        [InlineData("True", true)]
        [InlineData("false", false)]
        [InlineData("oie eu sou um teste", null)]
        public void Test_ToBooleanNulable(object value, bool? assert)
        {

            if (value is string x)
            {
                var result = x.ToBooleanNulable();
                Assert.Equal(assert, result);
            }
            else
            {
                var result = value.ToBooleanNulable();
                Assert.Equal(assert, result);
            }
        }


        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        [InlineData("", false)]
        [InlineData("1", true)]
        [InlineData(true, true)]
        [InlineData("0", false)]
        [InlineData("3", true)]
        [InlineData(false, false)]
        [InlineData("True", true)]
        [InlineData("false", false)]
        [InlineData("oie eu sou um teste", false)]
        public void Test_ToBoolean(object value, bool assert)
        {

            if (value is string x)
            {
                var result = x.ToBoolean();
                Assert.Equal(assert, result);
            }
            else
            {
                var result = value.ToBoolean();
                Assert.Equal(assert, result);
            }
        }

        #endregion
        #region Teste IsNullOrWhiteSpace
        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("valid", false)]
        [InlineData("   valid   ", false)]
        [InlineData("valid string", false)]
        public void Test_IsNullOrWhiteSpace(string input, bool assert)
        {
            // Act
            var result = input.IsNullOrWhiteSpace();

            // Assert
            Assert.Equal(assert, result);
        }

        #endregion
        #region List

        [Theory]
        [InlineData(null, true)]
        [InlineData(new string[] { }, true)]
        [InlineData(new string[] { "", "" }, false)]
        [InlineData(new string[] { "Teste", "Vallue" }, false)]
        public void Test_ListIsNullOrEmpty(string[] value, bool status)
        {
            var result = value.IsNullOrEmpty();
            Assert.Equal(status, result);
        }

        #endregion

        #region Json
        [Fact]
        public void Test_JsonSerializeDeserialize()
        {
            var value = new RandomTestClass();

            var objString = value.ToSerialize();

            Assert.NotEmpty(objString);

            var x2 = objString.ToDeserialize<RandomTestClass>();

            Assert.Equivalent(value, x2);
        }

        #endregion
    }


    public class RandomTestClass
    {
        public RandomTestClass(int id, string name, DateTime date, bool isActive)
        {
            Id=id;
            Name=name;
            Date=date;
            IsActive=isActive;
        }
        public RandomTestClass()
        {
            Random _random = new Random();
            this.Id = _random.Next(1, 1000);
            this.Name = GenerateRandomString(10);
            this.Date = DateTime.Now.AddDays(_random.Next(-100, 100));
            this.IsActive = _random.Next(0, 2) == 1;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }



        private static string GenerateRandomString(int length)
        {
            Random _random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[_random.Next(chars.Length)];
            }
            return new string(stringChars);
        }
    }
}
