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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
                    await service.AutoCheckoutPendingAsync();
                }

                // Auto checkout every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
