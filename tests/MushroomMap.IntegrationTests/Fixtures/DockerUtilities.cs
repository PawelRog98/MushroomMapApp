using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace MushroomMap.IntegrationTests.Fixtures;

public static class DockerUtilities
{
    private static readonly byte[] PingRequest =
        "GET /_ping HTTP/1.1\r\nHost: docker\r\nConnection: close\r\n\r\n"u8.ToArray();

    public static void EnsureDockerAvailable()
    {
        if (ResolveEndpoint() is not null)
            return;

        StartPodmanService();

        if (ResolveEndpoint() is not null)
            return;

        throw new InvalidOperationException(
            $"Docker is not available. Run: podman system service --time=0 unix://{RuntimeDir}/podman/podman.sock &");
    }

    private static string RuntimeDir =>
        Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ?? "/run/user/1000";

    private static string? ResolveEndpoint()
    {
        foreach (var endpoint in CandidateEndpoints())
        {
            if (!IsReachable(endpoint))
                continue;

            Environment.SetEnvironmentVariable("DOCKER_HOST", endpoint);
            return endpoint;
        }

        return null;
    }

    private static IEnumerable<string> CandidateEndpoints()
    {
        var configured = Environment.GetEnvironmentVariable("DOCKER_HOST");
        if (!string.IsNullOrWhiteSpace(configured))
            yield return configured;

        yield return $"unix://{RuntimeDir}/podman/podman.sock";
        yield return "unix:///var/run/docker.sock";
        yield return "unix:///run/podman/podman.sock";
    }

    private static bool IsReachable(string endpoint)
    {
        if (!endpoint.StartsWith("unix://", StringComparison.Ordinal))
            return true;

        try
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            socket.ReceiveTimeout = 3000;
            socket.Connect(new UnixDomainSocketEndPoint(endpoint["unix://".Length..]));
            socket.Send(PingRequest);

            var response = new byte[128];
            var length = socket.Receive(response);
            return Encoding.ASCII.GetString(response, 0, length).StartsWith("HTTP/", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static void StartPodmanService()
    {
        var podman = FindOnPath("podman");
        if (podman is null)
            return;

        var endpoint = $"unix://{RuntimeDir}/podman/podman.sock";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = podman,
                Arguments = $"system service --time=0 {endpoint}",
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch
        {
            return;
        }

        for (var i = 0; i < 60 && !IsReachable(endpoint); i++)
            Thread.Sleep(500);
    }

    private static string? FindOnPath(string name)
        => (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator)
            .Select(directory => Path.Combine(directory, name))
            .FirstOrDefault(File.Exists);
}
