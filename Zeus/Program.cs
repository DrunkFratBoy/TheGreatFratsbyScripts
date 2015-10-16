using System;
using System.Linq;
using System.Windows.Input;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;

using SharpDX;
using SharpDX.Direct3D9;

namespace Zeus
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Zeus.Init();
            Orbwalker.Init();
        }
    }
}
