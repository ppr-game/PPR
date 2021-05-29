using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NCalc;

using SFML.System;

namespace PPR.UI.Animations {
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public struct AnimationContext {
        public int x { get; set; }
        public int y { get; set; }
        public char character { get; set; }
        public byte bgR { get; set; }
        public byte bgG { get; set; }
        public byte bgB { get; set; }
        public byte bgA { get; set; }
        public byte fgR { get; set; }
        public byte fgG { get; set; }
        public byte fgB { get; set; }
        public byte fgA { get; set; }
        public float time { get; set; }
        public Dictionary<string, double> args;
        public static readonly Dictionary<string, double> customVars = new Dictionary<string, double>();
        public double val(string name) => customVars[name];
        public double arg(string name) => args[name];
        // ReSharper disable once InconsistentNaming
        private static readonly Random _random = new Random();
        public int randomInt(int min, int max) => _random.Next(min, max);
        public double random(double min, double max) => _random.NextDouble() * (max - min) + min;
        public double posRandom(int x, int y) => posRandom(x, y, 0d);
        public double posRandom(int x, int y, double @default) =>
            Manager.positionRandoms.TryGetValue(new Vector2i(x, y), out float value) ? value : @default;
        public double lerp(double a, double b, double t) => t <= 0 ? a : t >= 1 ? b : a + (b - a) * t;
        public double abs(double value) => Math.Abs(value);
        public double ceil(double value) => ceiling(value);
        // ReSharper disable once MemberCanBeMadeStatic.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public double ceiling(double value) => Math.Ceiling(value);
        public double clamp(double value, double min, double max) => Math.Clamp(value, min, max);
        public double floor(double value) => Math.Floor(value);
        public double max(double a, double b) => Math.Max(a, b);
        public double min(double a, double b) => Math.Min(a, b);
        public double pow(double a, double b) => Math.Pow(a, b);
        public double round(double value) => Math.Round(value);
        public double sign(double value) => Math.Sign(value);
        public double sqrt(double value) => Math.Sqrt(value);
        public double toDouble(double value) => value;
    }
    
    public struct CustomAnimation {
        public Func<AnimationContext, int> x;
        public Func<AnimationContext, int> y;
        public Func<AnimationContext, string> character;
        public Func<AnimationContext, byte> bgR;
        public Func<AnimationContext, byte> bgG;
        public Func<AnimationContext, byte> bgB;
        public Func<AnimationContext, byte> bgA;
        public Func<AnimationContext, byte> fgR;
        public Func<AnimationContext, byte> fgG;
        public Func<AnimationContext, byte> fgB;
        public Func<AnimationContext, byte> fgA;

        public CustomAnimation(DeserializedAnimation animation) {
            x = animation.x is null ? null : new Expression(animation.x).ToLambda<AnimationContext, int>();
            y = animation.y is null ? null : new Expression(animation.y).ToLambda<AnimationContext, int>();
            character = animation.character is null ? null :
                new Expression(animation.character).ToLambda<AnimationContext, string>();

            bgR = animation.background?[0] is null ? null :
                new Expression(animation.background[0]).ToLambda<AnimationContext, byte>();
            bgG = animation.background?[1] is null ? null :
                new Expression(animation.background[1]).ToLambda<AnimationContext, byte>();
            bgB = animation.background?[2] is null ? null :
                new Expression(animation.background[2]).ToLambda<AnimationContext, byte>();
            bgA = animation.background?[3] is null ? null :
                new Expression(animation.background[3]).ToLambda<AnimationContext, byte>();

            fgR = animation.foreground?[0] is null ? null :
                new Expression(animation.foreground[0]).ToLambda<AnimationContext, byte>();
            fgG = animation.foreground?[1] is null ? null :
                new Expression(animation.foreground[1]).ToLambda<AnimationContext, byte>();
            fgB = animation.foreground?[2] is null ? null :
                new Expression(animation.foreground[2]).ToLambda<AnimationContext, byte>();
            fgA = animation.foreground?[3] is null ? null :
                new Expression(animation.foreground[3]).ToLambda<AnimationContext, byte>();
        }
    }
}
