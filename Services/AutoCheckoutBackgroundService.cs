namespace HRMSystem.Services
{
    public class AutoCheckoutBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AutoCheckoutBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var target = new DateTime(now.Year, now.Month, now.Day, 23, 59, 0);

                var delay = target - now;
                if (delay.TotalMilliseconds < 0)
                    delay = TimeSpan.FromHours(24);

                await Task.Delay(delay, stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
                    await service.AutoCheckoutPendingAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
