using Game.Core;
using System;

Console.WriteLine("=================================");
Console.WriteLine("  凯恩斯经济模拟引擎");
Console.WriteLine("  Keynes Economic Simulation Engine");
Console.WriteLine("=================================");
Console.WriteLine();

var engine = new Engine();
engine.Initialize();
engine.Start();

Console.WriteLine();
Console.WriteLine("Engine demo completed!");
