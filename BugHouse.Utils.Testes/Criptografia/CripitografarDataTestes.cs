using AutoFixture;
using BugHouse.Utils.Criptografia;
using BugHouse.Utils.Extensions;
using System;

namespace BugHouse.Utils.Testes.Criptografia
{
    public class CripitografarDataTestes
    {

        private TesteCriptografia _objectTest;
        private readonly Fixture _fixture;

        public CripitografarDataTestes()
        {
            _fixture = new Fixture();
            _objectTest = _fixture.Create<TesteCriptografia>();
        }


        [Fact]
        public void Test_CriptografarDados()
        {
            var service = new CripitografarDataService();
            var result = service.Criptograr(_objectTest, _objectTest.Nome, _objectTest.DataNascimento);

            Assert.NotNull(result);
            Assert.NotEmpty(result);



        }

        [Fact]
        public void Test_DescriptografarDados()
        {
            var service = new CripitografarDataService();
            var result = service.Criptograr(_objectTest, _objectTest.Nome, _objectTest.DataNascimento);

            Assert.NotNull(result);
            Assert.NotEmpty(result);

            var resultFinal = service.Descriptografar<TesteCriptografia>(result, _objectTest.Nome, _objectTest.DataNascimento);

            Assert.NotNull(resultFinal);
            Assert.Equal(_objectTest.ToSerialize(), resultFinal.ToSerialize());
        }



        public class TesteCriptografia
        {
            public string Nome { get; set; }
            public int Idade { get; set; }
            public DateTime DataNascimento { get; set; }
        }
    }
}
