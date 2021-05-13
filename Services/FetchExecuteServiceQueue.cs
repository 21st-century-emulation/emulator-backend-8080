using System;
using System.Threading;
using System.Threading.Tasks;
using emulator_backend_8080.Models;
using Microsoft.Extensions.Logging;

namespace emulator_backend_8080.Services
{
    public class FetchExecuteServiceQueue
    {
        public ILogger<FetchExecuteServiceQueue> _logger;
        private string _computerIdWaitingForProcessing;
        private SemaphoreSlim _checkStringLock = new SemaphoreSlim(1, 1);
        private Cpu _cpu;
        private SemaphoreSlim _cpuLock = new SemaphoreSlim(1, 1);

        public FetchExecuteServiceQueue(ILogger<FetchExecuteServiceQueue> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal async Task SetStateAsync(Cpu cpu, CancellationToken cancellationToken = default)
        {
            if (!await _cpuLock.WaitAsync(1000, cancellationToken))
            {
                _logger.LogError("Couldn't get write lock to set cpu state");
                return;
            }

            _cpu = cpu;
            _cpuLock.Release();
        }

        internal async Task<Cpu?> GetStateAsync(CancellationToken cancellationToken = default)
        {
            if (!await _cpuLock.WaitAsync(1000, cancellationToken))
            {
                _logger.LogError("Couldn't get read lock for cpu state");
                return null;
            }

            var returnedCpu = _cpu;
            _cpuLock.Release();
            return returnedCpu;
        }

        internal async Task<bool> SetComputerToWaitingAsync(string id, CancellationToken cancellationToken = default)
        {
            if (!await _checkStringLock.WaitAsync(1000, cancellationToken))
            {
                return false;
            }

            _computerIdWaitingForProcessing = id;
            _checkStringLock.Release();
            return true;
        }

        internal async Task<string> GetComputerWaitingAsync(CancellationToken cancellationToken = default)
        {
            if (!await _checkStringLock.WaitAsync(1000, cancellationToken))
            {
                _logger.LogError("Deadlock access id for waiting computer");
                return null;
            }

            var computerId = _computerIdWaitingForProcessing;
            _checkStringLock.Release();
            return computerId;
        }
    }
}
