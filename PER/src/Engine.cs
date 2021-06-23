using System;
using System.Reflection;

using NLog;

using PER.Abstractions;
using PER.Util;

namespace PER {
    public class Engine {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly string version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        public event EventHandler setupFinished;

        public IReadOnlyStopwatch clock => _clock;
        public double deltaTime { get; private set; }
        
        public double tickInterval { get; set; }
        public IGame game { get; init; }

        private Stopwatch _clock;
        private TimeSpan _prevTime;
        private double _tickAccumulator;
        
        public void Start() {
            try {
                logger.Info($"PER v{version}");
                Setup();
                while(Loop()) UpdateDeltaTime();
                Stop();
            }
            catch(Exception exception) {
                logger.Error("Uncaught exception! Please, report the text below to the developer of the game.");
                logger.Fatal(exception);
                throw;
            }
        }

        private void Setup() {
            _clock = new Stopwatch();
            _clock.Reset();
            
            game.Setup();
            
            logger.Info("Setup finished");
            setupFinished?.Invoke(this, EventArgs.Empty);
        }

        private bool Loop() {
            game.Loop();

            TryTick();
            return true;
        }

        private void TryTick() {
            if(tickInterval <= 0d) return;
            _tickAccumulator += deltaTime;

            while(_tickAccumulator >= tickInterval) {
                Tick();
                _tickAccumulator -= tickInterval;
            }
        }

        private void Tick() {
            game.Tick();
        }

        private void UpdateDeltaTime() {
            TimeSpan time = clock.time;
            deltaTime = (clock.time - _prevTime).TotalSeconds;
            _prevTime = time;
        }

        private void Stop() {
            game.Stop();
        }
    }
}
