using AutoMapper;
using FluentAssertions;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace AdaskoTheBeAsT.AutoMapper.SimpleInjector.Test.Integrations
{
#pragma warning disable CA1812
    public class ServiceLifetimeTests
    {
        internal interface ISingletonService
        {
            Bar DoTheThing(Foo theObj);
        }

        [Fact]
        public void CanUseDefaultInjectedIMapperInSingletonService()
        {
            // Arrange
            using var container = new Container();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();
            container.Register<ISingletonService, TestSingletonService>(Lifestyle.Singleton);
            container.AddAutoMapper(
                cfg =>
                {
                    cfg.WithMapperAssemblyMarkerTypes(typeof(ServiceLifetimeTests));
                    cfg.WithMapperConfigurationExpressionAction(
                        (
                            container1,
                            expression) => expression.CreateMap<Foo, Bar>().ReverseMap());
                });
            Bar actual;

            // Act
            using (ThreadScopedLifestyle.BeginScope(container))
            {
                var service = container.GetInstance<ISingletonService>();
                actual = service.DoTheThing(new Foo { TheValue = 1 });
            }

            // Assert
            actual.Should().NotBeNull();
            actual.TheValue.Should().Be(1);
        }

        internal class TestSingletonService : ISingletonService
        {
            private readonly IMapper _mapper;

            public TestSingletonService(IMapper mapper)
            {
                _mapper = mapper;
            }

            public Bar DoTheThing(Foo theObj)
            {
                var bar = _mapper.Map<Bar>(theObj);
                return bar;
            }
        }

        internal class Foo
        {
            public int TheValue { get; set; }
        }

        internal class Bar
        {
            public int TheValue { get; set; }
        }
    }
#pragma warning restore CA1812
}