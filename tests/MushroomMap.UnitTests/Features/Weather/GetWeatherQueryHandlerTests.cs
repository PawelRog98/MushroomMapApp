using FluentAssertions;
using Moq;
using MushroomMapApp.Domain.Interfaces;
using MushroomMapApp.Features.Weather.GetWeather;
using MushroomMapApp.Infrastructure.ExternalApis.WeatherApi.Interfaces;
using Xunit;

namespace MushroomMap.UnitTests.Features.Weather;

public class GetWeatherQueryHandlerTests
{
    private readonly Mock<IWeatherClient> _weatherClientMock;
    private readonly Mock<IRedisCache> _redisCacheMock;
    private readonly GetWeatherQueryHandler _handler;

    public GetWeatherQueryHandlerTests()
    {
        _weatherClientMock = new Mock<IWeatherClient>();
        _redisCacheMock = new Mock<IRedisCache>();
        _handler = new GetWeatherQueryHandler(_weatherClientMock.Object, _redisCacheMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCachedValue_WhenCacheHit()
    {
        var cached = new WeatherDataDto { Temperature = 15.5, Time = "2026-10-08T10:00" };

        _redisCacheMock
            .Setup(x => x.GetAsync<WeatherDataDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var query = new GetWeatherQuery(new GetWeatherRequest(50, 19));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeSameAs(cached);
        _weatherClientMock.Verify(
            x => x.GetWeather(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_FetchesFromApi_AndCaches_WhenCacheMiss()
    {
        var weather = new WeatherDataDto
        {
            Time = "2026-10-08T10:00",
            Temperature = 21.5,
            Precipitation = 0.5,
            WindSpeed = 12.3,
            WeatherCode = 1
        };

        _redisCacheMock
            .Setup(x => x.GetAsync<WeatherDataDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherDataDto?)null);

        _weatherClientMock
            .Setup(x => x.GetWeather(50, 19, It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        var query = new GetWeatherQuery(new GetWeatherRequest(50, 19));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeSameAs(weather);

        _redisCacheMock.Verify(
            x => x.SetAsync(
                "weather:50:19",
                weather,
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UsesCorrectCacheKey()
    {
        _redisCacheMock
            .Setup(x => x.GetAsync<WeatherDataDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherDataDto?)null);

        _weatherClientMock
            .Setup(x => x.GetWeather(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WeatherDataDto());

        var query = new GetWeatherQuery(new GetWeatherRequest(-33, 151));

        await _handler.Handle(query, CancellationToken.None);

        _redisCacheMock.Verify(
            x => x.GetAsync<WeatherDataDto>("weather:-33:151", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CachesResult_WhenApiReturnsWeather()
    {
        var weather = new WeatherDataDto { Temperature = 20.0 };

        _redisCacheMock
            .Setup(x => x.GetAsync<WeatherDataDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherDataDto?)null);

        _weatherClientMock
            .Setup(x => x.GetWeather(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(weather);

        var query = new GetWeatherQuery(new GetWeatherRequest(50, 19));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeSameAs(weather);
        _redisCacheMock.Verify(
            x => x.SetAsync(It.IsAny<string>(), It.IsAny<WeatherDataDto>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
