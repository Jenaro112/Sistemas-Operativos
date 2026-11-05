using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lamport.Common;

public static class ServiceSetup
{
    /// <summary>Registra el proceso Lamport (nombre y desfase del reloj físico vienen de variables de entorno).</summary>
    public static LamportProcess AddLamportProcess(this WebApplicationBuilder builder, string defaultName, double defaultOffsetSeconds)
    {
        var process = new LamportProcess(
            Env.Str("SERVICE_NAME", defaultName),
            Env.Num("PHYSICAL_CLOCK_OFFSET_SECONDS", defaultOffsetSeconds));
        builder.Services.AddSingleton(process);
        builder.Services.AddControllers().AddApplicationPart(typeof(EventsController).Assembly);
        return process;
    }

    /// <summary>Demora de red simulada (NETWORK_DELAY_MS) antes de enviar cada mensaje.</summary>
    public static Task NetworkDelay() =>
        Env.Int("NETWORK_DELAY_MS", 0) is var ms and > 0 ? Task.Delay(ms) : Task.CompletedTask;
}
